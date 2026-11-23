"""Author the NutriPick Warteg kit.

Coordinate convention: Blender Z up, open front toward -Y.
Unity receives +Z as the front after FBX export (axis_forward=-Z, axis_up=Y).
"""

import math
from pathlib import Path

import bpy
from mathutils import Vector

ROOT = Path(r"D:\Unity Projects\VR_Project")
OUT_DIR = ROOT / "Assets" / "Models" / "Environment" / "Warteg"
BLEND_DIR = ROOT / "Blender" / "Scene2_Warteg"
for folder in (OUT_DIR, BLEND_DIR):
    folder.mkdir(parents=True, exist_ok=True)

# ---------------------------------------------------------------------------
# Reset without restarting the shared Blender session
# ---------------------------------------------------------------------------
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
for block in (bpy.data.meshes, bpy.data.materials, bpy.data.curves, bpy.data.armatures):
    for item in list(block):
        if item.users == 0:
            block.remove(item)

scene = bpy.context.scene
scene.unit_settings.system = "METRIC"
scene.unit_settings.scale_length = 1.0
scene.render.fps = 30

# Interior height. The ceiling is raised without moving the counter or signage.
CEILING_Z = 3.40
WALL_HEIGHT = 3.34
WALL_CENTER_Z = 0.16 + WALL_HEIGHT / 2
ROOF_Z = 3.56
ROOF_RIDGE_Z = 3.68

# ---------------------------------------------------------------------------
# Materials
# ---------------------------------------------------------------------------
MATS = {}


def mat(name, color, roughness=0.7, metallic=0.0, emission=None, alpha=1.0):
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
        bsdf.inputs["Emission Strength"].default_value = 2.5
    if alpha < 1.0:
        bsdf.inputs["Alpha"].default_value = alpha
        for attr, value in (("surface_render_method", "BLENDED"), ("blend_method", "BLEND")):
            try:
                setattr(material, attr, value)
            except Exception:
                pass
    material.diffuse_color = (*color, alpha)
    MATS[name] = material
    return material


M_WALL = mat("M_Warteg_WallCream", (0.90, 0.85, 0.72), 0.8)
M_WALL_WARM = mat("M_Warteg_WallWarm", (0.80, 0.66, 0.48), 0.8)
M_WALL_LIGHT = mat("M_Warteg_WallLight", (0.96, 0.92, 0.82), 0.8)
M_WOOD = mat("M_Warteg_Wood", (0.52, 0.34, 0.19), 0.65)
M_WOOD_DARK = mat("M_Warteg_WoodDark", (0.30, 0.19, 0.10), 0.65)
M_TERRACOTTA = mat("M_Warteg_Terracotta", (0.66, 0.30, 0.18), 0.7)
M_TILE = mat("M_Warteg_Tile", (0.76, 0.70, 0.60), 0.55)
M_TILE_DARK = mat("M_Warteg_TileDark", (0.32, 0.30, 0.28), 0.55)
M_AWNING = mat("M_Warteg_AwningRed", (0.62, 0.12, 0.10), 0.8)
M_AWNING_CREAM = mat("M_Warteg_AwningCream", (0.94, 0.88, 0.74), 0.8)
M_METAL = mat("M_Warteg_Metal", (0.55, 0.56, 0.54), 0.35, metallic=1.0)
M_METAL_DARK = mat("M_Warteg_MetalDark", (0.07, 0.07, 0.07), 0.4, metallic=0.7)
M_GLASS = mat("M_Warteg_Glass", (0.55, 0.72, 0.70), 0.08, alpha=0.32)
M_BULB = mat("M_Warteg_Bulb", (1.0, 0.80, 0.45), 0.3, emission=(1.0, 0.62, 0.25))
M_LEAF = mat("M_Warteg_Leaf", (0.16, 0.40, 0.16), 0.7)
M_LEAF_LIGHT = mat("M_Warteg_LeafLight", (0.26, 0.52, 0.20), 0.7)
M_GAS = mat("M_Warteg_Gas", (0.82, 0.38, 0.10), 0.5)
M_CRATE_RED = mat("M_Warteg_CrateRed", (0.65, 0.12, 0.10), 0.6)
M_CRATE_BLUE = mat("M_Warteg_CrateBlue", (0.10, 0.24, 0.45), 0.6)
M_CHALK = mat("M_Warteg_Chalkboard", (0.07, 0.16, 0.12), 0.8)
M_SIGN = mat("M_Warteg_SignGreen", (0.06, 0.22, 0.13), 0.6)
M_GOLD = mat("M_Warteg_SignGold", (0.84, 0.64, 0.22), 0.45, metallic=0.3)
M_CERAMIC = mat("M_Warteg_Ceramic", (0.94, 0.92, 0.86), 0.3)
M_RICE = mat("M_Warteg_Rice", (0.93, 0.90, 0.82), 0.7)
M_CONCRETE = mat("M_Warteg_Concrete", (0.62, 0.60, 0.54), 0.8)

