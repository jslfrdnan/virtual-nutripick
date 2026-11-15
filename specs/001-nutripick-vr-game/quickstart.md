# Quickstart: NutriPick VR

**Feature**: `001-nutripick-vr-game` | **Date**: 2026-11-16

This guide proves the gamepad-first build works end-to-end. It is a validation guide, not an implementation tutorial.

## Prerequisites

- **Unity 6000.6.0f1** with Windows Build Support. This is the project's pinned version and the required editor; install it via Unity Hub if it is not listed. Do not open the project with 6000.2.6f2.
- **A Rexus gamepad** connected by USB or Bluetooth. It should appear to Windows as a standard XInput/gamepad device.
- **Blender 4.x** is not required to play. The asset team supplies the missing food and bazaar assets; the build uses placeholder prefabs until delivery.
- The project already contains:
  - Input System 1.20.0,
  - UGUI and TextMeshPro,
  - URP 17.6.0,
  - the existing food and prop FBX assets.

## First-Time Setup

1. Open the project in Unity 6000.6.0f1 and wait for the asset import to finish.
2. Confirm the project compiles. If `Assets/Scripts/RexusInputProbe.cs` still exists, delete it; it is invalid C# and blocks Play Mode.
3. Open `Assets/Scenes/MainVR.unity`.
4. Confirm the scene contains:
   - the `PlayerRig` prefab with `CharacterController`, camera, reticle, and `GamepadInput`,
   - the screen-space HUD canvas,
   - an EventSystem with `InputSystemUIInputModule`,
   - the test food/plate props.
5. Press **Play** and confirm the gamepad is detected. If not, check the Input System debugger and the fallback keyboard/mouse bindings.
6. Open `Assets/Scenes/MainVR.unity` (Level 1). The existing `ceramic_plate (3)` object is now the serving `Plate`.
7. Confirm Build Settings contains, in order:
   - `MainVR`
   - `Level02_Padang`
   - `Level03_Bazaar`
8. Press **Play** and run the Level 1 validation scenario.

If the asset team has not delivered the missing assets, the scene uses placeholder prefabs with the same `FoodData`; this does not block the validation scenarios.

## Controls

| Input | Action |
|-------|--------|
| Left stick / WASD | Move |
| Right stick / mouse | Look |
| A / South / Enter / left mouse | Interact: pick up, place, confirm |
| B / East / Escape / right mouse | Cancel: drop carried food, close card |
| X / West / Tab | Inspect: open the full food card |
| Start | Serve |
| Select / Escape | Pause/resume |
| D-pad / left stick | Navigate report and recap buttons |

If the gamepad is not detected, the game shows a notice and falls back to keyboard and mouse. The fallback is fully playable.

## Validation Scenarios

### V1 — Empty plate is neutral

1. Enter Level 1 and do not place any food.
2. Look at the Health Balance Bar.

**Expected**: The bar is at 50, gray, labeled "Belum dinilai", with counts "Sehat 0 · Kurang sehat 0". The Serve station shows "Tambahkan 3 makanan lagi".

### V2 — Move, look, and reticle

1. Move with the left stick around the counter.
2. Look with the right stick.
3. Aim the reticle at a food item and then at empty space.

**Expected**: The camera moves smoothly and collides with the counter. The reticle shows a ring and compact label over food, and no prompt over empty space. The interaction prompt changes between "A: Ambil" and nothing.

### V3 — Pick up, carry, and place

1. Aim at a food item and press A.
2. Walk while carrying it.
3. Aim at a free plate slot and press A.
4. Repeat with a second and third item.

**Expected**: The item attaches in front of the camera and follows smoothly. The plate slot highlights when targeted. Each item snaps into a slot without overlapping. The bar moves after each placement. The plate does not accept a fourth item when all slots are full.

### V4 — Drop and recalculate

1. Pick up a plated item with A.
2. Press B away from the plate.
3. Pick up another plated item and place it on the stand.

**Expected**: The item leaves the plate, the slot frees, the bar recalculates, and the counts update. Dropped items return to their home stand if they leave the play area.

### V5 — Food information card

1. Aim at a food item and read the compact label.
2. Press X to open the full card.
3. Press B to close it.
4. Pick up the item and confirm the full card appears.

**Expected**: The compact label shows name, badge, and calories. The full card shows portion, calories, protein, fiber, added sugar, sodium, and description. The card does not cover the reticle or the plate.

### V6 — Serve and report

1. Place at least three items, including a vegetable and a protein.
2. Aim at the Serve station and press A, or press Start.

**Expected**: The plate locks, the bar animates to its final value in about one second, and the report shows the score, band, exactly two strengths, one improvement, three dimensions, and the disclaimer. "Ulangi level" and "Lanjut" are visible and navigable with the D-pad.

### V7 — Retry

1. From the report, choose **Ulangi level**.
2. Build a different plate and serve.

**Expected**: Level 1 reloads with an empty plate and a full timer. The new result replaces the old one. The recap (after Level 3) uses the latest attempt.

