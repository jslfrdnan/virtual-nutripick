# Scene 1 Home Assets

Aset 3D untuk Scene 1 (`MainVR`) dibuat ulang di Blender 5.2 dengan gaya **minimalis putih**:
dapur di dinding belakang, kitchen island, ruang makan, dan dekorasi hangat dari kayu.

## Sumber

- Blender source: `Blender/Scene1_Home/Scene1_Home.blend`
- Builder script: `Blender/Scene1_Home/scripts/build_scene1_assets.py`
- Referensi Google: `References/Scene1_Home/` + `sources.md`

## FBX

| File | Isi |
|---|---|
| `KitchenRoom.fbx` | lantai, dinding, plafon, pintu, jendela, kabinet, kompor, sink, kulkas, lampu gantung |
| `KitchenIsland.fbx` | kitchen island 2.2 × 0.9 m |
| `DiningSet.fbx` | meja makan, 4 kursi, rug, sideboard, lampu, wall art |
| `Decor.fbx` | tanaman, jam dinding, tempat sampah, fruit bowl, dish rack |

## Plating dan makanan

- `Assets/Models/Food/Plating/`: `Platter_Oval`, `Plate_Small`, `Bowl_Small`, `Tray_Bamboo`, `BananaLeaf`.
- `Assets/Models/Food/Home/`: 8 makanan interaktif (`Food_rendang` sampai `Food_mie_goreng`).

## Konvensi

- Skala 1 unit = 1 meter.
- Origin makanan di dasar, di tengah.
- Material memakai prefiks `M_Home_` dan `M_Food_`.
- FBX diekspor dengan `-Z Forward`, `Y Up`, `FBX_SCALE_ALL`.

## Rebuild

```powershell
& "D:\Blender Foundation\Blender 5.2\blender.exe" -b --python "Blender\Scene1_Home\scripts\build_scene1_assets.py"
```

Lalu di Unity jalankan **NutriPick → Build Scene 1 Home**.