# ---------------------------------------------------------------------------
# Collections
# ---------------------------------------------------------------------------
def collection(name):
    existing = bpy.data.collections.get(name)
    if existing:
        return existing
    col = bpy.data.collections.new(name)
    scene.collection.children.link(col)
    return col


COL_SHELL = collection("EXPORT_WartegShell")
COL_COUNTER = collection("EXPORT_WartegCounter")
COL_FURNITURE = collection("EXPORT_WartegFurniture")
COL_PROPS = collection("EXPORT_WartegProps")
COL_ALL = (COL_SHELL, COL_COUNTER, COL_FURNITURE, COL_PROPS)


def move_to(obj, target):
    for col in list(obj.users_collection):
        col.objects.unlink(obj)
    target.objects.link(obj)


def apply_scale(obj):
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)


def bevel_object(obj, width, segments=1):
    modifier = obj.modifiers.new("Bevel", "BEVEL")
    modifier.width = width
    modifier.segments = segments
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_apply(modifier=modifier.name)
    return obj


def torus(name, major_radius, minor_radius, loc, material, target,
          major_segments=12, minor_segments=6):
    bpy.ops.mesh.primitive_torus_add(
        major_radius=major_radius,
        minor_radius=minor_radius,
        major_segments=major_segments,
        minor_segments=minor_segments,
        location=loc,
    )
    obj = bpy.context.object
    obj.name = name
    obj.data.materials.append(material)
    move_to(obj, target)
    return obj


def box(name, size, loc, material, target, rot=(0, 0, 0), bevel=0.0):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc, rotation=rot)
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
    move_to(obj, target)
    return obj


def cyl(name, radius, depth, loc, material, target, rot=(0, 0, 0), vertices=12):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=loc, rotation=rot)
    obj = bpy.context.object
    obj.name = name
    obj.data.materials.append(material)
    move_to(obj, target)
    return obj


def cone(name, r_bottom, r_top, depth, loc, material, target, rot=(0, 0, 0), vertices=12):
    bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=r_bottom, radius2=r_top, depth=depth, location=loc, rotation=rot)
    obj = bpy.context.object
    obj.name = name
    obj.data.materials.append(material)
    move_to(obj, target)
    return obj


def sphere(name, radius, loc, material, target, scale=(1, 1, 1), segments=12, rings=8):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, radius=radius, location=loc)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    apply_scale(obj)
    obj.data.materials.append(material)
    move_to(obj, target)
    return obj


def sign_text(name, body, loc, size, material, target, extrude=0.008,
              max_width=2.4, max_height=0.24, spacing=1.08):
    """Extruded lettering for the awning banner: geometry, no textures."""
    bpy.ops.object.text_add(location=loc, rotation=(math.pi / 2, 0, 0))
    obj = bpy.context.object
    obj.name = name
    obj.data.body = body
    obj.data.size = size
    obj.data.align_x = "CENTER"
    obj.data.align_y = "CENTER"
    obj.data.space_character = spacing
    obj.data.extrude = extrude
    obj.data.resolution_u = 3
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.ops.object.convert(target="MESH")
    obj = bpy.context.object
    xs = [v.co.x for v in obj.data.vertices]
    ys = [v.co.y for v in obj.data.vertices]
    width, height = max(xs) - min(xs), max(ys) - min(ys)
    fit = min(max_width / width if width else 1.0,
              max_height / height if height else 1.0, 1.0)
    obj.scale = (fit, fit, fit)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    obj.data.materials.append(material)
    move_to(obj, target)
    return obj


