"""Rebuild / optimize the food models that were still rough.

Run headless:
    "C:\\Program Files\\Blender Foundation\\Blender 5.2\\blender.exe" --background --python Blender/Food/scripts/update_food_models.py

Outputs (imported by Unity afterwards):
    Assets/Models/Food/Home/Food_ayam_goreng.fbx   (decimated from yamgoreng.fbx)
    Assets/Models/Food/Home/Food_kerupuk.fbx
    Assets/Models/Food/Home/Food_nasi_putih.fbx
    Assets/Models/Food/Home/Food_sayur_singkong.fbx
"""

import math
import random
from pathlib import Path

import bpy
from mathutils import Matrix, Vector

ROOT = Path(__file__).resolve().parents[3]
FOOD_DIR = ROOT / "Assets" / "Models" / "Food"
HOME_DIR = FOOD_DIR / "Home"
HOME_DIR.mkdir(parents=True, exist_ok=True)

AYAM_SOURCE = FOOD_DIR / "yamgoreng.fbx"
AYAM_TARGET_TRIS = 4000
RANDOM_SEED = 20261123


# ---------------------------------------------------------------------------
# Scene helpers
# ---------------------------------------------------------------------------
def reset_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes, bpy.data.materials, bpy.data.curves):
        for block in list(datablocks):
            if block.users == 0:
                datablocks.remove(block)


def mat(name, color, roughness=0.6):
    material = bpy.data.materials.get(name)
    if material is None:
        material = bpy.data.materials.new(name)
    material.use_nodes = True
    bsdf = material.node_tree.nodes.get("Principled BSDF")
    if bsdf is not None:
        bsdf.inputs["Base Color"].default_value = (*color, 1.0)
        bsdf.inputs["Roughness"].default_value = roughness
    return material


def apply_transform(obj, location=False, rotation=True, scale=True):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.transform_apply(location=location, rotation=rotation, scale=scale)
    obj.select_set(False)


def join_objects(objects, name):
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    bpy.ops.object.join()
    joined = bpy.context.object
    joined.name = name
    return joined


def origin_bottom_center(obj):
    bpy.context.view_layer.update()

    # Bake the object's location into the mesh data first; otherwise the world
    # offset would be applied to local coordinates and the pivot drifts.
    obj.data.transform(Matrix.Translation(obj.location))
    obj.location = (0.0, 0.0, 0.0)
    obj.rotation_euler = (0.0, 0.0, 0.0)
    obj.scale = (1.0, 1.0, 1.0)
    bpy.context.view_layer.update()

    corners = [obj.matrix_world @ Vector(corner) for corner in obj.bound_box]
    min_corner = Vector((min(c.x for c in corners), min(c.y for c in corners), min(c.z for c in corners)))
    max_corner = Vector((max(c.x for c in corners), max(c.y for c in corners), max(c.z for c in corners)))
    offset = Vector(((min_corner.x + max_corner.x) * 0.5, (min_corner.y + max_corner.y) * 0.5, min_corner.z))

    obj.data.transform(Matrix.Translation(-offset))
    bpy.context.view_layer.update()
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


def prim(kind, **kwargs):
    if kind == "cube":
        bpy.ops.mesh.primitive_cube_add(**kwargs)
    elif kind == "cylinder":
        bpy.ops.mesh.primitive_cylinder_add(**kwargs)
    elif kind == "sphere":
        bpy.ops.mesh.primitive_uv_sphere_add(**kwargs)
    obj = bpy.context.object
    return obj


def box(name, size, location, material, bevel=0.0, rotation=(0, 0, 0)):
    obj = prim("cube", size=1.0, location=location, rotation=rotation)
    obj.name = name
    obj.scale = (size[0], size[1], size[2])
    apply_transform(obj)
    if bevel > 0:
        modifier = obj.modifiers.new("Bevel", "BEVEL")
        modifier.width = bevel
        modifier.segments = 1
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.modifier_apply(modifier=modifier.name)
    obj.data.materials.append(material)
    return obj


