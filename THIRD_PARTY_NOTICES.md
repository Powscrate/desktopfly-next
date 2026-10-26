# Third-party notices

The current source has no third-party NuGet dependencies. It bundles one third-party-derived biological geometry asset: `assets/models/neuromechfly-v2/drosophila.glb`, converted from NeuroMechFly v2 / FlyGym and redistributed under Apache License 2.0. Its provenance, source commits, SHA-256, and limitations are in that directory's README; a copy of the Apache license is included beside it. The mesh conversion was made available by `tac76/fly-brain-simulation`; its upstream NOTICE identifies the source and license. No FlyWire connectome data is bundled.

The application uses .NET 8 / Windows Forms and the .NET base class libraries supplied by the user's .NET runtime or included in a self-contained publish. Consult Microsoft's .NET license and notices for redistributed runtime binaries when packaging. No runtime network requests are implemented.
