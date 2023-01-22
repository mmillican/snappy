// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.S3;
using Amazon.S3.Util;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Snappy.ImageProcessor.Models;
using Snappy.Shared.Models;
using Snappy.Shared.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.CamelCaseLambdaJsonSerializer))]

namespace Snappy.ImageProcessor;

public class NewImageHandler
{
    private readonly string _storageBucketName = Environment.GetEnvironmentVariable("StorageBucketName");
    private readonly string _newImageQueueUri = Environment.GetEnvironmentVariable("NewImageQueueUrl");
    private readonly string _thumbnailWorkerTopicArn = Environment.GetEnvironmentVariable("ThumbnailWorkerTopicArn");
    private readonly string _albumTableName = Environment.GetEnvironmentVariable("AlbumTableName");
    private readonly string _photoTableName = Environment.GetEnvironmentVariable("PhotoTableName");

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
        _albumService = new AlbumService(_albumTableName);
        _photoService = new PhotoService(_photoTableName);
    }

    public NewImageHandler(IAmazonS3 s3Client) : this()
    {
        _s3Client = s3Client;
    }

    public async Task<string> FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        try
        {
            context.Logger.LogLine($"Processing {evnt.Records.Count} messages from queue");

            foreach(var sqsRecord in evnt.Records)
            {
                // TODO: Remove me
                context.Logger.LogLine($"... sqs body: {sqsRecord.Body}");
                var s3Message = JsonSerializer.Deserialize<S3EventNotification_new>(sqsRecord.Body, _jsonSerializerOptions);
                foreach(var s3Record in s3Message.Records)
                {
                    await ProcessS3Record(s3Record, context);
                }

                // Delete the message from the queue
                await _sqsClient.DeleteMessageAsync(_newImageQueueUri, sqsRecord.ReceiptHandle);
            }

            return "Ok";
        }
        catch(Exception ex)
        {
            context.Logger.LogError($"Error processing SQS event: {ex.StackTrace}");
            throw;
        }
    }

    public async Task ProcessS3Record(S3EventNotification_new.S3EventNotificationRecord record, ILambdaContext context)
    {
        // TODO: metadata

        var s3event = record.S3;
        var objectKey = s3event.Object.Key.Replace("+", " "); // When serialized, spaces become '+'; convert back to space.
        var fqObjectKey = $"s3://{s3event.Bucket.Name}/{objectKey}";

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
                AlbumSlug = objectKey.Substring(0, objectKey.LastIndexOf('/')),
                FileName = Path.GetFileName(objectKey),
                SavedFiledName = $"{imageId}{Path.GetExtension(objectKey)}",
                Title = Path.GetFileName(objectKey), // For now, default to the file name
                CreatedOn = DateTime.UtcNow, // TODO: Create a service for testing
                UpdatedOn = DateTime.UtcNow,
            };

            // Copy the file from the upload bucket
            context.Logger.LogLine("... copying object to storage bucket");
            var destFileKey = $"{photoRecord.AlbumSlug}/{photoRecord.SavedFiledName}";
            await _s3Client.CopyObjectAsync(s3event.Bucket.Name, objectKey, _storageBucketName, destFileKey);

            context.Logger.LogLine("... saving photo record");
            await _photoService.Save(photoRecord);

            // TODO: Submit resize requests

            context.Logger.LogLine("... creating album record if it doesn't exist");
            await _albumService.CreateAlbumIfNotExists(photoRecord.AlbumSlug);

            // TODO: Process metadata

            // Delete the original
            await _s3Client.DeleteObjectAsync(s3event.Bucket.Name, objectKey);
        }
        catch(Exception ex)
        {
            context.Logger.LogError($"Error processing image '{fqObjectKey}': {ex.Message}");
            context.Logger.LogError(ex.StackTrace);
            throw;
        }
    }
}
