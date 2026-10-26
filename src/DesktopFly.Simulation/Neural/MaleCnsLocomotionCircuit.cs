using System.Reflection;
using System.Text.Json;

namespace DesktopFly.Simulation.Neural;

/// <summary>
/// Small spiking model over a measured MaleCNS locomotor subgraph. The graph is
/// anatomical data; all membrane, transmitter-sign, drive and output parameters
/// are explicit model assumptions, not measured physiology.
/// </summary>
public sealed class MaleCnsLocomotionCircuit
{
    private const string ResourceName = "DesktopFly.Simulation.malecns-locomotor-v1.json";
    private const double SynapticGain = 2.4;
    private const double BaselineCurrent = 0.022;
    private const double AdaptationKick = 0.01;
    private const double ExcitatoryDecay = 0.8187308;
    private const double InhibitoryDecay = 0.9048374;
    private const double RateDecay = 0.9048374;
    private const double AdaptationDecay = 0.9950125;
    private const double MembraneDecay = 0.9512294;
    private const double RatePerSpike = 95.16258;

    private readonly string[] _roles;
    private readonly int?[] _legs;
    private readonly string?[] _sensoryKinds;
    private readonly int[] _rowStart;
    private readonly int[] _targets;
    private readonly double[] _weights;
    private readonly int[][] _descendingGroups;
    private readonly int[][] _sensoryGroups;
    private readonly double[] _voltage;
    private readonly double[] _adaptation;
    private readonly int[] _refractory;
    private readonly double[] _rates;
    private readonly double[] _excitatory;
    private readonly double[] _inhibitory;
    private readonly double[] _nextExcitatory;
    private readonly double[] _nextInhibitory;
    private readonly double[] _drive;
    private readonly double[] _sensoryDrive;
    private readonly double[] _motorActivation = new double[6];

    public int NeuronCount => _roles.Length;
    public int ConnectionCount => _targets.Length;
    public long TotalSpikes { get; private set; }
    public long MotorSpikes { get; private set; }
    public long SensorySpikes { get; private set; }
    public double MeanMotorActivation
    {
        get
        {
            var total = 0.0;
            for (var leg = 0; leg < _motorActivation.Length; leg++) total += _motorActivation[leg];
            return total / _motorActivation.Length;
        }
    }

    public MaleCnsLocomotionCircuit()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
            ?? throw new InvalidDataException($"Embedded MaleCNS circuit resource '{ResourceName}' is missing.");
        using var document = JsonDocument.Parse(stream);
        var root = document.RootElement;
        var neurons = root.GetProperty("neurons");
        var edges = root.GetProperty("edges");
        if (neurons.GetArrayLength() != 1045 || edges.GetArrayLength() != 17224)
            throw new InvalidDataException("MaleCNS locomotor circuit counts do not match the attributed v1.0 subgraph.");

        var count = neurons.GetArrayLength();
        _roles = new string[count];
        _legs = new int?[count];
        _sensoryKinds = new string?[count];
        var descending = new List<int>[10];
        for (var i = 0; i < descending.Length; i++) descending[i] = [];
        var sensory = Enumerable.Range(0, 6).Select(_ => new List<int>()).ToArray();
        var descendingKeys = new[] { ("DNa01", "left"), ("DNa01", "right"), ("DNa02", "left"), ("DNa02", "right"), ("MDN", "left"), ("MDN", "right"), ("DNp09", "left"), ("DNp09", "right"), ("DNg11", "left"), ("DNg11", "right") };
        var descendingLookup = descendingKeys.Select((key, index) => (key, index)).ToDictionary(item => item.key, item => item.index);

        var index = 0;
        foreach (var neuron in neurons.EnumerateArray())
        {
            var role = neuron.GetProperty("role").GetString() ?? throw new InvalidDataException("Neuron role is missing.");
            var type = neuron.GetProperty("type").GetString() ?? string.Empty;
            var side = neuron.GetProperty("side").GetString() ?? string.Empty;
            var legElement = neuron.GetProperty("leg");
            var leg = legElement.ValueKind == JsonValueKind.Number ? legElement.GetInt32() : (int?)null;
            var sensoryKind = neuron.GetProperty("sensoryKind").ValueKind == JsonValueKind.String
                ? neuron.GetProperty("sensoryKind").GetString() : null;
            if (leg is < 0 or >= 6) throw new InvalidDataException("Circuit contains an out-of-range leg assignment.");
            _roles[index] = role;
            _legs[index] = leg;
            _sensoryKinds[index] = sensoryKind;
            if (role == "descending" && descendingLookup.TryGetValue((type, side), out var group)) descending[group].Add(index);
            if (role == "sensory" && leg is int sensoryLeg) sensory[sensoryLeg].Add(index);
            index++;
        }

        _rowStart = new int[count + 1];
        var edgeCountPerSource = new int[count];
        var incomingMagnitude = new double[count];
        foreach (var edge in edges.EnumerateArray())
        {
            if (edge.GetArrayLength() != 3) throw new InvalidDataException("Malformed MaleCNS connection row.");
            var source = ReadIndex(edge[0], count);
            var target = ReadIndex(edge[1], count);
            var signedWeight = edge[2].GetDouble();
            if (!double.IsFinite(signedWeight)) throw new InvalidDataException("Non-finite MaleCNS connection weight.");
            edgeCountPerSource[source]++;
            incomingMagnitude[target] += Math.Abs(signedWeight);
        }
        for (var i = 0; i < count; i++) _rowStart[i + 1] = _rowStart[i] + edgeCountPerSource[i];
        _targets = new int[edges.GetArrayLength()];
        _weights = new double[_targets.Length];
        var fill = (int[])_rowStart.Clone();
        foreach (var edge in edges.EnumerateArray())
        {
            var source = ReadIndex(edge[0], count);
            var target = ReadIndex(edge[1], count);
            var slot = fill[source]++;
            var signedWeight = edge[2].GetDouble();
            _targets[slot] = target;
            // Relative contact counts and assumed transmitter signs are retained;
            // contact count is deliberately not interpreted as measured conductance.
            _weights[slot] = SynapticGain * signedWeight / Math.Max(60, incomingMagnitude[target]);
        }

