# Tasks: NutriPick VR — Healthy Food Selection Game

**Input**: Design documents from `/specs/001-nutripick-vr-game/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Tests**: EditMode tests are included for the pure nutrition, classification, level, and session logic because `plan.md` requires that coverage. UI and gamepad interaction are validated manually through `quickstart.md`; there is no TDD requirement for those layers.

**Organization**: Tasks are grouped by user story so each story can be implemented and tested as an independent increment.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: `[US1]`, `[US2]`, `[US3]`, `[US4]` for user story phases only
- Every task includes an exact file path

## Team Asset Handoff (Not Implementation Tasks)

The asset team supplies these assets. The implementation uses placeholder prefabs with the same `FoodData` and prefab contract until delivery. Do not block M2/M3 on art delivery.

| Asset | Type | Notes |
|-------|------|-------|
| `nasi_putih` | Food | Bowl/portion of white rice; 150 g; pivot at base center |
| `kerupuk` | Food | Small pile of crackers; use `Kerupuk.jpg`; 30 g |
| `sayur_singkong` | Food | Cassava-leaf vegetable; use `Sayursingkong1.jpg`; 100 g |
| `air_putih` | Drink | Clear water glass; 250 ml; 0.08 m × 0.12 m |
| `teh_manis` | Drink | Sweet tea glass; 250 ml; 0.08 m × 0.12 m |
| `bazaar_stand` | Environment | Modular market stand with counter, posts, canopy; ≤3k triangles |
| `bazaar_canopy` | Environment | Cloth canopy/tent and banner; reusable |

Integration requirements are in `content/food-catalog.md`. Placeholder tasks: T033. Integration task: T057.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and the shared runtime skeleton.

- [X] T001 Verify Unity 6000.6.0f1 is installed and open the project with it; do not open with 6000.2.6f2. Confirm `ProjectSettings/ProjectVersion.txt` and zero compile errors.
- [X] T002 Remove the invalid `Assets/Scripts/RexusInputProbe.cs` and the stub `Assets/Scenes/VRButton.cs`; the gamepad build does not use them.
- [X] T003 Create the folder structure under `Assets/`: `Input`, `Scripts/Player`, `Scripts/Nutrition`, `Scripts/Content`, `Scripts/Interaction`, `Scripts/UI`, `Scripts/Flow`, `Scripts/Utilities`, `Prefabs/Food`, `Prefabs/Interaction`, `Prefabs/Environment`, `Prefabs/UI`, `ScriptableObjects/Food`, `ScriptableObjects/Levels`, `ScriptableObjects/Rules`, `Tests/EditMode`, `Audio/SFX`.
- [X] T004 [P] Create `Assets/Scripts/NutriPick.Runtime.asmdef`, `Assets/Editor/NutriPick.Editor.asmdef`, and `Assets/Tests/EditMode/NutriPick.Tests.EditMode.asmdef`; reference the runtime assembly from the editor and test assemblies.
- [X] T005 [P] Create `Assets/Input/NutriPick.inputactions` with `Gameplay` and `Menu` action maps: left stick move, right stick look, A interact, B cancel, X inspect, Start serve, Select pause, D-pad/left stick navigate.
- [X] T006 [P] Configure the PC quality level in `ProjectSettings/QualitySettings.asset`: MSAA 4x, HDR off, render scale 1.0; import TMP Essentials if `Assets/TextMesh Pro/Resources/TMP Settings.asset` is missing.
- [X] T007 [P] Keep `Assets/Scenes/MainVR.unity` as Level 1; create `Assets/Scenes/Level02_Padang.unity` and `Assets/Scenes/Level03_Bazaar.unity`; add `MainVR`, `Level02_Padang`, `Level03_Bazaar` to Build Settings in that order.
- [X] T008 [P] Create `Assets/Scripts/UI/LevelTimerDisplay.cs`, `Assets/Scripts/UI/InteractionPrompt.cs`, and `Assets/Prefabs/UI/HUD.prefab` as a screen-space canvas at 1920×1080 with an EventSystem and `InputSystemUIInputModule`; add the center reticle, timer, bar, prompt, and info-card anchors.

**Checkpoint**: Project compiles with Unity 6000.6.0f1 and the input/HUD skeleton exists.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared data, scoring, session, and validation that every user story needs.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T009 Create `Assets/Scripts/Nutrition/FoodData.cs` with `id`, `displayName`, `description`, `composition` flags, `processing`, `isFried`, `isDrink`, `calories`, `protein`, `carbs`, `fat`, `fiber`, `addedSugar`, `sodium`, `portionLabel`, `icon`, `prefab`, and `healthyOverride`. Enforce: unique lowercase snake_case `id`; at least one composition flag; `isDrink` matches the Drink flag; `prefab` required.
- [X] T010 Create `Assets/Scripts/Nutrition/NutritionRules.cs` with the weights and thresholds from `contracts/meal-evaluation.md` and `data-model.md`; enforce `itemWeight + compositionWeight = 1`, positive targets, and bands that cover 0–100 without gaps.
- [X] T011 Create `Assets/Scripts/Content/LevelDefinition.cs` with `levelId`, `displayName`, `sceneName`, `levelIndex`, `settingDescription`, `foodPool`, `minItems`, `requiresDrink`, `minimumStands`, `timeLimitSeconds`, `graceSeconds`, `isFinalLevel`, and `learningFocus`. Enforce: 6–8 unique foods, `minItems` 3–4, Level 3 `requiresDrink` and `minimumStands >= 3`.
- [X] T012 Create `Assets/Scripts/Nutrition/MealResult.cs` and `Assets/Scripts/Nutrition/HealthDimensions.cs` with `score`, `band`, `bandLabel`, counts, item scores, average item health, composition score, strengths, improvement, dimensions, `isIncomplete`, and warnings.
- [X] T013 Create `Assets/Scripts/Nutrition/FoodClassifier.cs` with `Resolve(FoodData, NutritionRules)` using the ordered rules in `contracts/food-data.md`; include the `ForceHealthy`/`ForceLessHealthy` overrides.
- [X] T014 Create `Assets/Scripts/Nutrition/MealEvaluator.cs` as a pure deterministic function per `contracts/meal-evaluation.md`: empty plate returns 50 neutral; clamp 0–100; composition-aware score; order-independent.
- [X] T015 Create `Assets/Scripts/Flow/GameSession.cs` with `EnsureExists`, `BeginLevel`, `RecordResult`, `GetResult`, `ResetSession`, and `AverageScore`; retry replaces the level result and reset clears all results.
- [X] T016 [P] Create `Assets/ScriptableObjects/Rules/NutritionRules.asset` from the defaults in `data-model.md`.
- [X] T017 [P] Create 14 `FoodData` assets in `Assets/ScriptableObjects/Food/` from `content/food-catalog.md`: `FD_nasi_putih`, `FD_telur_rebus`, `FD_telur_ceplok`, `FD_tempe`, `FD_brokoli`, `FD_pisang`, `FD_mie_goreng`, `FD_kerupuk`, `FD_rendang`, `FD_ayam_goreng`, `FD_sayur_singkong`, `FD_burger`, `FD_air_putih`, `FD_teh_manis`. Leave each asset's `prefab` field unassigned until T032/T033; assign the matching prefab when those prefabs are created.
- [X] T018 [P] Create `Assets/ScriptableObjects/Levels/LD_MainVR.asset`, `LD_Level02_Padang.asset`, and `LD_Level03_Bazaar.asset` with the pools and rules from `data-model.md`; Level 3 must have `requiresDrink = true`, `minItems = 4`, `minimumStands = 4`, `isFinalLevel = true`.
- [X] T019 [P] Write `Assets/Tests/EditMode/FoodClassifierTests.cs` and `Assets/Tests/EditMode/MealEvaluatorTests.cs`; cover empty plate 50, balanced ≥70, fried <50, three bananas <40, clamping, order independence, classification thresholds, and overrides.
- [X] T020 [P] Write `Assets/Tests/EditMode/GameSessionTests.cs` and `Assets/Tests/EditMode/LevelDefinitionTests.cs`; cover begin/reset, record/replace, latest-attempt recap, 6–8 foods, 2 healthy/2 less healthy, Level 3 drink, stand count, and scene name.
- [X] T021 [P] Create `Assets/Editor/FoodDataValidator.cs` and `Assets/Editor/LevelDefinitionValidator.cs` with menu items `NutriPick/Validate Food Data` and `NutriPick/Validate Levels`; log asset paths for every error. These validators may report missing-prefab and missing-stand errors until T032/T033 and T055/T056; do not treat those as blocking at this checkpoint.

**Checkpoint**: All EditMode tests pass; both validators are implemented and run without exceptions. Full zero-error content validation is deferred to T067, after prefabs and scenes exist.

---

## Phase 3: User Story 1 - Build a meal with a gamepad (Priority: P1) 🎯 MVP

**Goal**: A first-person gamepad loop where the player moves, aims, picks up food, carries it, plates it, and serves a Level 1 meal.

**Independent Test**: Enter Level 1 with the Rexus gamepad, move and look, pick up three items, place them on the plate, serve, and reach a placeholder report. No bar, info card, or recap is required for this test.

### Implementation for User Story 1

- [X] T022 [P] [US1] Create `Assets/Scripts/Player/FirstPersonController.cs` and `Assets/Scripts/Player/GamepadInput.cs`; implement left-stick movement, right-stick look, gravity, `CharacterController` collision, and gamepad detection with keyboard/mouse fallback.
- [X] T023 [US1] Create `Assets/Scripts/Player/ReticleInteractor.cs` and `Assets/Scripts/Player/HeldFoodCarrier.cs`; implement the 3.0 m center raycast, hover/pick/place/drop/inspect states, and the hold anchor at 0.5 m forward and 0.15 m down. Create `FoodItem` and `PlateSlot` (T025/T026) before compiling this script.
- [X] T024 [US1] Create `Assets/Prefabs/Player/PlayerRig.prefab` with `CharacterController`, camera pivot, camera, hold anchor, `FirstPersonController`, `GamepadInput`, `ReticleInteractor`, and `HeldFoodCarrier`.
- [X] T025 [US1] Create `Assets/Scripts/Interaction/FoodItem.cs`; expose `Data`, `HomeStand`, `IsCarried`, `SetCarried`, and `ReturnHome`; enforce the on-stand/carried/plated/dropped states from `contracts/food-data.md`. Create `FoodStand` (T028) first, or use a `Transform homeAnchor` and resolve the stand at runtime.
- [X] T026 [P] [US1] Create `Assets/Scripts/Interaction/PlateSlot.cs`; implement `TryPlace`, `Remove`, occupancy, snap point, and slot highlight.
- [X] T027 [US1] Create `Assets/Scripts/Interaction/PlateController.cs`; track four slots, expose `Foods`, raise `PlateChanged`, implement `CanServe(out string reason)`, `Lock`, and `Clear`.
- [X] T028 [P] [US1] Create `Assets/Scripts/Content/FoodStand.cs`; implement `SpawnFoods`, `ReturnFood`, `Contains`, food anchors, and the stand display name.
- [X] T029 [P] [US1] Create `Assets/Scripts/Interaction/ServeStation.cs`; expose the reticle-targetable serve collider and the disabled state after serving. Use a UnityEvent for the serve action and do not reference `LevelFlowController` directly.
- [X] T030 [US1] Create `Assets/Scripts/Flow/LevelFlowController.cs` and `Assets/Scripts/Flow/SceneLoader.cs`; implement Explore → Plating → Ready → Served → Report, serve gating, timer start/stop, auto-serve, and scene loading by `LevelDefinition.sceneName`. Use UnityEvents or interfaces for `NutritionBar` and `HealthReportPresenter`; US1 uses the placeholder report and must not reference US2/US4 types directly.
- [X] T031 [P] [US1] Create `Assets/Prefabs/Food/FoodTemplate.prefab` with `FoodItem`, a primitive collider, a Rigidbody, a model child, and an info anchor.
- [X] T032 [US1] Create 9 food prefabs from the existing FBX assets in `Assets/Prefabs/Food/`: telur rebus, telur ceplok, tempe, brokoli, pisang, mie goreng, rendang, ayam goreng, burger. Use the `FoodTemplate` contract. Assign each prefab to the matching `FoodData.prefab` field.
- [X] T033 [P] [US1] Create 5 placeholder food prefabs in `Assets/Prefabs/Food/` for `nasi_putih`, `kerupuk`, `sayur_singkong`, `air_putih`, and `teh_manis`; use primitive shapes with the same `FoodData`, collider, and pivot contract. Assign each placeholder to the matching `FoodData.prefab` field.
- [X] T034 [US1] Create `Assets/Prefabs/Interaction/Plate.prefab` with four `PlateSlot` triggers, a `PlateController`, and a visible slot-highlight ring.
- [X] T035 [P] [US1] Create `Assets/Prefabs/Environment/FoodStand.prefab` and `Assets/Prefabs/Interaction/ServeStation.prefab`.
- [X] T036 [US1] Wire `Assets/Scenes/MainVR.unity` as Level 1: use the existing tabletop mockup, rename the empty `ceramic_plate (3)` to `Plate` with four slots, add the PlayerRig components, HUD, serve station, and 8 interactive Level 1 foods; configure `LD_MainVR` with `sceneName = MainVR`.
- [X] T037 [US1] Implement serve gating, the "Tambahkan X makanan lagi" prompt, the 180-second timer, and the full time-out flow in `LevelFlowController`: at 0 s auto-serve if the minimum is met; otherwise enter a 30 s grace state with a visible countdown; at grace end auto-serve with `isIncomplete = true` and the "Piring belum lengkap" warning; if the plate is empty at grace end, reload the level without recording a result. Force-return any carried item to its stand before evaluation. Wire the timer to `LevelTimerDisplay` and the prompts to `InteractionPrompt`. Create `Assets/Prefabs/UI/PlaceholderReport.prefab`.
- [ ] T038 [US1] Validate V1–V4 in `quickstart.md` with the Rexus gamepad: empty plate gate, move/look/reticle, pick/carry/place, drop, and serve.

**Checkpoint**: Level 1 is playable from scene start to a placeholder report using only the gamepad. This is the MVP.

---

## Phase 4: User Story 2 - See whether choices are healthy in real time (Priority: P2)

**Goal**: A live Health Balance Bar that previews every plate change and locks to a final value on serve.

**Independent Test**: In Level 1, add a healthy item and see the bar rise; add a less healthy item and see it fall; remove items and see the counts and number update. The info card and report are not required.

### Implementation for User Story 2

- [X] T039 [P] [US2] Create `Assets/Scripts/UI/NutritionBar.cs` with `SetPreview(MealResult)`, `SetFinal(MealResult)`, `SetNeutral()`, fill animation, band color, numeric value, label, and counts.
- [X] T040 [P] [US2] Create `Assets/Prefabs/UI/NutritionBar.prefab` with the track, fill, numeric value, band label, tick marks at 40/70, and healthy/less healthy counts.
- [X] T041 [US2] Wire `LevelFlowController` to call `MealEvaluator.Evaluate` on every `PlateController.PlateChanged` and pass the result to `NutritionBar.SetPreview`.
- [X] T042 [US2] Implement the empty 50 neutral state, 0.15 s preview animation, 1.0 s final animation, band colors, and the "Belum dinilai" label in `NutritionBar`.
- [X] T043 [US2] Implement the healthy/less healthy counts and the Indonesian labels "Sehat" and "Kurang sehat" in `Assets/Prefabs/UI/NutritionBar.prefab` and `Assets/Scripts/UI/NutritionBar.cs`.
- [ ] T044 [US2] Validate V5 in `quickstart.md`: empty state, add/remove movement, counts, final animation, and color + label agreement.

**Checkpoint**: The bar works independently of the info card and report; US1 still works with the bar disabled.

---

## Phase 5: User Story 3 - Learn about each food item (Priority: P2)

**Goal**: A compact reticle label and a full nutrition card for every food.

**Independent Test**: In Level 1, aim at any food and read the compact label; press X or pick it up and read the full card; press B and confirm the card dismisses. The bar and report are not required.

### Implementation for User Story 3

- [X] T045 [P] [US3] Create `Assets/Scripts/UI/FoodInfoCardPresenter.cs`; subscribe to `ReticleInteractor` hover, pick, and inspect events; show compact and full states.
- [X] T046 [P] [US3] Create `Assets/Prefabs/UI/FoodInfoCard.prefab` with the compact label and full card; include name, portion, badge, calories, protein, fiber, added sugar, sodium, description, and "Nilai gizi adalah perkiraan."
- [X] T047 [US3] Wire `ReticleInteractor` hover/pick/inspect/cancel events to `FoodInfoCardPresenter` in `MainVR` and the HUD prefab.
- [X] T048 [US3] Implement Indonesian nutrition formatting (comma decimals, "g", "mg", "kkal"), card positioning on the left, 0.2 s dismissal, and supportive copy in `FoodInfoCardPresenter`.
- [ ] T049 [US3] Validate V6 in `quickstart.md`: compact label, full card, inspect without pickup, dismissal, and no overlap with the reticle or plate.

**Checkpoint**: The information card works independently of the bar and report; US1 still works with the card hidden.

---

## Phase 6: User Story 4 - Three levels, bazaar, and recap (Priority: P3)

**Goal**: Level 2 and the Level 3 bazaar, full report, retry/next, and the final recap.

**Independent Test**: Play Level 1 → Level 2 → Level 3 bazaar, collect four items including a drink, serve, retry one level, and reach the recap with all three results. The bar and info card are already available from US2/US3.

### Implementation for User Story 4

- [X] T050 [P] [US4] Create `Assets/Scripts/UI/HealthReportPresenter.cs` and `Assets/Scripts/UI/RecapPresenter.cs`; render the report and recap from `MealResult` and `GameSession`.
- [X] T051 [P] [US4] Create `Assets/Prefabs/UI/HealthReport.prefab` and `Assets/Prefabs/UI/Recap.prefab` with the layout, text, and buttons from `contracts/ui-presentation.md`.
- [X] T052 [US4] Implement the report content in `Assets/Scripts/UI/HealthReportPresenter.cs`: score, band, exactly two strengths, one improvement, three dimensions, the "Piring belum lengkap" warning, and the disclaimer.
- [X] T053 [US4] Implement gamepad navigation and selection for all report/recap buttons using UGUI `Selectable` and the `Menu` action map.
- [X] T054 [US4] Build `Assets/Scenes/Level02_Padang.unity` with the Padang environment, `PlayerRig`, HUD, one stand, plate, serve station, and the 7 Level 2 foods.
- [X] T055 [US4] Build `Assets/Scenes/Level03_Bazaar.unity` with four signposted stands, a central serving table, a walkable path, `PlayerRig`, HUD, plate, and serve station.
- [X] T056 [US4] Distribute the 8 Level 3 foods across the four stands and configure `LD_Level03_Bazaar`:
  - Mie & Jajanan: fried noodles, burger.
  - Lauk: boiled egg, tempeh.
  - Sayur & Buah: broccoli, banana.
  - Minuman: water, sweet tea.
- [ ] T057 [US4] Integrate the team-provided assets when delivered: replace the 5 placeholder food prefabs and the placeholder bazaar stands/canopies; validate scale, pivot, colliders, materials, and `FoodData.prefab` references against `content/food-catalog.md`.
- [X] T058 [US4] Implement the Level 3 drink requirement and stand validation in `Assets/Scripts/Interaction/PlateController.cs` and `Assets/Editor/LevelDefinitionValidator.cs`: serving is blocked with "Pilih satu minuman" until a drink is on the plate; the validator checks 4 stands and at least one drink stand.
- [X] T059 [US4] Integrate `GameSession`: `BeginLevel` on scene start, `RecordResult` on serve, retry replaces the current result, next retains previous results, and the recap uses the latest attempts.
- [X] T060 [US4] Implement retry and next scene loading with a short fade in `LevelFlowController` and `SceneLoader`; add the Level 3 "Lihat rekap" action and the recap "Main lagi" reset.
- [ ] T061 [US4] Validate V7–V11 in `quickstart.md`: retry, next, recap, time-out, healthy/fried routes, and the bazaar with all four stands and a drink.

**Checkpoint**: All three levels and the recap are playable; each level retains its latest result.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Feedback, comfort, performance, and final validation.

- [X] T062 [P] Add audio clips to `Assets/Audio/SFX/` and wire `SfxPlayer` events for hover, pick, place, drop, serve, bar milestone, and report reveal.
- [X] T063 [P] Add gamepad rumble via `GamepadRumble` on pick (0.4/0.2 s), place (0.3/0.1 s), and serve (0.5/0.15 s); skip silently when unsupported.
- [X] T064 Add the plate slot highlight, bar easing, and report reveal animation in `PlateSlot`, `NutritionBar`, and `HealthReportPresenter`.
- [X] T065 Add the pause panel, gamepad disconnect handling, and keyboard/mouse fallback notice in `Assets/Prefabs/UI/Pause.prefab` and `GamepadInput`.
- [ ] T066 Optimize the heavy FBX assets in `Assets/Models/Food/` (rendang, fried chicken, burger): decimate to ≤5k triangles, add LODs above 3k triangles, cap textures at 1024 px, and keep each level ≤150k triangles and ≤100 draw calls.
- [X] T067 Run `NutriPick/Validate Food Data`, `NutriPick/Validate Levels`, and the full EditMode test suite; fix all errors and failures after all prefabs and scenes exist.
- [ ] T068 Run V1–V12 in `quickstart.md` with the Rexus gamepad, including pause/disconnect and the keyboard/mouse fallback.
- [X] T069 Build the Windows demo to `Builds/Windows/` and verify it runs outside the editor with the gamepad.
- [X] T070 Update `README.md` with the Unity version, gamepad controls, build steps, and the team asset handoff list; record any remaining placeholders.
- [ ] T071 Run the five-user test and record the SC-001–SC-013 results in `specs/001-nutripick-vr-game/quickstart.md` or a linked results note.

**Checkpoint**: The demo is stable, readable, and playable with the Rexus gamepad; all quickstart scenarios pass or have documented exceptions.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies; start immediately.
- **Phase 2 (Foundational)**: Depends on Phase 1; blocks all user stories.
- **Phase 3 (US1)**: Depends on Phase 2; MVP.
- **Phase 4 (US2)**: Depends on Phase 2; its UI can be built in parallel with US1, but final integration needs US1's `PlateController` and `LevelFlowController`.
- **Phase 5 (US3)**: Depends on Phase 2; its UI can be built in parallel with US1, but final integration needs US1's `ReticleInteractor`.
- **Phase 6 (US4)**: Depends on US1 (level flow, plate, serve), US2 (bar), and US3 (info card) for the full experience; the report/recap UI can be built in parallel.
- **Phase 7 (Polish)**: Depends on all desired user stories.

### User Story Dependencies

- **US1 (P1)**: No dependency on US2–US4. MVP.
- **US2 (P2)**: Independent bar component; integrates with US1's plate events.
- **US3 (P2)**: Independent info card; integrates with US1's reticle events.
- **US4 (P3)**: Integrates US1–US3 into the three-level flow and recap.

### Within Each User Story

- Data/scripts before prefabs and scenes.
- Create `FoodItem`, `PlateSlot`, and `FoodStand` before `ReticleInteractor`; use UnityEvents/interfaces for bar/report so US1 compiles without US2/US4.
- Prefabs before scene wiring.
- Scene wiring before validation.
- Run the relevant quickstart scenario before moving to the next priority.

### Parallel Opportunities

- Setup: T004–T008 are independent files.
- Foundational: T016–T021 are independent assets/tests after the scripts exist.
- US1: T022, T026, T028/T029, T031-T033, and T035 are parallelizable file sets; T023/T025 have ordering dependencies (T023 after T025/T026; T025 after T028).
- US2/US3: the bar and info-card UI can be built in parallel after US1's interfaces exist.
- US4: Level 2 and Level 3 scenes can be built in parallel; report and recap prefabs can be built in parallel.
- Polish: T062/T063 (audio/rumble) and T064/T065 (visual/pause) can run in parallel.

---

## Parallel Example: User Story 1

```text
Task: "Create Assets/Scripts/Player/FirstPersonController.cs and Assets/Scripts/Player/GamepadInput.cs"
Task: "Create Assets/Scripts/Interaction/PlateSlot.cs"
Task: "Create Assets/Scripts/Content/FoodStand.cs"
Task: "Create Assets/Scripts/Interaction/ServeStation.cs"

