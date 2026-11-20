import math
import os
from pathlib import Path

import bpy
import bmesh
from mathutils import Vector

ROOT = Path(r"D:\Unity Projects\VR_Project")
ENV_DIR = ROOT / "Assets/Models/Environment/Bazaar"
BLEND_DIR = ROOT / "Blender/Scene3_Bazaar"
PREVIEW_DIR = ROOT / "References/Scene3_Bazaar"

for folder in (ENV_DIR, BLEND_DIR, PREVIEW_DIR):
    folder.mkdir(parents=True, exist_ok=True)

# ---------------------------------------------------------------------------
# Scene reset (without restarting the MCP addon)
# ---------------------------------------------------------------------------
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
for block in (bpy.data.meshes, bpy.data.curves, bpy.data.materials):
    for item in list(block):
        if item.users == 0:
            block.remove(item)

scene = bpy.context.scene
scene.unit_settings.system = "METRIC"
scene.unit_settings.scale_length = 1.0
scene.render.engine = "BLENDER_EEVEE"

# ---------------------------------------------------------------------------
# Materials
# ---------------------------------------------------------------------------
MATS = {}


def mat(name, color, metallic=0.0, roughness=0.6, emission=None):
    if name in MATS:
        return MATS[name]
    material = bpy.data.materials.new(name)
    material.use_nodes = True
    bsdf = material.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*color, 1.0)
    bsdf.inputs["Metallic"].default_value = metallic
    bsdf.inputs["Roughness"].default_value = roughness
    if emission:
        bsdf.inputs["Emission Color"].default_value = (*emission, 1.0)
        bsdf.inputs["Emission Strength"].default_value = 3.0
    material.diffuse_color = (*color, 1.0)
    MATS[name] = material
    return material


M_WALL = mat("M_Bazaar_WallCream", (0.93, 0.89, 0.80), roughness=0.82)
M_WALL_WARM = mat("M_Bazaar_WallWarm", (0.86, 0.78, 0.64), roughness=0.85)
M_TERRACOTTA = mat("M_Bazaar_Terracotta", (0.62, 0.30, 0.18), roughness=0.8)
M_ROOF = mat("M_Bazaar_RoofDark", (0.28, 0.22, 0.18), roughness=0.7)
M_WOOD = mat("M_Bazaar_Wood", (0.55, 0.38, 0.22), roughness=0.6)
M_WOOD_DARK = mat("M_Bazaar_WoodDark", (0.32, 0.21, 0.12), roughness=0.65)
M_METAL = mat("M_Bazaar_MetalDark", (0.06, 0.06, 0.06), metallic=0.7, roughness=0.35)
M_METAL_LIGHT = mat("M_Bazaar_MetalLight", (0.55, 0.55, 0.52), metallic=1.0, roughness=0.3)
M_CANVAS = mat("M_Bazaar_CanvasCream", (0.95, 0.91, 0.80), roughness=0.9)
M_RED = mat("M_Bazaar_CanvasRed", (0.72, 0.16, 0.12), roughness=0.85)
M_YELLOW = mat("M_Bazaar_CanvasYellow", (0.92, 0.68, 0.16), roughness=0.85)
M_TEAL = mat("M_Bazaar_CanvasTeal", (0.12, 0.42, 0.40), roughness=0.85)
M_GREEN = mat("M_Bazaar_Leaf", (0.14, 0.38, 0.16), roughness=0.7)
M_GREEN_LIGHT = mat("M_Bazaar_LeafLight", (0.28, 0.55, 0.22), roughness=0.7)
M_GLASS = mat("M_Bazaar_Glass", (0.30, 0.48, 0.48), roughness=0.15)
M_CONCRETE = mat("M_Bazaar_Concrete", (0.62, 0.60, 0.55), roughness=0.8)
M_PAVER = mat("M_Bazaar_Paver", (0.76, 0.64, 0.50), roughness=0.85)
M_BULB = mat("M_Bazaar_Bulb", (1.0, 0.80, 0.45), roughness=0.2, emission=(1.0, 0.62, 0.25))
M_SIGN_GREEN = mat("M_Bazaar_SignGreen", (0.06, 0.24, 0.14), roughness=0.6)
M_SIGN_GOLD = mat("M_Bazaar_SignGold", (0.85, 0.66, 0.22), metallic=0.3, roughness=0.4)
M_FLAG_RED = mat("M_Bazaar_FlagRed", (0.80, 0.08, 0.08), roughness=0.8)
M_FLAG_WHITE = mat("M_Bazaar_FlagWhite", (0.95, 0.95, 0.92), roughness=0.8)
M_CHALK = mat("M_Bazaar_Chalkboard", (0.08, 0.15, 0.12), roughness=0.9)
M_CRATE = mat("M_Bazaar_Crate", (0.62, 0.45, 0.26), roughness=0.75)
M_ORANGE = mat("M_Bazaar_ProduceOrange", (0.90, 0.45, 0.10), roughness=0.6)
M_PRODUCE_GREEN = mat("M_Bazaar_ProduceGreen", (0.22, 0.52, 0.18), roughness=0.6)
M_PRODUCE_RED = mat("M_Bazaar_ProduceRed", (0.70, 0.12, 0.10), roughness=0.6)
M_BIN = mat("M_Bazaar_Bin", (0.10, 0.24, 0.16), roughness=0.7)

