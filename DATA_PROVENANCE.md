# Data provenance

No external scientific dataset, FlyWire export, connectome, pretrained model, or third-party biological asset is bundled or downloaded in this prototype. `data/models/synthetic-escape-v1.json` is a small, hand-authored description of the two-cell engineering demonstrator. It is not an authentic connectome and is not imported by the runtime. `data/configurations/default.json` is an example settings file.

Future integration must record the dataset/project owner, canonical source URL, release/version/date, license and redistribution terms, schema, extraction steps, mapping choices, missing/unsupported metadata, neuron and synapse counts, and an integrity hash. Large datasets should be downloaded explicitly by the user or managed outside the source tree, not fetched silently at startup.
