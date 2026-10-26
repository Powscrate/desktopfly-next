# DesktopFly Next

DesktopFly Next is an offline-first Windows desktop-fly simulation, currently an early **0.1.0 engineering prototype**. It combines a native transparent WinForms overlay with a headless .NET simulation core. The overlay uses the user-provided top-view fly graphic; its dark background is removed for transparent desktop placement. The GLB renderer/model is not part of the app.

**French UI is included.** Right-click the notification-area icon → “Language / Langue” → “Français”. Language is persisted locally. The initial language is English; the choice is made in-app.

## Current implementation

- Windows 10/11, x64 native WinForms overlay: borderless, always-on-top, virtual-desktop bounds, transparent background, click-through by default, optional grab/drag interaction, tray controls, local settings/logs, and English/French UI.
- A non-modal settings and diagnostics window with live state/threat/position/spike counts; adjustable simulation speed and fly size; pause, cursor-avoidance and desktop-diagnostics toggles; independent neural reset.
- A headless simulation embedding a bounded **MaleCNS v1.0 male Drosophila leg-locomotor subgraph**: 1,045 neurons and 17,224 measured directed connections. A sparse LIF-style model with explicit parameter assumptions drives a six-leg body interface. Source, license, extraction, and checksum are documented in [`DATA_PROVENANCE.md`](DATA_PROVENANCE.md) and [`data/neural/README.md`](data/neural/README.md).
- The supplied overhead raster remains the body artwork. Six-leg joint-angle animation replays a walking-recording-derived step trace from [NeLy-EPFL/FlyGym 2.1.1 (pinned source commit)](https://github.com/NeLy-EPFL/flygym/tree/a63dc91a7ce432452e6521c2f3dcc1a7c2c7c6bc), projected into simple 2D limbs; its gait timing is modulated by connectome motor activity through an explicit, unvalidated interface. Provenance and the unverified independent license status of the underlying recording are documented in [`data/locomotion/README.md`](data/locomotion/README.md). Walking and resting bouts, body movement and the escape policy remain modeled. Flight shows a soft high-frequency wingbeat fan and a modest zoom; the nominal ~200 Hz beat is literature-grounded, not driven by the leg connectome. No GLB or runtime network fetch.
- Offline runtime and no third-party NuGet packages. No screen capture, window-content inspection, keyboard logging, telemetry, or network requests.

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

The first command runs deterministic headless assertions without external test packages, including graph counts/motor recruitment, recorded FlyGym gait data, surface gait, walk/rest bouts, flight animation phase, deterministic replay, bounds, configuration, and cursor response. The second writes a CSV of a reproducible cursor-approach experiment. A scenario description is in [`experiments/closed-loop-cursor-threat.json`](experiments/closed-loop-cursor-threat.json).

## Honest scope and limitations

The MaleCNS file is a bounded, contact-count leg-circuit subgraph rather than a full nervous system. Contact counts are not conductances; transmitter signs, membrane dynamics, sensory tuning, descending drive mappings, 2D limb projection, body mechanics and walk/rest durations are assumptions. Leg motion now replays a single recorded six-leg step cycle, not a complete behavior sequence; wing motion is a high-speed visual approximation, not a neural flight model. This remains an unvalidated desktop-fly simulation, not a complete digital twin or a real animal. See [`NEURAL_MODEL.md`](NEURAL_MODEL.md), [`BIOLOGICAL_VALIDATION.md`](BIOLOGICAL_VALIDATION.md), and [`PROJECT_STATUS.md`](PROJECT_STATUS.md).

Windows compilation and UI interaction have **not been executed in this Linux environment** (no .NET SDK or Windows desktop runtime is installed). The authored headless tests likewise remain unexecuted here; no Windows binary or performance claim is supplied. The sprite's wing extraction/leg overlay and per-pixel rendering need Windows visual validation. The overlay samples only local cursor position and derived movement for a coarse threat signal.

## Documentation

- [BUILD.md](BUILD.md) · [ARCHITECTURE.md](ARCHITECTURE.md) · [NEURAL_MODEL.md](NEURAL_MODEL.md)
- [BIOLOGICAL_VALIDATION.md](BIOLOGICAL_VALIDATION.md) · [DATA_PROVENANCE.md](DATA_PROVENANCE.md)
- [PROJECT_STATUS.md](PROJECT_STATUS.md) · [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
- [CHANGELOG.md](CHANGELOG.md) · [RELEASE_CHECKLIST.md](RELEASE_CHECKLIST.md) · [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)