# Ordering dependencies:
# Create FoodItem (T025) after FoodStand (T028).
# Create ReticleInteractor (T023) after FoodItem (T025) and PlateSlot (T026).
```

## Parallel Example: User Story 4

```text
Task: "Build Assets/Scenes/Level02_Padang.unity"
Task: "Build Assets/Scenes/Level03_Bazaar.unity"
Task: "Create Assets/Prefabs/UI/HealthReport.prefab and Assets/Prefabs/UI/Recap.prefab"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational.
3. Complete Phase 3: US1.
4. **STOP and VALIDATE**: Run V1–V4 in `quickstart.md` with the Rexus gamepad.
5. Demo the Level 1 core loop with a placeholder report.

### Incremental Delivery

1. Setup + Foundational → foundation ready.
2. US1 → Level 1 core loop → validate V1–V4.
3. US2 → Health Balance Bar → validate V5.
4. US3 → Food information card → validate V6.
5. US4 → Levels 2/3, bazaar, report, recap → validate V7–V11.
6. Polish → audio, rumble, pause, performance, build, five-user test.

### Parallel Team Strategy

- One developer owns the player/interaction core (US1).
- One developer owns the nutrition data, evaluator, and bar (Phase 2 + US2).
- One developer owns the UI/info card/report (US3 + US4 UI).
- One developer owns the environments and content (Level 1/2/3 scenes, stand layout).
- The asset team delivers the 5 missing foods and bazaar modules; integration is T057.

