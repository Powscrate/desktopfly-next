# Changelog

## 0.1.0 — 2026-10-26

- Created .NET 8 solution with a WinForms Windows overlay and independent simulation library.
- Added tray pause/resume/reset, interaction toggle, language menu, diagnostics toggle and exit.
- Added a cursor-threat sensor, virtual desktop movement constraints, and two-neuron LIF-to-motor prototype; the earlier procedural fly remains as an asset-load fallback.
- Integrated a four-frame GDI+ sprite atlas baked from the licensed NeuroMechFly v2 69-part model with static FlyGym/MuJoCo neutral-pose transforms; atlas shading and illustrative wing poses are documented as presentation effects, not measured textures/gait.
- Added a non-modal settings/live-diagnostics window, adjustable simulation speed and fly size, cursor-threat toggle, independent neural reset, and French/English localization for the new controls.
- Added deterministic headless assertions, a reproducible closed-loop experiment CSV command, example configuration and synthetic circuit description.
- Documented build, architecture, limitations, provenance, biological assumptions, performance status and release checklist.
- Build and tests not run: .NET SDK and Windows runtime are absent from the implementation environment.
