// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.S3;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using SixLabors.ImageSharp;
using Snappy.Shared.Config;
using Snappy.Shared.Images;
using Snappy.Shared.Models;
using Snappy.Shared.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.CamelCaseLambdaJsonSerializer))]

namespace Snappy.ImageProcessor;

public class NewImageHandler
{
    private readonly IAmazonS3 _s3Client;
    private readonly IAmazonSQS _sqsClient;
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly AlbumService _albumService;
    private readonly PhotoService _photoService;

    private static JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public NewImageHandler()
    {
        _s3Client = new AmazonS3Client();
        _sqsClient = new AmazonSQSClient();
        _snsClient = new AmazonSimpleNotificationServiceClient();
        _albumService = new AlbumService(AWSEnvironment.DynamoTables.AlbumTableName);
        _photoService = new PhotoService(AWSEnvironment.DynamoTables.PhotoTableName);
    }

    public NewImageHandler(IAmazonS3 s3Client) : this()
    {
        _s3Client = s3Client;
    }

    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        try
        {
            context.Logger.LogLine($"Processing {evnt.Records.Count} messages from queue");

            foreach(var sqsRecord in evnt.Records)
            {
                context.Logger.LogDebug($"... record message id {sqsRecord.MessageId} / source: {sqsRecord.EventSource}");

                // TODO: Remove me
                context.Logger.LogLine($"... sqs body: {sqsRecord.Body}");

                if (sqsRecord.Body.Contains("s3:TestEvent"))
                {
                    context.Logger.LogLine("Found 'test' event; ignoring!");
                    continue;
                }

                var s3Message = JsonSerializer.Deserialize<Models.S3EventNotification>(sqsRecord.Body, _jsonSerializerOptions);
                foreach(var s3Record in s3Message.Records)
                {
                    await ProcessS3Record(s3Record, context);
                }

                // Delete the message from the queue
                await _sqsClient.DeleteMessageAsync(AWSEnvironment.Queues.NewImageQueueUri, sqsRecord.ReceiptHandle);
            }
        }
        catch(Exception ex)
        {
            context.Logger.LogError($"Error processing SQS event: {ex.StackTrace}");
            throw;
        }
    }

    public async Task ProcessS3Record(Models.S3EventNotification.S3EventNotificationRecord record, ILambdaContext context)
    {
        // TODO: metadata

        var s3event = record.S3;
        var objectKey = s3event.Object.Key.Replace("+", " "); // When serialized, spaces become '+'; convert back to space.
        var fqObjectKey = $"s3://{s3event.Bucket.Name}/{objectKey}";

        // TODO: Use content/mime type to check instead
        var fileExtension = Path.GetExtension(objectKey).ToLower();
        var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".gif", ".png", ".webp"};
        if (!allowedImageExtensions.Contains(fileExtension))
        {
            context.Logger.LogLine($"{fqObjectKey} is not an image. Skipping processing.");
            return;
        }

        // TODO: Handle file names with special characters/spaces
        try
        {
            var getObjectResponse = await _s3Client.GetObjectAsync(s3event.Bucket.Name, objectKey);
        }
        catch(AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // If the original object doesn't exist, don't continue processing
            context.Logger.LogLine($"Object '{fqObjectKey}' not found. Skipping record.");
            return;
        }

        try
        {
            context.Logger.LogLine($"Processing image '{fqObjectKey}'...");

            var imageId = Guid.NewGuid();

            var photoRecord = new Photo
            {
                Id = imageId,
                AlbumSlug = objectKey.Substring(0, objectKey.LastIndexOf('/')),
                FileName = Path.GetFileName(objectKey),
                SavedFileName = $"{imageId}{Path.GetExtension(objectKey)}".ToLower(),
                Title = Path.GetFileNameWithoutExtension(objectKey), // For now, default to the file name
                CreatedOn = DateTime.UtcNow, // TODO: Create a service for testing
                UpdatedOn = DateTime.UtcNow,
            };

            // Copy the file from the upload bucket
            context.Logger.LogLine("... copying object to storage bucket");
            var destFileKey = $"{photoRecord.AlbumSlug}/{photoRecord.SavedFileName}";
            await _s3Client.CopyObjectAsync(s3event.Bucket.Name, objectKey, AWSEnvironment.S3Buckets.StorageBucketName, destFileKey);

            context.Logger.LogLine($"... saving photo record ID {photoRecord.Id}");
            await _photoService.Save(photoRecord);

            await SubmitResizeRequest(AWSEnvironment.S3Buckets.StorageBucketName, destFileKey);

            context.Logger.LogLine("... creating album record if it doesn't exist");
            await _albumService.CreateAlbumIfNotExists(photoRecord.AlbumSlug);

            // Delete the original
            await _s3Client.DeleteObjectAsync(s3event.Bucket.Name, objectKey);
            context.Logger.LogDebug($"... deleting file from upload bucket");

            await ParsePhotoMetadata(destFileKey, photoRecord, context);
        }
        catch(Exception ex)
        {
            context.Logger.LogError($"Error processing image '{fqObjectKey}': {ex.Message}");
            context.Logger.LogError(ex.StackTrace);
            throw;
        }
    }

    private async Task SubmitResizeRequest(string bucketName, string objectKey)
    {
        var request = new ResizeImageRequest
        {
            BucketName = bucketName,
            ObjectKey = objectKey,
            Sizes = ImageHelper.GetImageSizes().ToList(),
        };

        var requestBody = JsonSerializer.Serialize(request, _jsonSerializerOptions);
        await _snsClient.PublishAsync(AWSEnvironment.SnsTopics.ThumbnailWorkerTopicArn, requestBody);
    }

    private async Task ParsePhotoMetadata(string objectKey, Photo photoRecord, ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation("... processing photo metadata");

            using var objectResponse = await _s3Client.GetObjectAsync(AWSEnvironment.S3Buckets.StorageBucketName, objectKey);

            context.Logger.LogDebug("... retrieved object from bucket");

            using var image = Image.Load(objectResponse.ResponseStream);

            var exif = image.Metadata.ExifProfile;
            if (exif is null)
            {
                context.Logger.LogError($"... Could not load EXIF data for '{photoRecord.Id}'");
                return;
            }

            // TODO: Other properties we might be interested in?
            // TODO: Map Orientation to enum / descriptive value
            // TODO: Reformat DateTime
            var interestedExifTags = new[]
            {
                "Orientation", "Make", "Model", "Software", "DateTime", "ExposureTime", "FNumber", "ApertureValue",
                "LensMake", "LensModel",
            };

            foreach(var tv in exif.Values.Where(x => interestedExifTags.Contains(x.Tag.ToString()) && x.GetValue() != null))
            {
                if (!photoRecord.Metadata.ContainsKey(tv.Tag.ToString()))
                {
                    photoRecord.Metadata.Add(tv.Tag.ToString(), tv.GetValue().ToString());
                }
            }

            context.Logger.LogDebug($"... saved {photoRecord.Metadata.Keys.Count} metadata properties");

            await _photoService.Save(photoRecord);

            context.Logger.LogInformation("... saved photo metadata");
        }
        catch(Exception ex)
        {
            context.Logger.LogError($"... error parsing photo metadata. Error: {ex.Message}");
        }
    }
}
