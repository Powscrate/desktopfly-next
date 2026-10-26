#!/usr/bin/env python3
"""Bake the licensed FlyGym mesh into a small, transparent desktop sprite atlas.

This is an offline asset-generation tool; DesktopFly itself does not require Python,
MuJoCo, OpenGL, or a GPU. The GLB's 69 meshes are assembled from rest-pose.json and
rendered with a software triangle painter for broad Windows graphics compatibility.

Requires Python 3.10+, numpy, Pillow, trimesh, and fast-simplification (via trimesh).
"""
from __future__ import annotations

import json
import math
from pathlib import Path

import numpy as np
import trimesh
from PIL import Image, ImageDraw, ImageFilter

HERE = Path(__file__).resolve().parent.parent
GLB = HERE / "drosophila.glb"
POSE = HERE / "rest-pose.json"
OUTPUT = HERE / "desktopfly-sprite-atlas.png"

WIDTH, HEIGHT = 320, 256
SUPERSAMPLE = 3
FRAMES = 4
# Mild wing beats around the attachment hinge; this is a visual animation only, not
# a measured or biologically validated gait.
WING_ANGLES_DEG = (0.0, 28.0, 0.0, -28.0)
MAX_FACES_PER_PART = 800
MAX_WING_FACES = 2600


def rotation_from_wxyz(q: list[float]) -> np.ndarray:
    w, x, y, z = q
    return np.array([
        [1 - 2 * (y * y + z * z), 2 * (x * y - z * w), 2 * (x * z + y * w)],
        [2 * (x * y + z * w), 1 - 2 * (x * x + z * z), 2 * (y * z - x * w)],
        [2 * (x * z - y * w), 2 * (y * z + x * w), 1 - 2 * (x * x + y * y)],
    ], dtype=np.float64)


def prepare_parts() -> tuple[list[dict], np.ndarray]:
    pose_data = json.loads(POSE.read_text(encoding="utf-8"))
    scene = trimesh.load(GLB, force="scene")
    by_name = {part["name"]: part for part in pose_data["parts"]}
    if set(scene.geometry) != set(by_name) or len(by_name) != 69:
        raise ValueError("The GLB and its 69-part neutral pose do not match.")

    prepared: list[dict] = []
    all_points: list[np.ndarray] = []
    for name, source in scene.geometry.items():
        part_pose = by_name[name]
        mesh = source
        face_budget = MAX_WING_FACES if "Wing" in name else MAX_FACES_PER_PART
        if len(mesh.faces) > face_budget:
            mesh = mesh.simplify_quadric_decimation(face_count=face_budget)
        vertices = np.asarray(mesh.vertices, dtype=np.float64)
        faces = np.asarray(mesh.faces, dtype=np.int32)
        geom_rotation = rotation_from_wxyz(part_pose["rotationWxyz"])
        geom_position = np.asarray(part_pose["positionMm"], dtype=np.float64)
        # MuJoCo's world frame is z-up and its compiled GLB vertices are in millimetres.
        world_vertices = vertices @ geom_rotation.T + geom_position
        # Area-weighted vertex normals, calculated locally to avoid SciPy at build time.
        face_vectors = np.cross(vertices[faces[:, 1]] - vertices[faces[:, 0]],
                                vertices[faces[:, 2]] - vertices[faces[:, 0]])
        vertex_normals = np.zeros_like(vertices)
        for corner in range(3):
            np.add.at(vertex_normals, faces[:, corner], face_vectors)
        vertex_normals = vertex_normals @ geom_rotation.T
        vertex_normals /= np.maximum(np.linalg.norm(vertex_normals, axis=1, keepdims=True), 1e-9)
        prepared.append({
            "name": name,
            "vertices": world_vertices,
            "normals": vertex_normals,
            "faces": faces,
            "rgba": tuple(int(c) for c in source.visual.face_colors[0]),
            "wing": "Wing" in name,
            "hinge": geom_position.copy(),
        })
        all_points.append(world_vertices)
    return prepared, np.concatenate(all_points)


