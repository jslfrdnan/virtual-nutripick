from pathlib import Path
import json

from PIL import Image, ImageEnhance, ImageDraw, ImageFont

PROJECT = Path(r"D:\Unity Projects\VR_Project")
REFS = PROJECT / "References" / "Scene1_Home" / "foods"
WORK = PROJECT / "Blender" / "Scene1_Home" / "textures" / "food"
FINAL = PROJECT / "Assets" / "Textures" / "Food" / "Home"
PREVIEW = PROJECT / "References" / "Scene1_Home" / "food_textures_preview.jpg"

WORK.mkdir(parents=True, exist_ok=True)
FINAL.mkdir(parents=True, exist_ok=True)

# name: (source file, crop box left, top, right, bottom, output size, darken, note)
TEXTURES = {
    "M_Food_Rendang": ("beef_rendang.jpg", 480, 320, 780, 620, 1024, 1.0, "rendang chunk surface"),
    "M_Food_RendangSauce": ("beef_rendang.jpg", 580, 540, 740, 670, 512, 0.78, "dark rendang sauce"),
    "M_Food_Bun": ("big_mac_hamburger.jpg", 420, 120, 830, 280, 1024, 1.0, "sesame bun crust"),
    "M_Food_Patty": ("big_mac_hamburger.jpg", 360, 490, 780, 540, 1024, 1.0, "grilled beef patty"),
    "M_Food_Cheese": ("big_mac_hamburger.jpg", 500, 800, 720, 845, 512, 1.0, "melted cheese"),
    "M_Food_Lettuce": ("big_mac_hamburger.jpg", 300, 480, 480, 555, 512, 1.05, "lettuce"),
    "M_Food_Tomato": ("mi_goreng_gm.jpg", 300, 540, 410, 640, 512, 1.05, "tomato slice"),
    "M_Food_EggWhite": ("boiled_egg_crossection.jpg", 330, 270, 430, 380, 1024, 1.35, "boiled egg white"),
    "M_Food_Yolk": ("boiled_egg_crossection.jpg", 245, 455, 500, 690, 1024, 1.0, "boiled egg yolk"),
    "M_Food_Tempeh": ("sliced_tempeh.jpg", 550, 300, 850, 480, 1024, 1.0, "tempeh bean surface"),
    "M_Food_TempehDark": ("sliced_tempeh.jpg", 550, 300, 850, 480, 512, 0.62, "dark tempeh edge"),
    "M_Food_Broccoli": ("broccoli_and_cross_section_edit.jpg", 270, 190, 500, 330, 1024, 1.05, "broccoli florets"),
    "M_Food_Stem": ("broccoli_and_cross_section_edit.jpg", 900, 470, 1020, 660, 512, 1.0, "broccoli stem"),
    "M_Food_Banana": ("banana_sitia_crete.jpg", 360, 440, 720, 540, 1024, 1.02, "banana peel"),
    "M_Food_BananaTip": ("banana_sitia_crete.jpg", 290, 675, 400, 745, 512, 0.72, "banana tip"),
    "M_Food_Noodle": ("mi_goreng_gm.jpg", 520, 250, 860, 500, 1024, 1.0, "fried noodles"),
    "M_Food_GreenOnion": ("mi_goreng_gm.jpg", 700, 180, 770, 230, 512, 1.05, "green onion"),
}

LICENSES = {
    "beef_rendang.jpg": ("CC0", "Wikimedia Commons / Beef Rendang"),
    "big_mac_hamburger.jpg": ("CC0", "Wikimedia Commons / Big Mac hamburger"),
    "boiled_egg_crossection.jpg": ("CC BY-SA 2.0", "Wikimedia Commons / Boiled Egg Crossection"),
    "sliced_tempeh.jpg": ("CC BY-SA 2.0", "Wikimedia Commons / Sliced tempeh"),
    "broccoli_and_cross_section_edit.jpg": ("GFDL 1.2", "Wikimedia Commons / Broccoli and cross section"),
    "banana_sitia_crete.jpg": ("CC BY-SA 4.0", "Wikimedia Commons / Banana, Sitia, Crete"),
    "mi_goreng_gm.jpg": ("CC BY-SA 3.0", "Wikimedia Commons / Mi Goreng GM"),
}


def crop_and_save(source_name, box, size, darken, output_name):
    source_path = REFS / source_name
    if not source_path.exists():
        raise FileNotFoundError(source_path)

    image = Image.open(source_path).convert("RGB")
    left, top, right, bottom = box
    image = image.crop((left, top, right, bottom))
    image = ImageEnhance.Color(image).enhance(1.05)
    image = ImageEnhance.Contrast(image).enhance(1.05)
    if darken != 1.0:
        image = ImageEnhance.Brightness(image).enhance(darken)
    image = image.resize((size, size), Image.Resampling.LANCZOS)

    work_path = WORK / f"{output_name}.png"
    final_path = FINAL / f"{output_name}.png"
    image.save(work_path, "PNG", optimize=True)
    image.save(final_path, "PNG", optimize=True)
    return image, work_path, final_path


def main():
    manifest = []
    previews = []

    for material_name, (source, left, top, right, bottom, size, darken, note) in TEXTURES.items():
        image, work_path, final_path = crop_and_save(
            source, (left, top, right, bottom), size, darken, f"{material_name}_Albedo"
        )
        manifest.append(
            {
                "material": material_name,
                "source": f"References/Scene1_Home/foods/{source}",
                "crop": [left, top, right, bottom],
                "output": f"Assets/Textures/Food/Home/{material_name}_Albedo.png",
                "size": size,
                "wrap": "Repeat",
                "license": LICENSES.get(source, ("unknown", "unknown"))[0],
                "note": note,
            }
        )
        previews.append((f"{material_name}_{size}", image))

    with (WORK / "texture_manifest.json").open("w", encoding="utf-8") as handle:
        json.dump(manifest, handle, indent=2)
        handle.write("\n")

    attribution = [
        "# Food Texture Attribution",
        "",
        "Tekstur makanan adalah crop/derivat dari gambar referensi berikut.",
        "Semua gambar disimpan di `References/Scene1_Home/foods/`.",
        "",
        "| Texture | Sumber | Lisensi |",
        "|---|---|---|",
    ]
    for entry in manifest:
        source = Path(entry["source"]).name
        license_name, credit = LICENSES.get(source, ("unknown", "unknown"))
        attribution.append(f"| `{Path(entry['output']).name}` | {source} ({credit}) | {license_name} |")
    attribution += [
        "",
        "Catatan: crop dan color-correct adalah perubahan; untuk lisensi CC BY-SA/GFDL, hasil turunan",
        "tetap mengikuti lisensi sumber dan atribusi di atas harus disertakan bila proyek dipublikasikan.",
        "",
    ]
    (FINAL / "ATTRIBUTION.md").write_text("\n".join(attribution), encoding="utf-8")

    cols = 4
    cell = 260
    rows = (len(previews) + cols - 1) // cols
    sheet = Image.new("RGB", (cols * cell, rows * (cell + 26)), (245, 245, 245))
    draw = ImageDraw.Draw(sheet)
    for index, (label, image) in enumerate(previews):
        thumb = image.copy()
        thumb.thumbnail((cell - 12, cell - 12))
        x = (index % cols) * cell
        y = (index // cols) * (cell + 26)
        sheet.paste(thumb, (x + 6, y + 6))
        draw.text((x + 6, y + cell + 2), label[:36], fill=(0, 0, 0))
    sheet.save(PREVIEW, "JPEG", quality=88)
    print(f"created {len(manifest)} textures")


if __name__ == "__main__":
    main()
