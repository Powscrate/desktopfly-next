using DesktopFly.Simulation.Configuration;
using DesktopFly.Simulation.Diagnostics;

namespace DesktopFly.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DesktopFlyNext");
        var log = new DiagnosticLog(Path.Combine(appData, "logs", "desktopfly.log"));
        try
        {
            var configPath = Path.Combine(appData, "config.json");
            var config = AppConfiguration.Load(configPath);
            Application.ThreadException += (_, e) => log.Write("ERROR", "UI thread exception", e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (_, e) => log.Write("FATAL", "Unhandled exception", e.ExceptionObject as Exception);
            log.Write("INFO", $"DesktopFly Next 0.1.0 starting; language={config.Language}");
            var overlay = new FlyOverlayForm(config, configPath, log);
            Application.Run(new FlyApplicationContext(overlay, config.StartInTray));
            log.Write("INFO", "Application stopped normally.");
        }
        catch (Exception ex)
        {
            log.Write("FATAL", "Startup failed", ex);
            MessageBox.Show($"DesktopFly Next could not start.\n\n{ex.Message}\n\nLog: {Path.Combine(appData, "logs", "desktopfly.log")}", "DesktopFly Next", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

internal sealed class FlyApplicationContext : ApplicationContext
{
    public FlyApplicationContext(FlyOverlayForm overlay, bool startInTray)
    {
        overlay.FormClosed += (_, _) => ExitThread();
        if (!startInTray) overlay.ShowOverlay();
    }
}
