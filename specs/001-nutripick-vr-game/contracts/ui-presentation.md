# Contract: UI and Presentation

**Feature**: `001-nutripick-vr-game`

The UI is a screen-space UGUI HUD with TextMeshPro. It is designed for 1920×1080 and is fully navigable with a Rexus gamepad. There is no world-space UI and no VR overlay.

## Canvas

- Render mode: Screen Space – Overlay.
- Reference resolution: 1920 × 1080.
- Canvas Scaler: Scale With Screen Size, match 0.5.
- EventSystem with `InputSystemUIInputModule`.
- All interactive report and recap controls are UGUI `Selectable`s with an explicit navigation order.

## Layout

| Element | Anchor | Position / size | Notes |
|---------|--------|-----------------|-------|
| Reticle | Center | 24 px dot, 40 px ring | Ring appears when a target is available |
| Health Balance Bar | Bottom center | 720 × 80 px, y = 120 px | Above the interaction prompt |
| Timer | Top center | 220 × 60 px, y = −40 px | Turns red under 30 s |
| Interaction prompt | Bottom center | 600 × 48 px, y = 56 px | "A: Ambil", "A: Taruh", etc. |
| Compact food label | Right of reticle | 320 × 96 px, offset (48, −48) | Name, badge, calories |
| Full food card | Left center | 420 × 560 px, x = 48 px | Does not cover the reticle |
| Report panel | Center | 1200 × 760 px | Full-screen modal |
| Recap panel | Center | 1200 × 760 px | Full-screen modal |
| Pause panel | Center | 560 × 360 px | Full-screen modal |

## Reticle

- Default: a small white dot with a dark outline for contrast.
- Target available: a thin ring expands around the dot.
- Carrying: the dot stays visible; the carried item sits below the center so it does not cover the target.
- The reticle is never hidden by the food card.

## Health Balance Bar

Elements:

1. Label: "Keseimbangan Gizi".
2. Track: 720 × 28 px, dark gray.
3. Fill: same size, color by band, animated.
4. Numeric value: "71" at the right end, 36 px bold.
5. Band label: "Seimbang" under the number, 24 px.
6. Counts: "Sehat 3 · Kurang sehat 1", 24 px.
7. Tick marks at 40 and 70 with small labels "Cukup" and "Seimbang".

Behaviour:

- Empty plate: fill at 50%, gray, label "Belum dinilai", counts 0/0.
- Preview: animate to the new value over 0.15 s on each plate change.
- Final: animate over approximately 1.0 s with a light ease, then lock.
- The bar never shows a score for an empty plate as if it were a result.
- The numeric value and label must update together.

## Timer

- Format: "02:59".
- Default: white text on a dark pill.
- Under 30 seconds: red text and a subtle pulse.
- At 0 with a valid plate: "Waktu habis" then auto-serve.
- At 0 below the minimum: "Waktu habis · Pilih X lagi" during grace.

## Interaction Prompt

- Center-bottom, single line.
- Examples:
  - "A: Ambil"
  - "A: Taruh · B: Lepas"
  - "A: Sajikan"
  - "X: Lihat info"
  - "Tambahkan 1 makanan lagi"
  - "Pilih satu minuman"
  - "Piring penuh"
- Use a gamepad glyph if available; otherwise use the letter label. Include a keyboard fallback hint in the pause panel.

## Compact Food Label

Shown when the reticle rests on a food:

- Name (24 px bold).
- Badge: "Sehat" green or "Kurang sehat" amber.
- Calories: "195 kkal".
- A small hint: "A: Ambil · X: Info".

Positioned to the right of the reticle, never covering it.

## Full Food Card

Shown on pick-up or X:

- Name and portion ("Nasi putih · 150 g").
- Classification badge.
- Nutrition rows:
  - "Kalori: 195 kkal"
  - "Protein: 4,0 g"
  - "Serat: 0,6 g"
  - "Gula tambahan: 0 g"
  - "Natrium: 2 mg"
- Description, 22 px, 2–3 lines.
- Footer: "Nilai gizi adalah perkiraan."

The card is fixed on the left; it does not follow the reticle.

## Report Panel

Content order:

1. Title: "Laporan Level 1" (or the level display name).
2. Score: large number and band label.
3. Two strengths, each with a check mark.
4. One improvement, with an arrow or lightbulb.
5. Three dimension rows:
   - "Energi" + rating + sentence.
   - "Fokus" + rating + sentence.
   - "Kesehatan jangka panjang" + rating + sentence.
