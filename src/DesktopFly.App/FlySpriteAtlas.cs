using System.Reflection;

namespace DesktopFly.App;

/// <summary>
/// Small software-rendered atlas baked from the bundled, assembled NeuroMechFly mesh.
/// Keeping rasterization offline preserves a lightweight, GPU-independent desktop overlay.
/// </summary>
internal sealed class FlySpriteAtlas : IDisposable
{
    public const int FrameWidth = 320;
    public const int FrameHeight = 256;
    private const string ResourceName = "DesktopFly.App.drosophila-atlas.png";
    private readonly Bitmap _bitmap;

    private FlySpriteAtlas(Bitmap bitmap) => _bitmap = bitmap;

    public static FlySpriteAtlas Load()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
            ?? throw new FileNotFoundException($"Embedded fly sprite atlas '{ResourceName}' was not found.");
        using var source = Image.FromStream(stream);
        if (source.Width != FrameWidth || source.Height != FrameHeight)
            throw new InvalidDataException($"Unexpected fly sprite image size {source.Width}x{source.Height}.");
        return new FlySpriteAtlas(new Bitmap(source));
    }

    public void Draw(Graphics graphics, RectangleF destination)
    {
        var source = new Rectangle(0, 0, FrameWidth, FrameHeight);
        graphics.DrawImage(_bitmap, destination, source, GraphicsUnit.Pixel);
    }

    public void Dispose() => _bitmap.Dispose();
}
