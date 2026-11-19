import math
import os
from pathlib import Path

import bpy
import bmesh
from mathutils import Vector

ROOT = Path(r"D:\Unity Projects\VR_Project")
ENV_DIR = ROOT / "Assets/Models/Environment/Home"
PLATE_DIR = ROOT / "Assets/Models/Food/Plating"
FOOD_DIR = ROOT / "Assets/Models/Food/Home"
BLEND_DIR = ROOT / "Blender/Scene1_Home"
PREVIEW_DIR = ROOT / "References/Scene1_Home"

for folder in (ENV_DIR, PLATE_DIR, FOOD_DIR, BLEND_DIR, PREVIEW_DIR):
    folder.mkdir(parents=True, exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.unit_settings.system = "METRIC"
scene.unit_settings.scale_length = 1.0

# ---------------------------------------------------------------------------
# Materials
# ---------------------------------------------------------------------------
MATS = {}


def mat(name, color, metallic=0.0, roughness=0.5, emission=None):
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
        bsdf.inputs["Emission Strength"].default_value = 2.0
    material.diffuse_color = (*color, 1.0)
    MATS[name] = material
    return material


M_WALL = mat("M_Home_Wall", (0.93, 0.92, 0.89), roughness=0.85)
M_CABINET = mat("M_Home_Cabinet", (0.97, 0.96, 0.93), roughness=0.42)
M_COUNTER = mat("M_Home_Counter", (0.86, 0.85, 0.82), roughness=0.28)
M_FLOOR = mat("M_Home_Floor", (0.56, 0.41, 0.27), roughness=0.48)
M_WOOD = mat("M_Home_Wood", (0.58, 0.42, 0.26), roughness=0.42)
M_BLACK = mat("M_Home_BlackMetal", (0.035, 0.035, 0.035), metallic=0.7, roughness=0.32)
M_STEEL = mat("M_Home_Steel", (0.72, 0.72, 0.74), metallic=1.0, roughness=0.22)
M_GLASS = mat("M_Home_Glass", (0.70, 0.85, 0.92), roughness=0.06)
M_GREEN = mat("M_Home_Plant", (0.12, 0.38, 0.14), roughness=0.6)
M_GREEN_LIGHT = mat("M_Home_PlantLight", (0.26, 0.55, 0.22), roughness=0.6)
M_TERRACOTTA = mat("M_Home_Terracotta", (0.62, 0.30, 0.18), roughness=0.8)
M_CERAMIC = mat("M_Home_Ceramic", (0.97, 0.96, 0.93), roughness=0.25)
M_LINEN = mat("M_Home_Linen", (0.86, 0.82, 0.74), roughness=0.9)
M_RUG = mat("M_Home_Rug", (0.80, 0.77, 0.70), roughness=0.95)
M_TILE = mat("M_Home_Tile", (0.90, 0.89, 0.86), roughness=0.4)
M_GROUT = mat("M_Home_Grout", (0.72, 0.71, 0.68), roughness=0.7)
M_ART = mat("M_Home_Art", (0.30, 0.42, 0.38), roughness=0.7)
M_ART_ACCENT = mat("M_Home_ArtAccent", (0.78, 0.52, 0.28), roughness=0.7)
M_BULB = mat("M_Home_Bulb", (1.0, 0.86, 0.62), roughness=0.2, emission=(1.0, 0.72, 0.35))

M_FOOD_RENDANG = mat("M_Food_Rendang", (0.34, 0.13, 0.055), roughness=0.55)
M_FOOD_RENDANG_SAUCE = mat("M_Food_RendangSauce", (0.20, 0.06, 0.025), roughness=0.45)
M_FOOD_BUN = mat("M_Food_Bun", (0.76, 0.48, 0.20), roughness=0.55)
M_FOOD_PATTY = mat("M_Food_Patty", (0.28, 0.13, 0.06), roughness=0.6)
M_FOOD_CHEESE = mat("M_Food_Cheese", (0.96, 0.73, 0.18), roughness=0.4)
M_FOOD_LETTUCE = mat("M_Food_Lettuce", (0.34, 0.64, 0.18), roughness=0.55)
M_FOOD_TOMATO = mat("M_Food_Tomato", (0.76, 0.12, 0.08), roughness=0.4)
M_FOOD_EGG_WHITE = mat("M_Food_EggWhite", (0.98, 0.97, 0.94), roughness=0.35)
M_FOOD_YOLK = mat("M_Food_Yolk", (0.96, 0.68, 0.08), roughness=0.4)
M_FOOD_TEMPEH = mat("M_Food_Tempeh", (0.78, 0.62, 0.38), roughness=0.7)
M_FOOD_TEMPEH_DARK = mat("M_Food_TempehDark", (0.52, 0.37, 0.20), roughness=0.8)
M_FOOD_BROCCOLI = mat("M_Food_Broccoli", (0.18, 0.46, 0.16), roughness=0.6)
M_FOOD_STEM = mat("M_Food_Stem", (0.46, 0.60, 0.25), roughness=0.65)
M_FOOD_BANANA = mat("M_Food_Banana", (0.95, 0.78, 0.18), roughness=0.5)
M_FOOD_BANANA_TIP = mat("M_Food_BananaTip", (0.34, 0.22, 0.10), roughness=0.8)
M_FOOD_NOODLE = mat("M_Food_Noodle", (0.80, 0.58, 0.25), roughness=0.55)
M_FOOD_ONION = mat("M_Food_GreenOnion", (0.30, 0.60, 0.20), roughness=0.6)


# ---------------------------------------------------------------------------
# Geometry helpers
# ---------------------------------------------------------------------------
def _apply_mods(obj):
    bpy.context.view_layer.objects.active = obj
    for modifier in list(obj.modifiers):
        try:
            bpy.ops.object.modifier_apply(modifier=modifier.name)
        except Exception:
            pass


def _add_bevel(obj, width):
    if width <= 0:
        return
    modifier = obj.modifiers.new("Bevel", "BEVEL")
    modifier.width = width
    modifier.segments = 2
    modifier.limit_method = "ANGLE"
    _apply_mods(obj)


def box(name, size, location, material, bevel=0.004, rotation=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    _add_bevel(obj, min(bevel, min(size) / 3.0))
    obj.data.materials.append(material)
    return obj


def cyl(name, radius, depth, location, material, rotation=(0, 0, 0), vertices=32, bevel=0.002):
    bpy.ops.mesh.primitive_cylinder_add(
        vertices=vertices, radius=radius, depth=depth, location=location, rotation=rotation
    )
    obj = bpy.context.object
    obj.name = name
    _add_bevel(obj, bevel)
    obj.data.materials.append(material)
    return obj


def cone(name, radius1, radius2, depth, location, material, rotation=(0, 0, 0), vertices=32):
    bpy.ops.mesh.primitive_cone_add(
        vertices=vertices,
        radius1=radius1,
        radius2=radius2,
        depth=depth,
        location=location,
        rotation=rotation,
    )
    obj = bpy.context.object
    obj.name = name
    obj.data.materials.append(material)
    return obj


def sphere(name, radius, location, material, scale=(1, 1, 1), segments=24, rings=12, smooth=True):
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
    curve.bevel_resolution = 3
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


def lathe(name, profile, material, segments=48, scale=(1, 1, 1), location=(0, 0, 0)):
    mesh = bpy.data.meshes.new(name)
    bm = bmesh.new()
    rings = []
    for radius, z in profile:
        if radius <= 1e-6:
            rings.append([bm.verts.new((0, 0, z))])
        else:
            ring = []
            for index in range(segments):
                angle = 2.0 * math.pi * index / segments
                ring.append(bm.verts.new((radius * math.cos(angle), radius * math.sin(angle), z)))
            rings.append(ring)

    for index in range(len(rings) - 1):
        lower, upper = rings[index], rings[index + 1]
        if len(lower) == 1 and len(upper) == 1:
            continue
        if len(lower) == 1:
            for j in range(len(upper)):
                bm.faces.new((lower[0], upper[j], upper[(j + 1) % len(upper)]))
        elif len(upper) == 1:
            for j in range(len(lower)):
                bm.faces.new((lower[j], lower[(j + 1) % len(lower)], upper[0]))
        else:
            for j in range(len(lower)):
                bm.faces.new(
                    (lower[j], lower[(j + 1) % len(lower)], upper[(j + 1) % len(upper)], upper[j])
                )
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.location = location
    obj.scale = scale
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(material)
    return obj


def join_objects(objects, name):
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
    min_x = min(corner.x for corner in corners)
    max_x = max(corner.x for corner in corners)
    min_y = min(corner.y for corner in corners)
    max_y = max(corner.y for corner in corners)
    min_z = min(corner.z for corner in corners)
    bpy.context.scene.cursor.location = ((min_x + max_x) / 2, (min_y + max_y) / 2, min_z)
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR")
    obj.location = (0, 0, 0)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    return obj


def export_fbx(objects, path):
    if not objects:
        print("skip empty export", path)
        return
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
# Room shell + kitchen
# ---------------------------------------------------------------------------
def build_room():
    objects = []

    # Floor and shell. Blender -Y becomes Unity +Z (kitchen at the back).
    objects.append(box("Floor", (6.0, 5.0, 0.1), (0, 0, -0.05), M_FLOOR, bevel=0))
    objects.append(box("Wall_Back", (6.0, 0.1, 2.8), (0, -2.45, 1.4), M_WALL, bevel=0))
    objects.append(box("Wall_Front", (6.0, 0.1, 2.8), (0, 2.45, 1.4), M_WALL, bevel=0))
    objects.append(box("Wall_Left", (0.1, 5.0, 2.8), (-2.95, 0, 1.4), M_WALL, bevel=0))
    objects.append(box("Wall_Right", (0.1, 5.0, 2.8), (2.95, 0, 1.4), M_WALL, bevel=0))
    objects.append(box("Ceiling", (6.0, 5.0, 0.1), (0, 0, 2.85), M_WALL, bevel=0))

    # Baseboards
    objects.append(box("Baseboard_Back", (6.0, 0.03, 0.12), (0, -2.385, 0.06), M_CABINET, bevel=0))
    objects.append(box("Baseboard_Front", (6.0, 0.03, 0.12), (0, 2.385, 0.06), M_CABINET, bevel=0))
    objects.append(box("Baseboard_Left", (0.03, 5.0, 0.12), (-2.885, 0, 0.06), M_CABINET, bevel=0))
    objects.append(box("Baseboard_Right", (0.03, 5.0, 0.12), (2.885, 0, 0.06), M_CABINET, bevel=0))

    # Window on the kitchen wall, to the right of the upper cabinets
    window_y = -2.385
    window_x = 1.5
    objects.append(box("WindowFrame_Top", (1.04, 0.06, 0.06), (window_x, window_y, 2.28), M_CABINET, bevel=0.005))
    objects.append(box("WindowFrame_Bottom", (1.04, 0.06, 0.06), (window_x, window_y, 1.22), M_CABINET, bevel=0.005))
    objects.append(box("WindowFrame_Left", (0.06, 0.06, 1.1), (window_x - 0.52, window_y, 1.75), M_CABINET, bevel=0.005))
    objects.append(box("WindowFrame_Right", (0.06, 0.06, 1.1), (window_x + 0.52, window_y, 1.75), M_CABINET, bevel=0.005))
    objects.append(box("WindowGlass", (0.96, 0.012, 1.0), (window_x, window_y - 0.01, 1.75), M_GLASS, bevel=0))
    objects.append(box("WindowMullion_V", (0.035, 0.035, 1.0), (window_x, window_y - 0.02, 1.75), M_CABINET, bevel=0.003))
    objects.append(box("WindowMullion_H", (0.96, 0.035, 0.035), (window_x, window_y - 0.02, 1.75), M_CABINET, bevel=0.003))
    objects.append(box("WindowSill", (1.12, 0.16, 0.05), (window_x, window_y + 0.07, 1.18), M_CABINET, bevel=0.008))

    # Door on the entrance wall
    door_y = 2.385
    objects.append(box("DoorPanel", (0.92, 0.06, 2.1), (0, door_y, 1.05), M_CABINET, bevel=0.01))
    objects.append(box("DoorFrame_Left", (0.06, 0.1, 2.2), (-0.49, door_y, 1.1), M_WALL, bevel=0.004))
    objects.append(box("DoorFrame_Right", (0.06, 0.1, 2.2), (0.49, door_y, 1.1), M_WALL, bevel=0.004))
    objects.append(box("DoorFrame_Top", (1.04, 0.1, 0.06), (0, door_y, 2.2), M_WALL, bevel=0.004))
    objects.append(cyl("DoorHandle", 0.014, 0.12, (0.32, door_y - 0.06, 1.05), M_BLACK, rotation=(math.pi / 2, 0, 0), vertices=16))

    # Kitchen base cabinets
    objects.append(box("CabinetBase", (4.0, 0.6, 0.85), (-0.4, -2.05, 0.425), M_CABINET, bevel=0.006))
    objects.append(box("CabinetToeKick", (4.0, 0.55, 0.1), (-0.4, -2.08, 0.05), M_BLACK, bevel=0.003))
    objects.append(box("CounterTop", (4.1, 0.66, 0.04), (-0.4, -2.03, 0.87), M_COUNTER, bevel=0.008))
    for index, x in enumerate((-2.0, -1.2, -0.4, 0.4, 1.2)):
        objects.append(box(f"CabinetDoor_{index + 1}", (0.74, 0.02, 0.72), (x, -1.735, 0.45), M_CABINET, bevel=0.006))
        objects.append(box(f"CabinetHandle_{index + 1}", (0.26, 0.015, 0.015), (x, -1.72, 0.78), M_BLACK, bevel=0.003))

    # Sink and faucet
    objects.append(box("SinkBasin", (0.55, 0.4, 0.02), (-1.4, -2.03, 0.895), M_STEEL, bevel=0.004))
    objects.append(box("SinkRim", (0.61, 0.46, 0.012), (-1.4, -2.03, 0.902), M_STEEL, bevel=0.004))
    objects.append(tube("Faucet", [(-1.4, -2.2, 0.9), (-1.4, -2.2, 1.14), (-1.4, -2.06, 1.18), (-1.4, -1.96, 1.13)], 0.012, M_STEEL))

    # Stove and oven
    objects.append(box("Cooktop", (0.6, 0.5, 0.02), (0.8, -2.03, 0.9), M_BLACK, bevel=0.004))
    for bx, by in ((0.65, -2.15), (0.95, -2.15), (0.65, -1.91), (0.95, -1.91)):
        objects.append(cyl("Burner", 0.07, 0.01, (bx, by, 0.915), M_STEEL, vertices=24, bevel=0.002))
    objects.append(box("OvenDoor", (0.6, 0.02, 0.55), (0.8, -1.735, 0.4), M_BLACK, bevel=0.006))
    objects.append(box("OvenHandle", (0.5, 0.02, 0.02), (0.8, -1.72, 0.65), M_STEEL, bevel=0.004))
    objects.append(box("RangeHood", (0.7, 0.45, 0.2), (0.8, -2.05, 2.0), M_STEEL, bevel=0.01))
    objects.append(box("RangeHoodDuct", (0.3, 0.3, 0.6), (0.8, -2.05, 2.4), M_STEEL, bevel=0.005))

    # Upper cabinets and open shelf
    objects.append(box("UpperCabinet", (2.8, 0.35, 0.75), (-1.0, -2.22, 2.0), M_CABINET, bevel=0.006))
    for index, x in enumerate((-2.1, -1.4, -0.7, 0.0)):
        objects.append(box(f"UpperDoor_{index + 1}", (0.65, 0.02, 0.7), (x, -2.04, 2.0), M_CABINET, bevel=0.006))
        objects.append(box(f"UpperHandle_{index + 1}", (0.22, 0.015, 0.015), (x, -2.02, 1.72), M_BLACK, bevel=0.003))
    objects.append(box("OpenShelf", (1.2, 0.25, 0.04), (1.3, -2.2, 1.55), M_WOOD, bevel=0.006))
    for x in (0.78, 1.82):
        objects.append(box("ShelfBracket", (0.04, 0.2, 0.04), (x, -2.2, 1.53), M_BLACK, bevel=0.002))

    # Backsplash tiles
    objects.append(box("Backsplash", (4.1, 0.02, 0.6), (-0.4, -2.385, 1.2), M_TILE, bevel=0))
    for index in range(14):
        x = -2.45 + index * 0.3
        objects.append(box(f"GroutV_{index}", (0.008, 0.008, 0.6), (x, -2.37, 1.2), M_GROUT, bevel=0))
    for z in (1.08, 1.32, 1.56):
        objects.append(box("GroutH", (4.1, 0.008, 0.008), (-0.4, -2.37, z), M_GROUT, bevel=0))

    # Fridge and microwave
    objects.append(box("Fridge", (0.75, 0.7, 2.0), (2.4, -2.05, 1.0), M_CABINET, bevel=0.01))
    objects.append(box("FridgeSeam", (0.75, 0.012, 0.012), (2.4, -1.695, 1.25), M_BLACK, bevel=0.002))
    objects.append(box("FridgeHandleTop", (0.025, 0.03, 0.5), (2.1, -1.68, 1.55), M_BLACK, bevel=0.004))
    objects.append(box("FridgeHandleBottom", (0.025, 0.03, 0.4), (2.1, -1.68, 0.8), M_BLACK, bevel=0.004))
    objects.append(box("Microwave", (0.5, 0.35, 0.3), (1.5, -2.1, 1.07), M_BLACK, bevel=0.006))
    objects.append(box("MicrowaveDoor", (0.4, 0.012, 0.22), (1.5, -1.92, 1.07), M_GLASS, bevel=0.002))

    # Counter decor: jars, kettle, cutting board
    for index, (x, height) in enumerate(((-2.2, 0.2), (-2.02, 0.16), (0.18, 0.24))):
        objects.append(cyl(f"Jar_{index + 1}", 0.055, height, (x, -2.15, 0.9 + height / 2), M_GLASS, vertices=24))
        objects.append(cyl(f"JarLid_{index + 1}", 0.058, 0.02, (x, -2.15, 0.9 + height + 0.01), M_WOOD, vertices=24))
    objects.append(cyl("Kettle", 0.09, 0.2, (1.05, -2.12, 1.0), M_STEEL, vertices=24))
    objects.append(tube("KettleHandle", [(1.05, -2.12, 1.1), (1.15, -2.12, 1.18), (1.05, -2.12, 1.24), (0.96, -2.12, 1.17)], 0.012, M_BLACK))
    objects.append(box("CuttingBoard", (0.3, 0.02, 0.4), (1.65, -2.33, 1.15), M_WOOD, bevel=0.008, rotation=(math.radians(-12), 0, 0)))

    # Pendant lights above the island
    for index, x in enumerate((-0.6, 0.0, 0.6)):
        objects.append(cyl(f"PendantCord_{index + 1}", 0.006, 0.6, (x, -0.2, 2.5), M_BLACK, vertices=8))
        objects.append(cone(f"PendantShade_{index + 1}", 0.13, 0.045, 0.18, (x, -0.2, 2.13), M_BLACK, vertices=32))
        objects.append(sphere(f"PendantBulb_{index + 1}", 0.035, (x, -0.2, 2.05), M_BULB, segments=16, rings=8))

    return objects


# ---------------------------------------------------------------------------
# Island
# ---------------------------------------------------------------------------
def build_island():
    objects = []
    objects.append(box("IslandBase", (2.0, 0.8, 0.85), (0, 0, 0.425), M_CABINET, bevel=0.008))
    objects.append(box("IslandTop", (2.2, 0.9, 0.05), (0, 0, 0.875), M_COUNTER, bevel=0.01))
    objects.append(box("IslandPanelLeft", (0.92, 0.02, 0.72), (-0.5, -0.41, 0.45), M_CABINET, bevel=0.006))
    objects.append(box("IslandPanelRight", (0.92, 0.02, 0.72), (0.5, -0.41, 0.45), M_CABINET, bevel=0.006))
    objects.append(box("IslandHandleLeft", (0.3, 0.015, 0.015), (-0.5, -0.395, 0.78), M_BLACK, bevel=0.003))
    objects.append(box("IslandHandleRight", (0.3, 0.015, 0.015), (0.5, -0.395, 0.78), M_BLACK, bevel=0.003))
    objects.append(box("IslandToeKick", (1.9, 0.75, 0.1), (0, 0, 0.05), M_BLACK, bevel=0.003))
    return objects


# ---------------------------------------------------------------------------
# Dining set
# ---------------------------------------------------------------------------
def build_chair(name, x, y, rotation_z):
    parts = []
    parts.append(box(f"{name}_Seat", (0.42, 0.42, 0.05), (0, 0, 0.45), M_WOOD, bevel=0.012))
    parts.append(box(f"{name}_BackPostLeft", (0.04, 0.04, 0.45), (-0.19, 0.19, 0.7), M_WOOD, bevel=0.008))
    parts.append(box(f"{name}_BackPostRight", (0.04, 0.04, 0.45), (0.19, 0.19, 0.7), M_WOOD, bevel=0.008))
    parts.append(box(f"{name}_BackTop", (0.42, 0.04, 0.06), (0, 0.19, 0.92), M_WOOD, bevel=0.008))
    parts.append(box(f"{name}_BackMid", (0.42, 0.04, 0.04), (0, 0.19, 0.75), M_WOOD, bevel=0.008))
    for lx in (-0.17, 0.17):
        for ly in (-0.17, 0.17):
            parts.append(box(f"{name}_Leg", (0.04, 0.04, 0.45), (lx, ly, 0.225), M_CABINET, bevel=0.006))
    parts.append(box(f"{name}_CrossFront", (0.34, 0.03, 0.03), (0, -0.17, 0.2), M_CABINET, bevel=0.004))
    parts.append(box(f"{name}_CrossBack", (0.34, 0.03, 0.03), (0, 0.17, 0.2), M_CABINET, bevel=0.004))
    chair = join_objects(parts, name)
    origin_bottom_center(chair)
    chair.rotation_euler[2] = rotation_z
    chair.location = (x, y, 0)
    return chair


def build_dining():
    objects = []
    objects.append(box("DiningRug", (2.6, 2.0, 0.02), (0, 1.0, 0.01), M_RUG, bevel=0.01))
    objects.append(box("DiningTableTop", (1.6, 0.9, 0.05), (0, 1.0, 0.73), M_WOOD, bevel=0.012))
    objects.append(box("DiningTableApron", (1.4, 0.7, 0.08), (0, 1.0, 0.66), M_CABINET, bevel=0.006))
    for lx in (-0.7, 0.7):
        for ly in (0.65, 1.35):
            objects.append(box("DiningTableLeg", (0.06, 0.06, 0.72), (lx, ly, 0.36), M_CABINET, bevel=0.008))
    objects.append(build_chair("Chair_NorthLeft", -0.45, 0.25, 0))
    objects.append(build_chair("Chair_NorthRight", 0.45, 0.25, 0))
    objects.append(build_chair("Chair_SouthLeft", -0.45, 1.75, math.pi))
    objects.append(build_chair("Chair_SouthRight", 0.45, 1.75, math.pi))

    # Pendant over the dining table
    objects.append(cyl("DiningPendantCord", 0.006, 0.6, (0, 1.0, 2.5), M_BLACK, vertices=8))
    objects.append(cone("DiningPendantShade", 0.16, 0.05, 0.2, (0, 1.0, 2.12), M_BLACK, vertices=32))
    objects.append(sphere("DiningPendantBulb", 0.04, (0, 1.0, 2.03), M_BULB, segments=16, rings=8))

    # Sideboard on the left wall
    objects.append(box("Sideboard", (0.4, 1.4, 0.8), (-2.6, 1.0, 0.4), M_CABINET, bevel=0.008))
    objects.append(box("SideboardTop", (0.45, 1.5, 0.04), (-2.6, 1.0, 0.82), M_WOOD, bevel=0.008))
    objects.append(box("SideboardDoorA", (0.02, 0.65, 0.7), (-2.38, 0.65, 0.4), M_CABINET, bevel=0.004))
    objects.append(box("SideboardDoorB", (0.02, 0.65, 0.7), (-2.38, 1.35, 0.4), M_CABINET, bevel=0.004))
    objects.append(box("SideboardHandleA", (0.015, 0.22, 0.015), (-2.365, 0.65, 0.7), M_BLACK, bevel=0.003))
    objects.append(box("SideboardHandleB", (0.015, 0.22, 0.015), (-2.365, 1.35, 0.7), M_BLACK, bevel=0.003))

    # Wall art on the right wall
    objects.append(box("WallArtFrame", (0.05, 0.8, 0.6), (2.92, 1.0, 1.6), M_WOOD, bevel=0.006))
    objects.append(box("WallArtCanvas", (0.02, 0.7, 0.5), (2.89, 1.0, 1.6), M_ART, bevel=0))
    objects.append(box("WallArtAccent", (0.012, 0.26, 0.32), (2.878, 0.88, 1.56), M_ART_ACCENT, bevel=0))

    # Centerpiece on the table
    objects.append(cyl("TableVase", 0.055, 0.18, (0.55, 1.0, 0.84), M_CERAMIC, vertices=24))
    for index in range(5):
        angle = index * math.tau / 5
        objects.append(sphere("TablePlant", 0.045, (0.55 + 0.06 * math.cos(angle), 1.0 + 0.06 * math.sin(angle), 0.98), M_GREEN, scale=(1, 1, 0.8), segments=12, rings=6))

    return objects


# ---------------------------------------------------------------------------
# Decor
# ---------------------------------------------------------------------------
def build_plant(name, x, y, scale=1.0):
    objects = []
    objects.append(cyl(f"{name}_Pot", 0.13 * scale, 0.24 * scale, (x, y, 0.12 * scale), M_TERRACOTTA, vertices=24))
    objects.append(cyl(f"{name}_Soil", 0.115 * scale, 0.02 * scale, (x, y, 0.235 * scale), M_BLACK, vertices=24))
    for index in range(7):
        angle = index * math.tau / 7
        px = x + 0.09 * scale * math.cos(angle)
        py = y + 0.09 * scale * math.sin(angle)
        objects.append(sphere(f"{name}_Leaf", 0.075 * scale, (px, py, 0.36 * scale), M_GREEN if index % 2 else M_GREEN_LIGHT, scale=(1, 1, 1.25), segments=12, rings=6))
    objects.append(sphere(f"{name}_Crown", 0.1 * scale, (x, y, 0.46 * scale), M_GREEN_LIGHT, scale=(1, 1, 1.1), segments=16, rings=8))
    return objects


def build_decor():
    objects = []
    objects.extend(build_plant("PlantCornerLeft", -2.55, -1.65, 1.15))
    objects.extend(build_plant("PlantCornerRight", 2.55, 0.0, 0.95))
    objects.extend(build_plant("PlantDining", -2.55, 2.0, 0.8))

    # Trash bin
    objects.append(cyl("TrashBin", 0.15, 0.5, (-2.55, 2.15, 0.25), M_STEEL, vertices=24))
    objects.append(cyl("TrashBinLid", 0.16, 0.03, (-2.55, 2.15, 0.515), M_BLACK, vertices=24))

    # Dish rack beside the sink
    objects.append(box("DishRackBase", (0.42, 0.3, 0.02), (-0.55, -2.12, 0.91), M_STEEL, bevel=0.004))
    for index in range(4):
        objects.append(box("DishRackBar", (0.38, 0.012, 0.12), (-0.55, -2.24 + index * 0.08, 0.97), M_STEEL, bevel=0.002))

    # Fruit bowl on the island
    bowl = lathe("FruitBowl", [(0, 0), (0.07, 0), (0.11, 0.03), (0.13, 0.07), (0.13, 0.075), (0.12, 0.075), (0.10, 0.04), (0, 0.025)], M_CERAMIC, segments=32)
    bowl.location = (0.85, -0.2, 0.9)
    objects.append(bowl)
    for index, (color, x, y) in enumerate(((M_FOOD_TOMATO, 0.82, -0.22), (M_FOOD_BANANA, 0.88, -0.18), (M_FOOD_LETTUCE, 0.86, -0.24))):
        objects.append(sphere("Fruit", 0.04, (x, y, 0.96), color, scale=(1, 1, 0.9), segments=16, rings=8))

    # Wall clock on the left wall
    objects.append(cyl("WallClock", 0.16, 0.03, (-2.91, 0.3, 1.8), M_CERAMIC, rotation=(0, math.pi / 2, 0), vertices=32))
    objects.append(box("ClockHandA", (0.012, 0.01, 0.12), (-2.885, 0.3, 1.8), M_BLACK, bevel=0))
    objects.append(box("ClockHandB", (0.09, 0.01, 0.012), (-2.885, 0.3, 1.8), M_BLACK, bevel=0))

    return objects


# ---------------------------------------------------------------------------
# Plating
# ---------------------------------------------------------------------------
PLATE_PROFILE = [
    (0.0, 0.004),
    (0.16, 0.004),
    (0.24, 0.014),
    (0.28, 0.03),
    (0.29, 0.045),
    (0.275, 0.05),
    (0.23, 0.034),
    (0.0, 0.022),
]

SMALL_PLATE_PROFILE = [
    (0.0, 0.003),
    (0.07, 0.003),
    (0.10, 0.012),
    (0.115, 0.025),
    (0.112, 0.03),
    (0.095, 0.022),
    (0.0, 0.015),
]

BOWL_PROFILE = [
    (0.0, 0.0),
    (0.05, 0.0),
    (0.075, 0.025),
    (0.085, 0.06),
    (0.086, 0.07),
    (0.078, 0.07),
    (0.068, 0.035),
    (0.0, 0.02),
]


def build_plating():
    objects = []
    platter = lathe("Platter_Oval", PLATE_PROFILE, M_CERAMIC, segments=48, scale=(1.35, 0.82, 1.0))
    objects.append(platter)
    plate = lathe("Plate_Small", SMALL_PLATE_PROFILE, M_CERAMIC, segments=40)
    objects.append(plate)
    bowl = lathe("Bowl_Small", BOWL_PROFILE, M_CERAMIC, segments=40)
    objects.append(bowl)

    # Bamboo tray
    objects.append(box("TrayBase", (0.5, 0.3, 0.02), (0, 0, 0.01), M_WOOD, bevel=0.006))
    objects.append(box("TrayRimLongA", (0.5, 0.02, 0.05), (0, -0.14, 0.035), M_WOOD, bevel=0.004))
    objects.append(box("TrayRimLongB", (0.5, 0.02, 0.05), (0, 0.14, 0.035), M_WOOD, bevel=0.004))
    objects.append(box("TrayRimShortA", (0.02, 0.3, 0.05), (-0.24, 0, 0.035), M_WOOD, bevel=0.004))
    objects.append(box("TrayRimShortB", (0.02, 0.3, 0.05), (0.24, 0, 0.035), M_WOOD, bevel=0.004))
    for index in range(7):
        objects.append(box("TraySlat", (0.44, 0.012, 0.008), (0, -0.12 + index * 0.04, 0.024), M_WOOD, bevel=0.002))

    # Banana leaf
    leaf = sphere("BananaLeaf", 0.2, (0, 0, 0.006), M_GREEN, scale=(1.0, 0.38, 0.03), segments=32, rings=8)
    objects.append(leaf)
    objects.append(box("BananaLeafVein", (0.32, 0.008, 0.008), (0, 0, 0.018), M_FOOD_STEM, bevel=0.002))

    return objects


# ---------------------------------------------------------------------------
# Foods
# ---------------------------------------------------------------------------
def build_rendang():
    parts = [cyl("RendangSauce", 0.055, 0.008, (0, 0, 0.004), M_FOOD_RENDANG_SAUCE, vertices=24)]
    positions = [
        (-0.025, -0.012, 0.02, 0.3),
        (0.015, -0.02, 0.022, -0.2),
        (0.03, 0.01, 0.02, 0.6),
        (-0.012, 0.025, 0.021, -0.5),
        (0.005, 0.0, 0.038, 0.1),
    ]
    for index, (x, y, z, angle) in enumerate(positions):
        parts.append(box(f"RendangChunk_{index}", (0.03, 0.024, 0.02), (x, y, z), M_FOOD_RENDANG, bevel=0.006, rotation=(0, 0, angle)))
    parts.append(cyl("RendangChili", 0.005, 0.035, (0.02, -0.035, 0.03), M_FOOD_TOMATO, rotation=(math.pi / 2, 0, 0.4), vertices=12))
    food = join_objects(parts, "Food_rendang")
    return origin_bottom_center(food)


def build_burger():
    parts = [
        cyl("BurgerBottomBun", 0.06, 0.025, (0, 0, 0.0125), M_FOOD_BUN, vertices=32),
        cyl("BurgerPatty", 0.062, 0.018, (0, 0, 0.034), M_FOOD_PATTY, vertices=32),
        box("BurgerCheese", (0.11, 0.11, 0.006), (0, 0, 0.047), M_FOOD_CHEESE, bevel=0.002, rotation=(0, 0, math.radians(20))),
        sphere("BurgerLettuce", 0.07, (0, 0, 0.057), M_FOOD_LETTUCE, scale=(1, 1, 0.16), segments=24, rings=8),
        cyl("BurgerTomato", 0.058, 0.008, (0, 0, 0.065), M_FOOD_TOMATO, vertices=32),
        sphere("BurgerTopBun", 0.065, (0, 0, 0.088), M_FOOD_BUN, scale=(1, 1, 0.55), segments=32, rings=12),
    ]
    for index in range(7):
        angle = index * math.tau / 7
        parts.append(sphere("Sesame", 0.006, (0.035 * math.cos(angle), 0.035 * math.sin(angle), 0.112), M_FOOD_BUN, scale=(1.4, 0.8, 0.4), segments=8, rings=4))
    food = join_objects(parts, "Food_burger")
    return origin_bottom_center(food)


def build_telur_rebus():
    parts = [
        sphere("BoiledEggWhole", 0.03, (-0.032, 0, 0.04), M_FOOD_EGG_WHITE, scale=(1, 1, 1.3), segments=24, rings=12),
        cyl("BoiledEggHalf", 0.032, 0.05, (0.032, 0, 0.025), M_FOOD_EGG_WHITE, vertices=24),
        sphere("BoiledEggYolk", 0.018, (0.032, 0, 0.052), M_FOOD_YOLK, scale=(1, 1, 0.45), segments=20, rings=8),
    ]
    food = join_objects(parts, "Food_telur_rebus")
    return origin_bottom_center(food)


def build_telur_ceplok():
    parts = [
        sphere("FriedEggWhite", 0.065, (0, 0, 0.006), M_FOOD_EGG_WHITE, scale=(1.0, 0.85, 0.09), segments=32, rings=10),
        sphere("FriedEggYolk", 0.025, (0.008, 0.005, 0.016), M_FOOD_YOLK, scale=(1, 1, 0.5), segments=24, rings=10),
    ]
    for index in range(5):
        angle = index * math.tau / 5
        parts.append(sphere("FriedEggCrisp", 0.018, (0.055 * math.cos(angle), 0.045 * math.sin(angle), 0.008), M_FOOD_EGG_WHITE, scale=(1, 1, 0.15), segments=12, rings=6))
    food = join_objects(parts, "Food_telur_ceplok")
    return origin_bottom_center(food)


def build_tempe():
    parts = []
    for index in range(5):
        parts.append(box(f"TempeSlice_{index}", (0.06, 0.038, 0.009), (-0.02 + index * 0.012, -0.01 + index * 0.008, 0.006 + index * 0.009), M_FOOD_TEMPEH, bevel=0.003, rotation=(0, 0, -0.12 + index * 0.06)))
    for index in range(8):
        parts.append(sphere("TempeSpeck", 0.0025, (-0.025 + (index % 4) * 0.018, -0.015 + (index // 4) * 0.02, 0.045), M_FOOD_TEMPEH_DARK, segments=6, rings=3))
    food = join_objects(parts, "Food_tempe")
    return origin_bottom_center(food)


def build_brokoli():
    parts = []
    positions = [(-0.04, -0.02), (0.0, -0.035), (0.04, -0.02), (-0.025, 0.02), (0.025, 0.025), (0.0, 0.0)]
    for index, (x, y) in enumerate(positions):
        parts.append(cyl(f"BroccoliStem_{index}", 0.008, 0.035, (x, y, 0.018), M_FOOD_STEM, vertices=12))
        parts.append(sphere(f"BroccoliFloret_{index}", 0.026, (x, y, 0.045), M_FOOD_BROCCOLI if index % 2 else M_FOOD_LETTUCE, scale=(1, 1, 0.85), segments=16, rings=8))
    food = join_objects(parts, "Food_brokoli")
    return origin_bottom_center(food)


def build_pisang():
    parts = []
    for index in range(3):
        y = -0.025 + index * 0.025
        points = [(-0.065, y, 0.025), (-0.02, y + 0.012, 0.04), (0.025, y + 0.012, 0.04), (0.065, y, 0.025)]
        parts.append(tube(f"Banana_{index}", points, 0.012, M_FOOD_BANANA))
        parts.append(sphere("BananaTipA", 0.009, (-0.067, y, 0.025), M_FOOD_BANANA_TIP, segments=10, rings=5))
        parts.append(sphere("BananaTipB", 0.009, (0.067, y, 0.025), M_FOOD_BANANA_TIP, segments=10, rings=5))
    food = join_objects(parts, "Food_pisang")
    return origin_bottom_center(food)


def build_mie_goreng():
    parts = []
    for index in range(9):
        angle = index * math.tau / 9
        points = [
            (0.025 * math.cos(angle), 0.025 * math.sin(angle), 0.008),
            (0.055 * math.cos(angle + 0.6), 0.055 * math.sin(angle + 0.6), 0.022),
            (0.035 * math.cos(angle + 1.2), 0.035 * math.sin(angle + 1.2), 0.035),
            (0.012 * math.cos(angle + 1.8), 0.012 * math.sin(angle + 1.8), 0.028),
        ]
        parts.append(tube(f"Noodle_{index}", points, 0.0045, M_FOOD_NOODLE, resolution=8))
    for index in range(4):
        angle = index * math.tau / 4
        parts.append(cyl("GreenOnion", 0.004, 0.012, (0.035 * math.cos(angle), 0.035 * math.sin(angle), 0.038), M_FOOD_ONION, rotation=(math.pi / 2, 0, angle), vertices=8))
    parts.append(box("NoodleTomato", (0.02, 0.014, 0.012), (0.02, -0.03, 0.035), M_FOOD_TOMATO, bevel=0.003))
    food = join_objects(parts, "Food_mie_goreng")
    return origin_bottom_center(food)


# ---------------------------------------------------------------------------
# Build everything
# ---------------------------------------------------------------------------
room_objects = build_room()
island_objects = build_island()
dining_objects = build_dining()
decor_objects = build_decor()
plating_objects = build_plating()
food_objects = [
    build_rendang(),
    build_burger(),
    build_telur_rebus(),
    build_telur_ceplok(),
    build_tempe(),
    build_brokoli(),
    build_pisang(),
    build_mie_goreng(),
]

export_fbx(room_objects, ENV_DIR / "KitchenRoom.fbx")
export_fbx(island_objects, ENV_DIR / "KitchenIsland.fbx")
export_fbx(dining_objects, ENV_DIR / "DiningSet.fbx")
export_fbx(decor_objects, ENV_DIR / "Decor.fbx")

export_fbx([obj for obj in plating_objects if obj.name == "Platter_Oval"], PLATE_DIR / "Platter_Oval.fbx")
export_fbx([obj for obj in plating_objects if obj.name == "Plate_Small"], PLATE_DIR / "Plate_Small.fbx")
export_fbx([obj for obj in plating_objects if obj.name == "Bowl_Small"], PLATE_DIR / "Bowl_Small.fbx")
export_fbx([obj for obj in plating_objects if obj.name.startswith("Tray")], PLATE_DIR / "Tray_Bamboo.fbx")
export_fbx([obj for obj in plating_objects if obj.name.startswith("BananaLeaf")], PLATE_DIR / "BananaLeaf.fbx")

for food in food_objects:
    export_fbx([food], FOOD_DIR / f"{food.name}.fbx")

# Save the source scene
bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_DIR / "Scene1_Home.blend"))

# ---------------------------------------------------------------------------
# Previews
# ---------------------------------------------------------------------------
def look_at(camera, target):
    direction = target - camera.location
    camera.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def setup_render():
    scene.render.resolution_x = 1280
    scene.render.resolution_y = 720
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    try:
        scene.render.engine = "BLENDER_EEVEE_NEXT"
    except Exception:
        scene.render.engine = "BLENDER_WORKBENCH"
    world = bpy.data.worlds.new("PreviewWorld")
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs[0].default_value = (0.08, 0.08, 0.08, 1)
    world.node_tree.nodes["Background"].inputs[1].default_value = 0.4
    scene.world = world


def add_preview_lights():
    bpy.ops.object.light_add(type="SUN", location=(2, -3, 4))
    sun = bpy.context.object
    sun.data.energy = 4.0
    sun.rotation_euler = (math.radians(50), math.radians(-20), math.radians(30))
    bpy.ops.object.light_add(type="AREA", location=(-2, 1, 2.5))
    area = bpy.context.object
    area.data.energy = 250
    area.data.size = 3.0
    look_at(area, Vector((0, 0, 1.0)))


setup_render()
add_preview_lights()

bpy.ops.object.camera_add(location=(2.2, 2.1, 1.7))
camera = bpy.context.object
camera.data.lens = 24
look_at(camera, Vector((-0.3, -1.0, 1.0)))
scene.camera = camera
scene.render.filepath = str(PREVIEW_DIR / "preview_room.png")
bpy.ops.render.render(write_still=True)

# Food preview: duplicate foods into a grid on a plane, hide the room.
preview_objects = []
bpy.ops.mesh.primitive_plane_add(size=2.0, location=(0, 0, -0.005))
plane = bpy.context.object
plane.name = "PreviewPlane"
plane.data.materials.append(mat("M_Preview", (0.78, 0.76, 0.72), roughness=0.8))
for index, food in enumerate(food_objects):
    duplicate = food.copy()
    duplicate.data = food.data.copy()
    bpy.context.collection.objects.link(duplicate)
    column = index % 4
    row = index // 4
    duplicate.location = (-0.33 + column * 0.22, -0.14 + row * 0.28, 0)
    preview_objects.append(duplicate)

hidden_states = []
for obj in scene.objects:
    if obj.type == "MESH" and obj not in preview_objects and obj != plane:
        hidden_states.append((obj, obj.hide_render))
        obj.hide_render = True

bpy.ops.object.camera_add(location=(0, -1.1, 0.72))
food_camera = bpy.context.object
food_camera.data.lens = 50
look_at(food_camera, Vector((0, 0, 0.04)))
scene.camera = food_camera
scene.render.filepath = str(PREVIEW_DIR / "preview_foods.png")
bpy.ops.render.render(write_still=True)

for obj, state in hidden_states:
    obj.hide_render = state
for duplicate in preview_objects:
    bpy.data.objects.remove(duplicate, do_unlink=True)
bpy.data.objects.remove(plane, do_unlink=True)

print("Scene 1 assets built.")
