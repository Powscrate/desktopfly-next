# Architecture

## Current data flow

```text
Windows cursor position + derived movement (local, coarse)
     ↓ StimulusSnapshot
CursorThreatSensor (normalized 0..1 threat approximation)
     ↓ behavioral command + assumed contact feedback
Embedded MaleCNS v1.0 leg-locomotor subgraph (1,045 neurons / 17,224 edges)
     ↓ modeled motor-population activity
Tripod-phase body/gait template → 2D surface movement and turns
     ↓ read-only position, heading, gait and flight phases
WinForms transparent overlay → separate per-pixel-alpha supplied-sprite window
     ↔ non-modal settings/diagnostics window (local controls, live counters)
     ↓ next tick samples cursor again
```

The neural/simulation project has no rendering or Windows dependency. `FlySimulation.Step` is deterministic for identical initial state, seed, and stimuli. A 1 ms sparse network update is accumulated under the app's timer tick. The network graph is measured MaleCNS wiring; LIF dynamics, signs, drive mapping, body and gait behavior are model assumptions. Cursor sensing occurs locally; no screen capture or application content is read.

## Projects and data

- `src/DesktopFly.Simulation`: vector math, cursor-threat approximation, `MaleCnsLocomotionCircuit`, headless behavior/locomotion, JSON configuration, and local diagnostic logger.
- `src/DesktopFly.App`: Windows-only WinForms overlay, tray/settings, native extended-window interop, cursor integration, English/French strings, and the user-provided transparent sprite with animated leg/wing drawing. The previous GLB renderer/model files have been removed.
- `tests/DesktopFly.Simulation.Tests`: no-package executable checks plus reproducible CSV experiment mode.
- `data/neural/`: embedded MaleCNS-derived locomotor subgraph and extraction report. Provenance, CC BY 4.0 attribution, selection, hash, and scientific limits are recorded alongside the data and in `DATA_PROVENANCE.md`.

## Boundaries and recovery

The simulation has no dependency on WinForms, GDI+, P/Invoke, or display APIs. UI exceptions and startup failures are written to a size-rotated local log; simulation tick failures are caught and logged by the overlay loop. Configuration updates write a temporary file before replacement. Unsupported configuration schema versions fail explicitly. The application requests `asInvoker`; it never requires elevation. The embedded neural data means startup and simulation require no network access.

## Window behavior

The main window is borderless, topmost, tool-window/no-activate, transparent-keyed and click-through unless the user turns on interaction mode. The sprite is drawn in a separate per-pixel-alpha window. Win32 extended styles are adjusted on the existing window when toggled. The main window covers the virtual screen rectangle, including negative monitor origins. The animated sprite resource has not yet been interactively tested across Windows versions, DPI configurations, sleep/resume, or mixed-DPI monitors; these remain release gates.

## Approximation boundaries

The displayed fly is the user-provided 56×57 raster cutout, not a 3D model or articulated anatomical rig. Light wing pixels are separated from the body and redrawn; simple leg strokes are procedurally animated. Heading alignment, a tripod phase template, surface traction, 2D movement, escape policy, wing animation and modest flight zoom are engineering/body-model choices. The included connectome subgraph contains leg circuitry only; wing animation is not driven by it. Neither the displayed gait nor flight has been validated against experimental motion. Real surface geometry, obstacle contact, flight physics and a complete neural model are not implemented.
