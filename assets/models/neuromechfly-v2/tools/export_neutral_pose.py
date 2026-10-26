#!/usr/bin/env python3
"""Recreate rest-pose.json from the licensed FlyGym 1.2.1 package assets.

This tool compiles only the NeuroMechFly body XML and mesh files. It does not load
or include brain/connectome data or any body-motion recording.

Requires flygym==1.2.1 and its pinned mujoco==3.2.7 dependency.
"""
from __future__ import annotations

import hashlib
import importlib.metadata
import importlib.util
import json
import re
import shutil
import struct
import tempfile
from pathlib import Path

import mujoco
import numpy as np

HERE = Path(__file__).resolve().parent.parent
GLB = HERE / "drosophila.glb"
OUTPUT = HERE / "rest-pose.json"
MODEL_XML = "neuromechfly_seqik_kinorder_ypr.xml"
POSE_YAML = "pose_tripod.yaml"


def glb_node_names(path: Path) -> list[str]:
    payload = path.read_bytes()
    if payload[:4] != b"glTF" or struct.unpack_from("<I", payload, 4)[0] != 2:
        raise ValueError("Expected a glTF 2.0 binary (GLB) file.")
    json_length, json_type = struct.unpack_from("<II", payload, 12)
    if json_type != 0x4E4F534A:
        raise ValueError("The first GLB chunk is not JSON.")
    document = json.loads(payload[20:20 + json_length].decode("utf-8").rstrip(" \t\r\n\0"))
    names = [node["name"] for node in document["nodes"] if "mesh" in node and "name" in node]
    if len(names) != 69 or len(set(names)) != 69:
        raise ValueError(f"Expected 69 uniquely named GLB mesh nodes, got {len(names)}.")
    return names


def main() -> None:
    flygym_version = importlib.metadata.version("flygym")
    if flygym_version != "1.2.1":
        raise RuntimeError(f"Expected flygym==1.2.1, found {flygym_version}.")
    if mujoco.__version__ != "3.2.7":
        raise RuntimeError(f"Expected mujoco==3.2.7, found {mujoco.__version__}.")
    spec = importlib.util.find_spec("flygym")
    if spec is None or spec.origin is None:
        raise RuntimeError("Could not locate the installed FlyGym package.")
    data_dir = Path(spec.origin).resolve().parent / "data"
    source_xml = data_dir / "mjcf" / MODEL_XML
    source_pose = data_dir / "pose" / POSE_YAML
    source_mesh = data_dir / "mesh"
    if not source_xml.is_file() or not source_pose.is_file() or not source_mesh.is_dir():
        raise FileNotFoundError("The installed FlyGym wheel is missing its model, pose, or mesh files.")

    xml = source_xml.read_text(encoding="utf-8")
    original_root = '<body name="FlyBody">'
    if xml.count(original_root) != 1:
        raise ValueError("Could not find the expected NeuroMechFly root body.")
    # FlyGym attaches this root to a free body in its arena; add the equivalent free joint
    # so MuJoCo can report the assembled geom transforms without constructing an arena.
    xml = xml.replace(original_root,
                      '<body name="FlyBody" pos="0 0 0"><freejoint name="DesktopFlyRoot"/>', 1)
    joints = re.findall(r"^\s*(joint_[\w]+):\s*(-?[\d.]+)\s*$",
                        source_pose.read_text(encoding="utf-8"), re.MULTILINE)

    with tempfile.TemporaryDirectory(prefix="desktopfly-neutral-pose-") as temporary:
        work = Path(temporary)
        mjcf_dir = work / "mjcf"
        mjcf_dir.mkdir()
        shutil.copytree(source_mesh, work / "mesh")
        xml_path = mjcf_dir / MODEL_XML
        xml_path.write_text(xml, encoding="utf-8")
        model = mujoco.MjModel.from_xml_path(str(xml_path))
        data = mujoco.MjData(model)
        root_joint = mujoco.mj_name2id(model, mujoco.mjtObj.mjOBJ_JOINT, "DesktopFlyRoot")
        if root_joint < 0:
            raise ValueError("MuJoCo did not compile the root free joint.")
        root_qpos = model.jnt_qposadr[root_joint]
        data.qpos[root_qpos:root_qpos + 7] = [0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0]
        for joint_name, degrees in joints:
            joint_id = mujoco.mj_name2id(model, mujoco.mjtObj.mjOBJ_JOINT, joint_name)
            if joint_id < 0:
                raise ValueError(f"FlyGym pose references unknown joint {joint_name}.")
            data.qpos[model.jnt_qposadr[joint_id]] = np.deg2rad(float(degrees))
        mujoco.mj_forward(model, data)

        names = glb_node_names(GLB)
        part_transforms: dict[str, dict] = {}
        for geom_id in range(model.ngeom):
            name = mujoco.mj_id2name(model, mujoco.mjtObj.mjOBJ_GEOM, geom_id)
            if name is None or name not in names:
                continue
            quaternion = np.empty(4, dtype=np.float64)
            mujoco.mju_mat2Quat(quaternion, np.ascontiguousarray(data.geom_xmat[geom_id], dtype=np.float64))
            part_transforms[name] = {
                "name": name,
                "positionMm": [round(float(value), 8) for value in data.geom_xpos[geom_id]],
                "rotationWxyz": [round(float(value), 10) for value in quaternion],
            }
        if set(part_transforms) != set(names):
            missing = sorted(set(names) - set(part_transforms))
            raise ValueError(f"Could not resolve all GLB parts from the FlyGym model: {missing}")

    document = {
        "schemaVersion": 1,
        "source": f"FlyGym {flygym_version} / NeuroMechFly v2",
        "sourceModel": MODEL_XML,
        "initialPose": POSE_YAML,
        "coordinateUnits": "millimeters",
        "quaternionOrder": "wxyz",
        "mujocoVersion": mujoco.__version__,
        "glbSha256": hashlib.sha256(GLB.read_bytes()).hexdigest(),
        "parts": [part_transforms[name] for name in names],
    }
    OUTPUT.write_text(json.dumps(document, indent=2) + "\n", encoding="utf-8")
    print(f"Wrote {OUTPUT} ({len(names)} FlyGym part transforms, MuJoCo {mujoco.__version__})")


if __name__ == "__main__":
    main()
