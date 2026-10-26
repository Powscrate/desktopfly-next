using DesktopFly.Simulation.Environment;
using DesktopFly.Simulation.Neural;

namespace DesktopFly.Simulation.Behavior;

public enum FlyState { Explore, Escape, Rest }

/// <summary>Headless closed loop: cursor sensor → LIF circuit → motor impulse → constrained movement.</summary>
public sealed class FlySimulation
{
    private readonly IThreatSensor _sensor;
    private readonly SyntheticEscapeCircuit _circuit = new();
    private readonly Random _random;
    private Vector2 _velocity;
    private double _escapeSeconds;
    private double _wanderClock;
    private double _heading;
    private double _lastThreat;
    private long _motorEvents;

    public Vector2 Position { get; private set; }
    public FlyState State { get; private set; } = FlyState.Explore;
    public double HeadingRadians => _heading;
    public double ThreatLevel => _lastThreat;
    public long SensorySpikeCount => _circuit.SensorySpikes;
    public long MotorSpikeCount => _circuit.MotorSpikes;
    public long MotorEvents => _motorEvents;
    public double Speed => _velocity.Length;

    public FlySimulation(Vector2 initialPosition, int randomSeed = 1, IThreatSensor? sensor = null)
    {
        Position = initialPosition;
        _random = new Random(randomSeed);
        _sensor = sensor ?? new CursorThreatSensor();
        _heading = _random.NextDouble() * Math.PI * 2;
    }

    public void Step(double dtSeconds, StimulusSnapshot environment)
    {
        if (!double.IsFinite(dtSeconds) || dtSeconds <= 0 || dtSeconds > 0.1)
            throw new ArgumentOutOfRangeException(nameof(dtSeconds), "Use a fixed simulation step in (0, 100 ms].");
        _lastThreat = _sensor.Sample(Position, environment);
        // Smaller neural step improves integration while retaining the app's 60 Hz body update.
        var neuralSteps = 4;
        var neuralDtMs = dtSeconds * 1000 / neuralSteps;
        for (var i = 0; i < neuralSteps; i++)
        {
            if (_circuit.Step(_lastThreat, neuralDtMs))
            {
                _motorEvents++;
                var away = (Position - environment.CursorPosition).Normalized;
                if (away == Vector2.Zero) away = new Vector2(Math.Cos(_heading + Math.PI), Math.Sin(_heading + Math.PI));
                _heading = Math.Atan2(away.Y, away.X);
                _velocity += away * 180;
                _escapeSeconds = Math.Max(_escapeSeconds, 0.65);
            }
        }

        _escapeSeconds = Math.Max(0, _escapeSeconds - dtSeconds);
        if (_escapeSeconds > 0) State = FlyState.Escape;
        else State = _lastThreat > 0.76 ? FlyState.Rest : FlyState.Explore;

        if (State == FlyState.Explore)
        {
            _wanderClock -= dtSeconds;
            if (_wanderClock <= 0)
            {
                _heading += (_random.NextDouble() - 0.5) * 1.5;
                _wanderClock = 0.8 + _random.NextDouble() * 1.8;
            }
            var desired = new Vector2(Math.Cos(_heading), Math.Sin(_heading)) * 24;
            _velocity += (desired - _velocity) * Math.Min(1, dtSeconds * 2.0);
        }
        else if (State == FlyState.Rest) _velocity *= Math.Max(0, 1 - dtSeconds * 3);
        else _velocity *= Math.Max(0, 1 - dtSeconds * 0.3);

        Position += _velocity * dtSeconds;
        var min = environment.VirtualDesktopMin + new Vector2(24, 30);
        var max = environment.VirtualDesktopMax - new Vector2(24, 30);
        if (Position.X < min.X || Position.X > max.X) { Position = new Vector2(Math.Clamp(Position.X, min.X, max.X), Position.Y); _velocity = new Vector2(-_velocity.X * 0.45, _velocity.Y); _heading = Math.Atan2(_velocity.Y, _velocity.X); }
        if (Position.Y < min.Y || Position.Y > max.Y) { Position = new Vector2(Position.X, Math.Clamp(Position.Y, min.Y, max.Y)); _velocity = new Vector2(_velocity.X, -_velocity.Y * 0.45); _heading = Math.Atan2(_velocity.Y, _velocity.X); }
    }

    public void Reset(Vector2 position)
    {
        Position = position; _velocity = Vector2.Zero; _escapeSeconds = 0; _wanderClock = 0; _lastThreat = 0; _motorEvents = 0;
        State = FlyState.Explore; ResetNeural(); _heading = _random.NextDouble() * Math.PI * 2;
    }

    /// <summary>Resets neural membrane state and spike counts without moving the fly.</summary>
    public void ResetNeural()
    {
        _circuit.Reset();
        _motorEvents = 0;
    }
}
