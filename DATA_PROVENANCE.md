# Data provenance

The app's runtime fly artwork is the user-provided top-view image retained at `assets/sprites/fly/desktopfly-source.png`. The runtime file, `assets/sprites/fly/desktopfly.png`, is the same 56×57 RGB graphic with its dark background converted to per-pixel alpha for the transparent desktop overlay. The accompanying side-view reference is retained at `assets/sprites/fly/desktopfly-side-reference.png` but is not used as an animation frame because it depicts a different camera angle. The artwork is not a measured gait or biological dataset; it is supplied as a visual sprite.

The former NeuroMechFly GLB renderer and its associated bundled model assets have been removed from this branch at the user's request. No model or sprite is fetched from the network at runtime.

No FlyWire connectome, neural dataset, or pretrained model is bundled. `data/models/synthetic-escape-v1.json` remains a small, hand-authored description of the two-cell engineering demonstrator; it is not an authentic connectome and is not imported as model data by the runtime. `data/configurations/default.json` is an example settings file.

Future scientific dataset integration must record the dataset/project owner, canonical source URL, release/version/date, license and redistribution terms, schema, extraction steps, mapping choices, missing/unsupported metadata, neuron and synapse counts, and an integrity hash. Large datasets should be downloaded explicitly by the user or managed outside the source tree, not fetched silently at startup.
