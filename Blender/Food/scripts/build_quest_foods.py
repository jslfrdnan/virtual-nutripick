"""Build the six quest foods and export them to Unity.

Run headless:
    "D:\\Blender Foundation\\Blender 5.2\\blender.exe" --background --python Blender/Food/scripts/build_quest_foods.py

Outputs:
    Assets/Models/Food/Home/Food_tahu.fbx
    Assets/Models/Food/Home/Food_jeruk.fbx
    Assets/Models/Food/Home/Food_wortel.fbx
    Assets/Models/Food/Home/Food_bayam.fbx
    Assets/Models/Food/Home/Food_ubi_jalar.fbx
    Assets/Models/Food/Home/Food_ikan_cakalang.fbx
    Assets/Textures/FoodIcons/<id>.png
    Blender/Food/quest_foods.blend
"""

import math
import os

import bpy
from mathutils import Matrix, Vector

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
FBX_DIR = os.path.join(ROOT, "Assets", "Models", "Food", "Home")
ICON_DIR = os.path.join(ROOT, "Assets", "Textures", "FoodIcons")
os.makedirs(ICON_DIR, exist_ok=True)


def reset_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for collection in (bpy.data.meshes, bpy.data.materials, bpy.data.curves):
        for block in list(collection):
            if block.users == 0:
                collection.remove(block)


def material(name, color, roughness=0.6, metallic=0.0):
    mat = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    mat.use_nodes = True
    bsdf = next(node for node in mat.node_tree.nodes if node.type == "BSDF_PRINCIPLED")
    bsdf.inputs["Base Color"].default_value = (*color, 1.0)
    bsdf.inputs["Roughness"].default_value = roughness
    bsdf.inputs["Metallic"].default_value = metallic
    mat.diffuse_color = (*color, 1.0)
    return mat


def apply_transform(obj):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    obj.select_set(False)


def join(parts, name):
    bpy.ops.object.select_all(action="DESELECT")
    for part in parts:
        part.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    obj = bpy.context.object
    obj.name = name
    return obj


def bottom_center(obj):
    bpy.context.view_layer.update()
    obj.data.transform(Matrix.Translation(obj.location))
    obj.location = (0, 0, 0)
    obj.rotation_euler = (0, 0, 0)
    obj.scale = (1, 1, 1)
    bpy.context.view_layer.update()
    corners = [obj.matrix_world @ Vector(corner) for corner in obj.bound_box]
    min_corner = Vector((min(c.x for c in corners), min(c.y for c in corners), min(c.z for c in corners)))
    max_corner = Vector((max(c.x for c in corners), max(c.y for c in corners), max(c.z for c in corners)))
    center = Vector(((min_corner.x + max_corner.x) * 0.5, (min_corner.y + max_corner.y) * 0.5, min_corner.z))
    obj.data.transform(Matrix.Translation(-center))
    bpy.context.view_layer.update()
    return obj


def smooth(obj):
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.shade_smooth()


def sphere(name, location, scale, segments=16, rings=8):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, radius=1.0, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    apply_transform(obj)
    return obj