# ===========================================================================
# SHELL
# ===========================================================================
box("FloorBase", (8.2, 6.2, 0.16), (0, 0, 0.08), M_TERRACOTTA, COL_SHELL)
box("FloorBorder_N", (8.0, 0.25, 0.02), (0, 2.85, 0.17), M_TILE, COL_SHELL)
box("FloorBorder_S", (8.0, 0.25, 0.02), (0, -2.85, 0.17), M_TILE, COL_SHELL)
box("FloorBorder_W", (0.25, 5.6, 0.02), (-3.85, 0, 0.17), M_TILE, COL_SHELL)
box("FloorBorder_E", (0.25, 5.6, 0.02), (3.85, 0, 0.17), M_TILE, COL_SHELL)
box("FloorMat", (2.2, 1.1, 0.02), (0, -1.9, 0.18), M_TILE_DARK, COL_SHELL)
box("StepFront", (8.2, 0.85, 0.12), (0, -3.45, 0.06), M_CONCRETE, COL_SHELL)

box("BackWall", (8.2, 0.16, WALL_HEIGHT), (0, 3.0, WALL_CENTER_Z), M_WALL, COL_SHELL)
box("LeftWall", (0.16, 6.0, WALL_HEIGHT), (-4.05, 0, WALL_CENTER_Z), M_WALL, COL_SHELL)
box("RightWall", (0.16, 6.0, WALL_HEIGHT), (4.05, 0, WALL_CENTER_Z), M_WALL, COL_SHELL)
box("FrontLintel", (8.2, 0.18, 1.10), (0, -3.0, 2.96), M_WALL, COL_SHELL)
box("PillarL", (0.28, 0.30, 2.25), (-3.95, -3.0, 1.285), M_WALL_WARM, COL_SHELL, bevel=0.01)
box("PillarR", (0.28, 0.30, 2.25), (3.95, -3.0, 1.285), M_WALL_WARM, COL_SHELL, bevel=0.01)

box("WainscotBack", (8.0, 0.05, 1.05), (0, 2.90, 0.76), M_WOOD, COL_SHELL)
box("WainscotL", (0.05, 5.8, 1.05), (-3.96, 0, 0.76), M_WOOD, COL_SHELL)
box("WainscotR", (0.05, 5.8, 1.05), (3.96, 0, 0.76), M_WOOD, COL_SHELL)
box("ChairRailBack", (8.0, 0.07, 0.07), (0, 2.86, 1.31), M_WOOD_DARK, COL_SHELL)
box("ChairRailL", (0.07, 5.8, 0.07), (-3.92, 0, 1.31), M_WOOD_DARK, COL_SHELL)
box("ChairRailR", (0.07, 5.8, 0.07), (3.92, 0, 1.31), M_WOOD_DARK, COL_SHELL)

box("Ceiling", (7.8, 5.8, 0.06), (0, 0.1, CEILING_Z), M_WALL_LIGHT, COL_SHELL)
box("Roof", (8.8, 6.8, 0.18), (0, 0.1, ROOF_Z), M_METAL_DARK, COL_SHELL)
for i in range(10):
    x = -4.0 + i * 0.8
    box(f"RoofRidge_{i:02d}", (0.06, 6.6, 0.06), (x, 0.1, ROOF_RIDGE_Z), M_METAL, COL_SHELL)

awning_rot = (math.radians(10.6), 0, 0)
box("Awning", (8.8, 2.7, 0.06), (0, -4.35, 2.45), M_AWNING, COL_SHELL, rot=awning_rot)
for i, x in enumerate((-3.3, -1.1, 1.1, 3.3)):
    box(f"AwningStripe_{i}", (0.6, 2.7, 0.025), (x, -4.35, 2.49), M_AWNING_CREAM, COL_SHELL, rot=awning_rot)
