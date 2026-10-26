using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using DesktopFly.Simulation.Behavior;
using DesktopFly.Simulation.Environment;
using DesktopFly.Simulation.Configuration;
using DesktopFly.Simulation.Diagnostics;

namespace DesktopFly.App;

internal sealed class FlyOverlayForm : Form
{
    private const int WsExTransparent = 0x00000020, WsExToolWindow = 0x00000080, WsExNoActivate = 0x08000000;
    private readonly Color _transparent = Color.Magenta;
    private readonly AppConfiguration _config;
    private readonly string _configPath;
    private readonly DiagnosticLog _log;
    private readonly NotifyIcon _tray;
    private readonly System.Windows.Forms.Timer _timer;
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private FlySimulation _simulation;
    private Strings _text;
    private Point _lastCursor;
    private double _lastSampleTime;
    private Vector2 _cursorVelocity;
    private Rectangle _virtualBounds;
    private double _animationTime;
    private double _simulationAccumulator;
    private bool _paused, _interactive, _showDiagnostics, _dragging;
    private Point _dragOffset;
    private ToolStripMenuItem _overlayItem = null!;
    private ToolStripMenuItem _pauseItem = null!;
    private ToolStripMenuItem _resetItem = null!;
    private ToolStripMenuItem _interactionItem = null!;
    private ToolStripMenuItem _diagnosticsItem = null!;
    private ToolStripMenuItem _languageItem = null!;
    private ToolStripMenuItem _englishItem = null!;
    private ToolStripMenuItem _frenchItem = null!;

