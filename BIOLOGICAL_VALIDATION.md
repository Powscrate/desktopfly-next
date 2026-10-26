# Biological grounding and validation

## Claims supported by this prototype

- The app now bundles a scan-derived NeuroMechFly v2 geometry asset for adult female *Drosophila melanogaster*; upstream documentation identifies the biomechanical model as based on a micro-CT scan of a real adult female fly. The current GDI+ renderer still shows its procedural 2D approximation; the GLB is not yet rendered by the app.
- A leaky integrate-and-fire abstraction is used for two computational units.
- A stimulus variable is integrated to threshold, producing discrete spikes; a second spiking unit receives excitatory input and affects simulated movement.

## Explicitly not established

There is no evidence in this code or its bundled data for an identified Drosophila neuron, synapse, visual pathway, descending neuron, muscle, or leg-specific motor circuit. “Visual-threat” and “escape-command” are functional labels for synthetic software units. Their connection, weights, timing, sensory transform, behavior labels and movement impulse are engineering approximations. This is not a FlyWire-derived circuit, a validated escape reflex model, nor an entire-brain simulation.

## Validation status

The authored headless checks cover threshold/reset/refractory behavior, sensor range and proximity dependence, deterministic replay, boundary constraints, configuration round-trip and one causal stimulus-to-motor path. They have not been run in the current environment because no .NET SDK is installed. No electrophysiology or behavioral dataset has been compared against model output. A future biological integration must identify source, version, license, exact neuron/synapse metadata, unsupported fields, and validation metrics before claiming correspondence.
