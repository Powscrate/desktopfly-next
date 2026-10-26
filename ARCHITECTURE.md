# Architecture

## Current data flow

```text
Windows cursor position + derived movement (local, coarse)
     ↓ StimulusSnapshot
CursorThreatSensor (normalized 0..1 threat approximation)
     ↓ current drive
LIF visual-threat unit → immediate excitatory synthetic synapse → LIF escape-command unit
     ↓ motor spike events
FlySimulation (wander/escape state, velocity, heading, virtual-screen constraints)
     ↓ read-only position/state
WinForms transparent overlay (static scan-derived 3D sprite; separate per-pixel-alpha window)
     ↔ non-modal settings/diagnostics window (local controls, live counters)
     ↓ next tick samples cursor again
```

The neural and simulation projects have no rendering or Windows dependencies. `FlySimulation.Step` is deterministic for identical initial state, seed, and stimuli. The Windows application integrates it on a UI timer and only renders the resulting state. Cursor sensing occurs in the app boundary; no screen capture or application content is read.

## Projects

- `src/DesktopFly.Simulation`: cross-platform vector math, environment signal contract, cursor-threat approximation, LIF neuron, synthetic two-cell circuit, headless behavior/locomotion, JSON configuration and local diagnostic logger.
- `src/DesktopFly.App`: Windows-only WinForms overlay, tray controls, non-modal settings/live diagnostics window, native extended-window-style interop, embedded FlyGym-derived static sprite, per-pixel-alpha sprite window; no substitute drawing, local cursor integration, English/French strings.
- `tests/DesktopFly.Simulation.Tests`: no-package executable checks plus reproducible CSV experiment mode.
- `data/`: example configuration and human-readable description of the synthetic circuit. The runtime circuit constants currently live in `SyntheticEscapeCircuit`; the JSON is descriptive, not loaded as a network model.

## Boundaries and recovery

The simulation has no dependency on WinForms, GDI+, P/Invoke, or display APIs. UI exceptions and startup failures are written to a size-rotated local log; simulation tick failures are caught and logged by the overlay loop. Configuration updates write a temporary file before replacement. Unsupported configuration schema versions fail explicitly. The application requests `asInvoker`; it never requires elevation.

## Window behavior

The window is borderless, topmost, tool-window/no-activate, transparent-keyed and click-through unless the user turns on interaction mode. Win32 extended styles are adjusted on the existing window when toggled. The window covers the virtual screen rectangle, including negative monitor origins. This implementation has not yet been interactively tested across Windows versions, DPI configurations, sleep/resume, or mixed-DPI monitors; those remain release gates.

## Approximation boundaries

The displayed anatomy is the licensed, 69-part FlyGym mesh assembled in the official tripod walking snapshot; its six legs and tarsal segments come from the source geometry, not hand-drawn stand-ins. It is rasterized to one static sprite at build time; the GLB is not loaded or rasterized live, and there is no articulated gait or wingbeat. Lighting, surface colors, and the soft shadow are presentation choices; the model has no UV texture maps. The on-screen locomotion and neural circuit remain simplified synthetic behavior, not a measured biological simulation or rigid-body flight. Real desktop obstacles, full sensory modalities, generalized neural-network import, and detailed diagnostic UI are not implemented.
