# Changelog

## Unreleased

- Replaced the two-cell synthetic LIF demonstration with an offline embedded MaleCNS v1.0 leg-locomotor subgraph (1,045 neurons / 17,224 measured connections), with CC BY 4.0 attribution and extraction checksums.
- Added an explicit sparse LIF-style model over the connectome graph and documented the assumed dynamics, transmitter signs, sensory mapping, descending commands, and gait/flight boundary.
- Added surface-walking behavior with animated six-leg strokes, alternating modeled walk/rest bouts, flight wing motion and a modest temporary zoom using the supplied overhead sprite. Procedural gait/wing movement and bout timing are illustrative, not experimentally calibrated.
- Added headless assertions for MaleCNS graph integrity/motor recruitment, gait/flight phase, and surface-constrained behavior. Windows build and tests remain unverified in this Linux environment.
- Kept the prior GLB renderer/model assets removed; the app remains offline-first with no FlyWire CC BY-NC data.

## 0.1.0 — 2026-10-26 (historical engineering milestone)

- Created .NET 8 solution with a WinForms Windows overlay and independent simulation library.
- Added tray pause/resume/reset, interaction toggle, language menu, diagnostics toggle and exit.
- Added a cursor-threat sensor, virtual desktop movement constraints, and an initial two-neuron LIF-to-motor prototype. This prototype is superseded by the connectome-derived locomotor implementation above.
- Added settings/live diagnostics, adjustable simulation speed and fly size, cursor-threat toggle, neural reset, and French/English localization.
- Added deterministic headless checks, a reproducible closed-loop experiment CSV command, example configuration and build/design documentation.
- Build and tests were not run in the original Linux implementation environment because the .NET SDK and Windows runtime were unavailable.