def cube(name, location, scale, rotation=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    apply_transform(obj)
    return obj


def cone(name, location, radius1, radius2, depth, rotation=(0, 0, 0), vertices=12):
    bpy.ops.mesh.primitive_cone_add(
        vertices=vertices, radius1=radius1, radius2=radius2, depth=depth,
        location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    apply_transform(obj)
    return obj


def cylinder(name, location, radius, depth, rotation=(0, 0, 0), vertices=12):
    bpy.ops.mesh.primitive_cylinder_add(
        vertices=vertices, radius=radius, depth=depth, location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    apply_transform(obj)
    return obj


def export(obj, food_id):
    location = obj.location.copy()
    obj.location = (0, 0, 0)
    bpy.context.view_layer.update()
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.export_scene.fbx(
        filepath=os.path.join(FBX_DIR, f"Food_{food_id}.fbx"),
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
    obj.location = location
    bpy.context.view_layer.update()


def build_tahu(materials):
    block = cube("TahuBlock", (0, 0, 0.026), (0.085, 0.065, 0.052))
    bevel = block.modifiers.new("Bevel", "BEVEL")
    bevel.width = 0.006
    bevel.segments = 2
    bpy.context.view_layer.objects.active = block
    bpy.ops.object.modifier_apply(modifier=bevel.name)
    block.data.materials.append(materials["tahu"])
    leaf = cube("TahuLeaf", (0, 0, 0.001), (0.12, 0.10, 0.002))
    leaf.data.materials.append(materials["tahu_leaf"])
    obj = join([block, leaf], "Food_tahu")
    smooth(obj)
    return obj


def build_jeruk(materials):
    body = sphere("JerukBody", (0, 0, 0.04), (0.04, 0.04, 0.04), segments=20, rings=12)
    body.data.materials.append(materials["jeruk"])
    leaf = cube("JerukDaun", (0.012, 0.0, 0.078), (0.022, 0.010, 0.002), rotation=(0, math.radians(20), 0))
    leaf.data.materials.append(materials["daun"])
    obj = join([body, leaf], "Food_jeruk")
    smooth(obj)
    return obj


def build_wortel(materials):
    body = cone("WortelBody", (0, 0, 0.02), 0.018, 0.004, 0.13, rotation=(0, math.radians(90), 0), vertices=10)
    body.data.materials.append(materials["wortel"])
    parts = [body]
    for i in range(4):
        leaf = cone(
            f"WortelDaun{i}", (-0.062, -0.012 + i * 0.008, 0.075), 0.004, 0.001, 0.07,
            rotation=(math.radians(-25 + i * 16), math.radians(20), math.radians(-15 + i * 10)),
            vertices=5)
        leaf.data.materials.append(materials["daun"])
        parts.append(leaf)
    obj = join(parts, "Food_wortel")
    smooth(obj)
    return obj


def build_bayam(materials):
    parts = []
    for i in range(6):
        stem = cylinder(
            f"BayamBatang{i}", (-0.012 + i * 0.005, 0.004 * (i % 2), 0.03), 0.0025, 0.06,
            rotation=(math.radians(8 - i * 3), 0, 0), vertices=6)
        stem.data.materials.append(materials["batang"])
        parts.append(stem)
    for i in range(8):
        angle = math.radians(-80 + i * 22)
        leaf = sphere(
            f"BayamDaun{i}",
            (math.cos(angle) * 0.028, math.sin(angle) * 0.018, 0.062 + (i % 3) * 0.007),
            (0.026, 0.016, 0.004), segments=10, rings=6)
        leaf.rotation_euler = (math.radians(18 + (i % 3) * 8), math.radians(-10 + (i % 4) * 7), angle)
        apply_transform(leaf)
        leaf.data.materials.append(materials["bayam"])
        parts.append(leaf)
    obj = join(parts, "Food_bayam")
    smooth(obj)
    return obj


def build_ubi(materials):
    parts = []
    body = sphere("UbiA", (0.0, 0.0, 0.026), (0.055, 0.028, 0.026))
    body.data.materials.append(materials["ubi"])
    parts.append(body)
    second = sphere("UbiB", (0.035, 0.02, 0.022), (0.045, 0.024, 0.022))
    second.data.materials.append(materials["ubi"])
    parts.append(second)
    cut = cylinder("UbiPotong", (-0.045, -0.01, 0.026), 0.027, 0.05, rotation=(0, math.radians(90), 0), vertices=14)
    cut.data.materials.append(materials["ubi"])
    parts.append(cut)
    face = cylinder("UbiIsi", (-0.0705, -0.01, 0.026), 0.025, 0.003, rotation=(0, math.radians(90), 0), vertices=14)
    face.data.materials.append(materials["ubi_isi"])
    parts.append(face)
    obj = join(parts, "Food_ubi_jalar")
    smooth(obj)
    return obj


def build_ikan(materials):
    body = sphere("IkanBadan", (0.0, 0.0, 0.045), (0.11, 0.026, 0.042), segments=20, rings=10)
    body.data.materials.append(materials["ikan"])
    belly = sphere("IkanPerut", (0.0, 0.0, 0.032), (0.10, 0.024, 0.026), segments=20, rings=10)
    belly.data.materials.append(materials["ikan_perut"])
    tail = cone("IkanEkor", (-0.125, 0.0, 0.045), 0.006, 0.032, 0.055, rotation=(0, math.radians(90), 0), vertices=8)
    tail.data.materials.append(materials["ikan_sirip"])
    dorsal = cube("IkanSirip", (0.0, 0.0, 0.088), (0.055, 0.002, 0.022), rotation=(0, math.radians(-18), 0))
    dorsal.data.materials.append(materials["ikan_sirip"])
    eyes = []
    for side in (1, -1):
        eye = sphere(f"IkanMata{side}", (0.085, 0.022 * side, 0.052), (0.008, 0.008, 0.008), segments=10, rings=6)
        eye.data.materials.append(materials["mata"])
        pupil = sphere(f"IkanPupil{side}", (0.090, 0.027 * side, 0.052), (0.004, 0.004, 0.004), segments=8, rings=4)
        pupil.data.materials.append(materials["pupil"])
        eyes.extend([eye, pupil])
    obj = join([body, belly, tail, dorsal] + eyes, "Food_ikan_cakalang")
    smooth(obj)
    return obj


def render_icons(objects):
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_WORKBENCH"
    scene.render.film_transparent = True
    scene.render.resolution_x = 256
    scene.render.resolution_y = 256
    scene.render.image_settings.file_format = "PNG"
    scene.render.image_settings.color_mode = "RGBA"
    scene.display.shading.light = "STUDIO"
    scene.display.shading.studio_light = "paint.sl"
    scene.display.shading.color_type = "MATERIAL"
    scene.display.shading.show_shadows = True
    scene.display.shading.show_cavity = True
    scene.display.render_aa = "8"

    bpy.ops.object.camera_add(location=(0.28, -0.28, 0.22))
    camera = bpy.context.object
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = 0.30
    scene.camera = camera

    for food_id, obj in objects.items():
        for other in scene.objects:
            if other.type == "MESH":
                other.hide_render = other != obj
        x = obj.location.x
        camera.location = (x - 0.24, -0.30, 0.20)
        direction = Vector((x, 0, 0.035)) - camera.location
        camera.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
        scene.render.filepath = os.path.join(ICON_DIR, f"{food_id}.png")
        bpy.ops.render.render(write_still=True)

    for obj in scene.objects:
        if obj.type == "MESH":
            obj.hide_render = False


def main():
    reset_scene()
    materials = {
        "tahu": material("M_Food_Tahu", (0.93, 0.90, 0.76), 0.65),
        "tahu_leaf": material("M_Food_TahuLeaf", (0.10, 0.35, 0.08), 0.45),
        "wortel": material("M_Food_Wortel", (0.95, 0.36, 0.06), 0.55),
        "daun": material("M_Food_Daun", (0.16, 0.44, 0.10), 0.5),
        "jeruk": material("M_Food_Jeruk", (0.96, 0.48, 0.06), 0.55),
        "bayam": material("M_Food_Bayam", (0.10, 0.38, 0.10), 0.5),
        "batang": material("M_Food_Batang", (0.38, 0.55, 0.14), 0.6),
        "ubi": material("M_Food_UbiKulit", (0.42, 0.24, 0.28), 0.7),
        "ubi_isi": material("M_Food_UbiIsi", (0.55, 0.24, 0.45), 0.6),
        "ikan": material("M_Food_IkanBadan", (0.38, 0.45, 0.55), 0.35),
        "ikan_perut": material("M_Food_IkanPerut", (0.78, 0.80, 0.82), 0.4),
        "ikan_sirip": material("M_Food_IkanSirip", (0.20, 0.24, 0.30), 0.5),
        "mata": material("M_Food_Mata", (0.95, 0.95, 0.92), 0.25),
        "pupil": material("M_Food_Pupil", (0.03, 0.03, 0.03), 0.2),
    }

    builders = [
        ("tahu", build_tahu, -0.55),
        ("jeruk", build_jeruk, -0.33),
        ("wortel", build_wortel, -0.11),
        ("bayam", build_bayam, 0.11),
        ("ubi_jalar", build_ubi, 0.33),
        ("ikan_cakalang", build_ikan, 0.55),
    ]

    objects = {}
    for food_id, builder, x in builders:
        obj = builder(materials)
        bottom_center(obj)
        obj.location = (x, 0, 0)
        objects[food_id] = obj

    for food_id, obj in objects.items():
        export(obj, food_id)

    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT, "Blender", "Food", "quest_foods.blend"))
    render_icons(objects)
    print("Quest foods built:", ", ".join(objects))


if __name__ == "__main__":
    main()
