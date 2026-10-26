# Project status — DesktopFly Next 0.1.0

Updated 2026-10-26.

## Summary

A first source-level vertical slice exists: native Windows overlay code, a dependency-free headless simulation core, a synthetic LIF-to-motor closed loop, basic menu localization (English/French), authored checks, an experiment runner, and build/design documentation.

## Phase tracker

| Phase | Status | Notes |
| --- | --- | --- |
| 1. Foundation | Partial | Solution/project structure, JSON config validation, rotating log, version and assertion executable are present; build/run not verified. |
| 2. Windows integration | Implemented in source; unverified | Transparent WinForms overlay, tray commands, click-through/interaction toggle, virtual-desktop bounds, local cursor stimulus. No Windows run or UI tests. |
| 3. Fly appearance | Prototype | Procedural 2D GDI+ fly with body, eyes, antennae, six leg strokes, wing membranes/veins. No 3D model or validated anatomy. |
| 4. Physical movement | Partial | Velocity, heading, fixed-ish update, virtual-screen clamp/reflection and escape impulse. No contact surfaces, collision geometry or true flight. |
| 5. Environmental perception | Partial | Local cursor proximity/movement only. No screen capture. Other requested modalities absent. |
| 6. Behavior | Partial | Explore, escape, rest software states; simple seeded wandering. No grooming, hunger/fatigue or extensible behavior tree. |
| 7. Neural engine | Minimal proof-of-concept | LIF and two-cell circuit; no general sparse network, delays, spike raster, adaptation or large-circuit import. |
| 8. Connectome | Not started | No third-party dataset bundled. |
| 9. Neural motor integration | Minimal | Motor spikes add an escape velocity impulse; explicit approximation, not a documented biological mapping. |
| 10. Closed loop | Minimal | Cursor-driven threat experiment is encoded and exportable through a CLI runner. |
| 11. Advanced realism | Not started | |
| 12. UI/scientific tools | Partial | Tray control and on-overlay status only; no settings window or neuron explorer. App strings offer English/French. |

## Files/components added

See `src/`, `tests/`, `data/`, `experiments/`, `docs`-level Markdown files and `scripts/publish-windows.ps1`. No dependencies beyond the .NET SDK/BCL are declared.

## Build and tests

- Environment inspected: Linux x86_64; no `dotnet`, `msbuild`, `csc`, Mono, or PowerShell executable was available.
- Build status: user reports the Windows app launched successfully on .NET SDK 8.0.427 after the GDI+ coordinate and DPI fixes.
- Automated test status: user reports all 7 headless checks passed.
- Windows overlay smoke/UI test status: user confirms the fly appeared and reacted to/avoided the mouse. Tray pause/resume, French menu selection, click-through/drag modes, and exit still need a smoke check.
- Performance status: no measurements. See `PERFORMANCE_REPORT.md`.

## Known issues / limitations

- Need compile and fix any compiler diagnostics on Windows/.NET 8.
- Need verify transparency key, click-through, tray interactions, screen-change handling, mixed DPI, wake/resume, multi-monitor negative coordinates, and shutdown on Windows 10/11.
- Tray/overlay strings are localized; startup error dialog and log messages remain English-only.
- Render/update work is coupled to a WinForms timer; there is no independent neural clock or resource profiling.
- Synthetic circuit metadata JSON is descriptive, while runtime parameters are code-defined.
- Not a complete definition-of-done release: no biological circuit integration, broader sensory suite, general network importer, or Windows packaging validation.

## Next concrete action

On a Windows machine with .NET 8 SDK, run `dotnet restore DesktopFlyNext.sln`, `dotnet build DesktopFlyNext.sln -c Release`, and the headless checks. Fix all compiler/runtime failures, then exercise click-through and display/DPI behaviors before claiming a Windows build. Implement separate neural/behavior diagnostics and expand unit/integration tests.
