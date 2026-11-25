# NutriPick VR

NutriPick VR is an educational first-person nutrition game built in Unity 6. The player uses a **Rexus gamepad** to move, aim a reticle, pick up food, plate it, and serve a meal. At the start the player picks one of three **quests**: **Anak-anak** (7-9), **Dewasa** (19-29), or **Lansia** (60-69). Each quest sets four tasks per level: two food-category requirements and two nutrient minimums. Scene 1 is a **home kitchen and dining room**: eight individual plates hold eight foods and the player plates breakfast on the dining table. Scene 2 is **Warteg Tegal Sari**, an open-front roadside warung with a seven-dish display (three portions per dish), benches, a cashier table, five set-dressing NPCs, and a row of shophouses. Scene 3 is the **Flamboyant bazaar district**: 30 open food stalls with 120 portions, a cross intersection, four coffee shops, two cafes, SMA Negeri 1 Manado and SMK Negeri 1 Manado backdrops, nine NPCs, and the Panggung Bazaar serving stage. All 18 foods carry energy, macro, sodium, sugar, vitamin A/C/D, calcium, and iron values, shown on the full food card. No level requires a drink. A Figma-style UI Canvas shows the food-card grid, the four-task checklist, the level report, and the campaign summary.

1. **Rumah** — breakfast at home (`MainVR`)
2. **Warteg** — lunch at Warteg Tegal Sari, seven dishes and five NPCs (`Level02_Warteg`)
3. **Bazaar** - dinner at the Flamboyant bazaar, with 30 open food stalls, 120 portions, a new intersection, four coffee shops, nine NPCs, and a serving stage (`Level03_Bazaar`)

## Requirements

- **Unity 6000.6.0f1** with Windows Build Support. This is the project's pinned version; do not open it with 6000.2.6f2.
- A **Rexus gamepad** (standard XInput/gamepad). Keyboard and mouse are the fallback.
- Blender 5.x is only needed to regenerate the environment kits. The exported FBX assets are already in the Unity project; playing does not require Blender.

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
| RT (toggle) | Sprint on/off (2x speed) |
| D-pad / up-down | Navigate report and pause buttons; the focused button gets a gold outline |

## Rexus Gamepad

The player uses the new Input System. An XInput, PS4, or Switch Pro-style Rexus pad is read as a `Gamepad`; a pad in DirectInput or Android mode is read through the generic `Joystick` fallback.

- Connect the controller before pressing Play. If Windows exposes it as a generic gamepad, open `Assets/Input/RexusJoystickProfile.asset` and fill the stick, axis, and button paths.
- In Play mode, press **F9** to open the controller diagnostics overlay. It lists the detected devices, layouts, live stick/button values, and can copy a report to the clipboard.
- Default mapping: left stick moves, right stick looks, A/Cross interacts, B/Circle cancels, X/Square inspects, Start/Options serves, Select/Share pauses.
- Keyboard and mouse remain available as a fallback.

## Open and Run

1. Open the project with Unity 6000.6.0f1.
2. Open `Assets/Scenes/MainVR.unity`.
3. Press **Play**.
4. Scene 1 opens with a **CARA BERMAIN** page listing the controller inputs and rules. Press **MULAI** with A/Enter, then choose **ANAK-ANAK**, **DEWASA**, or **LANSIA** on the quest page. Pick four foods that satisfy the level's tasks, place them on the serving plate, and press **Start**. The food-card grid, four-task checklist, level report, and campaign summary stay visible.
5. Scene 2 is `Assets/Scenes/Level02_Warteg.unity`. Pick four foods from the 21 portions (three per dish) that satisfy the lunch tasks, then plate them at the cashier table and press **Start**. Five NPCs - a cook, a server, and three diners - are set dressing only.
6. Scene 3 is `Assets/Scenes/Level03_Bazaar.unity`. The player spawns in front of the serving plate at the Panggung Bazaar stage. Collect four foods from any of the 30 open stalls and satisfy the dinner tasks. No drink is required in any level. After the final serve, **LIHAT RINGKASAN** opens the campaign summary with daily nutrition totals, %AKG, and over-limit warnings.

Build Settings contains `MainVR`, `Level02_Warteg`, and `Level03_Bazaar` in campaign order. Scene 1 uses `Assets/Models/Environment/Home/` (kitchen, island, dining, decor), `Assets/Models/Food/Plating/` (platters, plates, bowls), and `Assets/Prefabs/Interaction/ServingPlate.prefab` with `Assets/Models/Props/ceramic_plate.fbx`. Scene 2 uses the new `Assets/Models/Environment/Warteg/` kit. Scene 3 uses `Assets/Models/Environment/Bazaar/` for the school, stalls, cafes, stage, and street props. NPCs live in `Assets/Prefabs/NPC/`.

## Generate or Refresh Content

The project includes an editor builder that creates the ScriptableObjects, food prefabs, HUD, plate, stands, serve station, and the three level scenes:

