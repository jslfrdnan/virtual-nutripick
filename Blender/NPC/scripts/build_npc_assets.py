import math
import os
from pathlib import Path

import bpy
from mathutils import Vector

ROOT = Path(r"D:\Unity Projects\VR_Project")
OUT_DIR = ROOT / "Assets" / "Models" / "Characters" / "NPC"
BLEND_DIR = ROOT / "Blender" / "NPC"
for folder in (OUT_DIR, BLEND_DIR):
    folder.mkdir(parents=True, exist_ok=True)

# ---------------------------------------------------------------------------
# Scene reset without restarting Blender
# ---------------------------------------------------------------------------
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
for block in (bpy.data.meshes, bpy.data.armatures, bpy.data.materials, bpy.data.actions, bpy.data.curves):
    for item in list(block):
        if item.users == 0:
            block.remove(item)

scene = bpy.context.scene
scene.unit_settings.system = "METRIC"
scene.unit_settings.scale_length = 1.0
scene.render.engine = "BLENDER_EEVEE"
scene.render.fps = 30

# ---------------------------------------------------------------------------
# Materials
# ---------------------------------------------------------------------------
MATS = {}


def mat(name, color, roughness=0.65, metallic=0.0, emission=None):
    if name in MATS:
        return MATS[name]
    material = bpy.data.materials.new(name)
    material.use_nodes = True
    bsdf = next(node for node in material.node_tree.nodes if node.type == "BSDF_PRINCIPLED")
    bsdf.inputs["Base Color"].default_value = (*color, 1.0)
    bsdf.inputs["Roughness"].default_value = roughness
    bsdf.inputs["Metallic"].default_value = metallic
    if emission:
        bsdf.inputs["Emission Color"].default_value = (*emission, 1.0)
        bsdf.inputs["Emission Strength"].default_value = 1.0
    material.diffuse_color = (*color, 1.0)
    MATS[name] = material
    return material


M_SKIN_A = mat("M_NPC_SkinA", (0.72, 0.50, 0.34), 0.7)
M_SKIN_B = mat("M_NPC_SkinB", (0.54, 0.34, 0.20), 0.7)
M_HAIR = mat("M_NPC_Hair", (0.045, 0.035, 0.030), 0.8)
M_EYE = mat("M_NPC_Eye", (0.02, 0.02, 0.02), 0.4)
M_MOUTH = mat("M_NPC_Mouth", (0.32, 0.12, 0.10), 0.7)
M_SHIRT_TEAL = mat("M_NPC_ShirtTeal", (0.055, 0.30, 0.30), 0.8)
M_SHIRT_RUST = mat("M_NPC_ShirtRust", (0.52, 0.16, 0.10), 0.8)
M_SHIRT_CREAM = mat("M_NPC_ShirtCream", (0.82, 0.74, 0.60), 0.8)
M_SHIRT_GREEN = mat("M_NPC_ShirtGreen", (0.16, 0.30, 0.18), 0.8)
M_PANTS = mat("M_NPC_Pants", (0.10, 0.12, 0.15), 0.8)
M_PANTS_KHAKI = mat("M_NPC_PantsKhaki", (0.40, 0.34, 0.24), 0.8)
M_APRON = mat("M_NPC_Apron", (0.86, 0.82, 0.70), 0.85)
M_HIJAB = mat("M_NPC_Hijab", (0.30, 0.42, 0.42), 0.85)
M_HIJAB_CREAM = mat("M_NPC_HijabCream", (0.78, 0.70, 0.56), 0.85)
M_SHOE = mat("M_NPC_Shoe", (0.055, 0.045, 0.04), 0.8)
M_CAP = mat("M_NPC_Cap", (0.12, 0.18, 0.15), 0.8)
M_BELT = mat("M_NPC_Belt", (0.06, 0.05, 0.04), 0.6)


def clear_objects():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for action in list(bpy.data.actions):
        bpy.data.actions.remove(action)
    for block in (bpy.data.meshes, bpy.data.armatures):
        for item in list(block):
            if item.users == 0:
                block.remove(item)


def apply_scale(obj):
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)


