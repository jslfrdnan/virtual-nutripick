# Quest Kelompok & Gizi

Catatan implementasi untuk sistem quest NutriPick VR.

## Ringkasan

Pemain memilih satu kelompok di Scene 1. Pilihan berlaku untuk seluruh kampanye, tetapi task berubah tiap level: sarapan, makan siang, makan malam.

| Kelompok | Band umur | Fokus |
|---|---|---|
| Anak-anak | 7-9 tahun | Protein, kalsium, vitamin A, vitamin C |
| Dewasa | 19-29 tahun | Protein, serat, vitamin C, zat besi |
| Lansia | 60-69 tahun | Protein, kalsium, vitamin D, serat |

## Aturan serve

- Piring selalu 4 slot dan harus terisi 4 makanan.
- Tiap level punya 4 task: 2 komposisi + 2 ambang gizi minimum.
- Serve terkunci sampai keempat task lulus.
- Natrium, gula, dan lemak tidak mengunci serve; nilainya dibandingkan dengan 1/3 AKG per level dan 100% AKG di ringkasan akhir.
- Peringatan gorengan dihapus; gorengan tetap menambah lemak dan natrium.

## Data gizi

`FoodData` menyimpan energi, protein, karbohidrat, lemak, serat, gula, natrium, vitamin A, vitamin C, vitamin D, kalsium, dan zat besi. Semua nilai per porsi. Info card menampilkan 12 nilai absolut; kartu makanan tetap ringkas.

18 makanan: 12 lama + tahu, jeruk, wortel, bayam, ubi jalar, ikan cakalang.

## Report

- Report level: 4 status task + 7 nilai (energi, protein, karbohidrat, lemak, serat, natrium, gula).
- Ringkasan kampanye setelah Level 3: total 3 level + 12 nilai + %AKG + peringatan natrium/gula/lemak.

## Kode utama

- `Assets/Scripts/Quests/QuestDefinition.cs`
- `Assets/Scripts/Quests/QuestEvaluator.cs`
- `Assets/Scripts/Quests/NutritionTotals.cs`
- `Assets/Scripts/UI/QuestSelectPresenter.cs`
- `Assets/Scripts/UI/CampaignSummaryPresenter.cs`
- `Assets/Scripts/UI/MealStatusBarPresenter.cs`
- `Assets/Scripts/Flow/LevelFlowController.cs`
- `Assets/Editor/QuestDataBuilder.cs`
- `Assets/Editor/QuestFoodBuilder.cs`

## Aset baru

Model Blender: `Blender/Food/scripts/build_quest_foods.py` dan `Blender/Food/quest_foods.blend`.
FBX: `Assets/Models/Food/Home/Food_<id>.fbx`.
Ikon: `Assets/Textures/FoodIcons/<id>.png`.
Dokumentasi: `docs/asset/quest_foods/<id>_1_referensi.jpg`, `_2_modelling.jpg`, `_3_shading.jpg`.
Referensi: `References/Quest_Foods/`.

## Verifikasi

- EditMode: 36/36
- PlayMode: 40/40
- Scene validator: lulus
