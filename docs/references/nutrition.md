# Referensi Gizi & Quest

Dokumen ini mencatat sumber, asumsi, dan status data gizi yang dipakai NutriPick VR.

> Proyek ini adalah game edukasi, bukan alat medis. Nilai gizi di dalam game adalah perkiraan untuk kebutuhan gameplay dan tidak boleh dipakai sebagai nasihat klinis.

## 1. AKG yang dipakai game

Nilai berikut tersimpan di `Assets/ScriptableObjects/Quests/QD_*.asset` dan ditulis ulang oleh `Assets/Editor/QuestDataBuilder.cs`.

| Zat gizi | Anak 7-9 | Dewasa 19-29 | Lansia 60-69 |
|---|---|---|---|
| Energi | 1850 kkal | 2250 kkal | 1700 kkal |
| Protein | 45 g | 60 g | 60 g |
| Karbohidrat | 280 g | 360 g | 275 g |
| Lemak | 60 g | 65 g | 50 g |
| Serat | 26 g | 30 g | 25 g |
| Gula tambahan | 25 g | 50 g | 25 g |
| Natrium | 1200 mg | 1500 mg | 1200 mg |
| Vitamin A | 600 µg | 600 µg | 600 µg |
| Vitamin C | 45 mg | 75 mg | 75 mg |
| Vitamin D | 15 µg | 15 µg | 20 µg |
| Kalsium | 1000 mg | 1000 mg | 1000 mg |
| Besi | 10 mg | 12 mg | 9 mg |

Catatan penting:

- Angka ini adalah **versi sederhana dan unisex** untuk gameplay, bukan tabel AKG resmi lengkap.
- AKG Indonesia 2019 memisahkan laki-laki dan perempuan serta kelompok umur yang lebih rinci.
- Sebelum dipakai untuk publikasi akademik, nilai di atas perlu diaudit ulang terhadap dokumen AKG resmi.

Rujukan resmi: **Peraturan Menteri Kesehatan RI No. 28 Tahun 2019 tentang Angka Kecukupan Gizi yang Dianjurkan untuk Masyarakat Indonesia** (Kementerian Kesehatan RI).

## 2. Dasar task komposisi

Semua kelompok memakai pola komposisi yang sama; hanya ambang gizinya yang berbeda.

| Level | Komposisi wajib |
|---|---|
| Sarapan | 1 protein + 1 buah |
| Makan siang | 1 protein + 1 sayur |
| Makan malam | 1 protein + 1 sayur |

Status referensi:

- Pola ini adalah **keputusan desain gameplay**, bukan kutipan langsung dari pedoman resmi.
- Padanan terdekat adalah pedoman **"Isi Piringku"** dari Kementerian Kesehatan: sekitar separuh piring berisi sayur dan buah, separuh lainnya karbohidrat dan protein.
- Aturan lengkapnya ada di `docs/plans/quest-system.mdx` baris 35-41.

Implementasi:

- Definisi: `Assets/Editor/QuestDataBuilder.cs` baris 96-100 (Anak), 121-125 (Dewasa), 143-147 (Lansia).
- Evaluasi: `Assets/Scripts/Quests/QuestEvaluator.cs`.
- Aset: `Assets/ScriptableObjects/Quests/QD_anak.asset`, `QD_dewasa.asset`, `QD_lansia.asset`.

## 3. Nilai gizi per makanan

- 18 makanan menyimpan 12 nilai di `Assets/ScriptableObjects/Food/FD_*.asset`.
- Nilai tersebut adalah **estimasi gameplay**, bukan hasil ekstraksi TKPI/Panganku atau USDA.
- Nilai per porsi, bukan per 100 g, dan dipakai untuk preview HUD, penilaian task, serta report.
- Untuk mengganti nilai: edit aset `FD_*` atau ubah `QuestFoodBuilder.cs` / `QuestDataBuilder.cs`, lalu jalankan menu `NutriPick → Build Quest Data` / `Build Quest Foods`.

Sumber data yang disarankan jika ingin diverifikasi:

- **TKPI / Panganku** (Kementerian Kesehatan RI) — https://panganku.org/
- **USDA FoodData Central** — https://fdc.nal.usda.gov/

## 4. Referensi visual

- Scene 1 (dapur, ruang makan, plating, foto makanan): `References/Scene1_Home/sources.md`. Mencatat query, file, lisensi, dan tautan Wikimedia Commons.
- Enam makanan baru: `References/Quest_Foods/` (Tahu, Jeruk, Wortel, Bayam, Ubi Jalar, Cakalang). Gambar disediakan pengguna; belum ada catatan sumber/lisensi di repo.
- FBX mentah di root `Assets/` (BURGER, rendang, yamgoreng, dll.) belum memiliki catatan sumber/lisensi.

## 5. Batas dan peringatan yang dipakai game

- Task gizi memakai ambang **minimum** per level; serve terkunci sampai keempat task lulus.
- Natrium, gula, dan lemak tidak mengunci serve.
- Per level, ketiganya dibandingkan dengan **1/3 AKG harian**.
- Di ringkasan kampanye, total tiga level dibandingkan dengan **100% AKG harian**.
- Persentase AKG di report memakai nilai tabel pada bagian 1.

## 6. Daftar rujukan

1. Kementerian Kesehatan RI. *Peraturan Menteri Kesehatan No. 28 Tahun 2019 tentang Angka Kecukupan Gizi yang Dianjurkan untuk Masyarakat Indonesia.*
2. Kementerian Kesehatan RI. *Isi Piringku.*
3. Kementerian Kesehatan RI. *Tabel Komposisi Pangan Indonesia (TKPI)* / Panganku — https://panganku.org/
4. USDA. *FoodData Central* — https://fdc.nal.usda.gov/
5. Wikimedia Commons — sumber gambar referensi Scene 1, lihat `References/Scene1_Home/sources.md`.
