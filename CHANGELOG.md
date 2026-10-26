# Changelog

## 0.1.0 — 2026-10-26

- Created .NET 8 solution with a WinForms Windows overlay and independent simulation library.
- Added tray pause/resume/reset, interaction toggle, language menu, diagnostics toggle and exit.
- Added a cursor-threat sensor, virtual desktop movement constraints, and a two-neuron LIF-to-motor prototype. An early procedural fly drawing was later removed from the model-rendering path.
- Integrated one static sprite rendered from the licensed NeuroMechFly v2 69-part model using FlyGym/MuJoCo transforms from the official tripod walking snapshot. GDI+ presents it in a per-pixel-alpha window to avoid color-key fringes; the pose and tarsal geometry are source model data, while lighting/material colors are rendering choices. No fabricated wingbeat or hand-drawn anatomy is used.
- Added a non-modal settings/live-diagnostics window, adjustable simulation speed and fly size, cursor-threat toggle, independent neural reset, and French/English localization for the new controls.
- Added deterministic headless assertions, a reproducible closed-loop experiment CSV command, example configuration and synthetic circuit description.
- Documented build, architecture, limitations, provenance, biological assumptions, performance status and release checklist.
- Build and tests not run: .NET SDK and Windows runtime are absent from the implementation environment.