def add_vgroup(obj, name):
    group = obj.vertex_groups.new(name=name)
    group.add(list(range(len(obj.data.vertices))), 1.0, "REPLACE")


def box(name, size, loc, material, vgroup, bevel=0.0):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = size
    apply_scale(obj)
    if bevel > 0:
        modifier = obj.modifiers.new("Bevel", "BEVEL")
        modifier.width = bevel
        modifier.segments = 1
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.modifier_apply(modifier=modifier.name)
    obj.data.materials.append(material)
    add_vgroup(obj, vgroup)
    return obj


def cyl(name, radius, depth, loc, material, vgroup, vertices=10, rotation=(0, 0, 0)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=loc, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.data.materials.append(material)
    add_vgroup(obj, vgroup)
    return obj


def sphere(name, radius, loc, material, vgroup, scale=(1, 1, 1), segments=12, rings=8):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, radius=radius, location=loc)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    apply_scale(obj)
    obj.data.materials.append(material)
    add_vgroup(obj, vgroup)
    return obj


def join_parts(parts, name):
    bpy.ops.object.select_all(action="DESELECT")
    for obj in parts:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    obj = bpy.context.object
    obj.name = name
    bpy.context.scene.cursor.location = (0, 0, 0)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR")
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    return obj


def create_armature(name, height, shoulder_w, hip_w):
    hip_z = height * 0.52
    chest_z = height * 0.70
    neck_z = height * 0.83
    head_z = height * 0.90
    head_top = height
    arm_z = height * 0.80
    elbow_z = height * 0.65
    wrist_z = height * 0.52
    knee_z = height * 0.28
    ankle_z = 0.08
    shoulder_x = shoulder_w / 2 + 0.02
    leg_x = hip_w * 0.28

    arm_data = bpy.data.armatures.new(name + "_Armature")
    arm_obj = bpy.data.objects.new(name + "_Rig", arm_data)
    bpy.context.collection.objects.link(arm_obj)
    bpy.context.view_layer.objects.active = arm_obj
    bpy.ops.object.mode_set(mode="EDIT")
    edit = arm_data.edit_bones

    def bone(bname, head, tail, parent=None):
        b = edit.new(bname)
        b.head = head
        b.tail = tail
        b.use_connect = False
        if parent:
            b.parent = edit[parent]
        return b

    bone("root", (0, 0, 0), (0, 0, 0.10))
    bone("hips", (0, 0, hip_z), (0, 0, hip_z + 0.08), "root")
    bone("spine", (0, 0, hip_z + 0.08), (0, 0, chest_z), "hips")
    bone("chest", (0, 0, chest_z), (0, 0, neck_z), "spine")
    bone("neck", (0, 0, neck_z), (0, 0, head_z), "chest")
    bone("head", (0, 0, head_z), (0, 0, head_top), "neck")
    # 21st bone: jaw helper for future lip-sync / head attachments. It hangs off
    # the head, carries no vertex weights, and is keyed at rest in every clip, so
    # the silhouette and the five existing clips stay byte-for-byte the same.
    bone("jaw", (0, -0.02, head_top - 0.16), (0, -0.07, head_top - 0.26), "head")

    for side, sx in (("L", 1), ("R", -1)):
        bone("shoulder." + side, (0, 0, arm_z), (sx * shoulder_x, 0, arm_z), "chest")
        bone("upper_arm." + side, (sx * shoulder_x, 0, arm_z), (sx * shoulder_x, 0, elbow_z), "shoulder." + side)
        bone("fore_arm." + side, (sx * shoulder_x, 0, elbow_z), (sx * shoulder_x, 0, wrist_z), "upper_arm." + side)
        bone("hand." + side, (sx * shoulder_x, 0, wrist_z), (sx * shoulder_x, 0, wrist_z - 0.10), "fore_arm." + side)
        bone("thigh." + side, (sx * leg_x, 0, hip_z), (sx * leg_x, 0, knee_z), "hips")
        bone("shin." + side, (sx * leg_x, 0, knee_z), (sx * leg_x, 0, ankle_z), "thigh." + side)
        bone("foot." + side, (sx * leg_x, 0, ankle_z), (sx * leg_x, -0.20, 0.03), "shin." + side)

    bpy.ops.object.mode_set(mode="OBJECT")
    arm_obj.show_in_front = True
    arm_obj.data.display_type = "OCTAHEDRAL"
    return arm_obj, {
        "hip_z": hip_z,
        "chest_z": chest_z,
        "neck_z": neck_z,
        "head_z": head_z,
        "head_top": head_top,
        "arm_z": arm_z,
        "elbow_z": elbow_z,
        "wrist_z": wrist_z,
        "knee_z": knee_z,
        "ankle_z": ankle_z,
        "shoulder_x": shoulder_x,
        "leg_x": leg_x,
        "height": height,
        "shoulder_w": shoulder_w,
        "hip_w": hip_w,
    }


