namespace DesktopFly.Simulation.Environment;

/// <summary>Local-only coarse cursor stimulus; no pixels, windows, or typed input are observed.</summary>
public readonly record struct StimulusSnapshot(Vector2 CursorPosition, Vector2 CursorVelocityPixelsPerSecond,
    Vector2 VirtualDesktopMin, Vector2 VirtualDesktopMax, double LightLevel = 0.65, double FoodOdor = 0);

public interface IThreatSensor
{
    double Sample(Vector2 flyPosition, StimulusSnapshot environment);
}

/// <summary>Approximation: a fast cursor moving close to the fly is treated as a visual threat.</summary>
public sealed class CursorThreatSensor : IThreatSensor
{
    public double Sample(Vector2 flyPosition, StimulusSnapshot environment)
    {
        var distance = Vector2.Distance(flyPosition, environment.CursorPosition);
        var proximity = Math.Clamp(1 - distance / 260.0, 0, 1);
        var approachSpeed = environment.CursorVelocityPixelsPerSecond.Length;
        var speed = Math.Clamp(approachSpeed / 1200.0, 0, 1);
        return Math.Clamp(0.08 + 0.62 * proximity + 0.48 * speed * proximity, 0, 1);
    }
}
