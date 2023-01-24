using SixLabors.ImageSharp.Processing;

namespace Snappy.Shared.Images;

public class ImageSize
{
    public string Key { get; }
    public int Width { get; }
    public int Height { get; }

    public ResizeMode? ResizeMode { get; }

    public ImageSize(string key, int width, int height, ResizeMode? resizeMode = null)
    {
        Key = key;
        Width = width;
        Height = height;
        ResizeMode = resizeMode;
    }
}
