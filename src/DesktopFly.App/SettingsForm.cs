using DesktopFly.Simulation.Behavior;
using DesktopFly.Simulation.Configuration;
using DesktopFly.Simulation.Environment;

namespace DesktopFly.App;

internal readonly record struct SettingsSnapshot(FlyState State, double Threat, Vector2 Position,
    long SensorySpikes, long MotorSpikes, int NeuronCount, int ConnectionCount);

/// <summary>Lightweight, non-modal controls and live diagnostics; simulation continues while open.</summary>
internal sealed class SettingsForm : Form
{
    private readonly Action<bool> _setPaused;
    private readonly Action<int> _setSpeed;
    private readonly Action<int> _setFlySize;
    private readonly Action<bool> _setCursorThreat;
    private readonly Action<bool> _setShowDiagnostics;
    private readonly Action _resetSimulation;
    private readonly Action _resetNeural;
    private readonly Func<SettingsSnapshot> _snapshot;
    private readonly CheckBox _paused = new();
    private readonly TrackBar _speed = new();
    private readonly Label _speedValue = new();
    private readonly NumericUpDown _flySize = new();
    private readonly CheckBox _cursorThreat = new();
    private readonly CheckBox _showDiagnostics = new();
    private readonly Label _stateValue = new();
    private readonly Label _threatValue = new();
    private readonly Label _positionValue = new();
    private readonly Label _sensoryValue = new();
    private readonly Label _motorValue = new();
    private Strings _strings = null!;
    private readonly GroupBox _simulationGroup = new();
    private readonly GroupBox _behaviorGroup = new();
    private readonly GroupBox _diagnosticsGroup = new();
    private readonly Label _title = new();
    private readonly Label _speedTitle = new();
    private readonly Label _sizeTitle = new();
    private readonly Label _stateTitle = new();
    private readonly Label _threatTitle = new();
    private readonly Label _positionTitle = new();
    private readonly Label _sensoryTitle = new();
    private readonly Label _motorTitle = new();
    private readonly Label _neuronsTitle = new();
    private readonly Label _synapsesTitle = new();
    private readonly Label _neuronsValue = new();
    private readonly Label _synapsesValue = new();
    private readonly Button _resetSimulationButton = new();
    private readonly Button _resetNeuralButton = new();
    private readonly Button _closeButton = new();
    private readonly System.Windows.Forms.Timer _refreshTimer = new() { Interval = 250 };
    private bool _syncing;

    public SettingsForm(AppConfiguration config, Strings strings,
        Action<bool> setPaused, Action<int> setSpeed, Action<int> setFlySize,
        Action<bool> setCursorThreat, Action<bool> setShowDiagnostics,
        Action resetSimulation, Action resetNeural, Func<SettingsSnapshot> snapshot)
    {
        _strings = strings; _setPaused = setPaused; _setSpeed = setSpeed;
        _setFlySize = setFlySize; _setCursorThreat = setCursorThreat;
        _setShowDiagnostics = setShowDiagnostics; _resetSimulation = resetSimulation;
        _resetNeural = resetNeural; _snapshot = snapshot;

        Text = strings.Settings; StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle; MaximizeBox = false; MinimizeBox = true;
        ShowInTaskbar = true; TopMost = true; ClientSize = new Size(520, 590);
        BackColor = Color.FromArgb(29, 32, 40); ForeColor = Color.FromArgb(232, 235, 242);

        _title.AutoSize = true; _title.Font = new Font(Font, FontStyle.Bold); _title.Margin = new Padding(4, 2, 4, 8);
        _paused.AutoSize = true; _paused.Checked = config.StartPaused;
        _speed.Minimum = 25; _speed.Maximum = 300; _speed.TickFrequency = 25;
        _speed.SmallChange = 25; _speed.LargeChange = 50; _speed.Width = 205;
        _speedTitle.AutoSize = false; _speedTitle.Width = 140; _speedTitle.Height = 23;
        _sizeTitle.AutoSize = false; _sizeTitle.Width = 140; _sizeTitle.Height = 23;
        _speed.Value = Math.Clamp(config.SimulationSpeedPercent, _speed.Minimum, _speed.Maximum);
        _speedValue.AutoSize = true; _speedValue.Padding = new Padding(5, 12, 0, 0);
        _flySize.Minimum = 1; _flySize.Maximum = 3; _flySize.Value = Math.Clamp(config.FlySize, 1, 3); _flySize.Width = 72;
        _flySize.BackColor = Color.FromArgb(43, 47, 57); _flySize.ForeColor = ForeColor;
        _speed.BackColor = BackColor; _speed.ForeColor = ForeColor;
        _cursorThreat.AutoSize = true; _cursorThreat.Checked = config.CursorThreatEnabled;
        _showDiagnostics.AutoSize = true; _showDiagnostics.Checked = config.ShowDiagnostics;

        ConfigureGroup(_simulationGroup, 150);
        ConfigureGroup(_behaviorGroup, 104);
        ConfigureGroup(_diagnosticsGroup, 280);
        ConfigureMetric(_stateTitle, _stateValue);
        ConfigureMetric(_threatTitle, _threatValue);
        ConfigureMetric(_positionTitle, _positionValue);
        ConfigureMetric(_sensoryTitle, _sensoryValue);
        ConfigureMetric(_motorTitle, _motorValue);
        ConfigureMetric(_neuronsTitle, _neuronsValue);
        ConfigureMetric(_synapsesTitle, _synapsesValue);

        var content = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, Padding = new Padding(14), BackColor = BackColor
        };
        content.Controls.Add(_title);
        content.Controls.Add(_simulationGroup);
        content.Controls.Add(_behaviorGroup);
        content.Controls.Add(_diagnosticsGroup);