def cut_face_opening(obj, rx, ry, rz, thickness=0.010):
    """Open the front of a head shell so the face reads, then give it thickness.

    The shell is a UV sphere whose scale is already applied, so mesh-local
    coordinates are the scaled ellipsoid radii. Faces whose centres fall in the
    face window are removed; Solidify keeps the fabric watertight so backface
    culling never exposes the inside of the head.
    """
    import bmesh

    bm = bmesh.new()
    bm.from_mesh(obj.data)
    doomed = []
    for face in bm.faces:
        c = face.calc_center_median()
        nx, ny, nz = c.x / rx, c.y / ry, c.z / rz
        if ny < -0.35 and abs(nx) < 0.75 and -0.45 < nz < 0.60:
            doomed.append(face)
    if doomed:
        bmesh.ops.delete(bm, geom=doomed, context="FACES")
    bm.to_mesh(obj.data)
    bm.free()

    solid = obj.modifiers.new("Solidify", "SOLIDIFY")
    solid.thickness = thickness
    solid.offset = -1.0
    solid.use_rim = True
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_apply(modifier=solid.name)
    return obj


def build_body(name, variant, skin, shirt, pants, hijab=False):
    height = 1.72 if variant == "male" else 1.64
    shoulder_w = 0.44 if variant == "male" else 0.36
    hip_w = 0.34 if variant == "male" else 0.38
    depth = 0.22 if variant == "male" else 0.20
    arm_obj, p = create_armature(name, height, shoulder_w, hip_w)

    body_parts = []
    # hips / torso
    body_parts.append(box("Hips", (hip_w, depth * 0.9, 0.24), (0, 0, p["hip_z"]), pants, "hips", bevel=0.01))
    body_parts.append(box("Torso", (shoulder_w * 0.92, depth, p["arm_z"] - p["hip_z"] + 0.06),
                          (0, 0, (p["hip_z"] + p["arm_z"]) / 2), shirt, "chest", bevel=0.01))
    body_parts.append(box("Belt", (hip_w * 0.95, depth * 0.92, 0.045), (0, 0, p["hip_z"] + 0.09), M_BELT, "hips"))
    # neck and head. The crown is pinned to the declared body height so the
    # exported bounds read 1.72 m (male) / 1.64 m (female, hijab included).
    head_c = p["head_top"] - 0.13 * 1.08
    head_rx, head_ry, head_rz = 0.13 * 0.92, 0.13 * 0.95, 0.13 * 1.08

    def face_y(x, z, push=0.002):
        """Front surface of the head ellipsoid at (x, z), pushed slightly out."""
        t = 1.0 - (x / head_rx) ** 2 - ((z - head_c) / head_rz) ** 2
        return -(head_ry * math.sqrt(max(t, 0.0)) + push)

    body_parts.append(cyl("Neck", 0.055, 0.10, (0, 0, p["neck_z"] + 0.03), skin, "neck", vertices=10))
    body_parts.append(sphere("Head", 0.13, (0, 0, head_c), skin, "head", scale=(0.92, 0.95, 1.08), segments=14, rings=8))
    eye_z, mouth_z = head_c + 0.030, head_c - 0.020
    body_parts.append(sphere("EyeL", 0.016, (0.045, face_y(0.045, eye_z, 0.004), eye_z), M_EYE, "head", segments=8, rings=6))
    body_parts.append(sphere("EyeR", 0.016, (-0.045, face_y(-0.045, eye_z, 0.004), eye_z), M_EYE, "head", segments=8, rings=6))
    body_parts.append(box("Mouth", (0.045, 0.012, 0.008), (0, face_y(0, mouth_z, 0.004), mouth_z), M_MOUTH, "head"))
    # hair / hijab
    if hijab:
        hijab_rx, hijab_ry, hijab_rz = 0.145 * 1.05, 0.145 * 1.04, 0.145 * 1.10
        hijab_obj = sphere("Hijab", 0.145, (0, 0, head_c - 0.02), M_HIJAB,
                           "head", scale=(1.05, 1.04, 1.10), segments=14, rings=8)
        body_parts.append(cut_face_opening(hijab_obj, hijab_rx, hijab_ry, hijab_rz))
        body_parts.append(box("HijabDrape", (0.26, 0.08, 0.30), (0, 0.06, p["neck_z"] - 0.06), M_HIJAB, "neck", bevel=0.01))
    else:
        body_parts.append(sphere("Hair", 0.138, (0, 0.025, p["head_top"] - 0.113), M_HAIR,
                                 "head", scale=(1.0, 1.0, 0.72), segments=14, rings=8))
        if variant == "female":
            body_parts.append(box("HairBack", (0.18, 0.08, 0.30), (0, 0.105, head_c - 0.06), M_HAIR, "head", bevel=0.015))

    # arms
    for side, sx in (("L", 1), ("R", -1)):
        body_parts.append(cyl("UpperArm." + side, 0.058, p["arm_z"] - p["elbow_z"],
                              (sx * p["shoulder_x"], 0, (p["arm_z"] + p["elbow_z"]) / 2), shirt,
                              "upper_arm." + side, vertices=8))
        body_parts.append(cyl("ForeArm." + side, 0.047, p["elbow_z"] - p["wrist_z"],
                              (sx * p["shoulder_x"], 0, (p["elbow_z"] + p["wrist_z"]) / 2), skin,
                              "fore_arm." + side, vertices=8))
        body_parts.append(sphere("Hand." + side, 0.055, (sx * p["shoulder_x"], 0, p["wrist_z"] - 0.05),
                                 skin, "hand." + side, scale=(0.75, 0.8, 1.0), segments=8, rings=6))
        body_parts.append(cyl("Thigh." + side, 0.078, p["hip_z"] - p["knee_z"],
                              (sx * p["leg_x"], 0, (p["hip_z"] + p["knee_z"]) / 2), pants,
                              "thigh." + side, vertices=8))
        body_parts.append(cyl("Shin." + side, 0.062, p["knee_z"] - p["ankle_z"],
                              (sx * p["leg_x"], 0, (p["knee_z"] + p["ankle_z"]) / 2), pants,
                              "shin." + side, vertices=8))
        body_parts.append(box("Foot." + side, (0.12, 0.25, 0.08),
                              (sx * p["leg_x"], -0.05, 0.04), M_SHOE, "foot." + side, bevel=0.01))

    body = join_parts(body_parts, "SM_NPC_" + name + "_Body")
    body.parent = arm_obj
    mod = body.modifiers.new("Armature", "ARMATURE")
    mod.object = arm_obj

    # Separate apron and cap for role toggles.
    apron = join_parts([
        box("ApronPanel", (shoulder_w * 0.60, 0.02, 0.52), (0, -depth * 0.53, p["hip_z"] + 0.18), M_APRON, "chest", bevel=0.005),
        box("ApronStrapL", (0.035, 0.02, 0.22), (0.11, -depth * 0.53, p["chest_z"] - 0.02), M_APRON, "chest"),
        box("ApronStrapR", (0.035, 0.02, 0.22), (-0.11, -depth * 0.53, p["chest_z"] - 0.02), M_APRON, "chest"),
    ], "SM_NPC_" + name + "_Apron")
    apron.parent = arm_obj
    apron.modifiers.new("Armature", "ARMATURE").object = arm_obj

    cap_parts = [
        cyl("CapTop", 0.148, 0.09, (0, 0, p["head_top"] - 0.045), M_CAP, "head", vertices=12),
        box("CapBrim", (0.20, 0.12, 0.02), (0, -0.13, p["head_top"] - 0.085), M_CAP, "head"),
    ]
    cap = join_parts(cap_parts, "SM_NPC_" + name + "_Cap")
    cap.parent = arm_obj
    cap.modifiers.new("Armature", "ARMATURE").object = arm_obj

    return arm_obj, body, apron, cap, p


