using System.Reflection;

namespace DesktopFly.App;

/// <summary>Loads the user-provided overhead fly art and separates pale wing pixels for motion.</summary>
internal sealed class FlySpriteImage : IDisposable
{
    private const string ResourceName = "DesktopFly.App.desktopfly.png";
    private readonly Bitmap _bitmap;

    private FlySpriteImage(Bitmap bitmap) => _bitmap = bitmap;

    public static FlySpriteImage Load()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
            ?? throw new FileNotFoundException($"Embedded fly artwork '{ResourceName}' was not found.");
        using var source = Image.FromStream(stream);
        if (source.Width < 1 || source.Height < 1)
            throw new InvalidDataException($"Unexpected fly sprite image size {source.Width}x{source.Height}.");
        var body = new Bitmap(source);
        // The supplied top-down cutout has light gray wings in this small region.
        // Retain its body, eyes and antialiasing while moving those pixels into
        // separately redrawn hinged shapes; no alternate view is used as a frame.
        for (var y = 10; y < Math.Min(25, body.Height); y++)
        for (var x = 14; x < Math.Min(42, body.Width); x++)
        {
            var pixel = body.GetPixel(x, y);
            var luminance = (pixel.R + pixel.G + pixel.B) / 3;
            var chroma = Math.Max(pixel.R, Math.Max(pixel.G, pixel.B)) - Math.Min(pixel.R, Math.Min(pixel.G, pixel.B));
            if (pixel.A > 0 && luminance > 150 && chroma < 55)
                body.SetPixel(x, y, Color.FromArgb(0, pixel.R, pixel.G, pixel.B));
        }
        return new FlySpriteImage(body);
    }

    public void Draw(Graphics graphics, RectangleF destination)
    {
        var scale = Math.Min(destination.Width / _bitmap.Width, destination.Height / _bitmap.Height);
        var width = _bitmap.Width * scale;
        var height = _bitmap.Height * scale;
        var fitted = new RectangleF(destination.X + (destination.Width - width) / 2f,
            destination.Y + (destination.Height - height) / 2f, width, height);
        graphics.DrawImage(_bitmap, fitted, new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), GraphicsUnit.Pixel);
    }

    public void Dispose() => _bitmap.Dispose();
}