box("AwningValance", (8.8, 0.06, 0.30), (0, -5.67, 2.05), M_AWNING, COL_SHELL)
for i in range(8):
    x = -3.5 + i * 1.0
    box(f"ValanceStripe_{i}", (0.35, 0.08, 0.30), (x, -5.70, 2.05), M_AWNING_CREAM, COL_SHELL)
box("AwningBanner", (5.4, 0.05, 0.30), (0, -5.735, 2.05), M_AWNING_CREAM, COL_SHELL)
box("BannerBorderTop", (5.5, 0.04, 0.045), (0, -5.765, 2.22), M_GOLD, COL_SHELL)
box("BannerBorderBottom", (5.5, 0.04, 0.045), (0, -5.765, 1.88), M_GOLD, COL_SHELL)
box("BannerBorderLeft", (0.045, 0.04, 0.36), (-2.73, -5.765, 2.05), M_GOLD, COL_SHELL)
box("BannerBorderRight", (0.045, 0.04, 0.36), (2.73, -5.765, 2.05), M_GOLD, COL_SHELL)
sign_text("SignBanner_Text", "WARTEG", (0, -5.775, 2.048), 0.36, M_SIGN, COL_SHELL)
for i, x in enumerate((-4.0, -2.9, 2.9, 4.0)):
    cyl(f"AwningPost_{i}", 0.06, 2.05, (x, -5.35, 1.185), M_METAL_DARK, COL_SHELL, vertices=10)

box("BackWindowFrame", (1.6, 0.05, 1.15), (2.1, 2.89, 1.98), M_WOOD_DARK, COL_SHELL)
box("BackWindowGlass", (1.4, 0.02, 0.95), (2.1, 2.86, 1.98), M_GLASS, COL_SHELL)
box("BackWindowMullionV", (0.05, 0.03, 0.95), (2.1, 2.845, 1.98), M_WOOD_DARK, COL_SHELL)
box("BackWindowMullionH", (1.4, 0.03, 0.05), (2.1, 2.845, 1.98), M_WOOD_DARK, COL_SHELL)

box("MenuBoardFrame", (1.7, 0.06, 0.95), (-2.1, 2.88, 1.95), M_WOOD_DARK, COL_SHELL)
box("MenuBoard", (1.55, 0.02, 0.82), (-2.1, 2.84, 1.95), M_CHALK, COL_SHELL)

box("WallShelf", (2.8, 0.30, 0.05), (0.6, 2.72, 2.20), M_WOOD, COL_SHELL)
box("ShelfBracketL", (0.05, 0.26, 0.20), (-0.7, 2.78, 2.08), M_METAL_DARK, COL_SHELL)
box("ShelfBracketR", (0.05, 0.26, 0.20), (1.9, 2.78, 2.08), M_METAL_DARK, COL_SHELL)
for i, x in enumerate((-0.4, 0.2, 1.0, 1.5)):
    cyl(f"ShelfJar_{i}", 0.08, 0.22, (x, 2.70, 2.34), M_CERAMIC if i % 2 == 0 else M_GLASS, COL_SHELL, vertices=10)

box("PosterL", (0.02, 1.1, 0.8), (-3.93, -1.6, 2.05), M_TERRACOTTA, COL_SHELL)
box("PosterLInner", (0.015, 0.9, 0.6), (-3.92, -1.6, 2.05), M_AWNING_CREAM, COL_SHELL)
box("PosterR", (0.02, 0.9, 0.7), (3.93, 1.9, 2.05), M_AWNING_CREAM, COL_SHELL)
box("PosterRInner", (0.015, 0.7, 0.5), (3.92, 1.9, 2.05), M_TERRACOTTA, COL_SHELL)

# ===========================================================================
# COUNTER
# ===========================================================================
box("CounterBase", (6.4, 0.95, 0.78), (0, 1.4, 0.55), M_WOOD_DARK, COL_COUNTER, bevel=0.012)
for i in range(7):
    x = -2.7 + i * 0.9
    box(f"CounterPanel_{i}", (0.74, 0.03, 0.62), (x, 0.915, 0.53), M_WOOD, COL_COUNTER, bevel=0.004)