    public FlyOverlayForm(AppConfiguration config, string configPath, DiagnosticLog log)
    {
        _config = config; _configPath = configPath; _log = log; _text = new Strings(config.Language);
        _paused = config.StartPaused; _showDiagnostics = config.ShowDiagnostics;
        _virtualBounds = SystemInformation.VirtualScreen;
        var start = new Vector2(_virtualBounds.Left + _virtualBounds.Width * 0.48, _virtualBounds.Top + _virtualBounds.Height * 0.54);
        _simulation = new FlySimulation(start, config.RandomSeed);
        _lastCursor = Cursor.Position; _lastSampleTime = _clock.Elapsed.TotalSeconds;
        FormBorderStyle = FormBorderStyle.None; ShowInTaskbar = false; TopMost = true; StartPosition = FormStartPosition.Manual;
        Bounds = _virtualBounds; BackColor = _transparent; TransparencyKey = _transparent;
        DoubleBuffered = true; SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Text = _text.AppName;
        _timer = new System.Windows.Forms.Timer { Interval = 16 };
        _timer.Tick += OnTick;
        _tray = new NotifyIcon { Icon = SystemIcons.Application, Text = "DesktopFly Next", Visible = true, ContextMenuStrip = BuildMenu() };
        _tray.DoubleClick += (_, _) => { if (!Visible) SetOverlayVisible(true); else _paused = !_paused; UpdateMenu(); };
        _timer.Start();
        UpdateMenu();
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= WsExToolWindow | WsExNoActivate;
            if (!_interactive) cp.ExStyle |= WsExTransparent;
            return cp;
        }
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        _overlayItem = new ToolStripMenuItem(); _overlayItem.Click += (_, _) => SetOverlayVisible(!Visible);
        _pauseItem = new ToolStripMenuItem(); _pauseItem.Click += (_, _) => { _paused = !_paused; UpdateMenu(); Invalidate(); };
        _resetItem = new ToolStripMenuItem(); _resetItem.Click += (_, _) => { _simulation.Reset(CurrentCenter()); Invalidate(); };
        _interactionItem = new ToolStripMenuItem { CheckOnClick = true }; _interactionItem.CheckedChanged += (_, _) => SetInteraction(_interactionItem.Checked);
        _diagnosticsItem = new ToolStripMenuItem { CheckOnClick = true }; _diagnosticsItem.CheckedChanged += (_, _) => { _showDiagnostics = _diagnosticsItem.Checked; SaveConfig(); Invalidate(); };
        _languageItem = new ToolStripMenuItem();
        _englishItem = new ToolStripMenuItem("English") { Checked = _config.Language == "en" };
        _frenchItem = new ToolStripMenuItem("Français") { Checked = _config.Language == "fr" };
        _englishItem.Click += (_, _) => SetLanguage("en"); _frenchItem.Click += (_, _) => SetLanguage("fr");
        _languageItem.DropDownItems.Add(_englishItem); _languageItem.DropDownItems.Add(_frenchItem);
        var exit = new ToolStripMenuItem(); exit.Click += (_, _) => { Close(); Application.ExitThread(); };
        menu.Items.AddRange([_overlayItem, _pauseItem, _resetItem, new ToolStripSeparator(), _interactionItem, _diagnosticsItem, _languageItem, new ToolStripSeparator(), exit]);
        menu.Opening += (_, _) => UpdateMenu();
        return menu;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        try
        {
            var bounds = SystemInformation.VirtualScreen;
            if (bounds != _virtualBounds)
            {
                _virtualBounds = bounds; Bounds = bounds;
                _simulation.Reset(CurrentCenter()); _log.Write("INFO", $"Virtual desktop changed: {bounds}");
            }
            var now = _clock.Elapsed.TotalSeconds;
            var dt = Math.Clamp(now - _lastSampleTime, 0.001, 0.1);
            var cursor = Cursor.Position;
            _cursorVelocity = new Vector2((cursor.X - _lastCursor.X) / dt, (cursor.Y - _lastCursor.Y) / dt);
            _lastCursor = cursor; _lastSampleTime = now;
            _animationTime += dt;
            if (!_paused)
            {
                _simulationAccumulator = Math.Min(_simulationAccumulator + dt, 0.2);
                var stimulus = new StimulusSnapshot(new Vector2(cursor.X, cursor.Y), _cursorVelocity,
                    new Vector2(_virtualBounds.Left, _virtualBounds.Top), new Vector2(_virtualBounds.Right, _virtualBounds.Bottom));
                const double fixedStep = 1.0 / 60.0;
                while (_simulationAccumulator >= fixedStep)
                {
                    _simulation.Step(fixedStep, stimulus);
                    _simulationAccumulator -= fixedStep;
                }
            }
            else _simulationAccumulator = 0;
            Invalidate();
        }
        catch (Exception ex) { _log.Write("ERROR", "Overlay update failed", ex); }
    }

    internal void ShowOverlay() { if (!IsDisposed) Show(); }

    private void SetOverlayVisible(bool visible)
    {
        if (visible) Show(); else Hide();
        if (_overlayItem is not null) _overlayItem.Text = visible ? _text.HideOverlay : _text.ShowOverlay;
    }

    private Vector2 CurrentCenter() => new(_virtualBounds.Left + _virtualBounds.Width / 2.0, _virtualBounds.Top + _virtualBounds.Height / 2.0);

    private void SetInteraction(bool enabled)
    {
        _interactive = enabled;
        if (IsHandleCreated)
        {
            var style = GetWindowLong(Handle, -20);
            style = enabled ? style & ~WsExTransparent : style | WsExTransparent;
            SetWindowLong(Handle, -20, style);
            SetWindowPos(Handle, new IntPtr(-1), 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0010 | 0x0020);
        }
        _interactionItem.Checked = enabled; Invalidate();
    }

    private void SetLanguage(string language)
    {
        _config.Language = language; _text = new Strings(language); Text = _text.AppName; SaveConfig(); UpdateMenu();
        _resetItem.Text = _text.Reset; _interactionItem.Text = _text.Interaction;
        _diagnosticsItem.Text = _text.Diagnostics; _languageItem.Text = _text.Language;
        _englishItem.Checked = language == "en"; _frenchItem.Checked = language == "fr";
        _overlayItem.Text = Visible ? _text.HideOverlay : _text.ShowOverlay;
        Invalidate();
    }

    private void SaveConfig()
    {
        try { _config.ShowDiagnostics = _showDiagnostics; _config.Save(_configPath); }
        catch (Exception ex) { _log.Write("WARN", "Could not save settings", ex); }
    }

    private void UpdateMenu()
    {
        if (_pauseItem is null) return;
        _overlayItem.Text = Visible ? _text.HideOverlay : _text.ShowOverlay;
        _pauseItem.Text = _paused ? _text.Resume : _text.Pause;
        _resetItem.Text = _text.Reset;
        _interactionItem.Text = _text.Interaction; _interactionItem.Checked = _interactive;
        _diagnosticsItem.Text = _text.Diagnostics; _diagnosticsItem.Checked = _showDiagnostics;
        _languageItem.Text = _text.Language;
        _tray.ContextMenuStrip!.Items.OfType<ToolStripMenuItem>().Last().Text = _text.Exit;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(_transparent); e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var x = (float)(_simulation.Position.X - _virtualBounds.Left); var y = (float)(_simulation.Position.Y - _virtualBounds.Top);
        var scale = _config.FlySize * 0.85f;
        e.Graphics.TranslateTransform(x, y); e.Graphics.RotateTransform((float)(_simulation.HeadingRadians * 180 / Math.PI)); e.Graphics.ScaleTransform(scale, scale);
        DrawFly(e.Graphics);
        e.Graphics.ResetTransform();
        if (_showDiagnostics)
        {
            using var font = new Font("Segoe UI", 9, FontStyle.Regular);
            using var brush = new SolidBrush(Color.FromArgb(220, 235, 240, 248));
            var state = _paused ? _text.Paused : _simulation.State switch { FlyState.Escape => _text.Escape, FlyState.Rest => _text.Rest, _ => _text.Explore };
            e.Graphics.DrawString($"DesktopFly Next · {state} · {_text.Threat} {_simulation.ThreatLevel:0.00} · {_text.NeuralSpikes} {_simulation.MotorSpikeCount}", font, brush, 12, 12);
        }
        base.OnPaint(e);
    }

    private void DrawFly(Graphics g)
    {
        var phase = _animationTime * (2 + Math.Min(_simulation.Speed / 30, 2));
        var gait = Math.Sin(phase) * 3.8f;
        using var leg = new Pen(Color.FromArgb(255, 54, 44, 39), 1.5f);
        using var wing = new SolidBrush(Color.FromArgb(110, 190, 218, 220));
        using var wingLine = new Pen(Color.FromArgb(150, 112, 151, 158), 0.65f);
        using var thorax = new LinearGradientBrush(new RectangleF(-9, -7, 20, 14), Color.FromArgb(190, 132, 73), Color.FromArgb(80, 43, 34), 35);
        using var abdomen = new LinearGradientBrush(new RectangleF(-31, -6, 25, 12), Color.FromArgb(223, 158, 66), Color.FromArgb(86, 48, 34), 90);
        using var dark = new SolidBrush(Color.FromArgb(33, 28, 29));
        using var eye = new SolidBrush(Color.FromArgb(139, 24, 37));
        // Two translucent, veined wings, driven by flight/escape arousal.
        var flutter = _simulation.State == FlyState.Escape ? Math.Sin(_animationTime * 48) * 3 : Math.Sin(_animationTime * 7) * 1.5;
        var wingPath = new GraphicsPath(); wingPath.AddBezier(-1, -4, -7, -25 - (float)flutter, 13, -28 + (float)flutter, 18, -8);
        wingPath.AddBezier(18, -8, 14, -3, 5, -2, -1, -4);
        g.FillPath(wing, wingPath); g.DrawBezier(wingLine, -1, -4, -7, -25 - (float)flutter, 13, -28 + (float)flutter, 18, -8);
        g.DrawLine(wingLine, 1, -5, 9, -22); g.DrawLine(wingLine, 4, -5, 15, -13);
        wingPath.Dispose();
        var lowerWing = new GraphicsPath(); lowerWing.AddBezier(-1, 4, -7, 25 + (float)flutter, 13, 28 - (float)flutter, 18, 8); lowerWing.AddBezier(18, 8, 14, 3, 5, 2, -1, 4);
        g.FillPath(wing, lowerWing); g.DrawBezier(wingLine, -1, 4, -7, 25 + (float)flutter, 13, 28 - (float)flutter, 18, 8); g.DrawLine(wingLine, 2, 5, 11, 22); lowerWing.Dispose();
        // Six articulated legs. Alternating tripod gait is a visual approximation.
        for (var i = 0; i < 3; i++)
        {
            var ax = -4 + i * 6;
            var offset = (float)Math.Sin(phase + i * Math.PI) * gait;
            g.DrawLine(leg, ax, -4, ax + (i - 1) * 7, -12 - offset); g.DrawLine(leg, ax + (i - 1) * 7, -12 - offset, ax + (i - 1) * 10 + 4, -13 - offset);
            g.DrawLine(leg, ax, 4, ax + (i - 1) * 7, 12 + offset); g.DrawLine(leg, ax + (i - 1) * 7, 12 + offset, ax + (i - 1) * 10 + 4, 13 + offset);
        }
        g.FillEllipse(thorax, -10, -7, 19, 14);
        g.FillEllipse(abdomen, -31, -5.5f, 24, 11);
        using (var stripe = new Pen(Color.FromArgb(100, 48, 35, 29), 1)) { for (var sx = -27; sx < -8; sx += 6) g.DrawLine(stripe, sx, -4, sx, 4); }
        g.FillEllipse(dark, 5, -7, 12, 14); g.FillEllipse(eye, 10, -8, 11, 8); g.FillEllipse(eye, 10, 0, 11, 8);
        using var antenna = new Pen(Color.FromArgb(70, 49, 43), 1f);
        g.DrawBezier(antenna, 16, -3, 19, -10, 25, -12, 27, -9); g.DrawBezier(antenna, 16, 3, 19, 10, 25, 12, 27, 9);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (_interactive && e.Button == MouseButtons.Left)
        {
            _dragging = true; _dragOffset = new Point(e.X - (int)(_simulation.Position.X - _virtualBounds.Left), e.Y - (int)(_simulation.Position.Y - _virtualBounds.Top)); Capture = true;
        }
        base.OnMouseDown(e);
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_dragging) _simulation.Reset(new Vector2(e.X - _dragOffset.X + _virtualBounds.Left, e.Y - _dragOffset.Y + _virtualBounds.Top));
        base.OnMouseMove(e);
    }
    protected override void OnMouseUp(MouseEventArgs e) { _dragging = false; Capture = false; base.OnMouseUp(e); }
    protected override void OnFormClosing(FormClosingEventArgs e) { _timer.Stop(); _tray.Visible = false; _tray.Dispose(); SaveConfig(); base.OnFormClosing(e); }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)] private static extern int GetWindowLong(IntPtr hWnd, int index);
    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)] private static extern int SetWindowLong(IntPtr hWnd, int index, int value);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);
}