### V8 — Next and recap

1. Complete Level 1 and choose **Lanjut**.
2. Complete Level 2 and choose **Lanjut**.
3. Complete Level 3 and choose **Lihat rekap**.

**Expected**: Each level loads with its own environment and food stands. The recap shows three level rows, the average score, two strengths, one improvement, the closing message, the disclaimer, and **Main lagi**.

### V9 — Time-out

1. Start Level 1 and wait for the timer to reach 0 with fewer than three items.
2. Wait for the grace period.

**Expected**: The timer enters a grace state. If the plate has at least one item when grace ends, it auto-serves with a "Piring belum lengkap" warning. If the plate is empty, the level reloads and no result is recorded. There is no hard fail screen.

### V10 — Healthy and less healthy routes

1. Build a mostly healthy plate: rice, boiled egg, tempeh, broccoli.
2. Serve and note the score.
3. Retry and build a mostly fried plate: fried egg, fried noodles, crackers.
4. Serve and note the score.

**Expected**: The healthy plate scores at least 70 (Seimbang). The fried plate scores below 50 (Perlu perbaikan). Both produce a complete report; neither is blocked.

### V11 — Level 3 bazaar

1. Enter Level 3. Confirm the bazaar has four signposted stands and a central serving table.
2. Walk to Mie & Jajanan, Lauk, Sayur & Buah, and Minuman.
3. Pick up four items including one drink.
4. Return to the central table, place them, and serve.

**Expected**: The player can walk between all stands without obstruction. Each stand sells its own foods. Serving is blocked with "Pilih satu minuman" until a drink is placed. The Level 3 report and recap appear after serving.

### V12 — Pause, disconnect, and fallback

1. Press Select to pause and resume.
2. Disconnect the gamepad while playing.
3. Reconnect it, or use the keyboard/mouse fallback.

**Expected**: Pause freezes the timer and shows the pause panel. Disconnecting pauses automatically with "Controller terputus". Reconnecting or using keyboard/mouse resumes without losing the plate.

## Automated Tests

### Unity Test Runner

1. Open **Window → General → Test Runner**.
2. Select **EditMode**.
3. Click **Run All**.

**Expected**: All tests pass. Coverage includes:

- empty plate = 50 neutral,
- balanced plate ≥ 70,
- fried plate < 50,
- three bananas < 40,
- score clamping,
- classification thresholds and overrides,
- level validation (including Level 3 stand count and drink stand),
- session record/replace/reset.

### Command line

Replace `<UnityEditorPath>` with the installed Unity 6000.6.0f1 editor folder:

```powershell
& "<UnityEditorPath>\Unity.exe" -batchmode -projectPath "D:\Unity Projects\VR_Project" `
  -runTests -testPlatform EditMode `
  -testResults "C:\Users\Lenovo\AppData\Local\Temp\opencode\nutripick-editmode.xml" -quit
```

**Expected**: The command exits with code 0 and the XML result reports zero failures.

## Optional Windows Build

1. Open **File → Build Profiles**.
2. Select **Windows**.
3. Confirm the three level scenes are included in order.
4. Build to `Builds/Windows/`.
5. Run the executable and repeat V1, V6, V8, V11, and V12.

**Expected**: The build runs without the Unity editor and the gamepad works.

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| Compile error in `RexusInputProbe.cs` | Delete the file; it is an invalid placeholder |
| Rexus gamepad not detected | Check Windows Game Controllers, the Input System debugger, and the USB/Bluetooth connection; use keyboard/mouse fallback |
| Player does not move | Confirm `CharacterController`, `GamepadInput`, and the input actions are assigned; check the left stick binding |
| Reticle does not target food | Check the food layer, the `ReticleInteractor` layer mask, and the collider |
| Food floats above the plate | Fix the prefab pivot in Blender/Unity; pivot must be at the base center |
| Food is giant or tiny | Check FBX import scale; 1 Unity unit = 1 meter |
| Pink materials | Convert the imported material to URP/Lit and reassign textures |
| TMP text is missing | Import TMP Essentials from **Window → TextMeshPro → Import TMP Essential Resources** |
| Bar does not move | Confirm `PlateController.PlateChanged` fires and `LevelFlowController` calls the evaluator |
| Report shows the wrong level | Check `LevelDefinition.levelIndex` and the Build Settings scene order |
| Serve station never enables | Check `minItems`, the Level 3 drink requirement, and `PlateController.CanServe` |
| Level 3 stand unreachable | Check the walkway colliders and the stand approach point |
| Missing team asset | Use the placeholder prefab with the same `FoodData`; do not block validation on art delivery |

## Pass Criteria

The feature is ready for the demo when:

- V1–V12 pass with the Rexus gamepad.
- All EditMode tests pass.
- The three-level session takes 8–12 minutes.
- At least 90% of testers can explain why the bar moved.
- At least 90% of testers report no discomfort.
- The report and recap always include the educational disclaimer.