box("CounterTop", (6.7, 1.05, 0.06), (0, 1.4, 0.97), M_METAL, COL_COUNTER, bevel=0.010)
for i in range(7):
    x = -2.7 + i * 0.9
    box(f"TrayWell_{i}", (0.72, 0.48, 0.04), (x, 1.45, 1.02), M_METAL_DARK, COL_COUNTER)
# Seven filled tray positions so the case reads as a warteg food display.
tray_foods = (M_RICE, M_GOLD, M_LEAF, M_TERRACOTTA, M_CRATE_RED, M_GOLD, M_RICE)
for i in range(7):
    x = -2.7 + i * 0.9
    box(f"TrayFood_{i}", (0.64, 0.40, 0.045), (x, 1.45, 1.0625), tray_foods[i], COL_COUNTER)
    box(f"TrayFoodHeap_{i}", (0.46, 0.28, 0.035), (x, 1.44, 1.095), tray_foods[i], COL_COUNTER)

box("GlassFront", (6.4, 0.03, 0.42), (0, 0.95, 1.25), M_GLASS, COL_COUNTER)
box("GlassTop", (6.4, 0.62, 0.03), (0, 1.26, 1.47), M_GLASS, COL_COUNTER)
box("GlassSideL", (0.03, 0.62, 0.42), (-3.2, 1.26, 1.25), M_GLASS, COL_COUNTER)
box("GlassSideR", (0.03, 0.62, 0.42), (3.2, 1.26, 1.25), M_GLASS, COL_COUNTER)
box("DisplayBack", (6.4, 0.04, 0.46), (0, 1.58, 1.24), M_WOOD, COL_COUNTER)
for x in (-3.22, 3.22):
    for y in (0.95, 1.58):
        box(f"DisplayFramePost_{x:+.1f}_{y:.2f}", (0.05, 0.05, 0.52), (x, y, 1.25), M_METAL, COL_COUNTER)
for y in (0.95, 1.58):
    box(f"DisplayFrameTop_{y:.2f}", (6.45, 0.05, 0.04), (0, y, 1.47), M_METAL, COL_COUNTER)
    box(f"DisplayFrameBottom_{y:.2f}", (6.45, 0.05, 0.04), (0, y, 1.03), M_METAL, COL_COUNTER)
box("DisplayLight", (6.0, 0.05, 0.03), (0, 1.25, 1.44), M_BULB, COL_COUNTER)

box("CashierTop", (1.4, 0.7, 0.06), (2.7, -1.6, 0.76), M_WOOD, COL_COUNTER, bevel=0.010)
box("CashierApron", (1.25, 0.55, 0.05), (2.7, -1.6, 0.70), M_WOOD, COL_COUNTER, bevel=0.005)
box("CashierShelf", (1.20, 0.50, 0.03), (2.7, -1.6, 0.30), M_WOOD_DARK, COL_COUNTER, bevel=0.004)
for dx in (-0.6, 0.6):
    for dy in (-0.28, 0.28):
        box(f"CashierLeg_{dx:+.1f}_{dy:+.2f}", (0.07, 0.07, 0.60), (2.7 + dx, -1.6 + dy, 0.46), M_WOOD_DARK, COL_COUNTER, bevel=0.004)
box("CashierPanel", (1.3, 0.03, 0.38), (2.7, -1.94, 0.56), M_WALL_WARM, COL_COUNTER, bevel=0.004)
box("CashierBoard", (0.55, 0.03, 0.20), (2.7, -1.24, 1.05), M_WOOD_DARK, COL_COUNTER, rot=(math.radians(-18), 0, 0), bevel=0.004)
box("CashierBoxA", (0.22, 0.16, 0.08), (3.15, -1.55, 0.83), M_CERAMIC, COL_COUNTER)
box("CashierBoxB", (0.22, 0.16, 0.08), (3.15, -1.55, 0.91), M_CERAMIC, COL_COUNTER)

