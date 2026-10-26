# Neural model: MaleCNS leg-circuit subgraph

## Anatomical source and scope

The runtime embeds an offline, bounded locomotor subgraph extracted from the male adult Drosophila CNS connectome MaleCNS v1.0. It contains **1,045 neurons, 17,224 directed connections, and 708,689 retained synapse contacts**: 16 descending neurons, 622 selected ventral-nerve-cord interneurons, 220 leg motor neurons, 153 leg sensory neurons, and 34 ascending neurons. The source and extraction provenance, license, extraction commit, and SHA-256 are recorded in [`DATA_PROVENANCE.md`](DATA_PROVENANCE.md) and [`data/neural/README.md`](data/neural/README.md). The graph is loaded from an embedded resource; runtime network access is not used.

Each input edge is a real retained anatomical connection from the MaleCNS subgraph. It is not the complete connectome: the source retains edges with at least five contacts at synapse confidence 0.5 and selects bounded routes/cell populations. Omitted incoming synapses and source counts are documented in the accompanying report. This data is not a FlyWire connectome and the separate FlyWire-derived CC BY-NC data in the source repository is not included.

## Dynamics implemented here

The network advances in 1 ms steps. Each neuron has a dimensionless leaky integrate-and-fire membrane state (per-step leak multiplier 0.9512294, threshold 1, reset 0), a 2 ms refractory interval, and spike adaptation (increment 0.01; decay multiplier 0.9950125). Excitatory and inhibitory currents decay by 0.8187308 and 0.9048374 per millisecond (approximately 5 ms and 10 ms time constants); rate trace decay is 0.9048374. Baseline current is 0.022 and model synaptic gain is 2.4. These are engineering parameters, not measured physiology. Directed contact counts are normalized by each target's retained incoming magnitude; this prevents interpreting contact count as a measured conductance. Acetylcholine is assigned a positive sign, GABA and glutamate negative signs, and uncertain/modulatory transmitters zero direct current. These signs and all resulting dynamics are model assumptions, not experimentally measured currents or fitted physiology.

The existing cursor-proximity sensor is a local product behavior signal, not an identified retinal neuron. During exploration, a bilateral DNa02 rate-drive interface is used as an assumed locomotion command; during a brief cursor-triggered escape, bilateral MDN drive is used as an assumed reverse command. A coarse six-leg contact/movement signal is mapped to annotated leg sensory populations, although the dataset does not supply neuron-specific tuning or preferred directions. These are modeled interfaces, not added anatomical edges or validated sensory pathways.

Per-leg motor population activity affects the modeled gait rate. A tripod phase pattern, the 2D surface movement, heading/turns, and cursor-escape policy are body/behavior assumptions. The included subgraph contains **leg** motor anatomy only; it does not contain a flight or wingbeat circuit. Wing motion during the brief escape display is a visual animation, not MaleCNS-driven flight physiology.

## Limitations and validation

This is a sparse LIF-style simulation over a measured connectome-derived graph, not a whole-brain simulation or a complete mechanistic reconstruction. Muscles, body geometry, force, joint dynamics, sensory tuning, synaptic delays, many inputs, neuromodulation, and flight remain absent or modeled. The animation has not been calibrated against measured kinematics. The authored tests check data integrity, deterministic simulation, boundary constraints, and causal activity/movement; they are software checks, not biological validation.
