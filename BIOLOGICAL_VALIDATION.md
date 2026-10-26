# Biological grounding and validation

## Claims supported by the implementation

- Runtime neural data is a bounded MaleCNS v1.0 male Drosophila leg-locomotor subgraph with 1,045 neurons and 17,224 retained directed connections (708,689 contacts). The subgraph is derived from public connectome wiring, attributed and CC BY 4.0; see [`DATA_PROVENANCE.md`](DATA_PROVENANCE.md) and [`data/neural/README.md`](data/neural/README.md).
- The simulation advances a sparse, dimensionless LIF-style network whose edges follow that measured graph. This makes the app connectome-derived, but does not make its neural dynamics or behavior experimentally validated.
- The artwork is user-provided raster image, not a biological model. Six-leg joint-angle timing comes from a recording-derived FlyGym step trace; a simplified 2D joint-chain projection animates the sprite. Wing motion is a blur-style visual representation with a literature-grounded nominal frequency, not measured per-frame wing kinematics.

## Assumptions and limitations

MaleCNS edge contact counts are anatomical measurements, not effective conductances. The app's contact normalization, transmitter-effect signs, LIF parameters, adaptation, baseline activity, sensory drive mapping and descending command drives are assumptions. The extracted graph is a bounded selected subgraph; many inputs and much of the CNS are omitted. Sensory annotations do not identify per-neuron directional tuning. The six-leg source trace is one selected recorded step, not a full behavior recording. Its 2D projection, motor-rate-to-step-frequency mapping, 3–8 s walk / 1–3 s rest bout durations, body translation/heading, cursor-threat behavior, flight policy and wing animation are modeled approximations, not fitted behavior statistics.

The selected graph has leg motor and sensory anatomy but no wing motor/flight circuit. Wing flutter and the slight temporary zoom during escape are visual signals only, not neural or biomechanical flight. No membrane parameters or movement have been fitted to electrophysiology or measured behavioral kinematics. The app is not an entire-brain simulation, validated escape reflex, or complete fly digital twin.

## Validation status

Authored software checks cover the bundled graph counts/motor recruitment, sensor range, cursor response, deterministic replay, gait/wing phase advancement, boundary constraints, configuration round-trip and neural reset. They were not run in the present environment because no .NET SDK/Windows desktop runtime is available. No biological datasets have been compared against output. On Windows, run `dotnet run --project .\tests\DesktopFly.Simulation.Tests\DesktopFly.Simulation.Tests.csproj` and `dotnet run --project .\src\DesktopFly.App\DesktopFly.App.csproj -c Release`; compilation, runtime, and visual behavior remain unverified until then.
