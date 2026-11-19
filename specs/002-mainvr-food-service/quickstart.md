# Quickstart Validation: Scene 1 MainVR

**Feature**: `002-mainvr-food-service`
**Date**: 2026-11-16
**Spec**: `specs/002-mainvr-food-service/spec.md`

## 1. Prerequisites

- Unity **6000.6.0f1** with the project at `D:\Unity Projects\VR_Project`.
- A Rexus gamepad recognized by the Input System; keyboard and mouse are the fallback.
- `Assets/Models/Props/ceramic_plate.fbx` imported.
- `LD_MainVR.asset` has the eight Scene 1 foods and `minItems = 3`.
- `NutritionRules.asset` is assigned.
- `ServingPlate.prefab` and `FigmaHUD.prefab` exist.
- `FoodData` assets have `hasProteinType = true` and a `proteinType` value.
- No duplicate `EventSystem` in `MainVR`.

## 2. Open and run

1. Open `Assets/Scenes/MainVR.unity`.
2. Confirm Build Settings currently contains `MainVR`; `Level02_Padang` and `Level03_Bazaar` are absent from the working tree.
3. Press **Play**. Scene 1 starts directly in gameplay in the home kitchen and dining room; there is no Start screen.
4. Use the gamepad:
   - Left stick: move
   - Right stick: look
   - `A`: pick up / place / interact
   - `B`: drop / cancel
   - `X`: inspect / focus card
   - `Start`: serve
   - `Select`: pause

## 3. Validation scenarios

### V1 — Direct start

- Scene 1 starts immediately after Play.
- No Start screen or `Profil Dev` panel appears.
- The home kitchen, dining table, two platters, and eight foods are visible.
- Player movement, reticle, food cards, and Status Makanan are active immediately.

**Expected**: no console errors; the kitchen scene is interactive without pressing Start.

### V2 — Food card grid

- Eight cards are visible, one per food in `LD_MainVR.foodPool`.
- Each card shows image, name, protein type, calories, protein, carbohydrates, and fat.
- Values match the `FoodData` assets.
- A missing icon shows a placeholder, not a blank card.
- Targeting a food highlights its card; `X` or pickup focuses it.

**Expected**: all eight cards are readable at 1920×1080; no text overflow.

### V3 — Plate and placement

- The serving plate uses `ceramic_plate.fbx` and sits on the dining table.
- Four placement positions exist.
- Foods are picked from the two platters on the dining table.
- Repeat pickup/place/remove for all eight foods.
- Carrying food over a free position highlights it.
- `A` places food; food snaps into the position and stays stable for at least 10 seconds.
- `A` on a plated food picks it up again.
- `B` drops carried food without changing the plate.
- A fifth placement is refused because all four positions are occupied.
- Target a decorative prop: no prompt, no pickup.
- Drop food outside the table: it returns to its starting position.

**Expected**: no floating, clipping, or falling through the table.

### V4 — Status Makanan

- Empty plate: `BELUM DINILAI` / neutral.
- Add a healthy food: bar moves toward the balanced side and the label updates.
- Add a less healthy food: bar moves toward the needs-improvement side and the label updates.
- Remove food: bar recalculates.
- Serve: bar animates to its final value and remains visible.

**Expected**: label and color always agree; the bar updates within 0.5 s.

### V5 — Serve and Final Result

- Serve with two foods: blocked with an Indonesian remaining-count message.
- Serve with three foods: allowed.
- Serve with four foods: allowed; plate is full.
- The plate locks after serving.
- Final Result shows a green balanced panel and a red less-balanced panel.
- Each panel shows three feedback points and a status badge.
- `ULANGI LEVEL` reloads `MainVR` with an empty plate.
- `NEXT LEVEL` loads `Level02_Padang`.

**Expected**: both panels are readable; the correct outcome is highlighted.

### V6 — Keyboard/mouse fallback

- Disconnect the gamepad.
- Verify movement, look, interact, inspect, serve, and pause with keyboard/mouse.
- Reconnect the gamepad and verify the pause/disconnect behavior.

**Expected**: the full Scene 1 flow remains playable without a gamepad.

### V7 — Replication smoke test

1. Duplicate `MainVR` to a temporary scene.
2. Replace `LD_MainVR` with a different `LevelDefinition` and food pool.
3. Keep `ServingPlate.prefab`, `FigmaHUD.prefab`, and the same `LevelFlow` wiring.
4. Enter Play mode and complete one pickup, one placement, one card inspection, and one serve.

**Expected**: no code changes are required; the copied scene works.

## 4. Automated tests

Run from Unity Test Runner:

**Baseline (2026-11-16)**: 22/22 EditMode passed before Phase 2; 24/24 passed after the Phase 2 data changes. PlayMode baseline was **3/4**: `MainVRSpawnsFoodAndWiresHud` failed because `MainVR` contains **9 FoodItem** instances — a duplicate `rendang (1)` at `(-0.48, 1.36, 0.05)` and `telur_rebus` parented under `telur_ceplok`. The Scene 1 integration phase must remove the duplicate and restore the eight-food row.

**Phase 3 validation (2026-11-17)**: PlayMode **7/7 passed**, EditMode **24/24 passed**, and scene validation passed with `MainVR` (stands=1, plateSlots=4), `Level02_Padang`, and `Level03_Bazaar`.

**Phase 4 validation (2026-11-17)**: PlayMode **9/9 passed**, EditMode **26/26 passed**, and scene validation passed. `MainVR` now binds 8 food cards and updates the Status Makanan bar when food is plated.

**Phase 5 validation (2026-11-17)**: PlayMode **10/10 passed** after removing the Start screen, including Final Result panels and the replication smoke test. Final Result shows both panels, three points per panel, and Retry/Next actions.

- **EditMode**
  - `FoodData` protein type validation.
  - `MealResult` exactly three positive and three improvement points.
  - `FoodCardModel` formatting and placeholders.
  - `LevelDefinition` still validates the eight-food pool.
- **PlayMode**
  - Scene 1 starts immediately with no Start screen.
  - Food card grid binds eight cards.
  - Plate accepts three foods and locks on serve.
  - Status Makanan updates on plate change.
  - Final Result shows both panels and Retry/Next.
  - Existing Level 2 and Level 3 smoke tests still pass.

Batch example:

```powershell
& "D:\Unity Projects\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" `
  -batchmode -projectPath "D:\Unity Projects\VR_Project" `
  -runTests -testPlatform EditMode `
  -testResults "C:\Users\Lenovo\AppData\Local\Temp\opencode\nutripick-editmode-scene1.xml"
```

Repeat with `-testPlatform PlayMode` for PlayMode tests.

## 5. Build validation

1. **File → Build Profiles**.
2. Confirm `MainVR`, `Level02_Padang`, `Level03_Bazaar` are enabled in that order.
3. Build Windows 64-bit.
4. Launch `Builds/Windows/NutriPick.exe`.
5. Repeat V1–V5 in the build.

## 6. Manual UI Canvas fallback

The preferred path is the editor-generated `FigmaHUD.prefab`. If the UI must be assembled manually, use this outline:

```text
FigmaHUD (Canvas, Screen Space Overlay, 1920×1080, Match 0.5)
├── EventSystem
├── GameplayHUD
│   ├── Reticle (center, 8×8)
│   ├── FoodCardGrid (right side, 2×4 cards)
│   ├── StatusMakanan (bottom center, 720×80)
│   ├── Prompt (bottom center above status)
│   └── PausePanel
└── FinalResult (full-screen)
    ├── Title
    ├── BalancedPanel (green)
    │   ├── Badge
    │   └── 3 point texts
    ├── LessBalancedPanel (red)
    │   ├── Badge
    │   └── 3 point texts
    ├── ClosingMessage
    ├── RetryButton
    └── NextButton
```

Manual wiring:
- `FoodCardGridPresenter.cardContainer` → `FoodCardGrid`
- `FoodCardGridPresenter.cardPrefab` → `FoodCard.prefab`
- `FoodCardGridPresenter.rules` → `NutritionRules.asset`
- `FinalResultPresenter` fields → the corresponding panel texts/buttons
- `LevelFlowController` references → `foodCards`, `mealStatus`, `finalResult`, `prompt`, and pause
- `ReticleInteractor.foodCards` → `FoodCardGridPresenter`

## 7. Troubleshooting

| Symptom | Check |
|---------|-------|
| Scene does not start | `LevelFlowController.startScreen` must be null; check Console for missing references |
| Cards are blank | `FoodData.icon` or placeholder sprite assigned; `FoodCardGridPresenter.Bind` called |
| Protein type shows `—` | `FoodData.hasProteinType` is false |
| Card grid overlaps the plate | Move the grid to the right side; check CanvasScaler |
| Food falls through table | `TableSurfaceCollider` exists at the table surface |
| Plate visual is wrong size | Normalize `ceramic_plate.fbx` scale to the slot layout |
| Status bar does not update | `LevelFlow.mealStatus` assigned; `PlateController.PlateChanged` fires |
| Final Result is blank | `MealResult.positivePoints` / `improvementPoints` have three entries |
| Level 2/3 broke | New prefabs must be optional; old HUD/report fallbacks must remain |
| Duplicate EventSystem | Remove one; keep the one inside `FigmaHUD` |
