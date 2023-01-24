namespace Snappy.Shared.Images;

public class ResizeImageRequest
{
    public string BucketName { get; set; }
    public string ObjectKey { get; set; }

    public List<ImageSize> Sizes { get; set; } = new();
}
