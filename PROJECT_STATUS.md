# Project status — DesktopFly Next 0.1.0

Updated 2026-10-26.

## Summary

A first source-level vertical slice exists: native Windows overlay code, a dependency-free headless simulation core, a synthetic LIF-to-motor closed loop, basic menu localization (English/French), authored checks, an experiment runner, and build/design documentation.

## Phase tracker

| Phase | Status | Notes |
| --- | --- | --- |
| 1. Foundation | Partial | Solution/project structure, JSON config validation, rotating log, version and assertion executable; user has built the core/app on Windows .NET 8.0.427. |
| 2. Windows integration | Partially user-verified | User reports the overlay appeared and avoided the cursor; the tray controls/settings previously tested work. Multi-monitor, mixed-DPI, sleep/resume and accessibility remain unverified. |
| 3. Fly appearance | Prototype / asset sourced | The current renderer remains procedural 2D. A micro-CT-derived NeuroMechFly v2 GLB (69 named mesh parts, Apache-2.0, provenance documented) is bundled but not yet loaded or rendered. |
| 4. Physical movement | Partial | Velocity, heading, fixed-ish update, virtual-screen clamp/reflection and escape impulse. No contact surfaces, collision geometry or true flight. |
| 5. Environmental perception | Partial | Local cursor proximity/movement only, now user-toggleable. No screen capture. Other requested modalities absent. |
| 6. Behavior | Partial | Explore, escape, rest software states; simple seeded wandering. No grooming, hunger/fatigue or extensible behavior tree. |
| 7. Neural engine | Minimal proof-of-concept | LIF and two-cell circuit; no general sparse network, delays, spike raster, adaptation or large-circuit import. |
| 8. Connectome | Not started | No neural connectome dataset bundled. The separate 3D mesh is biological geometry, not neural connectivity. |
| 9. Neural motor integration | Minimal | Motor spikes add an escape velocity impulse; explicit approximation, not a documented biological mapping. |
| 10. Closed loop | Minimal | Cursor-driven threat experiment is encoded and exportable through a CLI runner. |
| 11. Advanced realism | Not started | |
| 12. UI/scientific tools | Partial | Tray controls plus a new non-modal settings/live-diagnostics window with speed, fly size, cursor-avoidance and overlay-diagnostics controls, neural reset, and basic activity counts. Neuron explorer and experiment UI absent. App strings offer English/French. |

## Files/components added

See `src/`, `tests/`, `data/`, `experiments/`, `docs`-level Markdown files and `scripts/publish-windows.ps1`. No dependencies beyond the .NET SDK/BCL are declared.

## Build and tests

- Environment inspected: Linux x86_64; no `dotnet`, `msbuild`, `csc`, Mono, or PowerShell executable was available.
- Build status: user reports the prior Windows app revision launched successfully on .NET SDK 8.0.427 after the GDI+ coordinate and DPI fixes. The settings-window revision is new and still needs a build/run.
- Automated test status: user reports all 7 checks in the previous revision passed. Two new headless checks (disabled threat sensor and neural reset preserving position) bring the suite to 9; they are authored but have not yet been run.
- Windows overlay smoke/UI test status: user confirms the fly appeared, avoided the cursor, and reports existing tray controls work. The newly added settings/diagnostics window has not yet been compiled or exercised on Windows.
- Performance status: no measurements. See `PERFORMANCE_REPORT.md`.

## Known issues / limitations

- Need compile and fix any compiler diagnostics on Windows/.NET 8.
- Need verify transparency key, click-through, tray interactions, screen-change handling, mixed DPI, wake/resume, multi-monitor negative coordinates, and shutdown on Windows 10/11.
- Tray/overlay strings are localized; startup error dialog and log messages remain English-only.
- Render/update work is coupled to a WinForms timer; there is no independent neural clock or resource profiling.
- Synthetic circuit metadata JSON is descriptive, while runtime parameters are code-defined.
- Not a complete definition-of-done release: no biological circuit integration, broader sensory suite, general network importer, or Windows packaging validation.

## Next concrete action

On Windows with .NET 8 SDK, pull the latest branch, build the solution, run the expanded headless checks, then launch the app and verify the new non-modal settings window (speed/size, pause, cursor sensor toggle, neural reset, live counts, French localization) while confirming the fly continues in the background. Fix any compiler/runtime/layout issues before advancing to a neuron explorer or broader sensory/behavior models.