        var buttonRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom, Height = 48, FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(6), BackColor = BackColor
        };
        StyleButton(_closeButton); StyleButton(_resetSimulationButton); StyleButton(_resetNeuralButton);
        buttonRow.Controls.Add(_closeButton); buttonRow.Controls.Add(_resetSimulationButton); buttonRow.Controls.Add(_resetNeuralButton);
        Controls.Add(content); Controls.Add(buttonRow);

        _paused.CheckedChanged += (_, _) => { if (!_syncing) _setPaused(_paused.Checked); };
        _speed.ValueChanged += (_, _) => { _speedValue.Text = $"{_speed.Value}%"; if (!_syncing) _setSpeed(_speed.Value); };
        _flySize.ValueChanged += (_, _) => { if (!_syncing) _setFlySize((int)_flySize.Value); };
        _cursorThreat.CheckedChanged += (_, _) => { if (!_syncing) _setCursorThreat(_cursorThreat.Checked); };
        _showDiagnostics.CheckedChanged += (_, _) => { if (!_syncing) _setShowDiagnostics(_showDiagnostics.Checked); };
        _resetSimulationButton.Click += (_, _) => _resetSimulation();
        _resetNeuralButton.Click += (_, _) => _resetNeural();
        _closeButton.Click += (_, _) => Close();
        _refreshTimer.Tick += (_, _) => RefreshSnapshot();
        FormClosed += (_, _) => { _refreshTimer.Stop(); _refreshTimer.Dispose(); };

        ApplyLanguage(strings);
        _speedValue.Text = $"{_speed.Value}%";
        RefreshSnapshot(); _refreshTimer.Start();
    }

    public void ApplyLanguage(Strings strings)
    {
        _strings = strings;
        Text = strings.Settings; _title.Text = strings.Settings;
        _simulationGroup.Text = strings.SimulationGroup;
        _behaviorGroup.Text = strings.BehaviorGroup;
        _diagnosticsGroup.Text = strings.DiagnosticsGroup;
        _paused.Text = strings.PauseSimulation;
        _speedTitle.Text = strings.SimulationSpeed;
        _sizeTitle.Text = strings.FlySize;
        _cursorThreat.Text = strings.CursorAvoidance;
        _showDiagnostics.Text = strings.ShowDiagnosticsCheck;
        _stateTitle.Text = strings.StateLabel; _threatTitle.Text = strings.ThreatLabel;
        _positionTitle.Text = strings.PositionLabel; _sensoryTitle.Text = strings.SensoryCount;
        _motorTitle.Text = strings.MotorCount; _neuronsTitle.Text = strings.NeuralUnits;
        _synapsesTitle.Text = strings.ConnectionCount;
        _resetSimulationButton.Text = strings.ResetSimulation;
        _resetNeuralButton.Text = strings.ResetNeural; _closeButton.Text = strings.Close;
        RefreshSnapshot();
    }

    public void SyncOverlayState(bool paused, bool diagnosticsVisible)
    {
        _syncing = true;
        _paused.Checked = paused;
        _showDiagnostics.Checked = diagnosticsVisible;
        _syncing = false;
    }

    private void RefreshSnapshot()
    {
        var snapshot = _snapshot();
        _stateValue.Text = snapshot.State switch
        {
            FlyState.Escape => _strings.Escape,
            FlyState.Rest => _strings.Rest,
            _ => _strings.Explore
        };
        _threatValue.Text = snapshot.Threat.ToString("0.00");
        _positionValue.Text = $"{snapshot.Position.X:0}, {snapshot.Position.Y:0} px";
        _sensoryValue.Text = snapshot.SensorySpikes.ToString();
        _motorValue.Text = snapshot.MotorSpikes.ToString();
        _neuronsValue.Text = snapshot.NeuronCount.ToString();
        _synapsesValue.Text = snapshot.ConnectionCount.ToString();
    }

    private void ConfigureGroup(GroupBox group, int height)
    {
        group.Width = 472; group.Height = height; group.ForeColor = ForeColor;
        group.BackColor = BackColor; group.Padding = new Padding(10);
    }

    private void ConfigureMetric(Label title, Label value)
    {
        title.AutoSize = false; title.Width = 194; title.Height = 23;
        value.AutoSize = false; value.Width = 220; value.Height = 23;
        title.ForeColor = ForeColor; value.ForeColor = ForeColor;
    }

    private void StyleButton(Button button)
    {
        button.AutoSize = true; button.FlatStyle = FlatStyle.Flat;
        button.BackColor = Color.FromArgb(48, 55, 69); button.ForeColor = ForeColor;
        button.FlatAppearance.BorderColor = Color.FromArgb(80, 90, 110);
        button.Padding = new Padding(7, 3, 7, 3); button.Margin = new Padding(5, 2, 5, 2);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        PopulateGroups();
    }

    private void PopulateGroups()
    {
        var simFlow = GroupContent(_simulationGroup);
        simFlow.Controls.Add(_paused);
        var speedRow = Row(_speedTitle, _speed, _speedValue);
        simFlow.Controls.Add(speedRow);
        var sizeRow = Row(_sizeTitle, _flySize);
        simFlow.Controls.Add(sizeRow);

        var behaviorFlow = GroupContent(_behaviorGroup);
        behaviorFlow.Controls.Add(_cursorThreat);
        behaviorFlow.Controls.Add(_showDiagnostics);

        var diagFlow = GroupContent(_diagnosticsGroup);
        diagFlow.Controls.Add(Row(_stateTitle, _stateValue));
        diagFlow.Controls.Add(Row(_threatTitle, _threatValue));
        diagFlow.Controls.Add(Row(_positionTitle, _positionValue));
        diagFlow.Controls.Add(Row(_sensoryTitle, _sensoryValue));
        diagFlow.Controls.Add(Row(_motorTitle, _motorValue));
        diagFlow.Controls.Add(Row(_neuronsTitle, _neuronsValue));
        diagFlow.Controls.Add(Row(_synapsesTitle, _synapsesValue));
    }

    private static FlowLayoutPanel GroupContent(GroupBox group)
    {
        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
            AutoScroll = true, Padding = new Padding(5), BackColor = group.BackColor
        };
        group.Controls.Add(flow);
        return flow;
    }

    private static FlowLayoutPanel Row(Label title, params Control[] controls)
    {
        var row = new FlowLayoutPanel
        {
            Width = 430, Height = controls.Any(control => control is TrackBar) ? 54 : 34,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false, Margin = new Padding(0, 1, 0, 1)
        };
        title.Margin = new Padding(3, 7, 4, 0);
        row.Controls.Add(title);
        foreach (var control in controls) { control.Margin = new Padding(2, 0, 2, 0); row.Controls.Add(control); }
        return row;
    }
}
