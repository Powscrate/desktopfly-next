# Project status — DesktopFly Next 0.1.0

Updated 2026-10-26.

## Summary

A first source-level vertical slice exists: native Windows overlay code, a dependency-free headless simulation core, a synthetic LIF-to-motor closed loop, basic menu localization (English/French), authored checks, an experiment runner, and build/design documentation.

## Phase tracker

| Phase | Status | Notes |
| --- | --- | --- |
| 1. Foundation | Partial | Solution/project structure, JSON config validation, rotating log, version and assertion executable; user has built the core/app on Windows .NET 8.0.427. |
| 2. Windows integration | Partially user-verified | User reports the overlay appeared and avoided the cursor; the tray controls/settings previously tested work. Multi-monitor, mixed-DPI, sleep/resume and accessibility remain unverified. |
| 3. Fly appearance | User-provided sprite integrated; Windows verification pending | Runtime display uses the supplied top-view fly graphic as a transparent sprite. The GLB renderer and related model assets have been removed. The image scales/rotates as the simulation moves, but its legs do not animate. Windows build and visual verification are pending. |
| 4. Physical movement | Partial | Velocity, heading, fixed-ish update, virtual-screen clamp/reflection and escape impulse. No contact surfaces, collision geometry or true flight. |
| 5. Environmental perception | Partial | Local cursor proximity/movement only, now user-toggleable. No screen capture. Other requested modalities absent. |
| 6. Behavior | Partial | Explore, escape, rest software states; simple seeded wandering. No grooming, hunger/fatigue or extensible behavior tree. |
| 7. Neural engine | Minimal proof-of-concept | LIF and two-cell circuit; no general sparse network, delays, spike raster, adaptation or large-circuit import. |
| 8. Connectome | Not started | No neural connectome dataset or biological neural model is bundled. |
| 9. Neural motor integration | Minimal | Motor spikes add an escape velocity impulse; explicit approximation, not a documented biological mapping. |
| 10. Closed loop | Minimal | Cursor-driven threat experiment is encoded and exportable through a CLI runner. |
| 11. Advanced realism | Partial | User-provided raster fly artwork with a transparent cutout. No GLB-based renderer is in the runtime. It is a static sprite and has no animated or validated walking gait; Windows build and visual verification are pending. |
| 12. UI/scientific tools | Partial | Tray controls plus a new non-modal settings/live-diagnostics window with speed, fly size, cursor-avoidance and overlay-diagnostics controls, neural reset, and basic activity counts. Neuron explorer and experiment UI absent. App strings offer English/French. |

## Files/components added

See `src/`, `tests/`, `data/`, `experiments/`, `docs`-level Markdown files and `scripts/publish-windows.ps1`. No dependencies beyond the .NET SDK/BCL are declared.

## Build and tests

- Environment inspected: Linux x86_64; no `dotnet`, `msbuild`, `csc`, Mono, or PowerShell executable was available.
- Build status: the user previously confirmed Windows .NET 8.0.427 builds and app operation on earlier revisions. The new user-provided sprite resource hookup has not yet been compiled or run on Windows; this Linux environment has no .NET SDK.
- Automated test status: user previously confirmed the 7-check suite passed. Two additional headless checks were authored afterward; the current 9-check suite has not been executed in this environment.
- Windows overlay smoke/UI test status: earlier overlay and settings behavior were user-confirmed. The replacement transparent sprite resource and no-substitute failure behavior remain unverified on Windows.
- Performance status: no measurements. See `PERFORMANCE_REPORT.md`.

## Known issues / limitations

- Need compile and fix any compiler diagnostics on Windows/.NET 8.
- Need verify transparency key, click-through, tray interactions, screen-change handling, mixed DPI, wake/resume, multi-monitor negative coordinates, and shutdown on Windows 10/11.
- Tray/overlay strings are localized; startup error dialog and log messages remain English-only.
- Render/update work is coupled to a WinForms timer; there is no independent neural clock or resource profiling. The user-provided sprite is integrated in source but untested; no articulated rig, leg cycle, or measured gait is provided.
- Synthetic circuit metadata JSON is descriptive, while runtime parameters are code-defined.
- Not a complete definition-of-done release: no biological circuit integration, broader sensory suite, general network importer, or Windows packaging validation.

## Next concrete action

On Windows with .NET 8 SDK, build the solution and run all 9 headless checks. Launch the app and compare the cutout sprite against the user-provided graphic: validate background removal, natural size, crisp/soft edges, click-through, and that it moves/turns cleanly. Verify click-through, DPI positioning, French/settings/tray interactions, shutdown, and rendering performance. The sprite has no articulated leg motion, so do not claim a walking gait. Fix compile, transparency, clipping, or appearance issues before broader feature work.
