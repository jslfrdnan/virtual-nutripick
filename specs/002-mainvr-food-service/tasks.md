# Tasks: Scene 1 MainVR — Food Plating, Figma-Style Food Cards, and Final Result

**Input**: Design documents from `/specs/002-mainvr-food-service/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Tests**: EditMode and PlayMode tasks are included because `plan.md` defines automated coverage for the data, card grid, plate/serve flow, Status Makanan, and Final Result. There is no TDD requirement; manual validation remains in `quickstart.md`.

**Organization**: Tasks are grouped by user story so each story can be implemented and validated as an independent increment.

> **Update 2026-11-17**: The Start screen was removed by user request. T013–T016 are retained as completed historical tasks; Scene 1 now starts immediately and `StartScreenPresenter` is legacy optional.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: `[US1]`, `[US2]`, `[US3]` for user story phases only
- Every task includes an exact file path

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm the Unity baseline and prepare shared UI assets.

- [X] T001 Verify Unity **6000.6.0f1**, open `Assets/Scenes/MainVR.unity`, and record the baseline EditMode/PlayMode results from `Assets/Tests/EditMode` and `Assets/Tests/PlayMode`; do not open with 6000.2.6f2.
- [X] T002 [P] Create the placeholder food sprite `Assets/Textures/FoodIcons/PlaceholderFood.png` and import it as a Sprite for cards with no icon.
- [X] T003 [P] Measure the imported bounds of `Assets/Models/Props/ceramic_plate.fbx` and record the scale/pivot decision in `specs/002-mainvr-food-service/research.md`.

**Checkpoint**: Unity baseline is known, and the plate/icon assets are ready to use.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared data, scoring, icons, and tests that every user story depends on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T004 [P] Add `ProteinType` (`Hewani`, `Nabati`) and `bool hasProteinType` to `Assets/Scripts/Nutrition/FoodData.cs`; validate that Scene 1 foods have `hasProteinType = true` and a valid type.
- [X] T005 Update the eight Scene 1 assets in `Assets/ScriptableObjects/Food/` — `FD_rendang.asset`, `FD_burger.asset`, `FD_telur_rebus.asset`, `FD_telur_ceplok.asset`, `FD_tempe.asset`, `FD_brokoli.asset`, `FD_pisang.asset`, `FD_mie_goreng.asset` — with `hasProteinType = true` and the values: Rendang/Burger/Boiled egg/Fried egg = `Hewani`; Tempeh/Broccoli/Banana/Fried noodles = `Nabati`.
- [X] T006 [P] Add `positivePoints[3]`, `improvementPoints[3]`, `statusBadge`, `statusSummary`, and `isBalanced` to `Assets/Scripts/Nutrition/MealResult.cs`; keep existing fields for Levels 2/3 compatibility.
- [X] T007 Extend `Assets/Scripts/Nutrition/MealEvaluator.cs` to populate exactly three positive and three improvement points, with supportive fallbacks, plus the `LEBIH SEHAT` / `PERLU DIUBAH` badge and balanced flag.
- [X] T008 [P] Create `Assets/Editor/FoodIconGenerator.cs` to render each food prefab to a 256×256 transparent PNG and assign the resulting Sprite to `FoodData.icon`; skip foods that already have a suitable icon.
- [X] T009 Run the food icon generator and save icons under `Assets/Textures/FoodIcons/`; assign them to the eight `Assets/ScriptableObjects/Food/FD_*.asset` files, using `PlaceholderFood.png` where generation is not possible.
- [X] T010 [P] Add EditMode tests in `Assets/Tests/EditMode/Scene1DataTests.cs` for protein type validation and exactly three positive/improvement points.
- [X] T011 Verify `Assets/ScriptableObjects/Levels/LD_MainVR.asset` has the eight Scene 1 foods, `minItems = 3`, `requiresDrink = false`, and `sceneName = MainVR`; update only if the values drift.

**Checkpoint**: Data, icons, scoring content, and tests are ready; user stories can begin.

---

## Phase 3: User Story 1 - Start, plate, and serve food in MainVR (Priority: P1) 🎯 MVP

**Goal**: The player can press Start, enter Scene 1, pick up three foods, place them on the ceramic plate, and serve; the plate locks and the existing report/fallback appears.

**Independent Test**: Open `Assets/Scenes/MainVR.unity`, press Start, pick up three foods, place them on `ServingPlate`, press Serve, and confirm the plate locks and the result/fallback appears.

### Implementation for User Story 1

- [X] T012 [P] [US1] Create `Assets/Prefabs/Interaction/ServingPlate.prefab` using `Assets/Models/Props/ceramic_plate.fbx`, normalized to the existing ~0.26 m plate diameter, with four `PlateSlot` children, snap points, highlights, and `PlateController`.
- [X] T013 [P] [US1] Create `Assets/Scripts/UI/StartScreenPresenter.cs` with `Show(Action onStart, Action onProfile)` and `Hide()`, following `specs/002-mainvr-food-service/contracts/ui-contracts.md`.
- [X] T014 [US1] Create the StartScreen root inside `Assets/Prefabs/UI/FigmaHUD.prefab` with Start and Profil Dev buttons, wire `StartScreenPresenter`, and keep Profil Dev optional/simple.
- [X] T015 [US1] Extend `Assets/Scripts/Flow/LevelFlowController.cs` with optional `startScreen`, `StartLevel()`, and wait-for-start behavior; if `startScreen` is null, keep the current immediate-start behavior.
- [X] T016 [US1] Create `Assets/Editor/FigmaHudBuilder.cs` to generate `Assets/Prefabs/UI/FigmaHUD.prefab` with a Screen Space Overlay Canvas (1920×1080, Match 0.5), one EventSystem, PausePanel, and the StartScreen root.
- [X] T017 [US1] Create `Assets/Editor/Scene1FigmaBuilder.cs` to replace the `Plate` visual in `Assets/Scenes/MainVR.unity` with `ServingPlate.prefab` and add `FigmaHUD.prefab`.
- [X] T018 [US1] Update `Assets/Scenes/MainVR.unity` with `ServingPlate`, `FigmaHUD`, and `LevelFlow` start-screen references; preserve the environment, decorative props, player rig, and existing food row.
- [X] T019 [US1] Add PlayMode tests in `Assets/Tests/PlayMode/Scene1FlowTests.cs` for start screen -> Start; all eight foods can be picked, placed, removed, and served; non-food props are non-interactable; food remains stable for 10 seconds; slot highlight works; plated food can be picked again; drop/cancel leaves the plate unchanged; dropped food returns to its start; serve is blocked with fewer than three foods and allowed with three or four; the refusal message is Indonesian; plate locks on serve; and the existing report/fallback appears.
- [X] T020 [US1] Run EditMode/PlayMode plus the existing Level 2/3 smoke tests in `Assets/Tests/PlayMode/LevelSmokeTests.cs`; fix regressions before moving on.

**Checkpoint**: User Story 1 is playable and independently testable with the old report as the result fallback.

---

## Phase 4: User Story 2 - Read food information in the Figma-style card grid (Priority: P2)

**Goal**: The player sees eight UI Canvas food cards with image, name, protein type, calories, protein, carbohydrates, and fat; targeting a food highlights/focuses its card; the Status Makanan bar updates with the plate.

**Independent Test**: Enter Scene 1, verify all eight cards, target each food and read its values, add/remove food, and confirm the Status Makanan bar changes.

### Implementation for User Story 2

- [X] T021 [P] [US2] Create `Assets/Scripts/UI/FoodCardModel.cs` to map `FoodData` to display fields, using `—` for missing protein type/optional values and `NutritionFormat` for numbers; add `Assets/Tests/EditMode/Scene1CardModelTests.cs` for formatting and placeholders.
- [X] T022 [US2] Create `Assets/Scripts/UI/FoodCardView.cs` with icon, name, protein type, calories, protein, carbohydrates, fat, health accent, focus outline, and placeholder icon.
- [X] T023 [US2] Create `Assets/Scripts/UI/FoodCardGridPresenter.cs` with `Bind`, `Highlight`, `Focus`, `ClearFocus`, and `ClearHighlight` per `specs/002-mainvr-food-service/contracts/ui-contracts.md`.
- [X] T024 [P] [US2] Create `Assets/Scripts/UI/MealStatusBarPresenter.cs` with neutral, preview, and final states, a marker, and a label that always accompanies the color.
- [X] T025 [US2] Extend `Assets/Editor/FigmaHudBuilder.cs` to generate `Assets/Prefabs/UI/FoodCard.prefab`, `Assets/Prefabs/UI/FoodCardGrid.prefab`, and `Assets/Prefabs/UI/StatusMakanan.prefab`, then compose them into `Assets/Prefabs/UI/FigmaHUD.prefab`.
- [X] T026 [US2] Add optional `foodCards` and `mealStatus` references to `Assets/Scripts/Flow/LevelFlowController.cs`; bind the grid from `level.foodPool` in `StartLevel`, update the status in `OnPlateChanged`, and fall back to `FoodInfoCardPresenter`/`NutritionBar` when null.
- [X] T027 [US2] Add optional `foodCards` to `Assets/Scripts/Player/ReticleInteractor.cs`; highlight on hover, focus on pickup/Inspect, clear on look-away/drop/place, and fall back to the old `infoCard` behavior when null.
- [X] T028 [US2] Wire `FigmaHUD/GameplayHUD/FoodCardGrid`, `StatusMakanan`, and `ReticleInteractor.foodCards` in `Assets/Editor/Scene1FigmaBuilder.cs` and `Assets/Scenes/MainVR.unity`.
- [X] T029 [US2] Add PlayMode tests in `Assets/Tests/PlayMode/Scene1UiTests.cs` for eight cards, correct values, highlight/focus, placeholder behavior, and Status Makanan updates.
- [X] T030 [P] [US2] Verify the eight `Assets/ScriptableObjects/Food/FD_*.asset` files have icons and protein types; fix any `—` values or missing sprites before visual QA.
- [X] T031 [US2] Run the existing Level 2/3 smoke tests in `Assets/Tests/PlayMode/LevelSmokeTests.cs` and confirm the old `Assets/Prefabs/UI/HUD.prefab` fallback still works when the new presenters are not assigned.

**Checkpoint**: User Story 2 is independently readable and testable; the old HUD still works for Levels 2/3.

---

## Phase 5: User Story 3 - Figma-style Final Result and reuse in later scenes (Priority: P3)

**Goal**: After serving, the player sees a two-panel Final Result with three feedback points per panel, status badges, and Retry/Next; the setup can be replicated in Scenes 2 and 3.

**Independent Test**: Serve a valid meal, verify both panels and three points each, use Retry and Next, then copy the setup into a temporary scene with a different food pool and complete a pickup/plate/inspect/serve flow.

### Implementation for User Story 3

- [X] T032 [P] [US3] Create `Assets/Scripts/UI/FinalResultPresenter.cs` with two panels, three points per panel, status badges, Retry, and Next, following `specs/002-mainvr-food-service/contracts/ui-contracts.md`.
- [X] T033 [US3] Extend `Assets/Editor/FigmaHudBuilder.cs` to generate `Assets/Prefabs/UI/FinalResult.prefab` and compose it into `Assets/Prefabs/UI/FigmaHUD.prefab`.
- [X] T034 [US3] Add optional `finalResult` to `Assets/Scripts/Flow/LevelFlowController.cs`; use it in the serve flow, and fall back to `HealthReportPresenter` when null.
- [X] T035 [US3] Wire `FigmaHUD/FinalResult` in `Assets/Editor/Scene1FigmaBuilder.cs` and `Assets/Scenes/MainVR.unity`.
- [X] T036 [US3] Add PlayMode tests in `Assets/Tests/PlayMode/Scene1FinalResultTests.cs` for both panels, exactly three points per panel, Retry, and Next.
- [X] T037 [US3] Update `specs/002-mainvr-food-service/contracts/scene-flow.md` and `specs/002-mainvr-food-service/quickstart.md` with the Final Result and replication steps.
- [X] T038 [US3] Run the replication smoke test in a temporary `Assets/Scenes/Scene1_ReplicationTest.unity`: swap the food pool and plate position, complete pickup/plate/inspect/serve, then delete the temporary scene.
- [X] T039 [US3] Run load/build smoke tests for `Assets/Scenes/Level02_Padang.unity` and `Assets/Scenes/Level03_Bazaar.unity` via `Assets/Tests/PlayMode/LevelSmokeTests.cs`, and confirm the new prefabs do not break their existing HUD, plate, or report references.

**Checkpoint**: User Story 3 is complete; Scene 1 is reusable for Scenes 2 and 3.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Validation, documentation, build, and cleanup.

- [X] T040 [P] Update `Assets/Editor/NutriPickSceneValidator.cs` to validate `FigmaHUD` references, duplicate EventSystem, `ServingPlate` slots, card grid count, and Final Result references.
- [X] T041 [P] Update `README.md` and `specs/002-mainvr-food-service/quickstart.md` with Scene 1 controls, UI flow, and validation steps.
- [ ] T042 Perform visual QA in `Assets/Scenes/MainVR.unity` and `Assets/Prefabs/UI/FigmaHUD.prefab` at 1920×1080 with the Rexus gamepad and keyboard/mouse fallback; fix card grid readability, Status Makanan labels, and Final Result layout.
- [X] T043 Run the full `Assets/Tests/EditMode` and `Assets/Tests/PlayMode` suites plus `NutriPick/Validate All` and `NutriPick/Validate Scenes` from `Assets/Editor/NutriPickSceneValidator.cs`.
- [ ] T044 Run `NutriPick/Build Windows`, launch `Builds/Windows/NutriPick.exe`, and repeat quickstart V1–V5.
- [X] T045 Clean up temporary scenes/assets/logs under `Assets/Scenes/MainVR.unity`, remove debug objects, and confirm `ProjectSettings/EditorBuildSettings.asset` order: `MainVR`, `Level02_Padang`, `Level03_Bazaar`.
- [X] T046 Run the final `specs/002-mainvr-food-service/quickstart.md` validation and commit the feature task list.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; start immediately.
- **Foundational (Phase 2)**: Depends on Setup; blocks all user stories.
- **User Story 1 (Phase 3)**: Depends on Foundational; creates the FigmaHUD shell and ServingPlate.
- **User Story 2 (Phase 4)**: Depends on Foundational and US1's FigmaHUD shell/LevelFlow start flow.
- **User Story 3 (Phase 5)**: Depends on Foundational, US1, and US2.
- **Polish (Phase 6)**: Depends on all desired user stories.

### User Story Dependencies

- **US1 (P1)**: Independent MVP after Foundational. Uses the existing report as the result fallback.
- **US2 (P2)**: Depends on US1's `FigmaHUD` shell and `LevelFlowController.StartLevel`; independently testable once cards/status are wired.
- **US3 (P3)**: Depends on US1/US2; independently testable once Final Result and Retry/Next are wired.

### Within Each Story

- Data/model tasks before presenters.
- Presenters before prefab composition.
- Prefab composition before scene wiring.
- Scene wiring before PlayMode tests.
- Existing Level 2/3 smoke tests after each integration checkpoint.

### Parallel Opportunities

- Setup: T002 and T003.
- Foundational: T004, T006, and T008.
- US1: T012 and T013.
- US2: T021, T024, and T030.
- US3: T032 is independent; the remaining tasks are sequential.
- Polish: T040 and T041.

---

## Parallel Example: User Story 1

```text
Task: "Create Assets/Prefabs/Interaction/ServingPlate.prefab using ceramic_plate.fbx"
Task: "Create Assets/Scripts/UI/StartScreenPresenter.cs"
```

## Parallel Example: User Story 2

```text
Task: "Create Assets/Scripts/UI/FoodCardModel.cs"
Task: "Create Assets/Scripts/UI/MealStatusBarPresenter.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3: Start screen + ServingPlate + plate/serve flow.
3. Stop and validate Scene 1 with three foods and the existing report fallback.
4. Demo the MVP before adding the full card grid.

### Incremental Delivery

1. Foundation → data, icons, tests.
2. US1 → start/plate/serve MVP.
3. US2 → card grid + Status Makanan.
4. US3 → Final Result + reuse.
5. Polish → validator, docs, visual QA, build.

### Parallel Team Strategy

- One developer can own data/scoring (Phase 2).
- One developer can own UI prefabs/presenters (US1/US2).
- One developer can own Final Result/replication (US3).
- Merge only after the shared `FigmaHUD.prefab` and `LevelFlowController` changes are stable.

---

## Notes

- `[P]` tasks touch different files and have no dependency on an incomplete task.
- `[US1]`/`[US2]`/`[US3]` labels map to the three user stories in `spec.md`.
- Do not delete `HUD.prefab`, `Plate.prefab`, `HealthReportPresenter`, `FoodInfoCardPresenter`, or `NutritionBar`; they are the Level 2/3 fallback.
- The Figma board is a visual direction; exact text/spacing/fonts may be approximated until a higher-resolution export is available.
- Keep the existing working tree changes intact; commit only the Scene 1 feature files.
