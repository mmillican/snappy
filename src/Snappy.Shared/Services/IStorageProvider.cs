using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Snappy.Shared.Config;

namespace Snappy.Shared.Services;

public interface IStorageProvider
{
    string ResolveFileUrl(string path);
}

public class AmazonS3StorageProvider : IStorageProvider
{
    private readonly IAmazonS3 _s3Client;
    private readonly AwsConfig _awsConfig;

    public AmazonS3StorageProvider(IAmazonS3 s3Client,
        IOptions<AwsConfig> awsOptions)
    {
        _s3Client = s3Client;
        _awsConfig = awsOptions.Value;
    }

    public string ResolveFileUrl(string path)
    {
        if (path.StartsWith("/"))
        {
            path = path.Substring(1);
        }

        if (_awsConfig.PreSignGetUrls)
        {
            var presignRequest = new GetPreSignedUrlRequest
            {
                BucketName = _awsConfig.StorageBucketName,
                Key = path,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(_awsConfig.PreSignGetUrlsDuration),
            };

            var presignedUrl = _s3Client.GetPreSignedURL(presignRequest);
            if (presignedUrl is not null)
            {
                return presignedUrl;
            }
        }

        return $"https://s3.amazonaws.com/{_awsConfig.StorageBucketName}/{path}";
    }

}
