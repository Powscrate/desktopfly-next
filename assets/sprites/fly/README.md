# Supplied fly sprite

`desktopfly-source.png` is the supplied overhead fly artwork. `desktopfly.png` is its 56×57 cutout with the dark background converted to alpha and is the image embedded by the application. `desktopfly-side-reference.png` is the supplied alternate-angle reference; it is not a second animation frame and is never alternated with the overhead image.

The app preserves the supplied body/eyes and separates light gray pixels in the wing region. Six articulated line limbs are animated from a 45-sample, six-leg joint-angle step trace derived from walking recordings and distributed with FlyGym; their mapping onto the 2D raster is a simplified projection, not a rendered anatomical body. A high-frequency translucent wing fan and modest temporary zoom signal the brief escape/flight state. The source data describes leg walking, not flight; the wing animation is not connectome-controlled.

The sprite itself is a single still image with no rig. It cannot supply photorealistic limb details, changing body pose, or true 3D depth. These overlays are illustrative and should not be described as a complete or experimentally validated fly animation. Provenance for the motion trace is in `data/locomotion/README.md`.
