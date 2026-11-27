import bpy
import os
from mathutils import Vector

# Run this script from Blender's Scripting workspace.
# Keep banana_3d.obj and banana_3d.mtl in the same folder as banana_skin.png.
folder = os.path.dirname(os.path.abspath(__file__))
obj_file = os.path.join(folder, "banana_3d.obj")

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.wm.obj_import(filepath=obj_file)

banana = bpy.context.selected_objects[0] if bpy.context.selected_objects else None
if banana:
    banana.name = "Banana_3D"
    if banana.type == "MESH":
        for poly in banana.data.polygons:
            poly.use_smooth = True
        # Recalculate normals outside.
        bpy.context.view_layer.objects.active = banana
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.mesh.normals_make_consistent(inside=False)
        bpy.ops.object.mode_set(mode='OBJECT')

# Camera and lighting for a clean preview render.
bpy.ops.object.camera_add(location=(0, -6.8, 5.2))
camera = bpy.context.object
camera.name = "Camera_Banana"
target = Vector((0, 0.25, 0))
direction = target - camera.location
camera.rotation_euler = direction.to_track_quat('-Z', 'Y').to_euler()
camera.data.lens = 55
bpy.context.scene.camera = camera

bpy.ops.object.light_add(type='AREA', location=(0, -2, 5))
key = bpy.context.object
key.data.energy = 850
key.data.shape = 'DISK'
key.data.size = 5

bpy.ops.object.light_add(type='AREA', location=(3, 3, 2))
fill = bpy.context.object
fill.data.energy = 450
fill.data.size = 4

scene = bpy.context.scene
scene.render.engine = 'BLENDER_EEVEE_NEXT'
scene.render.resolution_x = 1200
scene.render.resolution_y = 900
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = 'PNG'
scene.world.color = (0.8, 0.8, 0.8)
scene.render.filepath = os.path.join(folder, "banana_preview.png")
