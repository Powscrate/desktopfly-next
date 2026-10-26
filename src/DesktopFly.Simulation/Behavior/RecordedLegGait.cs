using System.Reflection;
using System.Text.Json;

namespace DesktopFly.Simulation.Behavior;

/// <summary>
/// Loads the compact six-leg joint-angle cycle distributed by FlyGym. The recorded
/// values are experimental-derived input; projecting them onto the 2D sprite is an
/// explicit rendering approximation, not a biomechanical forward simulation.
/// </summary>
public sealed class RecordedLegGait
{
    private const string ResourceName = "DesktopFly.Simulation.flygym-untethered-step-v1.json";
    public const int LegCount = 6;
    public const int JointCount = 7;
    private readonly double[,,] _angles;
    private readonly double[,] _means;

    public int FrameCount { get; }
    public double CycleDurationSeconds { get; }
    public double CycleFrequencyHz => 1 / CycleDurationSeconds;
    public static IReadOnlyList<string> LegOrder { get; } = Array.AsReadOnly(new[] { "RF", "LF", "RM", "LM", "RH", "LH" });

    public RecordedLegGait()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
            ?? throw new InvalidDataException($"Embedded recorded-leg gait resource '{ResourceName}' is missing.");
        using var document = JsonDocument.Parse(stream);
        var root = document.RootElement;
        FrameCount = root.GetProperty("cycleFrames").GetInt32();
        CycleDurationSeconds = root.GetProperty("cycleDurationSeconds").GetDouble();
        if (FrameCount < 2 || !double.IsFinite(CycleDurationSeconds) || CycleDurationSeconds <= 0)
            throw new InvalidDataException("Recorded leg gait has invalid timing metadata.");
        var legs = root.GetProperty("legs");
        if (legs.GetArrayLength() != LegCount)
            throw new InvalidDataException("Recorded gait must contain six leg traces.");
        _angles = new double[LegCount, FrameCount, JointCount];
        _means = new double[LegCount, JointCount];
        var legIndex = 0;
        foreach (var leg in legs.EnumerateArray())
        {
            if (!string.Equals(leg.GetProperty("id").GetString(), LegOrder[legIndex], StringComparison.Ordinal))
                throw new InvalidDataException("Recorded gait leg order does not match the documented RF/LF/RM/LM/RH/LH order.");
            var frames = leg.GetProperty("jointAnglesRadians");
            if (frames.GetArrayLength() != FrameCount)
                throw new InvalidDataException("Recorded gait frame count is inconsistent.");
            var frameIndex = 0;
            foreach (var frame in frames.EnumerateArray())
            {
                if (frame.GetArrayLength() != JointCount)
                    throw new InvalidDataException("Recorded gait joint count is inconsistent.");
                var jointIndex = 0;
                foreach (var angle in frame.EnumerateArray())
                {
                    var value = angle.GetDouble();
                    if (!double.IsFinite(value)) throw new InvalidDataException("Recorded gait contains a non-finite joint angle.");
                    _angles[legIndex, frameIndex, jointIndex] = value;
                    _means[legIndex, jointIndex] += value / FrameCount;
                    jointIndex++;
                }
                frameIndex++;
            }
            legIndex++;
        }
    }

    /// <summary>Samples cyclic, mean-centered angles for drawing one leg.</summary>
    public FlyLegJointPose Sample(int legIndex, double phaseRadians)
    {
        if (legIndex is < 0 or >= LegCount) throw new ArgumentOutOfRangeException(nameof(legIndex));
        if (!double.IsFinite(phaseRadians)) throw new ArgumentOutOfRangeException(nameof(phaseRadians));
        var cycle = phaseRadians / (Math.PI * 2);
        cycle -= Math.Floor(cycle);
        var framePosition = cycle * FrameCount;
        var frame0 = (int)framePosition;
        var frame1 = (frame0 + 1) % FrameCount;
        var amount = framePosition - frame0;
        Span<double> offsets = stackalloc double[JointCount];
        for (var joint = 0; joint < JointCount; joint++)
        {
            var a = _angles[legIndex, frame0, joint];
            var b = _angles[legIndex, frame1, joint];
            offsets[joint] = a + (b - a) * amount - _means[legIndex, joint];
        }
        return new FlyLegJointPose(offsets[0], offsets[1], offsets[2], offsets[3],
            offsets[4], offsets[5], offsets[6]);
    }
}
