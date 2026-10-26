# User-supplied fly sprite

`desktopfly.png` is the first, top-down graphic supplied by the user for the desktop overlay. It is the app's runtime sprite. `desktopfly-source.png` preserves that original 56×57 RGB image; `desktopfly.png` is the same image with the dark background converted to per-pixel alpha for transparent rendering. The companion side-view reference is retained as `desktopfly-side-reference.png` but is not used as an animation frame because it is a different viewing angle.

The runtime rotates/scales the top-view sprite with the simulated fly. This is a raster graphic rather than the NeuroMechFly GLB; the app does not load the GLB, and the sprite has no articulated leg cycle. The fly can therefore appear to glide instead of stepping.
