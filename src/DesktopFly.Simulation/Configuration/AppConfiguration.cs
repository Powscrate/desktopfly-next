using System.Text.Json;

namespace DesktopFly.Simulation.Configuration;

public sealed class AppConfiguration
{
    public int SchemaVersion { get; set; } = 1;
    public string Language { get; set; } = "en";
    public bool StartPaused { get; set; }
    public bool StartInTray { get; set; }
    public bool ShowDiagnostics { get; set; } = true;
    public int RandomSeed { get; set; } = 42;
    public int FlySize { get; set; } = 1;
    public int SimulationSpeedPercent { get; set; } = 100;
    public bool CursorThreatEnabled { get; set; } = true;
    public static AppConfiguration Load(string path)
    {
        if (!File.Exists(path)) return new AppConfiguration();
        var config = JsonSerializer.Deserialize<AppConfiguration>(File.ReadAllText(path)) ?? new AppConfiguration();
        if (config.SchemaVersion != 1) throw new InvalidDataException($"Unsupported config schema {config.SchemaVersion}.");
        if (config.Language is not ("en" or "fr")) config.Language = "en";
        config.FlySize = Math.Clamp(config.FlySize, 1, 3);
        config.SimulationSpeedPercent = Math.Clamp(config.SimulationSpeedPercent, 25, 300);
        return config;
    }
    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporary, path, true);
    }
}
