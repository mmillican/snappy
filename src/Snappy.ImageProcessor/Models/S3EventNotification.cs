using System.Text.Json.Serialization;

namespace Snappy.ImageProcessor.Models;

/// <summary>
/// This is basically a copy of the models from the S3 library, but modified for [de]serialization
/// </summary>
[Obsolete("change name")]
public class S3EventNotification_new
{
    [JsonPropertyName("Records")]
    public List<S3EventNotificationRecord> Records { get; set; }

    public class UserIdentityEntity
    {
        public string PrincipalId { get; set; }
    }

    public class S3BucketEntity
    {

        public string Name { get; set; }

        public UserIdentityEntity OwnerIdentity { get; set; }

        public string Arn { get; set; }
    }

    public class S3ObjectEntity
    {
        public string Key { get; set; }

        public long Size { get; set; }

        public string ETag { get; set; }

        public string VersionId { get; set; }

        public string Sequencer { get; set; }
    }

    public class S3Entity
    {
        public string ConfigurationId { get; set; }

        public S3BucketEntity Bucket { get; set; }

        public S3ObjectEntity Object { get; set; }

        public string S3SchemaVersion { get; set; }
    }

    public class RequestParametersEntity
    {
        public string SourceIPAddress { get; set; }
    }

    public class ResponseElementsEntity
    {

        public string XAmzId2 { get; set; }

        public string XAmzRequestId { get; set; }
    }

    public class S3EventNotificationRecord
    {

        public string AwsRegion { get; set; }

        public string EventName { get; set; }

        public string EventSource { get; set; }

        public DateTime EventTime { get; set; }

        public string EventVersion { get; set; }

        public RequestParametersEntity RequestParameters { get; set; }

        public ResponseElementsEntity ResponseElements { get; set; }

        public S3Entity S3 { get; set; }

        public UserIdentityEntity UserIdentity { get; set; }
    }
}
