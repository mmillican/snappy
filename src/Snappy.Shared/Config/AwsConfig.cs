namespace Snappy.Shared.Config;

public class AwsConfig
{
    public string StorageBucketName { get; set; }
    public bool PreSignGetUrls { get; set; }
    /// <summary>
	/// The number of minutes a URL should be pre-signed for
	/// </summary>
	/// <value></value>
    public int PreSignGetUrlsDuration { get; set; }
}
