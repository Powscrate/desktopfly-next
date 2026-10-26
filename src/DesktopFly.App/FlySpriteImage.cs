using System.Reflection;

namespace DesktopFly.App;

/// <summary>Loads the user-provided fly artwork as a transparent desktop sprite.</summary>
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
        return new FlySpriteImage(new Bitmap(source));
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
