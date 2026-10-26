# Data provenance

The repository now bundles a single geometry asset, `assets/models/neuromechfly-v2/drosophila.glb`: a 3D adult female *Drosophila melanogaster* mesh converted from FlyGym's NeuroMechFly v2 and simplified by the upstream converter. The source project says NeuroMechFly v2's biomechanical model is based on a micro-CT scan of a real adult female fly. Its file-level provenance, upstream commits, checksum, limitations, and Apache-2.0 license copy are recorded in `assets/models/neuromechfly-v2/README.md` and adjacent files. The asset is presently bundled but not loaded/rendered by the application.

No FlyWire connectome, neural dataset, or pretrained model is bundled. `data/models/synthetic-escape-v1.json` remains a small, hand-authored description of the two-cell engineering demonstrator; it is not an authentic connectome and is not imported as model data by the runtime. `data/configurations/default.json` is an example settings file.

Future integration must record the dataset/project owner, canonical source URL, release/version/date, license and redistribution terms, schema, extraction steps, mapping choices, missing/unsupported metadata, neuron and synapse counts, and an integrity hash. Large datasets should be downloaded explicitly by the user or managed outside the source tree, not fetched silently at startup.