def camera_frame(points: np.ndarray) -> tuple[np.ndarray, np.ndarray, np.ndarray, float, np.ndarray, np.ndarray]:
    # Natural side view with a slight downward look to show wing venation and the legs.
    view = np.array([0.0, 0.93, -0.37], dtype=np.float64)
    view /= np.linalg.norm(view)
    world_up = np.array([0.0, 0.0, 1.0])
    right = np.cross(view, world_up)
    right /= np.linalg.norm(right)
    up = np.cross(right, view)
    target = np.array([0.0, 0.0, 1.30])
    rel = points - target
    projected_x = rel @ right
    projected_y = rel @ up
    span_x = float(projected_x.max() - projected_x.min())
    span_y = float(projected_y.max() - projected_y.min())
    available_w = WIDTH - 26
    available_h = HEIGHT - 24
    scale = min(available_w / max(span_x, 1e-5), available_h / max(span_y, 1e-5))
    center_x = (projected_x.min() + projected_x.max()) * 0.5
    center_y = (projected_y.min() + projected_y.max()) * 0.5
    center = np.array([center_x, center_y], dtype=np.float64)
    return right, up, view, scale, center, target


def material_color(name: str, original: tuple[int, int, int, int], point: np.ndarray,
                   normal: np.ndarray, face_number: int) -> tuple[int, int, int, int]:
    r, g, b, alpha = original
    lower = name.lower()
    # Use the source vertex colors as the base palette, with subtle cuticle variation.
    if "eye" in lower:
        base = np.array([118.0, 23.0, 35.0])
        noise = 0.90 + 0.20 * (((face_number * 73 + 17) % 101) / 100.0)
        base *= noise
    elif "wing" in lower:
        base = np.array([190.0, 207.0, 207.0])
        # Preserve the source wing translucency while giving the membrane a cool sheen.
        alpha = 118
    elif "haltere" in lower:
        base = np.array([167.0, 140.0, 89.0])
    elif any(part in lower for part in ("arista", "funiculus", "pedicel")):
        base = np.array([75.0, 55.0, 37.0])
    elif "tarsus" in lower:
        base = np.array([104.0, 71.0, 43.0])
    elif any(part in lower for part in ("coxa", "femur", "tibia")):
        base = np.array([124.0, 83.0, 45.0])
    elif lower.startswith("a"):
        # Each abdominal plate has a gently different amber-brown tone, preserving
        # the real plates instead of painting a synthetic striped silhouette.
        shade = 0.78 + 0.025 * sum(ord(c) for c in name)
        base = np.array([159.0, 103.0, 45.0]) * min(shade, 1.04)
    elif "thorax" in lower or "head" in lower:
        base = np.array([157.0, 105.0, 47.0])
    else:
        base = np.array([r, g, b], dtype=np.float64)

    # Fine deterministic mottling gives the exoskeleton a non-plastic, matte finish.
    if "wing" not in lower and "eye" not in lower:
        grain = 0.94 + 0.12 * (((face_number * 37 + len(name) * 19) % 127) / 126.0)
        base *= grain
    return tuple(int(np.clip(c, 0, 255)) for c in base) + (alpha,)


