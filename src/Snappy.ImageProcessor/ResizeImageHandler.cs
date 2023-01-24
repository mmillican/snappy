using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Amazon.S3;
using Amazon.S3.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using Snappy.Shared.Images;

namespace Snappy.ImageProcessor;

public class ResizeImageHandler
{
    private readonly IAmazonS3 _s3Client;

    private static JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ResizeImageHandler()
    {
        _s3Client = new AmazonS3Client();
    }

    public async Task Handle(SNSEvent evt, ILambdaContext context)
    {
        var serialized = JsonSerializer.Serialize(evt);
        context.Logger.LogLine($"Processing request: {serialized}");

        foreach(var record in evt.Records)
        {
            await ProcessRecord(record, context);
        }
    }

    private async Task ProcessRecord(SNSEvent.SNSRecord record, ILambdaContext context)
    {
        var request = JsonSerializer.Deserialize<ResizeImageRequest>(record.Sns.Message, _jsonSerializerOptions);

        context.Logger.LogLine($"Resizing '{request.ObjectKey}' to {request.Sizes.Count} sizes");

        try
        {
            using var objectResponse = await _s3Client.GetObjectAsync(request.BucketName, request.ObjectKey);

            context.Logger.LogLine("... retrieved object from bucket");

            var imageContentType = objectResponse.Headers.ContentType;

            IImageFormat imageformat;
            using var image = Image.Load(objectResponse.ResponseStream, out imageformat);

            foreach(var size in request.Sizes)
            {
                context.Logger.LogLine($"... resizing image to '{size.Key}'");

                var resizedFileKey = ImageHelper.GetResizedFileName(request.ObjectKey, size.Key);

                using var outStream = new MemoryStream();

                var resizeOptions = new ResizeOptions();

                if (size.ResizeMode.HasValue)
                {
                    resizeOptions.Mode = size.ResizeMode.Value;
                }

                if (image.Width > image.Height || image.Width == image.Height)
                {
                    // Landscape or square
                    resizeOptions.Size = new Size(size.Width, size.Height);
                }
                else
                {
                    // Portrait
                    resizeOptions.Size = new Size(size.Height, size.Width);
                }

                image.Mutate(x => x.Resize(resizeOptions));

                image.Save(outStream, imageformat);

                var putResizedObjectRequest = new PutObjectRequest
                {
                    BucketName = request.BucketName,
                    Key = resizedFileKey,
                    ContentType = imageContentType,
                    InputStream = outStream,
                };

                await _s3Client.PutObjectAsync(putResizedObjectRequest);

                context.Logger.LogLine($"... Resized image to {size.Key} and saved to '{resizedFileKey}'");
            }
        }
        catch(Exception ex)
        {
            context.Logger.LogError($"Error resizing image '{request.ObjectKey}'. Exception: {ex.Message}");
            throw;
        }
    }
}
