namespace DesktopFly.App;

internal sealed class Strings(string language)
{
    private readonly bool _fr = language == "fr";
    public string AppName => "DesktopFly Next";
    public string ShowOverlay => _fr ? "Afficher la mouche" : "Show fly overlay";
    public string HideOverlay => _fr ? "Masquer la mouche" : "Hide fly overlay";
    public string Pause => _fr ? "Mettre en pause" : "Pause";
    public string Resume => _fr ? "Reprendre" : "Resume";
    public string Reset => _fr ? "Réinitialiser la mouche" : "Reset fly";
    public string Interaction => _fr ? "Mode interaction (cliquer / déplacer)" : "Interaction mode (click / drag)";
    public string Diagnostics => _fr ? "Afficher les diagnostics" : "Show diagnostics";
    public string Language => _fr ? "Langue / Language" : "Language / Langue";
    public string Exit => _fr ? "Quitter" : "Exit";
    public string Paused => _fr ? "En pause" : "Paused";
    public string Running => _fr ? "En activité" : "Running";
    public string Escape => _fr ? "Fuite" : "Escape";
    public string Explore => _fr ? "Exploration" : "Explore";
    public string Rest => _fr ? "Repos" : "Rest";
    public string Threat => _fr ? "menace" : "threat";
    public string NeuralSpikes => _fr ? "pics neuronaux" : "neural spikes";
}