def cyl(name, radius, depth, location, material, rotation=(0, 0, 0), vertices=24):
    obj = prim("cylinder", radius=radius, depth=depth, location=location, rotation=rotation, vertices=vertices)
    obj.name = name
    apply_transform(obj)
    obj.data.materials.append(material)
    return obj


def sphere(name, radius, location, material, scale=(1, 1, 1), segments=16, rings=8, smooth=True):
    obj = prim("sphere", radius=radius, location=location, segments=segments, ring_count=rings)
    obj.name = name
    obj.scale = scale
    apply_transform(obj)
    obj.data.materials.append(material)
    if smooth:
        bpy.ops.object.shade_smooth()
    return obj


# ---------------------------------------------------------------------------
# Ayam goreng: decimate the imported 178k-tri model
# ---------------------------------------------------------------------------
def build_ayam():
    reset_scene()
    bpy.ops.import_scene.fbx(filepath=str(AYAM_SOURCE))

    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    if not meshes:
        raise RuntimeError("No mesh found in " + str(AYAM_SOURCE))

    for obj in meshes:
        apply_transform(obj, location=True)

    chicken = meshes[0] if len(meshes) == 1 else join_objects(meshes, "Food_ayam_goreng")
    chicken.name = "Food_ayam_goreng"

    triangles = sum(len(poly.vertices) - 2 for poly in chicken.data.polygons)
    ratio = min(1.0, AYAM_TARGET_TRIS / max(1, triangles))
    print("ayam tris before", triangles, "ratio", ratio)

    bpy.context.view_layer.objects.active = chicken
    chicken.select_set(True)
    modifier = chicken.modifiers.new("Decimate", "DECIMATE")
    modifier.decimate_type = "COLLAPSE"
    modifier.ratio = ratio
    modifier.use_collapse_triangulate = True
    bpy.ops.object.modifier_apply(modifier=modifier.name)

    bpy.ops.object.shade_smooth()

    # The source FBX is authored in centimetres (Unity compensated with a 67x
    # prefab scale). Bake the real-world size into the mesh instead.
    bpy.context.view_layer.update()
    dimensions = chicken.dimensions
    factor = 0.160 / max(dimensions.x, dimensions.y, dimensions.z)
    chicken.data.transform(Matrix.Scale(factor, 4))
    chicken.data.update()
    origin_bottom_center(chicken)

    bpy.context.view_layer.update()
    print("ayam size after rescale", tuple(round(v, 4) for v in chicken.dimensions))
    bpy.ops.object.select_all(action="DESELECT")
    chicken.select_set(True)
    bpy.context.view_layer.objects.active = chicken

    triangles = sum(len(poly.vertices) - 2 for poly in chicken.data.polygons)
    print("ayam tris after", triangles)
    export_fbx([chicken], HOME_DIR / "Food_ayam_goreng.fbx")


# ---------------------------------------------------------------------------
# Kerupuk: wavy low-poly cracker disc
# ---------------------------------------------------------------------------
def build_kerupuk():
    reset_scene()
    material = mat("M_Food_Kerupuk", (0.88, 0.79, 0.60), roughness=0.75)

    cracker = cyl("KerupukBody", 0.048, 0.010, (0, 0, 0.005), material, vertices=28)

    for vertex in cracker.data.vertices:
        x, y, z = vertex.co
        radius = math.hypot(x, y)
        if radius < 0.001:
            continue

        angle = math.atan2(y, x)
        ripple = 1.0 + 0.085 * math.sin(4.0 * angle) + 0.05 * math.sin(7.0 * angle + 1.3)
        vertex.co.x = x * ripple
        vertex.co.y = y * ripple
        if z > 0.008:
            vertex.co.z += 0.0035 * math.sin(3.0 * angle + 0.6) + 0.0015 * math.sin(9.0 * angle)

    cracker.data.update()
    origin_bottom_center(cracker)
    print("kerupuk tris", sum(len(p.vertices) - 2 for p in cracker.data.polygons), "size", tuple(round(v, 4) for v in cracker.dimensions))
    export_fbx([cracker], HOME_DIR / "Food_kerupuk.fbx")