---

## Notes

- [P] tasks touch different files and have no dependency on an incomplete task.
- [US1]–[US4] map to the four user stories in `spec.md`.
- Every task includes a concrete path under `Assets/` or `specs/`.
- Placeholder art is acceptable for M2/M3; do not block on the asset team.
- Commit after each task or logical group, including `.meta` files.
- Stop at each checkpoint to validate the story independently.

## Implementation Notes (2026-11-16)

- Implemented the runtime scripts, editor builder, 14 `FoodData` assets, 14 food prefabs, HUD, plate, stands, serve station, and the three level scenes. **Level 1 is the existing `MainVR` tabletop scene**; `ceramic_plate (3)` is now the serving `Plate`, with a table-surface collider and 8 interactive foods.
- **EditMode tests**: 22/22 passed.
- **PlayMode smoke tests**: 4/4 passed (Level 1 spawn/wiring, plate-and-serve loop, Level 3 four-stand/drink setup, camera preview).
- **Content validation**: `NutriPick/Validate All` and `NutriPick/Validate Scenes` passed.
- **Windows build**: `Builds/Windows/NutriPick.exe` built successfully (about 147 MB). Build Settings order is `MainVR`, `Level02_Padang`, `Level03_Bazaar`.
- **Input**: `GamepadInput` uses direct Input System polling; `Assets/Input/NutriPick.inputactions` is a documentation/remapping asset and is not required at runtime.
- **Team assets**: Level 1 and Level 2 now use only the available FBX food models in `Assets/Models/Food`. `nasi_putih`, `kerupuk`, and `sayur_singkong` are reserved for future content; `air_putih` and `teh_manis` remain Level 3 drink placeholders. Bazaar stand/canopy pieces are still placeholders; integration remains T057.
- **Completion**: 63 of 71 tasks complete.
- **Remaining**: manual V1–V12 validation with the Rexus gamepad (T038, T044, T049, T061, T068), team asset integration (T057), heavy FBX optimization (T066), and the five-user test (T071).
