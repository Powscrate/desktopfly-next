namespace DesktopFly.Simulation.Behavior;

/// <summary>Centered joint-angle offsets sampled from one recorded six-leg walking step.</summary>
public readonly record struct FlyLegJointPose(
    double CoxaPitch,
    double CoxaRoll,
    double CoxaYaw,
    double FemurPitch,
    double FemurRoll,
    double TibiaPitch,
    double TarsusPitch);
