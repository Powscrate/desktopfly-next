using DesktopFly.Simulation.Behavior;
using DesktopFly.Simulation.Configuration;
using DesktopFly.Simulation.Environment;
using DesktopFly.Simulation.Neural;

if (args.Length > 0 && args[0] == "--experiment")
{
    var output = args.Length > 1 ? args[1] : Path.Combine(Environment.CurrentDirectory, "closed-loop-export.csv");
    RunExperiment(output);
}
else
{
    var suite = new Tests();
    suite.Run();
}

static void RunExperiment(string output)
{
    var simulation = new FlySimulation(new Vector2(650, 400), 42);
    var rows = new List<string> { "time_s,fly_x_px,fly_y_px,state,threat,sensory_spikes,motor_spikes" };
    var cursor = new Vector2(1100, 400);
    var previousCursor = cursor;
    const double dt = 1.0 / 60;
    for (var frame = 0; frame < 600; frame++)
    {
        // At 3 s, the simulated cursor approaches the fly, then remains nearby.
        if (frame is >= 180 and < 240) cursor = new Vector2(1100 - (frame - 179) * 6.0, 400);
        var velocity = (cursor - previousCursor) / dt;
        var environment = new StimulusSnapshot(cursor, velocity, Vector2.Zero, new Vector2(1366, 768));
        simulation.Step(dt, environment);
        var invariant = System.Globalization.CultureInfo.InvariantCulture;
        rows.Add(string.Join(",", new[] {
            (frame * dt).ToString("0.000", invariant),
            simulation.Position.X.ToString("0.00", invariant),
            simulation.Position.Y.ToString("0.00", invariant),
            simulation.State.ToString(),
            simulation.ThreatLevel.ToString("0.000", invariant),
            simulation.SensorySpikeCount.ToString(invariant),
            simulation.MotorSpikeCount.ToString(invariant)
        }));
        previousCursor = cursor;
    }
    var fullPath = Path.GetFullPath(output);
    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
    File.WriteAllLines(fullPath, rows);
    Console.WriteLine($"Wrote 600 reproducible closed-loop samples to {fullPath}");
}