# ---------------------------------------------------------------------------
# Geometry helpers, authored in Unity coordinates
# size/location are (x, y, z) in Unity; Blender mapping is (x, -z, y).
# ---------------------------------------------------------------------------
def _apply_mods(obj):
    bpy.context.view_layer.objects.active = obj
    for modifier in list(obj.modifiers):
        try:
            bpy.ops.object.modifier_apply(modifier=modifier.name)
        except Exception:
            pass


def _bevel(obj, width):
    if width <= 0:
        return
    modifier = obj.modifiers.new("Bevel", "BEVEL")
    modifier.width = width
    modifier.segments = 2
    modifier.limit_method = "ANGLE"
    _apply_mods(obj)


def box(name, size, location, material, bevel=0.0, rotation=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    _bevel(obj, min(bevel, min(size) / 3.0) if bevel else 0)
    obj.data.materials.append(material)
    return obj


def ubox(name, size, location, material, bevel=0.0, rotation=(0, 0, 0)):
    bsize = (size[0], size[2], size[1])
    bloc = (-location[0], -location[2], location[1])
    return box(name, bsize, bloc, material, bevel=bevel, rotation=rotation)


def cyl(name, radius, depth, location, material, rotation=(0, 0, 0), vertices=24, bevel=0.0):
    bpy.ops.mesh.primitive_cylinder_add(
        vertices=vertices, radius=radius, depth=depth, location=location, rotation=rotation
    )
    obj = bpy.context.object
    obj.name = name
    _bevel(obj, bevel)
    obj.data.materials.append(material)
    return obj


def ucyl(name, radius, height, location, material, rotation=(0, 0, 0), vertices=24, bevel=0.0):
    bloc = (-location[0], -location[2], location[1])
    return cyl(name, radius, height, bloc, material, rotation=rotation, vertices=vertices, bevel=bevel)


def sphere(name, radius, location, material, scale=(1, 1, 1), segments=20, rings=10, smooth=True):
    bpy.ops.mesh.primitive_uv_sphere_add(
        segments=segments, ring_count=rings, radius=radius, location=location
    )
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(material)
    if smooth:
        bpy.ops.object.shade_smooth()
    return obj


def usphere(name, radius, location, material, scale=(1, 1, 1), segments=20, rings=10):
    bloc = (-location[0], -location[2], location[1])
    bscale = (scale[0], scale[2], scale[1])
    return sphere(name, radius, bloc, material, scale=bscale, segments=segments, rings=rings)


def tube(name, points, radius, material, resolution=12):
    curve = bpy.data.curves.new(name, "CURVE")
    curve.dimensions = "3D"
    spline = curve.splines.new("BEZIER")
    spline.bezier_points.add(len(points) - 1)
    for point, coordinate in zip(spline.bezier_points, points):
        point.co = coordinate
        point.handle_left_type = "AUTO"
        point.handle_right_type = "AUTO"
    curve.bevel_depth = radius
    curve.bevel_resolution = 2
    curve.resolution_u = resolution
    obj = bpy.data.objects.new(name, curve)
    bpy.context.collection.objects.link(obj)
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.convert(target="MESH")
    obj = bpy.context.object
    obj.data.materials.append(material)
    bpy.ops.object.shade_smooth()
    return obj


def utube(name, unity_points, radius, material):
    points = [(-x, -z, y) for x, y, z in unity_points]
    return tube(name, points, radius, material)


def join_objects(objects, name):
    objects = [obj for obj in objects if obj and obj.name in bpy.data.objects]
    if not objects:
        raise RuntimeError("No objects to join for " + name)
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    bpy.ops.object.join()
    result = bpy.context.object
    result.name = name
    return result


def origin_bottom_center(obj):
    bpy.context.view_layer.update()
    corners = [obj.matrix_world @ Vector(corner) for corner in obj.bound_box]
    min_x = min(c.x for c in corners)
    max_x = max(c.x for c in corners)
    min_y = min(c.y for c in corners)
    max_y = max(c.y for c in corners)
    min_z = min(c.z for c in corners)
    bpy.context.scene.cursor.location = ((min_x + max_x) / 2, (min_y + max_y) / 2, min_z)
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR")
    obj.location = (0, 0, 0)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    return obj


def origin_world(obj):
    bpy.context.scene.cursor.location = (0, 0, 0)
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR")
    obj.location = (0, 0, 0)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    return obj


def export_fbx(objects, path):
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    bpy.ops.export_scene.fbx(
        filepath=str(path),
        use_selection=True,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        global_scale=1.0,
        apply_unit_scale=True,
        use_mesh_modifiers=True,
        mesh_smooth_type="FACE",
        object_types={"MESH"},
        path_mode="AUTO",
        embed_textures=False,
    )
    print("exported", path)


# ---------------------------------------------------------------------------
# School campus
# ---------------------------------------------------------------------------
def build_school():
    objs = []
    add = objs.append

    # Fence line at local x=-18, gate opening z=-3..+3.
    for side in (-1, 1):
        z0 = 3.0
        z1 = 30.0
        zc = side * (z0 + z1) / 2
        length = z1 - z0
        add(ubox(f"FenceBase_{side}", (0.25, 0.4, length), (-18, 0.2, zc), M_CONCRETE))
        for y in (0.8, 1.5, 2.1):
            add(ubox(f"FenceRail_{side}_{y}", (0.06, 0.06, length), (-18, y, zc), M_METAL))
        for z in [side * v for v in range(4, 31, 3)]:
            add(ubox(f"FencePost_{side}_{z}", (0.12, 2.2, 0.12), (-18, 1.1, z), M_METAL))
        # low hedge behind the fence
        add(ubox(f"HedgeFence_{side}", (0.9, 1.0, length - 2.0), (-16.9, 0.5, zc), M_GREEN, bevel=0.05))

    # Gate pillars, beam, and closed panels.
    for z in (-3.25, 3.25):
        add(ubox(f"GatePillar_{z}", (0.55, 3.4, 0.55), (-18, 1.7, z), M_CONCRETE, bevel=0.03))
        add(ubox(f"GateCap_{z}", (0.7, 0.18, 0.7), (-18, 3.48, z), M_TERRACOTTA, bevel=0.02))
    add(ubox("GateHeader", (0.55, 0.55, 7.0), (-18, 3.55, 0), M_WALL, bevel=0.02))
    add(ubox("GateSignTrim", (0.12, 1.28, 6.35), (-18.30, 3.95, 0), M_SIGN_GOLD))
    add(ubox("GateSignBoard", (0.10, 1.15, 6.2), (-18.38, 3.95, 0), M_SIGN_GREEN))

    for side in (-1, 1):
        zc = side * 1.55
        add(ubox(f"GateFrame_{side}", (0.09, 2.3, 2.95), (-18, 1.15, zc), M_METAL))
        for i in range(-4, 5):
            add(ubox(f"GateBar_{side}_{i}", (0.05, 2.05, 0.05), (-18, 1.15, zc + i * 0.29), M_METAL))
        for y in (0.55, 1.65):
            add(ubox(f"GateRail_{side}_{y}", (0.06, 0.06, 2.9), (-18, y, zc), M_METAL))
    add(ubox("GateLatch", (0.12, 0.18, 0.28), (-18.1, 1.1, 0), M_SIGN_GOLD))

    # Main building: local x=4..18, z=-26..+26, two floors.
    add(ubox("SchoolBase", (14, 7.6, 52), (11, 3.8, 0), M_WALL, bevel=0.02))
    add(ubox("SchoolFloorBand", (14.35, 0.22, 52.35), (11, 3.8, 0), M_TERRACOTTA))
    add(ubox("SchoolRoof", (14.25, 0.22, 52.25), (11, 7.72, 0), M_ROOF))
    add(ubox("ParapetWest", (0.28, 0.8, 52.25), (4.0, 8.05, 0), M_WALL_WARM))
    add(ubox("ParapetEast", (0.28, 0.8, 52.25), (18.0, 8.05, 0), M_WALL_WARM))
    add(ubox("ParapetNorth", (14.25, 0.8, 0.28), (11, 8.05, 26.0), M_WALL_WARM))
    add(ubox("ParapetSouth", (14.25, 0.8, 0.28), (11, 8.05, -26.0), M_WALL_WARM))

    # End wings.
    for side in (-1, 1):
        zc = side * 27.8
        add(ubox(f"WingBase_{side}", (14, 7.6, 4.0), (11, 3.8, zc), M_WALL, bevel=0.02))
        add(ubox(f"WingRoof_{side}", (14.2, 0.2, 4.2), (11, 7.7, zc), M_ROOF))
        add(ubox(f"WingParapet_{side}", (14.3, 0.8, 0.25), (11, 8.05, zc + side * 2.0), M_WALL_WARM))

    # Facade windows and doors.
    for zi in range(-22, 23, 4):
        for floor, y in ((1, 1.6), (2, 5.4)):
            add(ubox(f"WindowFrame_{zi}_{floor}", (0.16, 1.65, 2.0), (3.94, y, zi), M_WOOD_DARK))
            add(ubox(f"WindowGlass_{zi}_{floor}", (0.06, 1.35, 1.7), (3.88, y, zi), M_GLASS))
            add(ubox(f"WindowMullionV_{zi}_{floor}", (0.07, 1.35, 0.05), (3.84, y, zi), M_METAL))
            add(ubox(f"WindowMullionH_{zi}_{floor}", (0.07, 0.05, 1.7), (3.84, y, zi), M_METAL))
            add(ubox(f"WindowSill_{zi}_{floor}", (0.28, 0.09, 2.15), (3.80, y - 0.86, zi), M_CONCRETE))

    for z in (-14, 0, 14):
        add(ubox(f"DoorFrame_{z}", (0.16, 2.45, 1.5), (3.94, 1.22, z), M_WOOD_DARK))
        add(ubox(f"DoorPanel_{z}", (0.07, 2.25, 1.2), (3.88, 1.12, z), M_WOOD))
        add(ubox(f"DoorHandle_{z}", (0.06, 0.08, 0.08), (3.82, 1.05, z + 0.45), M_SIGN_GOLD))

    # Pilasters between bays.
    for z in range(-24, 25, 4):
        add(ubox(f"Pilaster_{z}", (0.42, 7.2, 0.38), (3.8, 3.6, z), M_WALL_WARM, bevel=0.01))

    # Covered walkway along the facade.
    add(ubox("WalkwayCanopy", (2.8, 0.16, 52), (2.6, 3.45, 0), M_TERRACOTTA, bevel=0.02))
    for z in range(-24, 25, 4):
        add(ubox(f"WalkwayColumn_{z}", (0.24, 3.4, 0.24), (1.35, 1.7, z), M_WALL, bevel=0.01))

    # Central entrance portico.
    add(ubox("PorticoRoof", (3.2, 0.22, 8.0), (2.6, 4.2, 0), M_TERRACOTTA, bevel=0.02))
    for z in (-3.4, -1.15, 1.15, 3.4):
        add(ubox(f"PorticoColumn_{z}", (0.3, 4.05, 0.3), (1.2, 2.02, z), M_WALL, bevel=0.01))
    add(ubox("PorticoStep1", (2.4, 0.15, 8.0), (2.0, 0.075, 0), M_CONCRETE))
    add(ubox("PorticoStep2", (1.8, 0.15, 7.2), (1.7, 0.15, 0), M_CONCRETE))

    # Yard path, circular plaza, flag pole and flag.
    add(ubox("YardPath", (22, 0.03, 4.0), (-7, 0.015, 0), M_PAVER))
    add(ucyl("YardPlaza", 5.0, 0.03, (-7, 0.015, 0), M_PAVER, vertices=40))
    add(ucyl("FlagPole", 0.07, 8.0, (-7, 4.0, 0), M_METAL_LIGHT, vertices=16))
    add(ubox("FlagRed", (1.6, 0.5, 0.04), (-6.2, 7.25, 0), M_FLAG_RED))
    add(ubox("FlagWhite", (1.6, 0.5, 0.04), (-6.2, 6.75, 0), M_FLAG_WHITE))

    # Hedges and trees in the yard.
    for side in (-1, 1):
        add(ubox(f"YardHedge_{side}", (0.8, 0.9, 8.0), (-13.5, 0.45, side * 12.0), M_GREEN, bevel=0.05))
        add(ubox(f"YardHedge2_{side}", (0.8, 0.9, 6.0), (-4.5, 0.45, side * 18.0), M_GREEN, bevel=0.05))
    for tx, tz in ((-14.5, -20), (-14.5, 20), (-10.0, -16), (-10.0, 16), (-3.0, -12), (-3.0, 12)):
        add(ucyl(f"TreeTrunk_{tx}_{tz}", 0.20, 3.0, (tx, 1.5, tz), M_WOOD_DARK, vertices=10))
        add(usphere(f"TreeCrownA_{tx}_{tz}", 1.25, (tx, 3.6, tz), M_GREEN, segments=16, rings=8))
        add(usphere(f"TreeCrownB_{tx}_{tz}", 0.95, (tx + 0.55, 4.1, tz + 0.25), M_GREEN_LIGHT, segments=16, rings=8))
        add(usphere(f"TreeCrownC_{tx}_{tz}", 0.9, (tx - 0.5, 4.0, tz - 0.3), M_GREEN, segments=16, rings=8))

    # Benches along the yard path.
    for bx, bz in ((-12.0, -8.5), (-12.0, 8.5), (-2.5, -8.5), (-2.5, 8.5)):
        add(ubox(f"BenchSeat_{bx}_{bz}", (1.5, 0.08, 0.45), (bx, 0.45, bz), M_WOOD))
        add(ubox(f"BenchBack_{bx}_{bz}", (1.5, 0.45, 0.06), (bx, 0.7, bz + 0.22), M_WOOD))
        for lx in (-0.6, 0.6):
            add(ubox(f"BenchLeg_{bx}_{bz}_{lx}", (0.08, 0.45, 0.4), (bx + lx, 0.22, bz), M_METAL))

    merged = join_objects(objs, "SchoolCampus")
    origin_bottom_center(merged)
    export_fbx([merged], ENV_DIR / "SchoolCampus.fbx")
    return merged


# ---------------------------------------------------------------------------
# Bazaar stand
# ---------------------------------------------------------------------------
def build_stand(closed=False):
    objs = []
    add = objs.append
    prefix = "Closed" if closed else "Open"

    add(ubox(f"{prefix}_Counter", (2.8, 0.9, 0.7), (0, 0.45, 0.85), M_WOOD, bevel=0.01))
    add(ubox(f"{prefix}_CounterTop", (3.0, 0.06, 0.8), (0, 0.92, 0.85), M_WOOD_DARK, bevel=0.01))
    if closed:
        add(ubox(f"{prefix}_Shutter", (2.8, 1.55, 0.07), (0, 1.42, 1.20), M_WOOD_DARK))
        for i in range(1, 5):
            add(ubox(f"{prefix}_ShutterSlat_{i}", (2.8, 0.04, 0.04), (0, 0.72 + i * 0.3, 1.24), M_METAL))
        add(ubox(f"{prefix}_RolledCanopy", (2.95, 0.26, 0.26), (0, 2.5, 1.28), M_RED, bevel=0.04))
    else:
        for i in range(7):
            x = -1.2 + i * 0.4
            add(ubox(f"{prefix}_Plank_{i}", (0.38, 0.78, 0.06), (x, 0.45, 1.19), M_WOOD_DARK))
        add(ubox(f"{prefix}_SideL", (0.06, 0.9, 0.7), (-1.4, 0.45, 0.85), M_WOOD_DARK))
        add(ubox(f"{prefix}_SideR", (0.06, 0.9, 0.7), (1.4, 0.45, 0.85), M_WOOD_DARK))
        add(ubox(f"{prefix}_FrontSignTrim", (2.7, 0.52, 0.05), (0, 0.55, 1.21), M_WOOD_DARK))
        add(ubox(f"{prefix}_FrontSignBoard", (2.55, 0.42, 0.06), (0, 0.55, 1.24), M_CANVAS))
        add(ubox(f"{prefix}_BackShelf", (2.8, 0.35, 0.5), (0, 1.15, -0.85), M_WOOD))
        for i, x in enumerate((-1.0, 1.0)):
            add(ubox(f"{prefix}_Crate_{i}", (0.48, 0.32, 0.42), (x, 1.05, 0.9), M_CRATE, bevel=0.01))
            add(usphere(f"{prefix}_ProduceA_{i}", 0.10, (x - 0.1, 1.28, 0.9), M_ORANGE, segments=10, rings=6))
            add(usphere(f"{prefix}_ProduceB_{i}", 0.10, (x + 0.1, 1.28, 0.9), M_PRODUCE_GREEN, segments=10, rings=6))
            add(usphere(f"{prefix}_ProduceC_{i}", 0.09, (x, 1.28, 0.84), M_PRODUCE_RED, segments=10, rings=6))
        # Canopy stripes.
        for i in range(7):
            x = -1.2 + i * 0.4
            add(ubox(f"{prefix}_Canopy_{i}", (0.4, 0.06, 2.8), (x, 2.55, 0), M_RED if i % 2 == 0 else M_CANVAS))
        add(ubox(f"{prefix}_Valance", (2.9, 0.28, 0.06), (0, 2.42, 1.40), M_RED))
        for i in range(7):
            x = -1.2 + i * 0.4
            add(ubox(f"{prefix}_ValanceStripe_{i}", (0.4, 0.28, 0.07), (x, 2.42, 1.41), M_RED if i % 2 == 0 else M_CANVAS))
        add(usphere(f"{prefix}_Bulb", 0.065, (0, 2.3, 0.4), M_BULB, segments=10, rings=6))

    for x in (-1.4, 1.4):
        for z in (-1.1, 1.1):
            add(ubox(f"{prefix}_Post_{x}_{z}", (0.08, 2.65, 0.08), (x, 1.32, z), M_WOOD_DARK))
    add(ubox(f"{prefix}_SignTrim", (2.85, 0.68, 0.06), (0, 2.9, 1.16), M_WOOD_DARK))
    add(ubox(f"{prefix}_SignBoard", (2.7, 0.55, 0.07), (0, 2.9, 1.20), M_CANVAS))

    merged = join_objects(objs, "BazaarStandClosed" if closed else "BazaarStand")
    origin_bottom_center(merged)
    filename = "BazaarStandClosed.fbx" if closed else "BazaarStand.fbx"
    export_fbx([merged], ENV_DIR / filename)
    return merged


# ---------------------------------------------------------------------------
# Cafe kiosk
# ---------------------------------------------------------------------------
def build_cafe():
    objs = []
    add = objs.append

    # Building shell and roof.
    add(ubox("CafeBase", (7.0, 3.2, 4.0), (0, 1.6, -2.0), M_WALL, bevel=0.02))
    add(ubox("CafeRoof", (7.4, 0.22, 4.4), (0, 3.32, -2.0), M_TERRACOTTA))
    add(ubox("CafeParapet", (7.4, 0.55, 0.22), (0, 3.55, 0.2), M_WALL_WARM))
    add(ubox("CafeParapetBack", (7.4, 0.55, 0.22), (0, 3.55, -4.2), M_WALL_WARM))

    # Serving window, counter, and door.
    add(ubox("CafeWindowFrame", (3.9, 1.75, 0.14), (0, 1.82, 0.02), M_WOOD_DARK))
    add(ubox("CafeWindowGlass", (3.55, 1.45, 0.06), (0, 1.82, 0.09), M_GLASS))
    add(ubox("CafeWindowSill", (4.1, 0.14, 0.5), (0, 0.98, 0.18), M_WOOD))
    add(ubox("CafeDoorFrame", (1.25, 2.3, 0.1), (-2.55, 1.15, 0.02), M_WOOD_DARK))
    add(ubox("CafeDoor", (1.05, 2.1, 0.07), (-2.55, 1.05, 0.08), M_WOOD))
    add(ubox("CafeDoorHandle", (0.06, 0.06, 0.06), (-2.15, 1.05, 0.14), M_SIGN_GOLD))
    for sx in (-3.52, 3.52):
        add(ubox(f"CafeSideFrame_{sx}", (0.12, 1.5, 2.0), (sx, 1.75, -2.2), M_WOOD_DARK))
        add(ubox(f"CafeSideGlass_{sx}", (0.05, 1.3, 1.8),
                 (sx + (0.04 if sx > 0 else -0.04), 1.75, -2.2), M_GLASS))
    add(ubox("CafeCounter", (3.85, 0.9, 0.45), (0, 0.45, -0.15), M_WOOD))
    add(ubox("CafeCounterTop", (4.0, 0.07, 0.55), (0, 0.92, -0.15), M_WOOD_DARK))

    # Awning stripes and sign.
    for i in range(5):
        x = -2.6 + i * 1.3
        add(ubox(f"CafeAwning_{i}", (1.3, 0.06, 1.55), (x, 2.75, 0.78), M_RED if i % 2 == 0 else M_CANVAS))
    add(ubox("CafeValance", (6.6, 0.3, 0.06), (0, 2.56, 1.55), M_RED))
    add(ubox("CafeSignTrim", (6.5, 0.9, 0.08), (0, 3.05, 0.02), M_WOOD_DARK))
    add(ubox("CafeSignBoard", (6.3, 0.72, 0.09), (0, 3.05, 0.07), M_CANVAS))

    # Terrace deck and border.
    add(ubox("CafeDeck", (6.4, 0.06, 3.6), (0, 0.03, 2.1), M_WOOD))
    add(ubox("CafePlanterL", (0.4, 0.42, 3.2), (-3.2, 0.21, 2.1), M_WOOD_DARK))
    add(ubox("CafePlanterR", (0.4, 0.42, 3.2), (3.2, 0.21, 2.1), M_WOOD_DARK))
    add(ubox("CafePlanterFrontL", (2.0, 0.42, 0.4), (-2.3, 0.21, 3.75), M_WOOD_DARK))
    add(ubox("CafePlanterFrontR", (2.0, 0.42, 0.4), (2.3, 0.21, 3.75), M_WOOD_DARK))
    for x in (-3.2, 3.2):
        add(ubox(f"CafeHedge_{x}", (0.3, 0.22, 3.0), (x, 0.52, 2.1), M_GREEN))
    for x in (-2.3, 2.3):
        add(ubox(f"CafeHedgeFront_{x}", (1.8, 0.22, 0.3), (x, 0.52, 3.75), M_GREEN))

    # Tables and chairs.
    for ti, tx in enumerate((-1.6, 1.6)):
        add(ucyl(f"CafeTableLeg_{ti}", 0.06, 0.72, (tx, 0.36, 2.2), M_METAL))
        add(ucyl(f"CafeTableTop_{ti}", 0.55, 0.06, (tx, 0.75, 2.2), M_WOOD, vertices=28))
        for ci, (cx, cz) in enumerate(((tx - 0.82, 2.2), (tx + 0.82, 2.2), (tx, 1.45), (tx, 2.95))):
            add(ubox(f"CafeChairSeat_{ti}_{ci}", (0.45, 0.06, 0.45), (cx, 0.45, cz), M_WOOD))
            back_z = cz - 0.22 if cz < 2.2 else cz + 0.22
            add(ubox(f"CafeChairBack_{ti}_{ci}", (0.45, 0.5, 0.06), (cx, 0.7, back_z), M_WOOD))
            for lx in (-0.17, 0.17):
                for lz in (-0.17, 0.17):
                    add(ubox(f"CafeChairLeg_{ti}_{ci}_{lx}_{lz}", (0.04, 0.45, 0.04), (cx + lx, 0.22, cz + lz), M_METAL))

    # Menu board and potted plants.
    add(ubox("CafeMenuFrame", (0.95, 1.3, 0.08), (-3.0, 1.45, 1.35), M_WOOD_DARK))
    add(ubox("CafeMenuBoard", (0.82, 1.15, 0.06), (-3.0, 1.45, 1.40), M_CHALK))
    for px, pz in ((-2.95, 3.25), (2.95, 3.25)):
        add(ucyl(f"CafePot_{px}_{pz}", 0.22, 0.42, (px, 0.21, pz), M_TERRACOTTA, vertices=18))
        add(usphere(f"CafePlant_{px}_{pz}", 0.38, (px, 0.62, pz), M_GREEN, segments=14, rings=8))

    # String lights along the terrace front.
    utube("CafeStringCable", [(-3.2, 2.95, 3.8), (0, 2.82, 3.85), (3.2, 2.95, 3.8)], 0.012, M_METAL)
    for i in range(9):
        t = i / 8.0
        x = -3.2 + 6.4 * t
        y = 2.95 - 0.13 * math.sin(math.pi * t)
        add(usphere(f"CafeLight_{i}", 0.05, (x, y, 3.82), M_BULB, segments=8, rings=6))

    merged = join_objects(objs, "CafeKiosk")
    origin_bottom_center(merged)
    export_fbx([merged], ENV_DIR / "CafeKiosk.fbx")
    return merged


# ---------------------------------------------------------------------------
# Bazaar stage
# ---------------------------------------------------------------------------
def build_stage():
    objs = []
    add = objs.append

    add(ubox("StagePlatform", (6.0, 0.15, 6.0), (0, 0.075, 0), M_WOOD, bevel=0.01))
    add(ubox("StageEdge", (6.2, 0.06, 6.2), (0, 0.17, 0), M_WOOD_DARK))
    add(ubox("StageStep1", (3.2, 0.075, 0.45), (0, 0.038, 3.25), M_WOOD))
    add(ubox("StageStep2", (3.2, 0.15, 0.45), (0, 0.075, 3.0), M_WOOD))

    for x in (-2.7, 2.7):
        for z in (-2.7, 2.7):
            add(ubox(f"StagePost_{x}_{z}", (0.13, 3.3, 0.13), (x, 1.65, z), M_METAL))

    for i in range(7):
        x = -2.7 + i * 0.45
        add(ubox(f"StageCanopy_{i}", (0.45, 0.06, 6.4), (x, 3.28, 0), M_RED if i % 2 == 0 else M_CANVAS))
    add(ubox("StageValance", (6.3, 0.3, 0.06), (0, 3.08, 3.22), M_RED))

    add(ubox("StageBackdrop", (5.8, 2.3, 0.1), (0, 1.55, -2.82), M_RED))
    for x in (-2.0, 0, 2.0):
        add(ubox(f"StageBackdropStripe_{x}", (0.22, 2.3, 0.02), (x, 1.55, -2.76), M_CANVAS))

    add(ubox("StageCounter", (2.6, 0.6, 0.6), (0, 0.45, 2.35), M_WOOD, bevel=0.01))
    add(ubox("StageCounterTop", (2.8, 0.06, 0.7), (0, 0.75, 2.35), M_WOOD_DARK))
    add(ubox("StageCounterFront", (2.6, 0.5, 0.05), (0, 0.4, 2.66), M_CANVAS))
    for x in (-0.9, -0.3, 0.3, 0.9):
        add(ubox(f"StageCounterStripe_{x}", (0.3, 0.5, 0.06), (x, 0.4, 2.67), M_RED))

    add(ubox("StageHeaderTrim", (5.8, 0.8, 0.08), (0, 2.55, 3.05), M_WOOD_DARK))
    add(ubox("StageHeaderBoard", (5.6, 0.62, 0.09), (0, 2.55, 3.10), M_CANVAS))

    for i in range(9):
        x = -2.7 + 5.4 * i / 8.0
        add(usphere(f"StageLight_{i}", 0.055, (x, 2.98, 2.95), M_BULB, segments=8, rings=6))
    for x in (-2.4, 2.4):
        add(ubox(f"StageSpeaker_{x}", (0.4, 0.6, 0.4), (x, 1.05, -2.4), M_METAL))

    merged = join_objects(objs, "BazaarStage")
    origin_bottom_center(merged)
    export_fbx([merged], ENV_DIR / "BazaarStage.fbx")
    return merged


# ---------------------------------------------------------------------------
# Street props, authored in world coordinates
# ---------------------------------------------------------------------------
def build_street_props():
    objs = []
    add = objs.append

    # String-light poles and cables.
    for x in (-16.8, -1.2):
        for z in (-30, -15, 15, 30):
            add(ucyl(f"SLPole_{x}_{z}", 0.09, 3.7, (x, 1.85, z), M_METAL, vertices=12))
            add(ucyl(f"SLBase_{x}_{z}", 0.14, 0.16, (x, 0.08, z), M_METAL, vertices=12))
            add(ubox(f"SLBar_{x}_{z}", (0.85, 0.08, 0.08), (x, 3.55, z), M_METAL))
    for z in (-30, -15, 15, 30):
        utube(f"SLCable_{z}", [(-16.4, 3.5, z), (-9, 3.08, z), (-1.6, 3.5, z)], 0.014, M_METAL)
        for i in range(11):
            t = i / 10.0
            x = -16.4 + 14.8 * t
            y = 3.5 - 0.42 * math.sin(math.pi * t)
            add(usphere(f"SLBulb_{z}_{i}", 0.055, (x, y, z), M_BULB, segments=8, rings=6))

    # Lamp posts.
    for x, z in ((-16.8, -35), (-16.8, -8), (-16.8, 8), (-16.8, 35), (-1.2, -20), (-1.2, 20)):
        add(ucyl(f"LampPole_{x}_{z}", 0.10, 4.0, (x, 2.0, z), M_METAL, vertices=12))
        add(ucyl(f"LampBase_{x}_{z}", 0.16, 0.18, (x, 0.09, z), M_METAL, vertices=12))
        arm_x = 0.45 if x < -9 else -0.45
        add(ubox(f"LampArm_{x}_{z}", (0.9, 0.08, 0.08), (x + arm_x, 4.0, z), M_METAL))
        add(usphere(f"LampGlobe_{x}_{z}", 0.18, (x + arm_x * 2, 3.82, z), M_BULB, segments=12, rings=8))

    # Bollards around the plaza.
    for z in (-8.6, 8.6):
        for x in (-15, -12, -9, -6, -3):
            add(ucyl(f"Bollard_{x}_{z}", 0.09, 0.9, (x, 0.45, z), M_METAL, vertices=10))
            add(ucyl(f"BollardBand_{x}_{z}", 0.095, 0.09, (x, 0.74, z), M_CANVAS, vertices=10))

    # Planters and bins.
    for x in (-16.8, -1.2):
        for z in (-25, -5, 5, 25):
            add(ubox(f"Planter_{x}_{z}", (1.15, 0.4, 0.5), (x, 0.2, z), M_CONCRETE))
            add(usphere(f"PlanterShrubA_{x}_{z}", 0.34, (x - 0.25, 0.55, z), M_GREEN, segments=12, rings=6))
            add(usphere(f"PlanterShrubB_{x}_{z}", 0.30, (x + 0.28, 0.52, z), M_GREEN_LIGHT, segments=12, rings=6))
    for x, z in ((-16.5, -8.6), (-1.5, -8.6), (-16.5, 8.6), (-1.5, 8.6)):
        add(ucyl(f"Bin_{x}_{z}", 0.22, 0.75, (x, 0.38, z), M_BIN, vertices=12))
        add(ucyl(f"BinLid_{x}_{z}", 0.24, 0.06, (x, 0.79, z), M_METAL, vertices=12))

    # Vertical banners.
    banner_colors = [M_RED, M_YELLOW, M_TEAL]
    for i, (x, z) in enumerate(((-16.8, -12), (-16.8, 12), (-16.8, 22), (-1.2, -12), (-1.2, 12), (-1.2, -22))):
        add(ucyl(f"BannerPole_{x}_{z}", 0.06, 3.2, (x, 1.6, z), M_METAL, vertices=8))
        add(ubox(f"Banner_{x}_{z}", (0.8, 2.0, 0.04), (x + (0.45 if x < -9 else -0.45), 2.2, z), banner_colors[i % 3]))
        add(ubox(f"BannerStripe_{x}_{z}", (0.8, 0.18, 0.05), (x + (0.45 if x < -9 else -0.45), 1.35, z), M_CANVAS))

    # Benches.
    for x, z in ((-16.4, -10.5), (-16.4, 10.5), (-1.6, -10.5), (-1.6, 10.5)):
        add(ubox(f"StreetBenchSeat_{x}_{z}", (1.4, 0.08, 0.45), (x, 0.45, z), M_WOOD))
        add(ubox(f"StreetBenchBack_{x}_{z}", (1.4, 0.42, 0.06), (x, 0.68, z + (0.22 if z > 0 else -0.22)), M_WOOD))
        for lx in (-0.55, 0.55):
            add(ubox(f"StreetBenchLeg_{x}_{z}_{lx}", (0.08, 0.45, 0.38), (x + lx, 0.22, z), M_METAL))

    # Parked scooters near the cafes.
    for sx, sz, color in ((-17.2, 30.5, M_RED), (-17.2, -30.5, M_TEAL)):
        add(ubox(f"ScooterBody_{sz}", (0.42, 0.45, 1.2), (sx, 0.55, sz), color, bevel=0.05))
        add(ubox(f"ScooterDeck_{sz}", (0.36, 0.12, 1.0), (sx, 0.28, sz), M_METAL))
        for wz in (-0.45, 0.45):
            add(ucyl(f"ScooterWheel_{sz}_{wz}", 0.22, 0.08, (sx, 0.22, sz + wz), M_METAL, rotation=(0, math.pi / 2, 0), vertices=16))
        add(ubox(f"ScooterHandle_{sz}", (0.52, 0.06, 0.06), (sx, 1.02, sz + 0.42), M_METAL))
        add(ubox(f"ScooterShield_{sz}", (0.34, 0.5, 0.06), (sx, 0.78, sz + 0.48), color))

    # Street sign posts (boards are labelled with TMP in Unity).
    for x, z, rot in ((-6.0, -36.0, 0),):
        add(ucyl(f"StreetSignPole_{z}", 0.05, 2.7, (x, 1.35, z), M_METAL, vertices=8))
        if z > 0:
            add(ubox(f"StreetSignBoard_{z}", (0.05, 0.42, 1.3), (x - 0.04, 2.4, z), M_SIGN_GREEN))
        else:
            add(ubox(f"StreetSignBoard_{z}", (1.3, 0.42, 0.05), (x, 2.4, z), M_SIGN_GREEN))

    # Distant tree line so the playable block does not end in an empty horizon.
    perimeter = []
    for z in range(-40, 41, 10):
        perimeter.append((-27, z))
    for x in range(-20, 41, 10):
        perimeter.append((x, 49))
    for z in range(-40, 41, 10):
        perimeter.append((50, z))
    for x in range(0, 41, 10):
        perimeter.append((x, -55))
    for index, (tx, tz) in enumerate(perimeter):
        add(ucyl(f"PerimeterTrunk_{index}", 0.20, 3.5, (tx, 1.75, tz), M_WOOD_DARK, vertices=8))
        add(usphere(f"PerimeterCrownA_{index}", 1.6, (tx, 4.2, tz), M_GREEN, segments=12, rings=6))
        add(usphere(f"PerimeterCrownB_{index}", 1.1, (tx + 0.8, 4.7, tz + 0.3), M_GREEN_LIGHT, segments=12, rings=6))

    merged = join_objects(objs, "StreetProps")
    origin_world(merged)
    export_fbx([merged], ENV_DIR / "StreetProps.fbx")
    return merged


# ---------------------------------------------------------------------------
# Build and export
# ---------------------------------------------------------------------------
print("Building Scene 3 kits...")
school = build_school()
stand = build_stand(False)
closed = build_stand(True)
cafe = build_cafe()
stage = build_stage()
props = build_street_props()

# Arrange the kit sources in the .blend file for inspection.
school.location.x = 0
stand.location.x = 45
closed.location.x = 55
cafe.location.x = 65
stage.location.x = 78
props.location.x = 95

bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_DIR / "Scene3_Bazaar.blend"))
print("Saved", BLEND_DIR / "Scene3_Bazaar.blend")
print("Scene 3 kit build complete.")
