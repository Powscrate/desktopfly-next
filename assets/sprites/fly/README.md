# Supplied fly sprite

`desktopfly-source.png` is the supplied overhead fly artwork. `desktopfly.png` is its 56×57 cutout with the dark background converted to alpha and is the image embedded by the application. `desktopfly-side-reference.png` is the supplied alternate-angle reference; it is not a second animation frame and is never alternated with the overhead image.

The app preserves the supplied body/eyes, separates light gray pixels in the small wing area, then redraws those shapes with a brief flight flutter. Six stylized leg strokes animate against a tripod-phase gait template during surface walking. The body heading is aligned with its direction of travel. A small temporary zoom differentiates the brief escape/flight state. The movement/animation is procedural and illustrative: the single raster has no rigged limbs, and neither gait nor wingbeat is a measured animation from the source image. The neural graph includes leg—not wing—circuit anatomy.

No network asset retrieval occurs at runtime. The side reference and original source image are retained for provenance only.
