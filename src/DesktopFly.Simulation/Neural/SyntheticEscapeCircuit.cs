namespace DesktopFly.Simulation.Neural;

/// <summary>Two-neuron demonstrator: visual-threat afferent → escape command unit.</summary>
public sealed class SyntheticEscapeCircuit
{
    private readonly LifNeuron _sensory = new(timeConstantMs: 10, refractoryPeriodMs: 2);
    private readonly LifNeuron _escape = new(timeConstantMs: 15, refractoryPeriodMs: 3);
    private const double SensoryToMotorWeight = 23;
    public long SensorySpikes => _sensory.SpikeCount;
    public long MotorSpikes => _escape.SpikeCount;

    public bool Step(double threatLevel, double dtMs)
    {
        if (!double.IsFinite(threatLevel) || threatLevel < 0 || threatLevel > 1)
            throw new ArgumentOutOfRangeException(nameof(threatLevel), "Threat level must be between 0 and 1.");
        var sensorySpike = _sensory.Step(threatLevel * 25, dtMs);
        // Sensory spikes provide a transient excitatory synaptic current to the motor unit.
        var motorSpike = _escape.Step(sensorySpike ? SensoryToMotorWeight : 0, dtMs);
        return motorSpike;
    }

    public void Reset() { _sensory.Reset(); _escape.Reset(); }
}
