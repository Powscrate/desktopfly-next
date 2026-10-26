# Bundled MaleCNS locomotor data

## Attribution and license

The data is derived from **MaleCNS v1.0**, a public male *Drosophila melanogaster* connectome. Attribute the MaleCNS collaboration: **FlyEM at HHMI Janelia, University of Cambridge, MRC Laboratory of Molecular Biology, and Google Research**. The source and derived data are licensed under [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/).

The bounded graph and report were sourced from Denis Shiryaev's [`desktop-fly`](https://github.com/DenisSergeevitch/desktop-fly) repository at commit [`32b00011e83c3dc85fa3ea0b3934155b04f1635d`](https://github.com/DenisSergeevitch/desktop-fly/tree/32b00011e83c3dc85fa3ea0b3934155b04f1635d) (released 2026-09-05). The source repository's `data/DATA_LICENSE.md` identifies its MaleCNS subgraph/report separately as CC BY 4.0. Its other bundled files derived from FlyWire v783 (CC BY-NC 4.0) are not used here.

## Files and integrity

- `malecns-locomotor-v1.json` — graph, cell labels, retained edges, original contact counts and embedded source/extraction provenance. SHA-256: `8f76d94034dcf802453e3a0a8ed5342d122e57d37e2bb5ea28da66de0856f5d6`.
- `malecns-locomotor-report-v1.json` — extraction counts, output coverage and retained-input summaries.

The graph contains 1,045 cells and 17,224 retained directed connections (708,689 contacts): 16 descending neurons, 622 selected VNC interneurons, 220 leg motor neurons, 153 leg sensory neurons, and 34 ascending neurons. Retained edges come from MaleCNS v1.0 connections filtered at synapse confidence ≥0.5 and a threshold of ≥5 contacts. The subgraph is a bounded selection, not the full CNS.

## Interpretation limits

Measured anatomy/contact counts do not specify effective synaptic conductance, transmitter effect, single-neuron sensory tuning, muscle forces, or gait. The runtime's transmitter signs, normalized edge weights, one-millisecond LIF dynamics, sensory/body interface, descending command mappings, and phase-based contact feedback are modeled assumptions. The subgraph is leg locomotor anatomy and has no wing/flight circuit. Do not describe the rendered movement as an experimentally validated behavior.