box("PrepTableBase", (1.35, 0.55, 0.74), (3.3, 2.35, 0.53), M_WOOD_DARK, COL_COUNTER, bevel=0.008)
box("PrepTableTop", (1.45, 0.62, 0.05), (3.3, 2.35, 0.93), M_METAL, COL_COUNTER, bevel=0.006)
box("PrepTableShelf", (1.20, 0.45, 0.03), (3.3, 2.35, 0.30), M_WOOD, COL_COUNTER, bevel=0.004)
cyl("GasCylinder", 0.16, 0.58, (3.75, 2.55, 0.47), M_GAS, COL_COUNTER, vertices=12)
cyl("GasValve", 0.05, 0.10, (3.75, 2.55, 0.81), M_METAL_DARK, COL_COUNTER, vertices=8)
cyl("RiceCookerBody", 0.20, 0.28, (2.95, 2.35, 1.10), M_CERAMIC, COL_COUNTER, vertices=14)
cyl("RiceCookerLid", 0.21, 0.05, (2.95, 2.35, 1.265), M_METAL, COL_COUNTER, vertices=14)
cyl("RiceCookerKnob", 0.05, 0.05, (2.95, 2.35, 1.315), M_METAL_DARK, COL_COUNTER, vertices=8)
cyl("BurnerA", 0.14, 0.03, (2.35, 2.15, 0.99), M_METAL_DARK, COL_COUNTER, vertices=12)
cyl("BurnerB", 0.14, 0.03, (1.95, 2.15, 0.99), M_METAL_DARK, COL_COUNTER, vertices=12)
cyl("Wok", 0.25, 0.10, (2.35, 2.15, 1.04), M_METAL_DARK, COL_COUNTER, vertices=14)
cyl("Pot", 0.15, 0.20, (1.95, 2.15, 1.10), M_METAL, COL_COUNTER, vertices=12)

box("CondimentTray", (0.62, 0.26, 0.04), (2.05, 1.02, 1.02), M_METAL_DARK, COL_COUNTER)
for i, x in enumerate((1.86, 2.05, 2.24)):
    cyl(f"CondimentJar_{i}", 0.065, 0.15, (x, 1.02, 1.11), M_GLASS if i == 1 else M_CERAMIC, COL_COUNTER, vertices=10)

# ===========================================================================
# FURNITURE
# ===========================================================================
box("TableTopLong", (0.78, 2.6, 0.07), (-2.6, -1.4, 0.745), M_WOOD, COL_FURNITURE, bevel=0.012)
box("TableApronLong", (0.62, 2.42, 0.06), (-2.6, -1.4, 0.68), M_WOOD, COL_FURNITURE, bevel=0.006)
for dy in (-0.30, -2.50):
    box(f"TableCrossStretcher_{dy:+.2f}", (0.62, 0.06, 0.06), (-2.6, dy, 0.28), M_WOOD_DARK, COL_FURNITURE, bevel=0.004)
box("TableStretcher", (0.06, 2.30, 0.06), (-2.6, -1.4, 0.28), M_WOOD_DARK, COL_FURNITURE, bevel=0.004)
for dx in (-0.30, 0.30):
    for dy in (-1.10, 1.10):
        box(f"TableLeg_{dx:+.2f}_{dy:+.2f}", (0.07, 0.07, 0.62), (-2.6 + dx, -1.4 + dy, 0.47), M_WOOD_DARK, COL_FURNITURE, bevel=0.006)

for label, x in (("West", -3.25), ("East", -1.95)):
    box(f"Bench{label}Top", (0.32, 2.6, 0.06), (x, -1.4, 0.45), M_WOOD_DARK, COL_FURNITURE, bevel=0.010)
    box(f"Bench{label}Apron", (0.26, 2.42, 0.05), (x, -1.4, 0.395), M_WOOD, COL_FURNITURE, bevel=0.005)
    box(f"Bench{label}Stretcher", (0.06, 2.30, 0.05), (x, -1.4, 0.24), M_WOOD_DARK, COL_FURNITURE, bevel=0.004)
    for dx in (-0.11, 0.11):
        for dy in (-1.08, 1.08):
            box(f"Bench{label}Leg_{dx:+.2f}_{dy:+.2f}", (0.06, 0.06, 0.29),
                (x + dx, -1.4 + dy, 0.305), M_WOOD, COL_FURNITURE, bevel=0.004)

