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
    public string Settings => _fr ? "Paramètres et diagnostics" : "Settings and diagnostics";
    public string SimulationGroup => _fr ? "Simulation" : "Simulation";
    public string BehaviorGroup => _fr ? "Perception et comportement" : "Perception and behavior";
    public string DiagnosticsGroup => _fr ? "Activité en direct" : "Live diagnostics";
    public string PauseSimulation => _fr ? "Mettre la simulation en pause" : "Pause simulation";
    public string SimulationSpeed => _fr ? "Vitesse de simulation" : "Simulation speed";
    public string FlySize => _fr ? "Taille de la mouche" : "Fly size";
    public string CursorAvoidance => _fr ? "Éviter le curseur" : "Avoid cursor";
    public string ShowDiagnosticsCheck => _fr ? "Afficher les diagnostics sur le bureau" : "Show desktop diagnostics";
    public string StateLabel => _fr ? "État" : "State";
    public string ThreatLabel => _fr ? "Menace" : "Threat";
    public string PositionLabel => _fr ? "Position" : "Position";
    public string SensoryCount => _fr ? "Pics sensoriels" : "Sensory spikes";
    public string MotorCount => _fr ? "Pics moteurs" : "Motor spikes";
    public string NeuralUnits => _fr ? "Unités neuronales" : "Neurons";
    public string ConnectionCount => _fr ? "Connexions" : "Connections";
    public string ResetSimulation => _fr ? "Réinitialiser la simulation" : "Reset simulation";
    public string ResetNeural => _fr ? "Réinitialiser l'activité neuronale" : "Reset neural activity";
    public string Close => _fr ? "Fermer" : "Close";
    public string Exit => _fr ? "Quitter" : "Exit";
    public string Paused => _fr ? "En pause" : "Paused";
    public string Running => _fr ? "En activité" : "Running";
    public string Escape => _fr ? "Fuite" : "Escape";
    public string Explore => _fr ? "Exploration" : "Explore";
    public string Rest => _fr ? "Repos" : "Rest";
    public string Threat => _fr ? "menace" : "threat";
    public string NeuralSpikes => _fr ? "pics neuronaux" : "neural spikes";
}
