# Project status — DesktopFly Next 0.1.0

Updated 2026-10-26.

## Summary

The current source has a native Windows overlay, the supplied overhead raster fly, procedural leg/wing animation, and an offline MaleCNS v1.0-derived leg locomotor subgraph. The 1,045-neuron/17,224-edge graph is measured anatomy, while its LIF dynamics, body mapping, gait, cursor response and wing animation remain modeled assumptions. This is still an early prototype, not a validated fly simulation.

## Phase tracker

| Phase | Status | Notes |
| --- | --- | --- |
| 1. Foundation | Partial | Solution/project structure, JSON config validation, rotating log, version and assertion executable. |
| 2. Windows integration | Partially user-verified | Earlier user tests confirmed overlay/tray/settings behavior. Current animation and connectome resource hookup have not been built/run on Windows. Multi-monitor, mixed-DPI, sleep/resume and accessibility remain unverified. |
| 3. Fly appearance | Source implementation complete; Windows verification pending | Runtime uses supplied 56×57 top view; GLB renderer/model assets remain removed. Pale wing pixels are separated, with procedural wing/leg overlays and a small flight zoom. Visual artifacts and alignment need Windows validation. |
| 4. Physical movement | Partial | Planar surface walking with strong traction, persistent turns, screen bounds, and brief escape flight state. No desktop surface geometry, foot-force mechanics, collision geometry or flight physics. |
| 5. Environmental perception | Partial | Local cursor proximity/movement only, user-toggleable. No screen capture. Other modalities absent. |
| 6. Behavior | Partial | Explore, escape, rest software states; seeded surface wandering and cursor response. The policy is modeled, not an identified biological escape pathway. |
| 7. Neural engine | Partial research prototype | Sparse 1 ms LIF-style integration over a fixed 1,045-node, 17,224-edge subgraph. No general network importer, synaptic delays, detailed conductance model, or profiling. |
| 8. Connectome | Bounded subgraph integrated | MaleCNS v1.0 CC BY 4.0 leg-locomotor selection; not the full CNS. Extraction, checksum, credit, source roles and omitted-data limitations documented. No FlyWire CC BY-NC dataset copied. |
| 9. Neural/body integration | Partial, explicitly assumed | DNa02/MDN drive, coarse leg sensory interface, motor-rate-to-gait mapping and transmitter signs are modeled mappings, not validation. Wingbeat is visual only and is not driven by the leg graph. |
| 10. Closed loop | Partial | Cursor signal, network and motor activity are exercised by headless checks/CSV scenario; biological closed-loop equivalence is not claimed. |
| 11. Appearance/animation | Source implementation complete; verification pending | Supplied body image with procedural legs and wing motion; gait and flight imagery are not calibrated to measured kinematics. |
| 12. UI/scientific tools | Partial | Tray controls, non-modal settings/live diagnostics, reset, and English/French strings. No neuron explorer or experiment UI. |

## Build and tests

- Environment: Linux x86_64; no `dotnet`, MSBuild, C# compiler, Mono, or Windows desktop runtime is available.
- Build status: not run for this change. Do not infer validation from earlier builds; new connectome resource, simulation and sprite animation require Windows build verification.
- Automated test status: 9 headless checks are authored but not run here. They check data counts/recruitment, behavior, determinism, bounds and settings; they do not validate biology.
- UI/performance status: current animated sprite, network runtime cost, transparency, wing separation, DPI positioning and visual behavior have not been measured.

## Known limitations / next actions

- On Windows with .NET 8, run `dotnet run --project .\src\DesktopFly.App\DesktopFly.App.csproj -c Release` and `dotnet run --project .\tests\DesktopFly.Simulation.Tests\DesktopFly.Simulation.Tests.csproj`. Inspect sprite artifacts, heading, leg/wing motion, French/settings/tray behavior, and network CPU cost.
- The MaleCNS selection is incomplete; neuron dynamics/signs and sensory/body/behavior mappings are assumptions. No biological validation or movement calibration has occurred.
- The supplied graphic is a small raster; animated limb overlays are stylized. Flight is a brief visual state with no wing neural model or physical lift.
- Continue preserving offline-first/no-telemetry behavior, CC BY 4.0 attribution and license separation from FlyWire-derived CC BY-NC data.
