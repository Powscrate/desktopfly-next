# Biological grounding and validation

## Claims supported by this prototype

- The current app displays user-provided raster artwork of a fly; it does not load the NeuroMechFly GLB at runtime. The sprite silhouette is illustrative and is not a substitute for biological anatomical validation. The image has no articulated leg motion.
- A leaky integrate-and-fire abstraction is used for two computational units.
- A stimulus variable is integrated to threshold, producing discrete spikes; a second spiking unit receives excitatory input and affects simulated movement.

## Explicitly not established

There is no evidence in this code or its bundled data for an identified Drosophila neuron, synapse, visual pathway, descending neuron, muscle, or leg-specific motor circuit. “Visual-threat” and “escape-command” are functional labels for synthetic software units. Their connection, weights, timing, sensory transform, behavior labels and movement impulse are engineering approximations. This is not a FlyWire-derived circuit, a validated escape reflex model, nor an entire-brain simulation.

## Validation status

The authored headless checks cover threshold/reset/refractory behavior, sensor range and proximity dependence, deterministic replay, boundary constraints, configuration round-trip and one causal stimulus-to-motor path. They have not been run in the current environment because no .NET SDK is installed. No electrophysiology or behavioral dataset has been compared against model output. A future biological integration must identify source, version, license, exact neuron/synapse metadata, unsupported fields, and validation metrics before claiming correspondence.
