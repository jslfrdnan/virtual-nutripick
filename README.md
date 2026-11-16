# NutriPick VR

NutriPick VR is an educational first-person nutrition game built in Unity 6. The player uses a **Rexus gamepad** to move, aim a reticle, pick up food, plate it, and serve a meal. Scene 1 now uses a Figma-style UI Canvas: a Start screen, an eight-card food grid, a Status Makanan bar, and a two-panel Final Result. A live health balance and food information cards teach balanced choices across three levels:

1. **Warteg** — breakfast, built from the existing `MainVR` tabletop scene
2. **RM Padang** — lunch
3. **Bazaar** — dinner with four food stands

## Requirements

- **Unity 6000.6.0f1** with Windows Build Support. This is the project's pinned version; do not open it with 6000.2.6f2.
- A **Rexus gamepad** (standard XInput/gamepad). Keyboard and mouse are the fallback.
- Blender 4.x is **not** required to play. The asset team supplies the five missing foods and bazaar stand/canopy pieces; the current build uses placeholder prefabs until delivery.

## Controls

| Input | Action |
|-------|--------|
| Left stick / WASD | Move |
| Right stick / mouse | Look |
| A / South / Enter / left mouse | Interact: pick up, place, confirm |
| B / East / right mouse | Cancel: drop carried food |
| X / West / Tab | Inspect: full food card |
| Start / Space | Serve |
| Select / Escape | Pause/resume |
| D-pad / left stick | Navigate report and recap buttons |

## Open and Run

1. Open the project with Unity 6000.6.0f1.
2. Open `Assets/Scenes/MainVR.unity`.
3. Press **Play**.
4. Press **Start** on the Start screen. Pick up three foods, place them on the plate, and serve. Scene 1 shows the eight-card food grid, Status Makanan bar, and Final Result; Levels 2 and 3 retain the existing HUD/report fallback.

Build Settings contains exactly `MainVR`, `Level02_Padang`, and `Level03_Bazaar`, in that order. `MainVR` is the existing tabletop mockup; its `Plate` now uses `Assets/Prefabs/Interaction/ServingPlate.prefab` with `Assets/Models/Props/ceramic_plate.fbx`. Scene 1 uses `Assets/Prefabs/UI/FigmaHUD.prefab`; `Level01_Warteg.unity` is an optional generated fallback and is not in the build.

## Generate or Refresh Content

The project includes an editor builder that creates the ScriptableObjects, food prefabs, HUD, plate, stands, serve station, and the three level scenes:

- Menu: **NutriPick → Build All** (generates foods, Level 2, Level 3)
- Menu: **NutriPick → Setup Scene 1 Data** (protein types and LD_MainVR)
- Menu: **NutriPick → Generate Scene 1 Food Icons**
- Menu: **NutriPick → Build Serving Plate**
- Menu: **NutriPick → Build Figma HUD**
- Menu: **NutriPick → Build Scene 1 Figma Setup**
- Menu: **NutriPick → Build MainVR Level 1** (legacy automated wiring)
- Menu: **NutriPick → Fix MainVR Table Surface**
- Menu: **NutriPick → Validate All**
- Menu: **NutriPick → Validate Scenes**
- Menu: **NutriPick → Capture Scene Previews**
- Menu: **NutriPick → Build Windows**

The Windows build is written to `Builds/Windows/NutriPick.exe`.

## Tests

- **EditMode**: 26 tests covering classification, meal evaluation, session state, level validation, protein types, Final Result points, and food-card formatting.
- **PlayMode**: 11 tests covering Scene 1 Start/card grid/Status Makanan/Final Result/replication, MainVR plate-and-serve, Level 2 food spawning, and Level 3 four-stand/drink setup.

Run them from **Window → General → Test Runner**, or headlessly:

```powershell
& "D:\Unity Projects\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" `
  -batchmode -projectPath "D:\Unity Projects\VR_Project" `
  -runTests -testPlatform EditMode `
  -testResults "C:\Users\Lenovo\AppData\Local\Temp\opencode\nutripick-editmode.xml" -logFile "C:\Users\Lenovo\AppData\Local\Temp\opencode\unity-tests.log"
```

Use `-testPlatform PlayMode` for the PlayMode suite.

## Team Asset Handoff

The asset team supplies these assets; the implementation currently uses placeholders for the five foods and the bazaar modules:

| Asset | Type |
|-------|------|
| `nasi_putih` | Food — white rice |
| `kerupuk` | Food — crackers |
| `sayur_singkong` | Food — cassava leaves |
| `air_putih` | Drink — water |
| `teh_manis` | Drink — sweet tea |
| `bazaar_stand` | Environment — modular stand |
| `bazaar_canopy` | Environment — canopy/tent |

Integration requirements are in `specs/001-nutripick-vr-game/content/food-catalog.md`. The integration task is T057 in `specs/001-nutripick-vr-game/tasks.md`.

## Nutrition Disclaimer

This is an educational course project, not medical advice. Nutrition values are approximate and should be reviewed against TKPI/Panganku before a public demo.

## Project Documentation

- Specification: `specs/001-nutripick-vr-game/spec.md`
- Plan: `specs/001-nutripick-vr-game/plan.md`
- Tasks: `specs/001-nutripick-vr-game/tasks.md`
- Quickstart: `specs/001-nutripick-vr-game/quickstart.md`
- Contracts: `specs/001-nutripick-vr-game/contracts/`
- Food catalog: `specs/001-nutripick-vr-game/content/food-catalog.md`
- Scene 1 spec: `specs/002-mainvr-food-service/spec.md`
- Scene 1 plan: `specs/002-mainvr-food-service/plan.md`
- Scene 1 tasks: `specs/002-mainvr-food-service/tasks.md`
- Scene 1 quickstart: `specs/002-mainvr-food-service/quickstart.md`
- Scene 1 contracts: `specs/002-mainvr-food-service/contracts/`