def render_frame(parts: list[dict], points: np.ndarray, camera: tuple, wing_angle: float) -> Image.Image:
    right, up, view, scale, center, target = camera
    size = (WIDTH * SUPERSAMPLE, HEIGHT * SUPERSAMPLE)
    # Contact shadow: a softly blurred oval below the thorax, kept separate from the mesh.
    shadow = Image.new("RGBA", size, (0, 0, 0, 0))
    draw_shadow = ImageDraw.Draw(shadow)
    cx, cy = WIDTH * SUPERSAMPLE * 0.50, HEIGHT * SUPERSAMPLE * 0.70
    draw_shadow.ellipse((cx - 112 * SUPERSAMPLE, cy - 11 * SUPERSAMPLE,
                         cx + 112 * SUPERSAMPLE, cy + 10 * SUPERSAMPLE),
                        fill=(28, 24, 22, 72))
    shadow = shadow.filter(ImageFilter.GaussianBlur(8 * SUPERSAMPLE))

    opaque = Image.new("RGBA", size, (0, 0, 0, 0))
    wings = Image.new("RGBA", size, (0, 0, 0, 0))
    draw_body = ImageDraw.Draw(opaque)
    draw_wings = ImageDraw.Draw(wings)
    light = np.array([-0.32, -0.78, 0.78], dtype=np.float64)
    light /= np.linalg.norm(light)
    eye_light = np.array([-0.18, -0.70, 0.90], dtype=np.float64)
    eye_light /= np.linalg.norm(eye_light)
    camera_dir = -view
    tri_list: list[tuple[float, dict, np.ndarray, np.ndarray, np.ndarray, int]] = []

    angle = math.radians(wing_angle)
    flap = np.array([[1, 0, 0], [0, math.cos(angle), -math.sin(angle)],
                     [0, math.sin(angle), math.cos(angle)]], dtype=np.float64)
    for part in parts:
        verts = part["vertices"]
        normals = part["normals"]
        if part["wing"]:
            # Rotate about the wing mesh origin, the attachment point in the FlyGym MJCF.
            pivot = part["hinge"]
            verts = (verts - pivot) @ flap.T + pivot
            normals = normals @ flap.T
        for face_idx, face in enumerate(part["faces"]):
            tri = verts[face]
            normal = normals[face].mean(axis=0)
            normal /= max(float(np.linalg.norm(normal)), 1e-9)
            center_world = tri.mean(axis=0)
            depth = float(np.dot(center_world - target, view))
            # Sort far-to-near for painter's algorithm; split translucent membranes out.
            tri_list.append((depth, part, tri, normal, center_world, face_idx))

    tri_list.sort(key=lambda item: item[0], reverse=True)
    for _, part, tri, normal, center_world, face_idx in tri_list:
        projected = np.column_stack(((tri - target) @ right, (tri - target) @ up))
        px = (projected[:, 0] - center[0]) * scale + WIDTH * 0.5
        py = HEIGHT * 0.5 - (projected[:, 1] - center[1]) * scale
        polygon = [(int(round(x * SUPERSAMPLE)), int(round(y * SUPERSAMPLE))) for x, y in zip(px, py)]
        if len(set(polygon)) < 3:
            continue
        # Two-sided lighting keeps the very thin scan-derived wing membranes visible.
        n = normal if np.dot(normal, camera_dir) >= 0 else -normal
        lambert = max(float(np.dot(n, light)), 0.0)
        diffuse = (0.62 + 0.38 * lambert) if part["wing"] else (0.30 + 0.70 * lambert)
        gloss = max(float(np.dot(n, (light + camera_dir) / np.linalg.norm(light + camera_dir))), 0.0)
        specular = (gloss ** (28 if "eye" in part["name"].lower() else 42)) * (0.38 if "eye" in part["name"].lower() else 0.10)
        base = material_color(part["name"], part["rgba"], center_world, n, face_idx)
        color = tuple(int(np.clip(c * diffuse + 255 * specular, 0, 255)) for c in base[:3]) + (base[3],)
        (draw_wings if part["wing"] else draw_body).polygon(polygon, fill=color)

    image = Image.alpha_composite(shadow, opaque)
    image = Image.alpha_composite(image, wings)
    # Downsample once to antialias all projected mesh edges.
    return image.resize((WIDTH, HEIGHT), Image.Resampling.LANCZOS)


def main() -> None:
    parts, points = prepare_parts()
    camera = camera_frame(points)
    frames = [render_frame(parts, points, camera, angle) for angle in WING_ANGLES_DEG]
    atlas = Image.new("RGBA", (WIDTH * FRAMES, HEIGHT), (0, 0, 0, 0))
    for i, frame in enumerate(frames):
        atlas.alpha_composite(frame, (i * WIDTH, 0))
    atlas.save(OUTPUT, optimize=True)
    print(f"Wrote {OUTPUT} ({OUTPUT.stat().st_size:,} bytes, {FRAMES} frames, {WIDTH}x{HEIGHT} each)")


if __name__ == "__main__":
    main()
