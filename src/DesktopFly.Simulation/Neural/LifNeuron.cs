namespace DesktopFly.Simulation.Neural;

/// <summary>Current-based Leaky Integrate-and-Fire neuron with fixed-step Euler integration.</summary>
public sealed class LifNeuron
{
    public double RestingPotential { get; }
    public double Threshold { get; }
    public double ResetPotential { get; }
    public double TimeConstantMs { get; }
    public double RefractoryPeriodMs { get; }
    public double MembranePotential { get; private set; }
    public double RefractoryRemainingMs { get; private set; }
    public long SpikeCount { get; private set; }

    public LifNeuron(double restingPotential = -65, double threshold = -50, double resetPotential = -65,
        double timeConstantMs = 20, double refractoryPeriodMs = 2)
    {
        if (!double.IsFinite(restingPotential) || !double.IsFinite(threshold) || !double.IsFinite(resetPotential) ||
            !double.IsFinite(timeConstantMs) || timeConstantMs <= 0 || !double.IsFinite(refractoryPeriodMs) || refractoryPeriodMs < 0 || threshold <= resetPotential)
            throw new ArgumentOutOfRangeException(nameof(timeConstantMs), "Invalid LIF parameters.");
        (RestingPotential, Threshold, ResetPotential, TimeConstantMs, RefractoryPeriodMs) =
            (restingPotential, threshold, resetPotential, timeConstantMs, refractoryPeriodMs);
        MembranePotential = restingPotential;
    }

    /// <returns>True exactly when this step emits a spike.</returns>
    public bool Step(double inputCurrent, double dtMs)
    {
        if (!double.IsFinite(inputCurrent) || !double.IsFinite(dtMs) || dtMs <= 0 || dtMs > TimeConstantMs)
            throw new ArgumentOutOfRangeException(nameof(dtMs), "Input must be finite; timestep must be in (0, tau].");
        if (RefractoryRemainingMs > 0)
        {
            RefractoryRemainingMs = Math.Max(0, RefractoryRemainingMs - dtMs);
            MembranePotential = ResetPotential;
            return false;
        }
        // Current is in mV-equivalent drive units; resting leak is toward V_rest.
        MembranePotential += (dtMs / TimeConstantMs) * (RestingPotential - MembranePotential + inputCurrent);
        if (MembranePotential < Threshold) return false;
        MembranePotential = ResetPotential;
        RefractoryRemainingMs = RefractoryPeriodMs;
        SpikeCount++;
        return true;
    }

    public void Reset()
    {
        MembranePotential = RestingPotential;
        RefractoryRemainingMs = 0;
        SpikeCount = 0;
    }
}