def set_pose(arm, rotations=None, locations=None):
    for pb in arm.pose.bones:
        pb.rotation_mode = "XYZ"
        if rotations is None or pb.name not in rotations:
            pb.rotation_euler = (0, 0, 0)
        if locations is None or pb.name not in locations:
            pb.location = (0, 0, 0)
    if rotations:
        for name, rot in rotations.items():
            if name in arm.pose.bones:
                arm.pose.bones[name].rotation_euler = rot
    if locations:
        for name, loc in locations.items():
            if name in arm.pose.bones:
                arm.pose.bones[name].location = loc


def key_bones(arm, frame, rotations=None, locations=None):
    set_pose(arm, rotations, locations)
    for pb in arm.pose.bones:
        pb.keyframe_insert(data_path="rotation_euler", frame=frame)
        pb.keyframe_insert(data_path="location", frame=frame)


def make_action(arm, name, keys):
    action = bpy.data.actions.new(name)
    action.use_fake_user = True
    arm.animation_data_create()
    arm.animation_data.action = action
    for frame, rotations, locations in keys:
        key_bones(arm, frame, rotations, locations)
    return action


def build_actions(arm):
    arm.animation_data_create()
    arm.animation_data.use_nla = False
    r = math.radians
    idle = [
        (1, {"chest": (r(1), 0, 0), "head": (0, r(2), 0), "upper_arm.L": (r(2), 0, 0), "upper_arm.R": (r(-2), 0, 0)}, None),
        (45, {"chest": (r(-1), 0, 0), "head": (0, r(-3), 0), "upper_arm.L": (r(-1), 0, 0), "upper_arm.R": (r(1), 0, 0)}, None),
        (90, {"chest": (r(1), 0, 0), "head": (0, r(2), 0), "upper_arm.L": (r(2), 0, 0), "upper_arm.R": (r(-2), 0, 0)}, None),
    ]
    walk = [
        (1, {"thigh.L": (r(30), 0, 0), "thigh.R": (r(-30), 0, 0), "shin.L": (0, 0, 0), "shin.R": (r(35), 0, 0),
             "upper_arm.L": (r(-22), 0, 0), "upper_arm.R": (r(22), 0, 0)}, None),
        (8, {"thigh.L": (0, 0, 0), "thigh.R": (0, 0, 0), "shin.L": (r(20), 0, 0), "shin.R": (r(20), 0, 0),
             "upper_arm.L": (0, 0, 0), "upper_arm.R": (0, 0, 0)}, None),
        (15, {"thigh.L": (r(-30), 0, 0), "thigh.R": (r(30), 0, 0), "shin.L": (r(35), 0, 0), "shin.R": (0, 0, 0),
              "upper_arm.L": (r(22), 0, 0), "upper_arm.R": (r(-22), 0, 0)}, None),
        (22, {"thigh.L": (0, 0, 0), "thigh.R": (0, 0, 0), "shin.L": (r(20), 0, 0), "shin.R": (r(20), 0, 0),
              "upper_arm.L": (0, 0, 0), "upper_arm.R": (0, 0, 0)}, None),
        (28, {"thigh.L": (r(30), 0, 0), "thigh.R": (r(-30), 0, 0), "shin.L": (0, 0, 0), "shin.R": (r(35), 0, 0),
              "upper_arm.L": (r(-22), 0, 0), "upper_arm.R": (r(22), 0, 0)}, None),
    ]
    # Seated: pelvis drops so the butt lands on the 0.48 m Warteg bench and the
    # soles stay on the floor (measured with the hip translation active).
    seated = {"hips": (0, -0.31, 0)}
    eat_rot = {
        "thigh.L": (r(78), 0, 0), "thigh.R": (r(78), 0, 0),
        "shin.L": (r(-78), 0, 0), "shin.R": (r(-78), 0, 0),
        "chest": (r(-3), 0, 0), "head": (r(4), 0, 0),
        "upper_arm.R": (r(-35), 0, r(-20)), "fore_arm.R": (r(-95), 0, 0),
    }
    eat = [
        (1, eat_rot, seated),
        (20, {**eat_rot, "fore_arm.R": (r(-65), 0, 0), "head": (r(7), 0, 0)}, seated),
        (40, {**eat_rot, "fore_arm.R": (r(-95), 0, 0), "head": (r(3), 0, 0)}, seated),
        (60, eat_rot, seated),
    ]
    talk = [
        (1, {"head": (0, r(-7), 0), "upper_arm.L": (r(6), 0, 0)}, None),
        (15, {"head": (0, r(8), 0), "upper_arm.R": (r(-8), 0, 0)}, None),
        (30, {"head": (0, r(-4), 0), "upper_arm.L": (r(-4), 0, 0)}, None),
        (45, {"head": (0, r(9), 0), "upper_arm.R": (r(-5), 0, 0)}, None),
        (60, {"head": (0, r(-7), 0), "upper_arm.L": (r(6), 0, 0)}, None),
    ]
    wipe = [
        (1, {"upper_arm.R": (r(-55), 0, r(-20)), "fore_arm.R": (r(-70), 0, 0), "chest": (0, r(-4), 0)}, None),
        (20, {"upper_arm.R": (r(-60), 0, r(25)), "fore_arm.R": (r(-55), 0, 0), "chest": (0, r(5), 0)}, None),
        (40, {"upper_arm.R": (r(-55), 0, r(-20)), "fore_arm.R": (r(-70), 0, 0), "chest": (0, r(-4), 0)}, None),
        (60, {"upper_arm.R": (r(-55), 0, r(-20)), "fore_arm.R": (r(-70), 0, 0), "chest": (0, r(-4), 0)}, None),
    ]
    actions = [
        make_action(arm, "Idle", idle),
        make_action(arm, "Walk", walk),
        make_action(arm, "Eat", eat),
        make_action(arm, "Talk", talk),
        make_action(arm, "Wipe", wipe),
    ]
    arm.animation_data.action = None
    for action in actions:
        track = arm.animation_data.nla_tracks.new()
        track.name = action.name
        strip = track.strips.new(action.name, 1, action)
        strip.name = action.name
    arm.animation_data.use_nla = True
    return actions