# ---------------------------------------------------------------------------
# Nasi putih: irregular rice mound with a few grains
# ---------------------------------------------------------------------------
def build_nasi_putih():
    reset_scene()
    random.seed(RANDOM_SEED)
    rice = mat("M_Food_NasiPutih", (0.96, 0.95, 0.90), roughness=0.55)

    parts = [sphere("RiceMound", 0.062, (0, 0, 0.030), rice, scale=(1.0, 0.92, 0.55), segments=28, rings=14)]

    for index in range(14):
        angle = random.uniform(0, math.tau)
        radius = random.uniform(0.012, 0.050)
        x = math.cos(angle) * radius
        y = math.sin(angle) * radius
        z = 0.030 + 0.030 * math.sqrt(max(0.0, 1.0 - (radius / 0.062) ** 2)) - 0.004
        grain = sphere(
            f"RiceGrain_{index}",
            random.uniform(0.0045, 0.0065),
            (x, y, z),
            rice,
            scale=(1.9, 0.85, 0.55),
            segments=8,
            rings=4,
        )
        grain.rotation_euler = (random.uniform(-0.4, 0.4), random.uniform(-0.4, 0.4), angle)
        apply_transform(grain)
        parts.append(grain)

    mound = join_objects(parts, "Food_nasi_putih")
    origin_bottom_center(mound)
    print("nasi tris", sum(len(p.vertices) - 2 for p in mound.data.polygons), "size", tuple(round(v, 4) for v in mound.dimensions))
    export_fbx([mound], HOME_DIR / "Food_nasi_putih.fbx")


# ---------------------------------------------------------------------------
# Sayur singkong: sauce base with leaves and carrot strips
# ---------------------------------------------------------------------------
def build_sayur_singkong():
    reset_scene()
    random.seed(RANDOM_SEED + 1)
    sauce = mat("M_Food_SayurSauce", (0.55, 0.44, 0.12), roughness=0.45)
    leaf = mat("M_Food_SayurLeaf", (0.20, 0.34, 0.12), roughness=0.65)
    carrot = mat("M_Food_SayurCarrot", (0.80, 0.35, 0.06), roughness=0.5)

    parts = [sphere("SayurBase", 0.070, (0, 0, 0.012), sauce, scale=(1.0, 1.0, 0.22), segments=28, rings=10)]

    for index in range(14):
        angle = random.uniform(0, math.tau)
        radius = random.uniform(0.0, 0.052)
        length = random.uniform(0.020, 0.034)
        width = random.uniform(0.010, 0.018)
        x = math.cos(angle) * radius
        y = math.sin(angle) * radius
        z = 0.024 + random.uniform(0.0, 0.010)
        piece = box(
            f"SayurLeaf_{index}",
            (length, width, 0.0035),
            (x, y, z),
            leaf,
            bevel=0.001,
            rotation=(random.uniform(-0.35, 0.35), random.uniform(-0.35, 0.35), angle),
        )
        parts.append(piece)

    for index in range(5):
        angle = random.uniform(0, math.tau)
        radius = random.uniform(0.010, 0.040)
        x = math.cos(angle) * radius
        y = math.sin(angle) * radius
        z = 0.030 + random.uniform(0.0, 0.008)
        strip = box(
            f"SayurCarrot_{index}",
            (0.022, 0.006, 0.005),
            (x, y, z),
            carrot,
            bevel=0.001,
            rotation=(random.uniform(-0.2, 0.2), random.uniform(-0.3, 0.3), angle + 0.5),
        )
        parts.append(strip)

    dish = join_objects(parts, "Food_sayur_singkong")
    origin_bottom_center(dish)
    print("sayur tris", sum(len(p.vertices) - 2 for p in dish.data.polygons), "size", tuple(round(v, 4) for v in dish.dimensions))
    export_fbx([dish], HOME_DIR / "Food_sayur_singkong.fbx")


def main():
    print("ROOT", ROOT)
    build_ayam()
    build_kerupuk()
    build_nasi_putih()
    build_sayur_singkong()
    print("DONE")


if __name__ == "__main__":
    main()