- Menu: **NutriPick → Build All** (generates foods, Level 2, Level 3)
- Menu: **NutriPick → Setup Scene 1 Data** (protein types and LD_MainVR)
- Menu: **NutriPick → Generate Scene 1 Food Icons**
- Menu: **NutriPick → Build Serving Plate**
- Menu: **NutriPick → Build Quest Data** (3 quest definitions and food micronutrients)
- Menu: **NutriPick → Build Quest Foods** (6 new food prefabs, data, and icons)
- Menu: **NutriPick → Build Figma HUD**
- Menu: **NutriPick → Build Scene 1 Figma Setup**
- Menu: **NutriPick → Build Scene 1 Home** (kitchen + dining + platters)
- Menu: **NutriPick → Build NPC Kit** (three rigs, five clips, seven prefabs)
- Menu: **NutriPick → Build Scene 2 Warteg** (open-front warung, seven dishes with three portions each, five NPCs, shophouse world)
- Menu: **NutriPick → Build Scene 3 Bazaar** (Flamboyant bazaar, 30 food stalls, intersection, four coffee shops, school/SMK backdrop, stage, nine NPCs)
- Menu: **NutriPick → Validate All**
- Menu: **NutriPick → Validate Scenes**
- Menu: **NutriPick → Capture Scene Previews**
- Menu: **NutriPick → Build Windows**

The Windows build is written to `Builds/Windows/NutriPick.exe`.

## Tests

- **EditMode**: 36 tests covering classification, meal evaluation, session state, level validation, campaign chain, protein types, quest evaluation, AKG warnings, food-card formatting, food texture assignment, food asset quality, icon presence, and polygon budget.
- **PlayMode**: 40 tests covering quest selection, the four-task serve gate, HUD progress, info-card micronutrients, level report, campaign summary, Scene 1 food plates/start guide/card grid/focus highlight/replication, MainVR plate-and-serve, Scene 2 Warteg display/21 portions/shophouse world/NPC cast, Scene 3 30-stall layout/120 foods/intersection/coffee shops/no drinks/NPC cast/spawn at the plate, serving plate seating, held-food follow, food portion separation, camera height, pause navigation/highlight, sprint toggle, Level 2/3 smoke tests, and gamepad/generic-joystick/keyboard input. The current run is 40 passed, 0 skipped, 0 failed.

Run them from **Window → General → Test Runner**, or headlessly:

```powershell
& "D:\Unity Projects\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" `
  -batchmode -projectPath "D:\Unity Projects\VR_Project" `
  -runTests -testPlatform EditMode `
  -testResults "C:\Users\Lenovo\AppData\Local\Temp\opencode\nutripick-editmode.xml" -logFile "C:\Users\Lenovo\AppData\Local\Temp\opencode\unity-tests.log"
```

Use `-testPlatform PlayMode` for the PlayMode suite.

## Scene 3 Assets

The Flamboyant bazaar is generated by `Blender/Scene3_Bazaar/scripts/build_scene3_assets.py` and exported to `Assets/Models/Environment/Bazaar/`. The six kits are `SchoolCampus`, `BazaarStand`, `BazaarStandClosed`, `CafeKiosk`, `BazaarStage`, and `StreetProps`. Scene 3 reuses the existing food prefabs, `ServingPlate`, `ServeStation`, and `FigmaHUD`; the final layout contains 30 open food stalls, 120 portions, a cross intersection, four coffee shops, and a visible serving table at the stage.

## Scene 2 Warteg Assets

The open-front Warung Tegal is generated by `Blender/Scene2_Warteg/scripts/build_scene2_assets.py` and exported to `Assets/Models/Environment/Warteg/`. The four modules are `WartegShell`, `WartegCounter`, `WartegFurniture`, and `WartegProps`: a cream-and-wood shell, red striped awning, glass food display with seven tray wells, long benches, cashier table, drink cooler, rice cooker, gas cylinder, crates, pendant lamps, and potted plants. The Unity materials are at `Assets/Materials/Warteg/`.

## NPC Kit

The three low-poly bodies and five clips are generated by `Blender/NPC/scripts/build_npc_assets.py` and exported to `Assets/Models/Characters/NPC/`. Each FBX uses a 21-bone generic rig and contains `Idle`, `Walk`, `Eat`, `Talk`, and `Wipe`. Seven prefab variants live in `Assets/Prefabs/NPC/`; the chef apron and cap are separate meshes toggled per role. NPCs use the `NPC` layer and are excluded from the interaction raycast. Scene 2 has five NPCs (cook, server, three diners); Scene 3 has nine (walkers, stall visitors, a cafe guest, and stage audience).

## Food Textures

Scene 1 foods use **17 albedo textures** cropped from the reference photos in `References/Scene1_Home/foods/`. The reproducible crop manifest is at `Blender/Scene1_Home/textures/food/texture_manifest.json`, the preparation script is `Blender/Scene1_Home/scripts/prepare_food_textures.py`, and the final PNGs live in `Assets/Textures/Food/Home/`. Food materials stay **URP Unlit** so the photo-derived colors remain consistent under the room lighting. Attribution and share-alike notes are in `Assets/Textures/Food/Home/ATTRIBUTION.md`.

## MCP Integration

Both MCP servers are configured globally in `~/.config/opencode/opencode.json`:

- **Blender MCP** — `blender-mcp` (uv tool) plus the `blender_mcp.py` addon; server entry `blender` connects to `localhost:9876`.
- **Unity MCP** — `mcpforunityserver` (uv tool) as `unity`, using `--transport stdio`; the Unity package is embedded at `Packages/com.coplaydev.unity-mcp` (v10.0.0). `McpAutoStartOnce` selects the stdio bridge and starts it when the Editor loads.

Open Blender and Unity, then run `opencode mcp list`; both should report `connected`. The Unity bridge listens on port `6400`.

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
- Scene 3 plan: `specs/006-scene3-flamboyant-bazaar/plan.mdx`
- Rexus gamepad plan: `specs/007-rexus-gamepad/plan.mdx`
- Scene 2 Warteg/NPC spec: `specs/008-scene2-warteg-npcs/spec.md`
- Scene 2 Warteg/NPC plan: `specs/008-scene2-warteg-npcs/plan.mdx`
