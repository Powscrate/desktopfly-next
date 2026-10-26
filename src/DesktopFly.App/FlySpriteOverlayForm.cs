using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using DesktopFly.Simulation.Behavior;
using DesktopFly.Simulation.Environment;

namespace DesktopFly.App;

/// <summary>
/// A compact per-pixel-alpha layered window for the fly sprite. This avoids color-key
/// matting around the PNG's antialiased and translucent pixels on the main overlay.
/// </summary>
internal sealed class FlySpriteOverlayForm : Form
{
    private const int WsExTransparent = 0x00000020;
    private const int WsExToolWindow = 0x00000080;
    private const int WsExNoActivate = 0x08000000;
    private const int WsExLayered = 0x00080000;
    private const int UlwAlpha = 0x00000002;
    private const byte AcSrcOver = 0;
    private const byte AcSrcAlpha = 1;
    private const uint DibRgbColors = 0;
    private const uint BiRgb = 0;
    private const int CanvasSize = 288;
    private const int CanvasBytes = CanvasSize * CanvasSize * 4;

    private readonly FlySpriteImage _sprite;
    private readonly Bitmap _canvas = new(CanvasSize, CanvasSize, PixelFormat.Format32bppPArgb);
    private readonly byte[] _pixels = new byte[CanvasBytes];
    private IntPtr _memoryDc;
    private IntPtr _dib;
    private IntPtr _dibBits;
    private IntPtr _previousBitmap;