def export_character(arm, body, apron, cap, name):
    bpy.ops.object.select_all(action="DESELECT")
    arm.select_set(True)
    body.select_set(True)
    apron.select_set(True)
    cap.select_set(True)
    bpy.context.view_layer.objects.active = arm
    path = str(OUT_DIR / (name + ".fbx"))
    bpy.ops.export_scene.fbx(
        filepath=path,
        use_selection=True,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        global_scale=1.0,
        apply_unit_scale=True,
        use_mesh_modifiers=True,
        mesh_smooth_type="FACE",
        object_types={"ARMATURE", "MESH"},
        path_mode="AUTO",
        embed_textures=False,
        add_leaf_bones=False,
        bake_anim=True,
        bake_anim_use_all_bones=True,
        bake_anim_use_nla_strips=True,
        bake_anim_use_all_actions=False,
        bake_anim_force_startend_keying=True,
        bake_anim_step=1.0,
        bake_anim_simplify_factor=0.0,
    )
    print("exported", path)


def build_character(name, variant, shirt, pants, hijab=False, skin=None):
    clear_objects()
    arm, body, apron, cap, p = build_body(name, variant, skin or M_SKIN_A, shirt, pants, hijab=hijab)
    build_actions(arm)
    export_character(arm, body, apron, cap, name)
    # Keep a source file with the last character.
    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_DIR / "NPC.blend"))
    return p


print("Building NPC kit...")
build_character("Npc_Male", "male", M_SHIRT_TEAL, M_PANTS, skin=M_SKIN_A)
build_character("Npc_Female", "female", M_SHIRT_RUST, M_PANTS_KHAKI, skin=M_SKIN_B)
build_character("Npc_FemaleHijab", "female", M_SHIRT_CREAM, M_PANTS, hijab=True, skin=M_SKIN_A)
print("NPC kit complete.")