6. Disclaimer: "Ini adalah game edukasi, bukan nasihat medis. Nilai gizi adalah perkiraan."
7. Buttons: "Ulangi level" and "Lanjut" (or "Lihat rekap" in Level 3).

If `isIncomplete` is true, show a warning above the score: "Piring belum lengkap, tetapi tetap dinilai."

## Recap Panel

Content order:

1. Title: "Rekap NutriPick".
2. Three level rows: name, score, band.
3. Average score and band.
4. Two strengths across the session.
5. One improvement across the session.
6. Closing message: "Mulai dari satu perubahan kecil: tambahkan sayur di setiap makan."
7. Disclaimer.
8. Button: "Main lagi".

## Pause Panel

- Title: "Jeda".
- Buttons: "Lanjut" and "Keluar".
- If the gamepad disconnected: "Controller terputus. Sambungkan kembali atau gunakan keyboard."
- The pause panel is modal and pauses the timer.

## Color Palette

| Token | Hex | Use |
|-------|-----|-----|
| Green | `#2E7D32` | Balanced, healthy badge, positive |
| Yellow | `#F9A825` | Adequate, less healthy badge |
| Red | `#C62828` | Needs improvement, timer under 30 s |
| Neutral | `#9E9E9E` | Empty/not rated, disabled |
| Ink | `#212121` | Primary text |
| Muted ink | `#616161` | Secondary text |
| Panel | `#FAFAFA` | Card and report background |
| Panel border | `#E0E0E0` | Card outline |
| Bar track | `#37474F` | Bar background |
| Overlay | `rgba(0,0,0,0.55)` | Modal backdrop |

Never use color as the only signal. Every color band also has a text label and a number.

## Typography

| Role | Size (px) | Weight |
|------|-----------|--------|
| Report title | 44 | Bold |
| Score | 72 | Bold |
| Section heading | 30 | Semi-bold |
| Body | 24 | Regular |
| Caption | 20 | Regular |
| Button label | 28 | Semi-bold |
| HUD label | 22 | Regular |

- Use LiberationSans SDF or a font with full Indonesian Latin coverage.
- Use Indonesian decimal comma: "4,0 g", "800 mg".
- High contrast: Ink on Panel, white on Green/Red.
- Add a subtle outline only if readability requires it.

## Gamepad Navigation

- Report and recap panels use UGUI `Selectable` navigation.
- The first button is selected when the panel opens.
- D-pad or left stick moves the selection; A confirms; B closes only where allowed.
- The selected button has a visible highlight (outline + scale).
- The pause panel uses the same navigation.
- No mouse is required, but mouse clicks still work as a fallback.

## Copy and Tone

- Supportive, never judgmental.
- Use "kurang sehat", not "tidak sehat", "buruk", or "dilarang".
- No medical claims or diagnoses.
- Address the player as "kamu".
- Keep sentences short and concrete.
- Use "Sajikan", not "Submit" or "Serve", in the UI.
- Use "Ulangi level", "Lanjut", "Lihat rekap", "Main lagi".

## Accessibility

- Text must be readable at 1920×1080 at normal viewing distance.
- The bar has a number and label, not only color.
- Buttons have text labels and a visible disabled state.
- Avoid rapid flashing; bar animation is smooth and under 1.2 s.
- No head bob or forced camera motion.
- Keyboard/mouse fallback is documented in the pause panel.
- Reticle size and prompt contrast are configurable in the HUD prefab.

## Acceptance Checks

- [ ] Empty plate shows 50, gray, "Belum dinilai", counts 0/0.
- [ ] Adding a healthy item moves the bar and increases the healthy count.
- [ ] Adding a less healthy item moves the bar and increases the less healthy count.
- [ ] Removing an item recalculates the bar.
- [ ] Serving animates to the final value in about 1 s.
- [ ] The compact label appears on hover and hides when the reticle leaves.
- [ ] The full card shows all required fields and dismisses correctly.
- [ ] The report shows exactly two strengths, one improvement, three dimensions, and the disclaimer.
- [ ] The recap shows all three levels and the average.
- [ ] All report and recap buttons are navigable with the gamepad.
- [ ] All text is legible at 1920×1080.
