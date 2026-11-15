# Implementation Plan: NutriPick VR — Healthy Food Selection Game

**Branch**: `001-nutripick-vr-game` | **Date**: 2026-11-16 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-nutripick-vr-game/spec.md`

## Summary

Build a three-level educational first-person game inside the existing Unity project. The player uses a **Rexus gamepad** to move, look, aim a center reticle, pick up food, carry it, and place it on a plate. A live Health Balance Bar shows the meal's balance, and serving produces a supportive report. The three levels are a warteg breakfast, a Padang lunch, and a **bazaar dinner with multiple food stands**.

The technical approach is deliberately small and testable: a standard `CharacterController` first-person rig, Unity Input System gamepad bindings, a reticle-based pick/carry/place interaction, custom plate slots, ScriptableObject food content, a pure C# meal evaluator, screen-space TextMeshPro UI, and 3D food stands. Existing Blender FBX food and prop assets are reused; the asset team supplies five missing foods and modular bazaar stands, and the implementation uses placeholder prefabs until delivery. Level 1 is completed end-to-end before Levels 2 and 3.

No VR headset or motion controller is available, so the current build is gamepad-first. The design keeps a first-person, real-world-scale layout so stereoscopic VR can be added later, but VR is out of scope for this version.

## Technical Context

**Language/Version**: C# on Unity 6000.6.0f1 (Unity 6.6), .NET Standard 2.1

**Primary Dependencies**: Universal Render Pipeline 17.6.0, Input System 1.20.0, UGUI 2.6.0, TextMeshPro, Unity Test Framework 1.8.0. XR Interaction Toolkit and OpenXR remain installed but are not used by the runtime.

**Storage**: ScriptableObject assets for food, rules, and levels; in-memory `GameSession` for per-run results; no database, backend, or network

**Testing**: Unity Test Framework EditMode tests for classification, scoring, level validation, and session state; manual PlayMode acceptance checklist with a Rexus gamepad

**Target Platform**: Windows PC at 1920×1080 with a Rexus gamepad. Keyboard and mouse are the fallback. VR headset support is deferred.

**Project Type**: Single Unity first-person application

**Performance Goals**: 60 fps at 1920×1080 on a mid-range laptop; ≤150k triangles per level; ≤100 draw calls; textures ≤1024 px

**Constraints**: Gamepad-only primary input; first-person reticle; no motion controllers; no fail state; three scenes; 14 unique foods; Level 3 bazaar with at least three stands; existing assets reused; missing assets supplied by the asset team; no external services

**Scale/Scope**: Three levels, 14 unique foods, four Level 3 stands, approximately 20 prefabs, one persistent session, 8–12 minute play session

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

`.specify/memory/constitution.md` is the unmodified template: it contains no ratified project name, principles, constraints, or governance rules. There are therefore no project-specific gates to enforce.

Applied defaults for this plan:

- **Compile-clean first**: the project must compile before any feature work. The invalid `RexusInputProbe.cs` is replaced with a working gamepad input component in M0.
- **Pure logic is tested**: scoring, classification, level validation, and session state are covered by EditMode tests.
- **No external services**: the game is fully local and offline.
- **Scope discipline**: Level 1 ships end-to-end before Levels 2 and 3.
- **Accessibility**: the bar never relies on color alone; text is legible at 1080p.
- **No medical claims**: every report carries an educational disclaimer.
- **Input resilience**: the gamepad is primary, but keyboard and mouse remain a fallback.

**Result**: PASS. No violations, so the Complexity Tracking table is not applicable.

**Post-design re-check**: PASS. The design introduces no external services, no persistence layer, and no runtime platform beyond Windows desktop.

## Project Structure

### Documentation (this feature)

```text
specs/001-nutripick-vr-game/
├── plan.md              # This file
├── research.md          # Phase 0 decisions
├── data-model.md        # Phase 1 entities and rules
├── quickstart.md        # Phase 1 validation guide
├── contracts/
│   ├── food-data.md
│   ├── meal-evaluation.md
│   ├── level-flow.md
│   ├── interaction.md
│   └── ui-presentation.md
├── content/
│   └── food-catalog.md  # 14 foods with approximate nutrition values
├── checklists/
│   └── requirements.md
└── spec.md
```

### Source Code (repository root)

Team-owned asset sources are not part of this implementation. See [content/food-catalog.md](./content/food-catalog.md) for the missing-asset handoff list and integration requirements.

```text
Assets/
├── Audio/
│   └── SFX/                      # hover, pick, place, drop, serve, report
├── Input/
│   └── NutriPick.inputactions    # Gameplay and Menu action maps
├── Editor/
│   ├── NutriPick.Editor.asmdef
│   ├── FoodDataValidator.cs
│   └── LevelDefinitionValidator.cs
├── Materials/
├── Models/
│   ├── Food/                     # existing FBX + team-provided exports
│   └── Props/                    # stalls, counters, tables, chairs
├── Prefabs/
│   ├── Food/                     # 14 food prefabs
│   ├── Interaction/              # Plate, PlateSlot, ServeStation
│   ├── Environment/              # FoodStand, BazaarStand, Canopy
│   └── UI/                       # HUD, InfoCard, Report, Recap
├── Scenes/
│   ├── MainVR.unity              # sandbox, not in build settings
│   ├── Level01_Warteg.unity
│   ├── Level02_Padang.unity
│   └── Level03_Bazaar.unity
├── ScriptableObjects/
│   ├── Food/                     # 14 FoodData assets
│   ├── Levels/                   # 3 LevelDefinition assets
│   └── Rules/                    # NutritionRules asset
├── Scripts/
│   ├── Player/
│   │   ├── FirstPersonController.cs
│   │   ├── GamepadInput.cs
│   │   ├── ReticleInteractor.cs
│   │   └── HeldFoodCarrier.cs
│   ├── Nutrition/
│   │   ├── FoodData.cs
│   │   ├── NutritionRules.cs
│   │   ├── FoodClassifier.cs
│   │   ├── MealEvaluator.cs
│   │   ├── MealResult.cs
│   │   └── HealthDimensions.cs
│   ├── Content/
│   │   ├── LevelDefinition.cs
│   │   └── FoodStand.cs
│   ├── Interaction/
│   │   ├── FoodItem.cs
│   │   ├── PlateSlot.cs
│   │   ├── PlateController.cs
│   │   └── ServeStation.cs
│   ├── UI/
│   │   ├── NutritionBar.cs
│   │   ├── FoodInfoCardPresenter.cs
│   │   ├── LevelTimerDisplay.cs
│   │   ├── InteractionPrompt.cs
│   │   ├── HealthReportPresenter.cs
│   │   └── RecapPresenter.cs
│   ├── Flow/
│   │   ├── GameSession.cs
│   │   ├── LevelFlowController.cs
│   │   └── SceneLoader.cs
│   ├── Utilities/
│   │   ├── NutritionFormat.cs
│   │   ├── SfxPlayer.cs
│   │   └── GamepadRumble.cs
│   └── NutriPick.Runtime.asmdef
├── Settings/
├── Tests/
│   └── EditMode/
│       ├── NutriPick.Tests.EditMode.asmdef
│       ├── FoodClassifierTests.cs
│       ├── MealEvaluatorTests.cs
│       ├── GameSessionTests.cs
│       └── LevelDefinitionTests.cs
└── TextMesh Pro/
```

**Structure Decision**: A single Unity project with one scene per level and one persistent `GameSession`. Runtime code is isolated in `NutriPick.Runtime.asmdef`; editor validators and EditMode tests reference it. Food and level content live in ScriptableObjects so designers can tune values without touching code. `MainVR.unity` remains a sandbox and is not in build settings.

## Implementation Phases

### M0 — Foundation (blocking)

Fix the project and establish the gamepad-first input and HUD foundation.

- Delete `Assets/Scripts/RexusInputProbe.cs`; it is invalid C# and blocks compilation. Replace it with `GamepadInput` and a `NutriPick.inputactions` asset.
- Delete `Assets/Scenes/VRButton.cs`; the gamepad build does not use 3D XR buttons.
- Add `NutriPick.Runtime.asmdef`, `NutriPick.Editor.asmdef`, and `NutriPick.Tests.EditMode.asmdef`.
- The `PlayerRig` prefab is created in M2 (US1); M0 only prepares the input actions and HUD skeleton.
- Add a screen-space HUD canvas with EventSystem and `InputSystemUIInputModule`.
- Verify the Rexus gamepad is detected as a standard gamepad; test left stick, right stick, A/B/X, Start, and Select. If it is not detected, verify the fallback keyboard/mouse bindings and document the device name.
- Configure the PC quality level: MSAA 4x, HDR off, render scale 1.0.
- Confirm TextMeshPro essentials are imported and LiberationSans SDF renders Indonesian text.
- Create empty `Level01_Warteg`, `Level02_Padang`, and `Level03_Bazaar` scenes and add them to Build Settings.
- **Exit criteria**: project compiles with zero errors; input actions and the screen-space HUD skeleton exist; the Rexus gamepad is detected and the fallback bindings are verified; all three level scenes open. The player rig is created in M2 (US1).

### M1 — Nutrition Core

Build the brain before the visuals.

- Implement `FoodData`, `NutritionRules`, `FoodClassifier`, `MealEvaluator`, `MealResult`, `HealthDimensions`, and `LevelDefinition`.
- Create the `NutritionRules` asset with the thresholds and weights in [contracts/meal-evaluation.md](./contracts/meal-evaluation.md).
- Create 14 `FoodData` assets from [content/food-catalog.md](./content/food-catalog.md).
- Create three `LevelDefinition` assets with the food pools, minimum item counts, drink rule, stand count, and 180-second timer.
- Implement `FoodDataValidator` and `LevelDefinitionValidator` editor menus.
- Write EditMode tests for empty plate (50), balanced plate (>70), fried plate (<50), all-banana plate (<40), clamping, classification, overrides, level validation, and session state.
- **Exit criteria**: all EditMode tests pass; the evaluator is deterministic and has no scene references; validators are implemented and run without exceptions. Full zero-error content validation is deferred to M7/T067, after prefabs and scenes exist.

### M2 — Interaction Core

Build the loop with placeholder art if necessary.

- Create the `PlayerRig` prefab: `CharacterController`, camera pivot, camera, center reticle, `FirstPersonController`, `GamepadInput`, `ReticleInteractor`, and `HeldFoodCarrier`.
- Create the food prefab template: `FoodItem`, collider, Rigidbody, model child, and info anchor. No XR components.
- Create 14 food prefabs from existing FBX and placeholder primitives for missing assets.
- Implement `ReticleInteractor`: raycast from the camera center, hover detection, pick up, carry, place, drop, and inspect.
- Implement `HeldFoodCarrier`: hold position in front of the camera, smooth follow, drop/return.
- Create the plate prefab with four `PlateSlot` triggers, a `PlateController`, and a visible ring highlight.
- Implement `FoodStand` with food anchors and `ServeStation` with a reticle-targetable collider.
- Implement `LevelTimerDisplay`, `InteractionPrompt`, and the HUD; wire the timer and prompt into `LevelFlowController`.
- Implement `LevelFlowController` with Explore → Plating → Ready → Served → Report states.
- **Exit criteria**: in the sandbox scene, a player can move with the gamepad, pick up three foods, place them on the plate, press Serve, and reach a placeholder report. The bar and info card are added in M3 (US2/US3).

### M3 — Level 1 Vertical Slice

Make the first real level.

- Build the warteg environment from `warteg_stall.fbx`, `display_counter.fbx`, `wooden_table.fbx`, and `wooden_chair.fbx`.
- Add the eight Level 1 foods: rice, boiled egg, fried egg, tempeh, broccoli, banana, fried noodles, crackers. Use a placeholder rice prefab until the asset team delivers the final asset.
- Lay out the counter, plate, HUD, info card, and serve station for comfortable first-person reach.
- Implement the Health Balance Bar and food information card (US2/US3): preview/final animation, counts, compact/full card, and inspect.
- Wire the placeholder report and verify the Level 1 end-to-end loop; the full report content is implemented in M5 (US4).
- Add retry and next-level actions.
- Playtest one full session and fix blocking issues.
- **Exit criteria**: Level 1 is playable from scene start to a placeholder report; the bar and info card work; all Level 1 acceptance scenarios in the spec pass. The full report and recap are validated in M5.

### M4 — Levels 2 and 3

Reuse the architecture; add content only.

- Level 2: Padang environment from `Wrng_padangggg.fbx`, `PadangPNG.png`, table, and chairs; seven foods: rice, rendang, fried chicken, cassava leaves, boiled egg, broccoli, crackers. Use the placeholder rice and cassava-leaf prefabs until team assets arrive.
- Level 3: bazaar environment with at least four stands and a central serving table. Distribute the eight foods:
  - Mie & Jajanan: fried noodles, burger.
  - Lauk: boiled egg, tempeh.
  - Sayur & Buah: broccoli, banana.
  - Minuman: water, sweet tea.
- Build the bazaar from existing stalls plus team-provided canopy/stand modules. If they are not delivered, use placeholder stands. Ensure a clear walkable path between stands.
- Enforce the Level 3 drink requirement in `PlateController.CanServe`.
- Integrate the five team-provided food assets and bazaar modules when delivered; validate scale, pivot, colliders, and materials against the handoff spec in [content/food-catalog.md](./content/food-catalog.md).
- **Exit criteria**: Levels 2 and 3 are playable; the player can walk between all Level 3 stands; Level 3 cannot be served without a drink; team assets are integrated or placeholders are documented.

### M5 — Report, Recap, and Session

Close the loop.

- Implement `GameSession` with `DontDestroyOnLoad` and per-level results.
- Implement `BeginLevel`, `RecordResult`, `GetResult`, `ResetSession`, and `HasResult`.
- Implement retry (reload current scene, replace result) and next (load next scene).
- Implement the full report content: score, band, exactly two strengths, one improvement, three health dimensions, the incomplete-plate warning, and the disclaimer.
- Implement the final recap panel in Level 3: three level rows, average score, two strengths, one improvement, closing message, restart, and disclaimer.
- Implement gamepad navigation for all report and recap buttons.
- **Exit criteria**: the full report and recap render correctly; a full three-level session retains all results; retry replaces only the current level; recap shows the latest attempts; restart clears the session.

### M6 — Polish

Only after the content is complete.

- Add audio: hover, pick, place, drop, serve, bar tick, report reveal.
- Add gamepad rumble: pick (0.2 s), place (0.1 s), serve (0.15 s).
- Add plate slot highlight, bar animation easing, and report reveal animation.
- Tune lighting, materials, and scale for readability and comfort.
- Reduce heavy FBX meshes and add LODs where needed.
- Verify all text at 1080p; verify the bar is never color-only.
- **Exit criteria**: 60 fps at 1080p; no console errors; all text readable by five testers; audio and rumble fire on every required event.

### M7 — Validation and Demo

- Run the five-user test in [quickstart.md](./quickstart.md) with the Rexus gamepad.
- Fix blocking bugs only; defer cosmetic issues.
- Build a Windows demo and verify it runs outside the editor.
- Write a three-minute demo script: Level 1 healthy plate, Level 2 less healthy plate, Level 3 bazaar walk and drink choice, recap.
- **Exit criteria**: SC-001 to SC-013 pass or have documented exceptions; demo runs without developer intervention.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Unity 6000.6.0f1 is required and must be installed | Cannot open or test the project correctly | Install 6000.6.0f1 with Windows Build Support via Unity Hub; do not open the project with 6000.2.6f2 |
| Rexus gamepad is not detected as a standard XInput gamepad | Core input fails | Test in M0 with the Input System debugger; add a keyboard/mouse fallback; document the exact device name and bindings |
| No physical VR headset | VR validation impossible | Gamepad-first is the agreed scope; VR is explicitly deferred |
| Asset team delivery is late | Missing food and bazaar assets delay final art | Keep placeholder prefabs behind the same interface; integrate delivered assets in M4; the exact handoff list is in [content/food-catalog.md](./content/food-catalog.md) |
| Rendang (10 MB), fried chicken (5.4 MB), and burger (3.4 MB) FBX files are heavy | Low frame rate and long imports | Audit triangle counts; decimate to ≤5k triangles per food; add LODs; cap textures at 1024 px |
| Bazaar level scope grows beyond four stands | Missed deadline | Four stands only; reuse existing stalls; no new architecture; Level 1 and 2 ship first |
| Nutrition values are approximate | Misleading education | Use public references, label values as estimates, show the disclaimer, and review the catalog before the demo |
| Scene merge conflicts in a group project | Lost work | One scene per owner; prefab everything interactive; commit `.meta` files; use UnityYAMLMerge and Git LFS for FBX, textures, and audio |

## Complexity Tracking

No constitution violations. The design uses one runtime assembly, one persistent session object, three level scenes, one first-person controller, and no external services. No additional projects or abstraction layers are justified.

## References

- [spec.md](./spec.md) — feature specification
- [research.md](./research.md) — Phase 0 decisions and worked scoring examples
- [data-model.md](./data-model.md) — entities, fields, validation, and state transitions
- [contracts/food-data.md](./contracts/food-data.md) — food content contract
- [contracts/meal-evaluation.md](./contracts/meal-evaluation.md) — classification and scoring contract
- [contracts/level-flow.md](./contracts/level-flow.md) — scene, session, and retry/next contract
- [contracts/interaction.md](./contracts/interaction.md) — gamepad, reticle, carry, plate, and feedback contract
- [contracts/ui-presentation.md](./contracts/ui-presentation.md) — HUD, report, color, typography, and copy contract
- [quickstart.md](./quickstart.md) — setup, controls, and validation scenarios
- [content/food-catalog.md](./content/food-catalog.md) — 14 foods with approximate nutrition values
