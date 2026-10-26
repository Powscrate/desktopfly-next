# NeuroMechFly v2 Drosophila body mesh

`drosophila.glb` is a 3D adult female *Drosophila melanogaster* biomechanical mesh converted from FlyGym's NeuroMechFly v2 model and simplified with `fast-simplification`. It is a scientific scan-derived anatomical mesh, not a photogrammetry texture set or a fully rigged animation asset.

## Provenance

- Original model: NeuroMechFly v2 / FlyGym, https://github.com/NeLy-EPFL/flygym
- NeuroMechFly v2 states its biomechanical model is based on a micro-CT scan of a real adult female fly, with some body segments adjusted to better reflect anatomical evidence.
- GLB conversion source: `tac76/fly-brain-simulation`, `viewer/public/assets/fly.glb`, commit `7ec43e5121c5e85de92e458d7b32c02e8f7886fa` (repository commit `7ea10330747bd2a1417ad27acca2ae71a7d77bc0`), https://github.com/tac76/fly-brain-simulation/tree/7ea10330747bd2a1417ad27acca2ae71a7d77bc0
- The upstream project's `NOTICE` explicitly identifies this GLB as converted from FlyGym's NeuroMechFly model and identifies the mesh license as Apache-2.0.
- FlyGym source license: Apache License 2.0, copyright 2023–2026 The NeuroMechFly v2 Authors. A copy is included as `LICENSE-APACHE-2.0.txt`.
- SHA-256: `278143b583d4288d5aba20457ad972a7c8d849efecfd5faad6f716a4bae1d1a8`

## Contents and current limitations

The GLB is about 3 MB and contains 69 named mesh objects, including thorax, head, abdomen segments, eyes, two wings, halteres, antenna segments, and individual leg segments. It has approximately 75,000 position vertices and 452,532 triangle indices (150,844 triangles). It uses per-vertex colors and does not embed texture images, normals, or a rigged skeleton/animation. Mesh part separation provides a basis for future articulation but does not itself include a gait.

The asset is included as source data only in this milestone: the current DesktopFly renderer has not yet loaded or rendered this model. The next visual milestone is a native lightweight renderer, including validation of camera/orientation, transparency, and performance on integrated graphics. A rendered 2D sprite would be a fallback, not a claim of live 3D rendering.

No FlyWire connectome or other non-commercial scientific dataset is included with this model.
