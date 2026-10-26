using DesktopFly.Simulation.Environment;
using DesktopFly.Simulation.Neural;

namespace DesktopFly.Simulation.Behavior;

public enum FlyState { Explore, Escape, Rest }

/// <summary>
/// Desktop-surface locomotion driven by a bounded MaleCNS leg-circuit model.
/// A compact tripod gait and the flight/wing policy are declared body-model
/// assumptions; the bundled anatomical graph contains leg, not wing, neurons.
/// </summary>
public sealed class FlySimulation
{
    private readonly IThreatSensor _sensor;
    private readonly MaleCnsLocomotionCircuit _circuit = new();
    private readonly Random _random;
    private Vector2 _velocity;
    private double _escapeSeconds;
    private double _escapeCooldown;
    private double _wanderClock;
    private double _walkBoutRemaining;
    private double _restBoutRemaining;
    private double _heading;
    private double _lastThreat;
    private double _previousThreat;
    private double _stridePhase;
    private double _wingPhase;
    private double _neuralRemainderMs;
    private readonly double[] _legContacts = new double[6];
    private readonly double[] _legMotion = new double[6];
    private long _motorEvents;

    public Vector2 Position { get; private set; }
    public FlyState State { get; private set; } = FlyState.Explore;
    public double HeadingRadians => _heading;
    public double ThreatLevel => _lastThreat;
    public long SensorySpikeCount => _circuit.SensorySpikes;
    public long MotorSpikeCount => _circuit.MotorSpikes;
    public long MotorEvents => _motorEvents;
    public double Speed => _velocity.Length;
    public double StridePhase => _stridePhase;
    public double WingPhase => _wingPhase;
    public bool IsWalking => State == FlyState.Explore && Speed > 1;
    public bool IsFlying => State == FlyState.Escape;
    public double MotorActivity => _circuit.MeanMotorActivation;
    public int NeuralNeuronCount => _circuit.NeuronCount;
    public int NeuralConnectionCount => _circuit.ConnectionCount;

    public FlySimulation(Vector2 initialPosition, int randomSeed = 1, IThreatSensor? sensor = null)
    {
        Position = initialPosition;
        _random = new Random(randomSeed);
        _sensor = sensor ?? new CursorThreatSensor();
        _heading = _random.NextDouble() * Math.PI * 2;
        _walkBoutRemaining = NextWalkBout();
    }

