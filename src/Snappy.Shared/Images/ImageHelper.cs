using SixLabors.ImageSharp.Processing;

namespace Snappy.Shared.Images;

public static class ImageSizes
{
    public const string Square = "square";
    public const string Thumb = "thumb";
    public const string Square400 = "square400";
    public const string Small = "small";
    public const string Medium = "medium";
    public const string Large = "large";
}
public static class ImageHelper
{
    private static readonly List<ImageSize> _sizes = new List<ImageSize>
    {
        new ImageSize(ImageSizes.Thumb, 100, 100, ResizeMode.Crop),
        new ImageSize(ImageSizes.Square, 200, 200, ResizeMode.Crop),
        new ImageSize(ImageSizes.Square400, 400, 400, ResizeMode.Crop),
        new ImageSize(ImageSizes.Small, 400, 266),
        new ImageSize(ImageSizes.Medium, 800, 531),
        new ImageSize(ImageSizes.Large, 1600, 1063),
    };

    public static IEnumerable<ImageSize> GetImageSizes() => _sizes;

    public static string GetResizedFileName(string originalName, string size = null)
    {
        var filename = Path.GetFileName(originalName);
        var newFileKeyPrefix = originalName.Replace(filename, "");

        if (!string.IsNullOrEmpty(size))
        {
            var extension = Path.GetExtension(filename);
            filename = filename.Replace(extension, "");
            filename = $"{filename}-{size}{extension}";
        }

        return $"{newFileKeyPrefix}{filename}";
    }
}