internal sealed class Tests
{
    private int _passed;
    private int _total;
    public void Run()
    {
        Test("cursor threat sensor bounded and distance-sensitive", ThreatSensor);
        Test("disabled cursor threat sensor returns zero", ThreatSensorCanBeDisabled);
        Test("MaleCNS v1.0 subgraph loads and recruits motor neurons", MaleCnsCircuit);
        Test("connectome-driven locomotion and threat escape", ClosedLoopEscape);
        Test("walking gait and flight wing phases advance", GaitAndFlightAnimation);
        Test("neural reset preserves body position", NeuralResetLeavesBody);
        Test("same seed produces reproducible movement", DeterministicReplay);
        Test("fly remains inside virtual desktop bounds", BoundaryConstraint);
        Test("configuration round-trips and validates language", ConfigurationRoundTrip);
        Console.WriteLine($"{_passed}/{_total} checks passed.");
    }
    private void Test(string name, Action action)
    {
        _total++;
        try { action(); _passed++; Console.WriteLine($"PASS {name}"); }
        catch (Exception ex) { Console.Error.WriteLine($"FAIL {name}: {ex.Message}"); Environment.ExitCode = 1; }
    }
    private static void Assert(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static void ThreatSensor()
    {
        var sensor = new CursorThreatSensor();
        var env = new StimulusSnapshot(new Vector2(300, 300), Vector2.Zero, Vector2.Zero, new Vector2(1000, 800));
        var far = sensor.Sample(Vector2.Zero, env);
        var near = sensor.Sample(new Vector2(300, 300), env);
        Assert(near > far, "proximity should increase threat");
        Assert(near is >= 0 and <= 1, "threat must be normalized");
    }
    private static void ThreatSensorCanBeDisabled()
    {
        var sensor = new CursorThreatSensor();
        var environment = new StimulusSnapshot(new Vector2(200, 200), new Vector2(900, 0), Vector2.Zero,
            new Vector2(1000, 800), CursorThreatEnabled: false);
        Assert(sensor.Sample(new Vector2(200, 200), environment) == 0, "disabled cursor threat should be zero");
    }
    private static void MaleCnsCircuit()
    {
        var circuit = new MaleCnsLocomotionCircuit();
        Assert(circuit.NeuronCount == 1045 && circuit.ConnectionCount == 17224,
            "embedded MaleCNS v1.0 locomotor subgraph counts differ from its provenance");
        circuit.SetLegFeedback(new double[6], Enumerable.Repeat(0.8, 6).ToArray());
        circuit.SetDescendingDrive(45, 0);
        circuit.StepMilliseconds(1000);
        Assert(circuit.TotalSpikes > 0 && circuit.MotorSpikes > 0,
            "descending drive failed to recruit the measured leg circuit");
        Assert(circuit.MeanMotorActivation > 0, "motor population rate did not reach the body interface");
    }
    private static void ClosedLoopEscape()
    {
        var sim = new FlySimulation(new Vector2(500, 400), 17);
        var before = sim.Position;
        var env = new StimulusSnapshot(before, Vector2.Zero, Vector2.Zero, new Vector2(1000, 800));
        for (var i = 0; i < 240; i++) sim.Step(1.0 / 60, env);
        Assert(sim.NeuralNeuronCount == 1045 && sim.NeuralConnectionCount == 17224,
            "runtime is not using the documented MaleCNS locomotor subgraph");
        Assert(sim.SensorySpikeCount > 0 && sim.MotorSpikeCount > 0,
            "sensory/motor populations were not recruited during locomotion");
        Assert(sim.MotorEvents > 0 && Vector2.Distance(sim.Position, before) > 1,
            "cursor-threat response did not produce a surface displacement");
    }
    private static void GaitAndFlightAnimation()
    {
        var sim = new FlySimulation(new Vector2(500, 400), 5);
        var surface = new StimulusSnapshot(new Vector2(900, 700), Vector2.Zero, Vector2.Zero,
            new Vector2(1000, 800), CursorThreatEnabled: false);
        for (var i = 0; i < 50; i++) sim.Step(1.0 / 60, surface);
        Assert(sim.IsWalking && sim.StridePhase > 0 && !sim.IsFlying,
            "surface walking did not advance the six-leg gait phase");
        var threat = surface with { CursorPosition = sim.Position, CursorThreatEnabled = true };
        sim.Step(1.0 / 60, threat);
        Assert(sim.IsFlying && sim.WingPhase > 0, "escape did not animate wings or signal brief flight");
    }
    private static void NeuralResetLeavesBody()
    {
        var sim = new FlySimulation(new Vector2(500, 400), 23);
        var environment = new StimulusSnapshot(new Vector2(500, 400), Vector2.Zero, Vector2.Zero, new Vector2(1000, 800));
        for (var i = 0; i < 20; i++) sim.Step(1.0 / 60, environment);
        var position = sim.Position;
        Assert(sim.MotorSpikeCount > 0, "test stimulus did not activate the motor unit");
        sim.ResetNeural();
        Assert(sim.Position == position, "neural reset moved the fly");
        Assert(sim.SensorySpikeCount == 0 && sim.MotorSpikeCount == 0, "neural counters were not reset");
    }
    private static void DeterministicReplay()
    {
        var a = new FlySimulation(new Vector2(400, 400), 99);
        var b = new FlySimulation(new Vector2(400, 400), 99);
        for (var i = 0; i < 400; i++)
        {
            var cursor = new Vector2(200 + i % 300, 300);
            var stimulus = new StimulusSnapshot(cursor, new Vector2(20, -3), Vector2.Zero, new Vector2(1000, 800));
            a.Step(1.0 / 60, stimulus); b.Step(1.0 / 60, stimulus);
        }
        Assert(a.Position == b.Position && a.MotorSpikeCount == b.MotorSpikeCount, "seeded replay diverged");
    }
    private static void BoundaryConstraint()
    {
        var sim = new FlySimulation(new Vector2(40, 40), 5);
        var env = new StimulusSnapshot(new Vector2(10000, 10000), Vector2.Zero, Vector2.Zero, new Vector2(200, 160));
        for (var i = 0; i < 2000; i++) sim.Step(1.0 / 60, env);
        Assert(sim.Position.X is >= 24 and <= 176 && sim.Position.Y is >= 30 and <= 130, "fly escaped bounds");
    }
    private static void ConfigurationRoundTrip()
    {
        var path = Path.Combine(Path.GetTempPath(), $"desktopfly-{Guid.NewGuid():N}.json");
        try
        {
            new AppConfiguration { Language = "fr", FlySize = 2 }.Save(path);
            var loaded = AppConfiguration.Load(path);
            Assert(loaded.Language == "fr" && loaded.FlySize == 2, "configuration values were not retained");
            File.WriteAllText(path, "{\"SchemaVersion\":1,\"Language\":\"es\",\"FlySize\":9,\"SimulationSpeedPercent\":500}");
            var sanitized = AppConfiguration.Load(path);
            Assert(sanitized.Language == "en" && sanitized.FlySize == 3 && sanitized.SimulationSpeedPercent == 300,
                "unsupported language or out-of-range settings were not sanitized");
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
}