    public void Step(double dtSeconds, StimulusSnapshot environment)
    {
        if (!double.IsFinite(dtSeconds) || dtSeconds <= 0 || dtSeconds > 0.1)
            throw new ArgumentOutOfRangeException(nameof(dtSeconds), "Use a fixed simulation step in (0, 100 ms].");

        _lastThreat = _sensor.Sample(Position, environment);
        _escapeCooldown = Math.Max(0, _escapeCooldown - dtSeconds);
        if (_lastThreat >= 0.66 && _previousThreat < 0.66 && _escapeCooldown <= 0)
        {
            var away = (Position - environment.CursorPosition).Normalized;
            if (away == Vector2.Zero) away = new Vector2(Math.Cos(_heading), Math.Sin(_heading));
            _heading = Math.Atan2(away.Y, away.X);
            _escapeSeconds = 0.72;
            _escapeCooldown = 1.4;
            _motorEvents++;
        }
        _previousThreat = _lastThreat;
        _escapeSeconds = Math.Max(0, _escapeSeconds - dtSeconds);
        if (_escapeSeconds > 0)
        {
            State = FlyState.Escape;
        }
        else if (_lastThreat > 0.84)
        {
            State = FlyState.Rest;
        }
        else if (_restBoutRemaining > 0)
        {
            _restBoutRemaining = Math.Max(0, _restBoutRemaining - dtSeconds);
            State = FlyState.Rest;
            if (_restBoutRemaining == 0) _walkBoutRemaining = NextWalkBout();
        }
        else
        {
            _walkBoutRemaining -= dtSeconds;
            if (_walkBoutRemaining <= 0)
            {
                _restBoutRemaining = NextRestBout();
                State = FlyState.Rest;
            }
            else
            {
                State = FlyState.Explore;
            }
        }

        if (State == FlyState.Explore)
        {
            _wanderClock -= dtSeconds;
            if (_wanderClock <= 0)
            {
                // Persistent, short surface-walking bouts with occasional small turns.
                _heading += (_random.NextDouble() - 0.5) * 0.95;
                _wanderClock = 1.4 + _random.NextDouble() * 2.5;
            }
        }

        Array.Clear(_legContacts);
        Array.Clear(_legMotion);
        if (State == FlyState.Explore)
        {
            for (var leg = 0; leg < 6; leg++)
            {
                // Alternating tripod phase is a kinematic gait template, not a
                // connectome-derived oscillator or experimental replay.
                var phase = _stridePhase + ((leg is 0 or 3 or 4) ? 0 : Math.PI);
                _legContacts[leg] = Math.Cos(phase) > 0 ? 0.75 : 0.08;
                _legMotion[leg] = Math.Abs(Math.Sin(phase));
            }
        }
        _circuit.SetLegFeedback(_legContacts, _legMotion);
        _circuit.SetDescendingDrive(State == FlyState.Explore ? 45 : 0,
            State == FlyState.Escape ? 45 : 0);
        _neuralRemainderMs += dtSeconds * 1000;
        var neuralMilliseconds = (int)_neuralRemainderMs;
        _neuralRemainderMs -= neuralMilliseconds;
        _circuit.StepMilliseconds(neuralMilliseconds);

        if (State == FlyState.Explore)
        {
            var strideRate = 5.5 + 5.0 * _circuit.MeanMotorActivation;
            _stridePhase = WrapPhase(_stridePhase + dtSeconds * Math.PI * 2 * strideRate);
            var walkingSpeed = 17 + 15 * _circuit.MeanMotorActivation;
            var desired = new Vector2(Math.Cos(_heading), Math.Sin(_heading)) * walkingSpeed;
            // Strong ground traction avoids long inertial, space-floating glides.
            _velocity += (desired - _velocity) * Math.Min(1, dtSeconds * 7.0);
        }
        else if (State == FlyState.Rest)
        {
            _velocity = Vector2.Zero;
            _stridePhase = 0;
        }
        else
        {
            _velocity = new Vector2(Math.Cos(_heading), Math.Sin(_heading)) * 112;
        }

        if (State == FlyState.Escape)
            _wingPhase = WrapPhase(_wingPhase + dtSeconds * Math.PI * 2 * 28);
        else
            _wingPhase = WrapPhase(_wingPhase + (0 - _wingPhase) * Math.Min(1, dtSeconds * 9));

        Position += _velocity * dtSeconds;
        var inset = new Vector2(24, 30);
        var min = environment.VirtualDesktopMin + inset;
        var max = environment.VirtualDesktopMax - inset;
        if (Position.X < min.X || Position.X > max.X || Position.Y < min.Y || Position.Y > max.Y)
        {
            Position = Vector2.Clamp(Position, min, max);
            var center = (environment.VirtualDesktopMin + environment.VirtualDesktopMax) * 0.5;
            _heading = Math.Atan2(center.Y - Position.Y, center.X - Position.X);
            _velocity = State == FlyState.Escape
                ? new Vector2(Math.Cos(_heading), Math.Sin(_heading)) * 112
                : Vector2.Zero;
        }
    }

    public void Reset(Vector2 position)
    {
        Position = position;
        _velocity = Vector2.Zero;
        _escapeSeconds = _escapeCooldown = _wanderClock = _lastThreat = _previousThreat = 0;
        _restBoutRemaining = 0;
        _walkBoutRemaining = NextWalkBout();
        _stridePhase = _wingPhase = _neuralRemainderMs = 0;
        _motorEvents = 0;
        State = FlyState.Explore;
        ResetNeural();
        _heading = _random.NextDouble() * Math.PI * 2;
    }

    /// <summary>Resets neural membrane state and spike counts without moving the fly.</summary>
    public void ResetNeural()
    {
        _circuit.Reset();
        _motorEvents = 0;
    }

    // These bouts are an explicit product-level behavior prior, not durations
    // fitted to a fly-behavior dataset.
    private double NextWalkBout() => 3.0 + _random.NextDouble() * 5.0;
    private double NextRestBout() => 1.0 + _random.NextDouble() * 2.0;

    private static double WrapPhase(double phase)
    {
        phase %= Math.PI * 2;
        return phase < 0 ? phase + Math.PI * 2 : phase;
    }
}