for i, (x, y) in enumerate(((1.75, -1.05), (2.45, -0.75))):
    seat = cyl(f"StoolSeat_{i}", 0.20, 0.06, (x, y, 0.47), M_WOOD, COL_FURNITURE, vertices=12)
    bevel_object(seat, 0.008, 2)
    for j in range(3):
        angle = j * math.tau / 3
        px = x + math.cos(angle) * 0.12
        py = y + math.sin(angle) * 0.12
        # The legs start at the interior floor (z=0.16) and reach the seat underside.
        leg = cyl(f"StoolLeg_{i}_{j}", 0.025, 0.295, (px, py, 0.3075), M_WOOD_DARK, COL_FURNITURE, vertices=8)
        bevel_object(leg, 0.004, 1)
    torus(f"StoolStretcher_{i}", 0.12, 0.012, (x, y, 0.30), M_WOOD_DARK, COL_FURNITURE)

# ===========================================================================
# PROPS
# ===========================================================================
for i, (x, y, z, material, rot) in enumerate((
    (3.6, -2.65, 0.33, M_CRATE_RED, 0.10),
    (3.62, -2.60, 0.68, M_CRATE_RED, -0.06),
    (3.15, -2.95, 0.33, M_CRATE_BLUE, 0.18),
)):
    box(f"Crate_{i}", (0.55, 0.40, 0.35), (x, y, z), material, COL_PROPS, rot=(0, 0, rot))

cyl("Bucket", 0.16, 0.30, (-3.45, -2.25, 0.31), M_CRATE_BLUE, COL_PROPS, vertices=12)
cyl("BucketHandle", 0.15, 0.02, (-3.45, -2.25, 0.46), M_METAL_DARK, COL_PROPS, vertices=12)

for i, x in enumerate((-2.0, 0.0, 2.0)):
    cyl(f"PendantCord_{i}", 0.008, 0.98, (x, 0.25, 2.885), M_METAL_DARK, COL_PROPS, vertices=6)
    cone(f"PendantShade_{i}", 0.18, 0.06, 0.16, (x, 0.25, 2.35), M_AWNING, COL_PROPS, vertices=12)
    sphere(f"PendantBulb_{i}", 0.055, (x, 0.25, 2.27), M_BULB, COL_PROPS, segments=10, rings=6)

box("DrinkCoolerBody", (0.70, 0.55, 1.70), (3.45, 0.30, 1.01), M_WALL_LIGHT, COL_PROPS, bevel=0.006)
box("DrinkCoolerGlass", (0.02, 0.45, 1.50), (3.09, 0.30, 1.05), M_GLASS, COL_PROPS)
box("DrinkCoolerFrameTop", (0.05, 0.50, 0.05), (3.08, 0.30, 1.82), M_METAL, COL_PROPS)
box("DrinkCoolerFrameBottom", (0.05, 0.50, 0.05), (3.08, 0.30, 0.30), M_METAL, COL_PROPS)
box("DrinkCoolerShelfA", (0.40, 0.45, 0.03), (3.30, 0.30, 0.85), M_METAL, COL_PROPS)
box("DrinkCoolerShelfB", (0.40, 0.45, 0.03), (3.30, 0.30, 1.35), M_METAL, COL_PROPS)
bottle_colors = (M_GAS, M_CRATE_RED, M_CRATE_BLUE, M_LEAF, M_GOLD, M_CRATE_RED, M_CRATE_BLUE, M_GAS)
for i, x in enumerate((3.18, 3.40)):
    for j, y in enumerate((-0.12, 0.06, 0.24, 0.42, 0.60)):
        material = bottle_colors[(i * 5 + j) % len(bottle_colors)]
        z = 0.74 if i == 0 else 1.24
        cyl(f"CoolerBottle_{i}_{j}", 0.045, 0.22, (x, y, z), material, COL_PROPS, vertices=8)

for i, x in enumerate((-5.0, 5.0)):
    y = -4.6
    cone(f"PlantPot_{i}", 0.14, 0.19, 0.32, (x, y, 0.30), M_TERRACOTTA, COL_PROPS, vertices=12)
    sphere(f"PlantFoliage_{i}", 0.28, (x, y, 0.72), M_LEAF, COL_PROPS, scale=(1.0, 1.0, 1.15), segments=12, rings=8)
    sphere(f"PlantFoliageTop_{i}", 0.18, (x, y, 0.94), M_LEAF_LIGHT, COL_PROPS, scale=(1.0, 1.0, 1.0), segments=10, rings=6)

