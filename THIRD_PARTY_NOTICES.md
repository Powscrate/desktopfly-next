# Third-party notices

## MaleCNS locomotor connectome subgraph

The files `data/neural/malecns-locomotor-v1.json` and `data/neural/malecns-locomotor-report-v1.json` are derived from the public MaleCNS v1.0 connectome. Credit the MaleCNS collaboration: FlyEM at HHMI Janelia, University of Cambridge, MRC Laboratory of Molecular Biology, and Google Research. The source and derived data are licensed under the Creative Commons Attribution 4.0 International license (CC BY 4.0): <https://creativecommons.org/licenses/by/4.0/>. Extraction provenance and checksums are in [`DATA_PROVENANCE.md`](DATA_PROVENANCE.md) and `data/neural/README.md`.

The bounded subgraph/report were sourced from Denis Shiryaev's `desktop-fly` repository, commit `32b00011e83c3dc85fa3ea0b3934155b04f1635d`, where the MaleCNS-only addition is separately identified as CC BY 4.0. No FlyWire-derived CC BY-NC 4.0 graph or point data from that repository is included.

## Application/runtime

There are no third-party NuGet dependencies. The runtime fly image is user-provided artwork at `assets/sprites/fly/` and is embedded as a transparent PNG resource. The former NeuroMechFly GLB model and renderer assets have been removed at the user's request. The app is offline-first and does not make runtime network requests.

The application uses .NET 8 / Windows Forms and the .NET base class libraries supplied by the user's .NET runtime or included in a self-contained publish. Consult Microsoft's .NET license and notices for redistributed runtime binaries when packaging.
