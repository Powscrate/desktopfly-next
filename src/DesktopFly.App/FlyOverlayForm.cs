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
    private readonly FlySpriteImage? _flyImage;
    private readonly FlySpriteOverlayForm? _flySpriteWindow;
    private FlySimulation _simulation;
    private Strings _text;
    private Point _lastCursor;
    private double _lastSampleTime;
    private Vector2 _cursorVelocity;
    private Rectangle _virtualBounds;
    private double _simulationAccumulator;
    private bool _paused, _interactive, _showDiagnostics, _dragging, _spriteRenderingFailed;
    private Point _dragOffset;
    private ToolStripMenuItem _overlayItem = null!;
    private ToolStripMenuItem _settingsItem = null!;
    private ToolStripMenuItem _pauseItem = null!;
    private ToolStripMenuItem _resetItem = null!;
    private ToolStripMenuItem _interactionItem = null!;
    private ToolStripMenuItem _diagnosticsItem = null!;
    private ToolStripMenuItem _languageItem = null!;
    private ToolStripMenuItem _englishItem = null!;
    private ToolStripMenuItem _frenchItem = null!;
    private SettingsForm? _settingsForm;

    public FlyOverlayForm(AppConfiguration config, string configPath, DiagnosticLog log)
    {
        _config = config; _configPath = configPath; _log = log; _text = new Strings(config.Language);
        FlySpriteImage? loadedImage = null;
        try
        {
            loadedImage = FlySpriteImage.Load();
            _flySpriteWindow = new FlySpriteOverlayForm(loadedImage);
            _flyImage = loadedImage;
            _log.Write("INFO", "Loaded the user-provided fly graphic as a transparent desktop sprite.");
        }
        catch (Exception ex)
        {
            loadedImage?.Dispose();
            _flySpriteWindow = null;
            _flyImage = null;
            _log.Write("WARN", "Could not load the supplied fly graphic; no substitute fly will be drawn.", ex);
        }
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
        _tray.DoubleClick += (_, _) => { if (!Visible) SetOverlayVisible(true); else SetPaused(!_paused); };
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
        _pauseItem = new ToolStripMenuItem(); _pauseItem.Click += (_, _) => SetPaused(!_paused);
        _resetItem = new ToolStripMenuItem(); _resetItem.Click += (_, _) => { _simulation.Reset(CurrentCenter()); Invalidate(); };
        _settingsItem = new ToolStripMenuItem(); _settingsItem.Click += (_, _) => OpenSettings();
        _interactionItem = new ToolStripMenuItem { CheckOnClick = true }; _interactionItem.CheckedChanged += (_, _) => SetInteraction(_interactionItem.Checked);
        _diagnosticsItem = new ToolStripMenuItem { CheckOnClick = true }; _diagnosticsItem.CheckedChanged += (_, _) => { _showDiagnostics = _diagnosticsItem.Checked; SaveConfig(); UpdateMenu(); Invalidate(); };
        _languageItem = new ToolStripMenuItem();
        _englishItem = new ToolStripMenuItem("English") { Checked = _config.Language == "en" };
        _frenchItem = new ToolStripMenuItem("Français") { Checked = _config.Language == "fr" };
        _englishItem.Click += (_, _) => SetLanguage("en"); _frenchItem.Click += (_, _) => SetLanguage("fr");
        _languageItem.DropDownItems.Add(_englishItem); _languageItem.DropDownItems.Add(_frenchItem);
        var exit = new ToolStripMenuItem(); exit.Click += (_, _) => { Close(); Application.ExitThread(); };
        menu.Items.AddRange([_overlayItem, _pauseItem, _resetItem, _settingsItem, new ToolStripSeparator(), _interactionItem, _diagnosticsItem, _languageItem, new ToolStripSeparator(), exit]);
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
            if (!_paused)
            {
                _simulationAccumulator = Math.Min(_simulationAccumulator + dt * _config.SimulationSpeedPercent / 100.0, 0.2);
                var stimulus = new StimulusSnapshot(new Vector2(cursor.X, cursor.Y), _cursorVelocity,
                    new Vector2(_virtualBounds.Left, _virtualBounds.Top), new Vector2(_virtualBounds.Right, _virtualBounds.Bottom),
                    CursorThreatEnabled: _config.CursorThreatEnabled);
                const double fixedStep = 1.0 / 60.0;
                while (_simulationAccumulator >= fixedStep)
                {
                    _simulation.Step(fixedStep, stimulus);
                    _simulationAccumulator -= fixedStep;
                }
            }
            else _simulationAccumulator = 0;
            RenderFlySprite();
            Invalidate();
        }
        catch (Exception ex) { _log.Write("ERROR", "Overlay update failed", ex); }
    }

    internal void ShowOverlay() { if (!IsDisposed) Show(); }

    private void SetOverlayVisible(bool visible)
    {
        if (visible)
        {
            Show();
            RenderFlySprite();
        }
        else
        {
            _flySpriteWindow?.Hide();
            Hide();
        }
        if (_overlayItem is not null) _overlayItem.Text = visible ? _text.HideOverlay : _text.ShowOverlay;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        RenderFlySprite();
    }

    private Vector2 CurrentCenter() => new(_virtualBounds.Left + _virtualBounds.Width / 2.0, _virtualBounds.Top + _virtualBounds.Height / 2.0);

    private void SetPaused(bool paused)
    {
        _paused = paused; _config.StartPaused = paused; _simulationAccumulator = 0;
        SaveConfig(); UpdateMenu(); Invalidate();
    }

    private void OpenSettings()
    {
        if (_settingsForm is null || _settingsForm.IsDisposed)
        {
            _settingsForm = new SettingsForm(_config, _text,
                setPaused: SetPaused,
                setSpeed: value => { _config.SimulationSpeedPercent = value; SaveConfig(); },
                setFlySize: value => { _config.FlySize = value; SaveConfig(); Invalidate(); },
                setCursorThreat: value => { _config.CursorThreatEnabled = value; SaveConfig(); },
                setShowDiagnostics: value => { _showDiagnostics = value; SaveConfig(); UpdateMenu(); Invalidate(); },
                resetSimulation: () => { _simulation.Reset(CurrentCenter()); Invalidate(); },
                resetNeural: () => { _simulation.ResetNeural(); Invalidate(); },
                snapshot: () => new SettingsSnapshot(_simulation.State, _simulation.ThreatLevel,
                    _simulation.Position, _simulation.SensorySpikeCount, _simulation.MotorSpikeCount));
            _settingsForm.FormClosed += (_, _) => _settingsForm = null;
        }
        _settingsForm.Show(); _settingsForm.BringToFront(); _settingsForm.Activate();
    }

    private void SetInteraction(bool enabled)
    {
        _interactive = enabled;
        if (IsHandleCreated)
        {
            var style = GetWindowLong(Handle, -20);
            style = enabled ? style & ~WsExTransparent : style | WsExTransparent;
            SetWindowLong(Handle, -20, style);
            SetWindowPos(Handle, new IntPtr(-1), 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0010 | 0x0020);
            _flySpriteWindow?.KeepAboveOverlay();
        }
        _interactionItem.Checked = enabled; Invalidate();
    }

    private void SetLanguage(string language)
    {
        _config.Language = language; _text = new Strings(language); Text = _text.AppName; SaveConfig(); UpdateMenu();
        _resetItem.Text = _text.Reset; _settingsItem.Text = _text.Settings; _interactionItem.Text = _text.Interaction;
        _diagnosticsItem.Text = _text.Diagnostics; _languageItem.Text = _text.Language;
        _settingsForm?.ApplyLanguage(_text);
        _englishItem.Checked = language == "en"; _frenchItem.Checked = language == "fr";
        _overlayItem.Text = Visible ? _text.HideOverlay : _text.ShowOverlay;
        Invalidate();
    }

    private void SaveConfig()
    {
        try { _config.ShowDiagnostics = _showDiagnostics; _config.StartPaused = _paused; _config.Save(_configPath); }
        catch (Exception ex) { _log.Write("WARN", "Could not save settings", ex); }
    }

    private void UpdateMenu()
    {
        if (_pauseItem is null) return;
        _overlayItem.Text = Visible ? _text.HideOverlay : _text.ShowOverlay;
        _pauseItem.Text = _paused ? _text.Resume : _text.Pause;
        _resetItem.Text = _text.Reset; _settingsItem.Text = _text.Settings;
        _interactionItem.Text = _text.Interaction; _interactionItem.Checked = _interactive;
        _diagnosticsItem.Text = _text.Diagnostics; _diagnosticsItem.Checked = _showDiagnostics;
        _settingsForm?.SyncOverlayState(_paused, _showDiagnostics);
        _languageItem.Text = _text.Language;
        _tray.ContextMenuStrip!.Items.OfType<ToolStripMenuItem>().Last().Text = _text.Exit;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(_transparent); e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        if (_showDiagnostics)
        {
            using var font = new Font("Segoe UI", 9, FontStyle.Regular);
            using var brush = new SolidBrush(Color.FromArgb(220, 235, 240, 248));
            var state = _paused ? _text.Paused : _simulation.State switch { FlyState.Escape => _text.Escape, FlyState.Rest => _text.Rest, _ => _text.Explore };
            e.Graphics.DrawString($"DesktopFly Next · {state} · {_text.Threat} {_simulation.ThreatLevel:0.00} · {_text.NeuralSpikes} {_simulation.MotorSpikeCount}", font, brush, 12, 12);
        }
        base.OnPaint(e);
    }

    private void RenderFlySprite()
    {
        if (!Visible || _flySpriteWindow is null || _spriteRenderingFailed) return;
        try
        {
            _flySpriteWindow.Present(_simulation.Position, _simulation.HeadingRadians,
                _config.FlySize * 0.85f);
        }
        catch (Exception ex)
        {
            _spriteRenderingFailed = true;
            _flySpriteWindow.Hide();
            _log.Write("ERROR", "Per-pixel-alpha rendering of the supplied fly graphic failed; hiding it rather than drawing a substitute.", ex);
            Invalidate();
        }
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
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _timer.Stop(); _settingsForm?.Close(); _tray.Visible = false; _tray.Dispose();
        _flySpriteWindow?.Dispose(); _flyImage?.Dispose(); SaveConfig(); base.OnFormClosing(e);
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)] private static extern int GetWindowLong(IntPtr hWnd, int index);
    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)] private static extern int SetWindowLong(IntPtr hWnd, int index, int value);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);
}