cyl("PowerPole", 0.14, 6.0, (5.8, -4.8, 3.0), M_CONCRETE, COL_PROPS, vertices=10)
box("PowerPoleArm", (1.5, 0.10, 0.10), (5.8, -4.8, 5.55), M_WOOD_DARK, COL_PROPS)
for i, x in enumerate((5.25, 5.8, 6.35)):
    cyl(f"PowerInsulator_{i}", 0.05, 0.14, (x, -4.8, 5.68), M_CERAMIC, COL_PROPS, vertices=8)

# Wall clock, facing the interior (+Y on the back wall).
cyl("WallClock", 0.20, 0.04, (-3.1, 2.84, 2.30), M_CERAMIC, COL_PROPS, rot=(math.radians(90), 0, 0), vertices=16)
box("ClockHandA", (0.02, 0.02, 0.14), (-3.1, 2.81, 2.30), M_METAL_DARK, COL_PROPS)
box("ClockHandB", (0.10, 0.02, 0.02), (-3.06, 2.81, 2.30), M_METAL_DARK, COL_PROPS)

# ===========================================================================
# FBX handedness: Blender exports static meshes mirrored in X.
# Mirror the authored placement so Unity lands on the intended coordinates.
# ===========================================================================
for col in COL_ALL:
    for obj in col.objects:
        if obj.type != "MESH":
            continue
        obj.location.x *= -1.0
        obj.rotation_euler.y *= -1.0
        obj.rotation_euler.z *= -1.0

# ===========================================================================
# Lighting for preview render only (not part of the exports)
# ===========================================================================
preview_collection = collection("PREVIEW")
bpy.ops.object.light_add(type="SUN", location=(0, -10, 10))
sun = bpy.context.object
sun.name = "PreviewSun"
sun.data.energy = 3.0
sun.rotation_euler = (math.radians(50), 0, math.radians(-30))
move_to(sun, preview_collection)

bpy.ops.object.light_add(type="AREA", location=(0, -1.5, 2.55))
area = bpy.context.object
area.name = "PreviewFill"
area.data.energy = 350.0
area.data.shape = "RECTANGLE"
area.data.size = 4.0
area.data.size_y = 2.5
move_to(area, preview_collection)

world = bpy.data.worlds.new("PreviewWorld")
world.use_nodes = True
bg = next(node for node in world.node_tree.nodes if node.type == "BACKGROUND")
bg.inputs["Color"].default_value = (0.50, 0.58, 0.68, 1.0)
bg.inputs["Strength"].default_value = 0.6
scene.world = world

bpy.ops.object.camera_add(location=(0, -15.0, 6.2))
camera = bpy.context.object
camera.name = "PreviewCamera"
camera.data.lens = 34
direction = Vector((0, 0, 1.5)) - camera.location
camera.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
move_to(camera, preview_collection)
scene.camera = camera
scene.render.engine = "BLENDER_EEVEE"
scene.render.resolution_x = 1400
scene.render.resolution_y = 900
scene.render.film_transparent = False

# ===========================================================================
# Exports
# ===========================================================================
def export_collection(col, filename):
    bpy.ops.object.select_all(action="DESELECT")
    for obj in col.objects:
        if obj.type == "MESH":
            obj.select_set(True)
    if not any(obj.select_get() for obj in col.objects):
        print("nothing to export for", filename)
        return
    path = str(OUT_DIR / filename)
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
        object_types={"MESH"},
        path_mode="AUTO",
        embed_textures=False,
        add_leaf_bones=False,
    )
    print("exported", path)


print("Building Warteg kit...")
for col, filename in (
    (COL_SHELL, "WartegShell.fbx"),
    (COL_COUNTER, "WartegCounter.fbx"),
    (COL_FURNITURE, "WartegFurniture.fbx"),
    (COL_PROPS, "WartegProps.fbx"),
):
    export_collection(col, filename)

bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_DIR / "Warteg.blend"))
print("Warteg kit complete.")
