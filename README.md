# DesktopFly Next

DesktopFly Next is an offline-first Windows desktop-fly simulation, currently at an early **0.1.0 engineering prototype** milestone. It combines a native transparent WinForms overlay with a separate, headless .NET simulation core. The overlay displays the fly graphic supplied by the user, with its dark background removed for transparent desktop placement. The GLB renderer is no longer part of the app's runtime path.

**French UI is included.** Right-click the notification-area icon → “Language / Langue” → “Français”. Language is persisted locally. The initial language is English; the choice is made in-app.

## Current implementation

- Windows 10/11, x64 native WinForms overlay: borderless, always-on-top, virtual-desktop bounds, transparency-key background, click-through by default, optional grab/drag interaction, pause/resume/reset/show-hide/exit from a tray menu, optional start-in-tray configuration, English/French UI strings, local configuration and rotating local logs.
- A non-modal settings and diagnostics window with live state/threat/position/spike counts; adjustable simulation speed and fly size; pause, cursor-avoidance and desktop-diagnostics toggles; independent neural reset.
- A deterministic, headless simulation core: cursor-proximity/motion threat signal → two LIF neurons (sensory afferent and synthetic escape-command unit) → motor spike → physically constrained movement. This is an explicit, causal neural-to-motor path; the tiny circuit is synthetic, not a FlyWire model.
- Basic exploratory/escape/rest states displayed with the user-provided fly sprite, cut out from its original dark background and shown in a per-pixel-alpha window. The image is scaled and rotated with the simulation. The GLB is no longer embedded in the app; if the sprite cannot load, the fly is hidden and an error is logged.
- Dependency-free console checks and a reproducible closed-loop CSV experiment runner.

## Build and run (Windows)

Install the .NET 8 SDK, then follow [BUILD.md](BUILD.md). Quick start:

```powershell
dotnet run --project src/DesktopFly.App/DesktopFly.App.csproj
```

The app lives in the notification area. Right-click its icon for controls and open **Settings and diagnostics** (or **Paramètres et diagnostics**) to adjust simulation speed (25–300%), fly size, pause state and cursor-threat input, reset neural state, and view live state/threat/position/spike counts. Settings save locally. By default, the overlay is click-through; switch on interaction mode to drag the fly. Set `StartInTray` to `true` in `%LOCALAPPDATA%\DesktopFlyNext\config.json` to launch with the overlay hidden; choose “Show fly overlay” from the tray menu to reveal it. No administrator rights or network access are required. User settings/logs are in `%LOCALAPPDATA%\DesktopFlyNext\`.

## Tests and experiment

```powershell
dotnet run --project tests/DesktopFly.Simulation.Tests/DesktopFly.Simulation.Tests.csproj
dotnet run --project tests/DesktopFly.Simulation.Tests/DesktopFly.Simulation.Tests.csproj -- --experiment experiments/closed-loop-export.csv
```

The first command runs deterministic headless assertions without external test packages. The second writes a CSV of the simulated cursor-approach experiment. A scenario description is in [`experiments/closed-loop-cursor-threat.json`](experiments/closed-loop-cursor-threat.json).

## Honest scope and limitations

This is a first vertical slice, not the full roadmap in the original product brief. Windows compilation/UI interaction have **not been executed in this Linux environment** (no .NET SDK or Windows desktop runtime is installed). The simulation tests are authored but likewise unexecuted here. The replacement user-supplied sprite resource has not yet been compiled or visually tested on Windows; no Windows binary or performance claim is supplied. See [`PROJECT_STATUS.md`](PROJECT_STATUS.md) for exact phase status and next steps.

The supplied sprite translates and turns with the simplified desktop simulation, but its legs are not animated; it can look like it is gliding rather than walking. No rigged or measured biomechanical gait is implemented. Also not implemented: UV texture maps or photorealistic surface materials; advanced neuron explorer/spike raster tooling; flight physics; obstacle/window geometry; formal connectome importer or scientific neural dataset; delayed/sparse general-purpose network engine; broader stimulus suite; Windows UI automation; installer; and measured performance report. The overlay samples only local cursor position and derived movement for the fly's threat stimulus; it does not capture the screen, inspect window content, record keyboard input, or send telemetry.

## Documentation

- [BUILD.md](BUILD.md) · [ARCHITECTURE.md](ARCHITECTURE.md) · [NEURAL_MODEL.md](NEURAL_MODEL.md)
- [BIOLOGICAL_VALIDATION.md](BIOLOGICAL_VALIDATION.md) · [DATA_PROVENANCE.md](DATA_PROVENANCE.md)
- [PROJECT_STATUS.md](PROJECT_STATUS.md) · [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
- [CHANGELOG.md](CHANGELOG.md) · [RELEASE_CHECKLIST.md](RELEASE_CHECKLIST.md) · [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)
