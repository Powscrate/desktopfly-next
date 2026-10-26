using System.Text;

namespace DesktopFly.Simulation.Diagnostics;

/// <summary>Minimal rotating local log; writes are serialized and never performed by simulation ticks.</summary>
public sealed class DiagnosticLog
{
    private readonly string _path;
    private readonly object _gate = new();
    public DiagnosticLog(string path) => _path = path;
    public void Write(string level, string message, Exception? error = null)
    {
        lock (_gate)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            if (File.Exists(_path) && new FileInfo(_path).Length > 1_000_000)
                File.Move(_path, _path + ".1", true);
            var line = $"{DateTimeOffset.Now:O} [{level}] {message}{(error is null ? "" : Environment.NewLine + error)}{Environment.NewLine}";
            File.AppendAllText(_path, line, Encoding.UTF8);
        }
    }
}