        _descendingGroups = descending.Select(group => group.ToArray()).ToArray();
        _sensoryGroups = sensory.Select(group => group.ToArray()).ToArray();
        _voltage = new double[count];
        _adaptation = new double[count];
        _refractory = new int[count];
        _rates = new double[count];
        _excitatory = new double[count];
        _inhibitory = new double[count];
        _nextExcitatory = new double[count];
        _nextInhibitory = new double[count];
        _drive = new double[count];
        _sensoryDrive = new double[count];
    }

    /// <summary>Clear and set the modeled descending-neuron drive for one frame.</summary>
    public void SetDescendingDrive(double forwardDrive, double reverseDrive)
    {
        Array.Clear(_drive);
        SetPopulationDrive(2, forwardDrive);
        SetPopulationDrive(3, forwardDrive);
        SetPopulationDrive(4, reverseDrive);
        SetPopulationDrive(5, reverseDrive);
    }

    /// <summary>
    /// Apply assumed leg proprioception/contact signals. The source annotations do
    /// not provide neuron-specific tuning; this coarse mapping is a model interface.
    /// </summary>
    public void SetLegFeedback(ReadOnlySpan<double> contact, ReadOnlySpan<double> movement)
    {
        if (contact.Length != 6 || movement.Length != 6)
            throw new ArgumentException("Six leg feedback values are required.");
        Array.Clear(_sensoryDrive);
        for (var leg = 0; leg < 6; leg++)
        {
            var contactValue = Math.Clamp(contact[leg], 0, 1);
            var movementValue = Math.Clamp(movement[leg], 0, 1);
            foreach (var neuron in _sensoryGroups[leg])
            {
                var kind = _sensoryKinds[neuron];
                var value = kind == "campaniform" ? contactValue :
                    kind == "hair_plate" ? movementValue * 0.65 : Math.Max(contactValue * 0.45, movementValue);
                _sensoryDrive[neuron] = value * 0.10;
            }
        }
    }

    public void StepMilliseconds(int milliseconds)
    {
        if (milliseconds is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(milliseconds));
        for (var elapsed = 0; elapsed < milliseconds; elapsed++) StepOneMillisecond();
        UpdateMotorActivation();
    }

    public void Reset()
    {
        Array.Clear(_voltage); Array.Clear(_adaptation); Array.Clear(_refractory);
        Array.Clear(_rates); Array.Clear(_excitatory); Array.Clear(_inhibitory);
        Array.Clear(_nextExcitatory); Array.Clear(_nextInhibitory); Array.Clear(_drive);
        Array.Clear(_sensoryDrive); Array.Clear(_motorActivation);
        TotalSpikes = MotorSpikes = SensorySpikes = 0;
    }

    private void SetPopulationDrive(int group, double rate)
    {
        var current = Math.Clamp(rate, 0, 87.5) * 0.004;
        foreach (var neuron in _descendingGroups[group]) _drive[neuron] = current;
    }

    private void StepOneMillisecond()
    {
        var count = _roles.Length;
        for (var i = 0; i < count; i++)
        {
            _excitatory[i] = _excitatory[i] * ExcitatoryDecay + _nextExcitatory[i];
            _inhibitory[i] = _inhibitory[i] * InhibitoryDecay + _nextInhibitory[i];
            _nextExcitatory[i] = _nextInhibitory[i] = 0;
        }
        for (var i = 0; i < count; i++)
        {
            _rates[i] *= RateDecay;
            _adaptation[i] *= AdaptationDecay;
            if (_refractory[i] > 0) { _refractory[i]--; continue; }
            _voltage[i] = Math.Max(-1, _voltage[i] * MembraneDecay + _excitatory[i] + _inhibitory[i]
                + BaselineCurrent + _drive[i] + _sensoryDrive[i] - _adaptation[i]);
            if (_voltage[i] < 1) continue;
            _voltage[i] = 0;
            _refractory[i] = 2;
            _adaptation[i] += AdaptationKick;
            _rates[i] += RatePerSpike;
            TotalSpikes++;
            if (_roles[i] == "motor") MotorSpikes++;
            if (_roles[i] == "sensory") SensorySpikes++;
            for (var edge = _rowStart[i]; edge < _rowStart[i + 1]; edge++)
            {
                var weight = _weights[edge];
                if (weight >= 0) _nextExcitatory[_targets[edge]] += weight;
                else _nextInhibitory[_targets[edge]] += weight;
            }
        }
    }

    private void UpdateMotorActivation()
    {
        for (var leg = 0; leg < 6; leg++)
        {
            var total = 0.0;
            var count = 0;
            for (var i = 0; i < _roles.Length; i++)
            {
                if (_roles[i] != "motor" || _legs[i] != leg) continue;
                total += _rates[i];
                count++;
            }
            var rate = total / Math.Max(1, count);
            _motorActivation[leg] = rate / (rate + 50);
        }
    }

    private static int ReadIndex(JsonElement value, int count)
    {
        var raw = value.GetDouble();
        var index = (int)raw;
        if (!double.IsFinite(raw) || raw != index || index < 0 || index >= count)
            throw new InvalidDataException("MaleCNS edge references an invalid neuron index.");
        return index;
    }
}
