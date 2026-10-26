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
        Test("LIF threshold, reset, and refractory", LifThresholdAndRefractory);
        Test("LIF rejects invalid integration step", LifRejectsInvalidStep);
        Test("cursor threat sensor bounded and distance-sensitive", ThreatSensor);
        Test("disabled cursor threat sensor returns zero", ThreatSensorCanBeDisabled);
        Test("neural motor circuit causally triggers escape movement", ClosedLoopEscape);
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
    private static void LifThresholdAndRefractory()
    {
        var neuron = new LifNeuron(timeConstantMs: 10, refractoryPeriodMs: 2);
        Assert(neuron.Step(200, 1), "strong input should cross threshold");
        Assert(neuron.MembranePotential == neuron.ResetPotential, "spike must reset membrane");
        Assert(!neuron.Step(200, 1), "neuron must remain refractory");
        Assert(!neuron.Step(200, 1), "last refractory interval must not emit");
        Assert(neuron.Step(200, 1), "neuron should fire after refractory period");
    }
    private static void LifRejectsInvalidStep()
    {
        var neuron = new LifNeuron();
        try { neuron.Step(0, 0); throw new InvalidOperationException("zero step was accepted"); }
        catch (ArgumentOutOfRangeException) { }
    }
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
    private static void ClosedLoopEscape()
    {
        var sim = new FlySimulation(new Vector2(500, 400), 17);
        var before = sim.Position;
        var env = new StimulusSnapshot(before, Vector2.Zero, Vector2.Zero, new Vector2(1000, 800));
        for (var i = 0; i < 240; i++) sim.Step(1.0 / 60, env);
        Assert(sim.SensorySpikeCount > 0, "sensory LIF never spiked");
        Assert(sim.MotorSpikeCount > 0 && sim.MotorEvents > 0, "threat did not cause neural motor output");
        Assert(sim.State == FlyState.Escape || Vector2.Distance(sim.Position, before) > 1, "neural output did not affect locomotion");
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