    public FlySpriteOverlayForm(FlySpriteImage atlas)
    {
        _sprite = atlas;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;
        ClientSize = new Size(CanvasSize, CanvasSize);
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            var parameters = base.CreateParams;
            parameters.ExStyle |= WsExLayered | WsExTransparent | WsExToolWindow | WsExNoActivate;
            return parameters;
        }
    }

    public void Present(Vector2 center, double headingRadians, float scale,
        double stridePhase, bool walking, double wingPhase, bool flying)
    {
        if (IsDisposed) return;
        if (!IsHandleCreated) _ = Handle;
        EnsureDib();
        if (!Visible) Show();

        using (var graphics = Graphics.FromImage(_canvas))
        {
            graphics.Clear(Color.Transparent);
            graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TranslateTransform(CanvasSize / 2f, CanvasSize / 2f);
            // The supplied sprite's head points down (+Y); align it with the
            // screen-space walking heading instead of letting it slide sideways.
            graphics.RotateTransform((float)(headingRadians * 180.0 / Math.PI - 90));
            graphics.ScaleTransform(scale * (flying ? 1.07f : 1f), scale * (flying ? 1.07f : 1f));
            DrawWings(graphics, wingPhase, flying);
            _sprite.Draw(graphics, new RectangleF(-42, -34, 84, 68));
            DrawLegs(graphics, stridePhase, walking);
        }

        var data = _canvas.LockBits(new Rectangle(0, 0, CanvasSize, CanvasSize),
            ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
        try
        {
            var rowBytes = CanvasSize * 4;
            if (Math.Abs(data.Stride) != rowBytes)
                throw new InvalidDataException($"Unexpected sprite bitmap stride {data.Stride}.");
            for (var y = 0; y < CanvasSize; y++)
                Marshal.Copy(IntPtr.Add(data.Scan0, y * data.Stride), _pixels, y * rowBytes, rowBytes);
        }
        finally
        {
            _canvas.UnlockBits(data);
        }
        Marshal.Copy(_pixels, 0, _dibBits, CanvasBytes);

        var screenPosition = new NativePoint(
            (int)Math.Round(center.X - CanvasSize / 2.0),
            (int)Math.Round(center.Y - CanvasSize / 2.0));
        var windowSize = new NativeSize(CanvasSize, CanvasSize);
        var sourcePosition = new NativePoint(0, 0);
        var blend = new BlendFunction
        {
            BlendOp = AcSrcOver,
            BlendFlags = 0,
            SourceConstantAlpha = 255,
            AlphaFormat = AcSrcAlpha
        };
        if (!UpdateLayeredWindow(Handle, IntPtr.Zero, ref screenPosition, ref windowSize,
                _memoryDc, ref sourcePosition, 0, ref blend, UlwAlpha))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not update the fly's per-pixel-alpha window.");
        }
    }

    public void KeepAboveOverlay()
    {
        if (!IsHandleCreated || !Visible) return;
        SetWindowPos(Handle, new IntPtr(-1), 0, 0, 0, 0,
            0x0001 | 0x0002 | 0x0010 | 0x0200); // no move/size/activate/owner-z-order change
    }

    private static void DrawWings(Graphics graphics, double phase, bool flying)
    {
        var sweep = flying ? Math.Sin(phase) : 0;
        var lift = flying ? Math.Cos(phase) : 0;
        using var fill = new SolidBrush(Color.FromArgb(flying ? 105 : 70, 205, 216, 212));
        using var outline = new Pen(Color.FromArgb(flying ? 175 : 120, 89, 101, 96), 0.9f);
        foreach (var side in new[] { -1f, 1f })
        {
            var root = new PointF(side * 2.2f, -3.4f);
            var shoulder = new PointF(side * (6.5f + 2.1f * (float)sweep), -8.0f - 1.0f * (float)lift);
            var tip = new PointF(side * (12.0f + 3.8f * (float)sweep), -17.0f + 2.3f * (float)lift);
            var returnPoint = new PointF(side * (6.0f + 2.6f * (float)sweep), -6.0f);
            using var wing = new GraphicsPath();
            wing.StartFigure();
            wing.AddBezier(root, new PointF(side * 4.0f, -7.0f), shoulder, tip);
            wing.AddBezier(tip, new PointF(side * 9.0f, -11.0f), returnPoint, root);
            wing.CloseFigure();
            graphics.FillPath(fill, wing);
            graphics.DrawPath(outline, wing);
        }
    }

    private static void DrawLegs(Graphics graphics, double phase, bool walking)
    {
        using var pen = new Pen(Color.FromArgb(220, 67, 45, 37), 1.45f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        foreach (var side in new[] { -1f, 1f })
        for (var pair = 0; pair < 3; pair++)
        {
            var localPhase = phase + (pair == 1 ? Math.PI : 0) + (side > 0 ? Math.PI : 0);
            var stride = walking ? (float)Math.Cos(localPhase) * 3.2f : 0;
            var lift = walking ? Math.Max(0, (float)Math.Sin(localPhase)) * 1.3f : 0;
            var baseY = (pair - 1) * 2.7f;
            var root = new PointF(side * 2.8f, baseY);
            var coxa = new PointF(side * (6.0f + lift), baseY + stride * 0.25f);
            var knee = new PointF(side * (10.2f + lift), baseY + stride * 0.7f - lift);
            var foot = new PointF(side * (15.5f + lift), baseY + stride);
            graphics.DrawLine(pen, root, coxa);
            graphics.DrawLine(pen, coxa, knee);
            graphics.DrawLine(pen, knee, foot);
        }
    }

    private void EnsureDib()
    {
        if (_dib != IntPtr.Zero) return;
        var info = new BitmapInfo
        {
            Header = new BitmapInfoHeader
            {
                Size = (uint)Marshal.SizeOf<BitmapInfoHeader>(),
                Width = CanvasSize,
                Height = -CanvasSize, // top-down 32-bit BGRA pixels
                Planes = 1,
                BitCount = 32,
                Compression = BiRgb,
                SizeImage = CanvasBytes
            }
        };
        _memoryDc = CreateCompatibleDC(IntPtr.Zero);
        if (_memoryDc == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not create the fly sprite device context.");
        _dib = CreateDIBSection(_memoryDc, ref info, DibRgbColors, out _dibBits, IntPtr.Zero, 0);
        if (_dib == IntPtr.Zero || _dibBits == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not create the fly sprite alpha surface.");
        _previousBitmap = SelectObject(_memoryDc, _dib);
        if (_previousBitmap == IntPtr.Zero || _previousBitmap == new IntPtr(-1))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not select the fly sprite alpha surface.");
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // The alpha DIB is presented by UpdateLayeredWindow, not WinForms background painting.
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        // UpdateLayeredWindow supplies the complete premultiplied-alpha surface.
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _canvas.Dispose();
        if (_memoryDc != IntPtr.Zero)
        {
            if (_previousBitmap != IntPtr.Zero && _previousBitmap != new IntPtr(-1))
                SelectObject(_memoryDc, _previousBitmap);
            if (_dib != IntPtr.Zero) DeleteObject(_dib);
            DeleteDC(_memoryDc);
            _memoryDc = IntPtr.Zero;
            _dib = IntPtr.Zero;
            _dibBits = IntPtr.Zero;
        }
        base.Dispose(disposing);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint { public int X, Y; public NativePoint(int x, int y) { X = x; Y = y; } }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeSize { public int Width, Height; public NativeSize(int width, int height) { Width = width; Height = height; } }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct BlendFunction { public byte BlendOp, BlendFlags, SourceConstantAlpha, AlphaFormat; }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfoHeader
    {
        public uint Size;
        public int Width, Height;
        public ushort Planes, BitCount;
        public uint Compression, SizeImage;
        public int XPelsPerMeter, YPelsPerMeter;
        public uint ColorsUsed, ColorsImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfo { public BitmapInfoHeader Header; }

    [DllImport("gdi32.dll", SetLastError = true)] private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern IntPtr CreateDIBSection(IntPtr hdc, ref BitmapInfo info, uint usage, out IntPtr bits, IntPtr section, uint offset);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern bool DeleteObject(IntPtr hgdiobj);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern bool DeleteDC(IntPtr hdc);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref NativePoint destination, ref NativeSize size, IntPtr hdcSrc, ref NativePoint source, uint colorKey, ref BlendFunction blend, int flags);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool SetWindowPos(IntPtr hwnd, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);
}
