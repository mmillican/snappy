namespace Snappy.Shared.Config;

public static class AWSEnvironment
{
    public static class S3Buckets
    {
        public static readonly string StorageBucketName = Environment.GetEnvironmentVariable("StorageBucketName");
    }

    public static class Queues
    {
        public static readonly string NewImageQueueUri = Environment.GetEnvironmentVariable("NewImageQueueUrl");
    }

    public static class SnsTopics
    {
        public static readonly string ThumbnailWorkerTopicArn = Environment.GetEnvironmentVariable("ThumbnailWorkerTopicArn");
    }

    public static class DynamoTables
    {
        public static readonly string AlbumTableName = Environment.GetEnvironmentVariable("AlbumTableName");
        public static readonly string PhotoTableName = Environment.GetEnvironmentVariable("PhotoTableName");
    }
}
