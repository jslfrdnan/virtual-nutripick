# Implementation Plan: Scene 1 MainVR — Food Plating, Figma-Style Food Cards, and Final Result

**Branch**: `002-mainvr-food-service` | **Date**: 2026-11-16 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/002-mainvr-food-service/spec.md`

> **Update 2026-11-17**: The Start screen was removed by user request. Scene 1 now starts immediately; `StartScreenPresenter` remains as optional legacy support only.

## Summary

Rebuild Scene 1 (`MainVR`) as the reference NutriPick level: a start screen, a ceramic serving plate using `Assets/Models/Props/ceramic_plate.fbx`, eight existing food prefabs, a UI Canvas food card grid, a Status Makanan bar, and a two-panel Final Result screen. The interaction, plate, food cards, status bar, and result screen must be reusable in Scenes 2 and 3 through prefabs and ScriptableObject configuration, without scene-specific code.

The implementation reuses the existing `FoodData`, `LevelDefinition`, `MealEvaluator`, `PlateController`, `PlateSlot`, `FoodItem`, `ReticleInteractor`, and `LevelFlowController` architecture. It adds Figma-oriented UI presenters and a new shared plate prefab, while keeping the old HUD and plate prefabs available for Levels 2 and 3 until they are migrated.

## Technical Context

**Language/Version**: C# 9 / Unity **6000.6.0f1**

**Primary Dependencies**:
- Unity Universal Render Pipeline `17.6.0`
- Unity Input System `1.20.0`
- Unity UI / TextMeshPro (`com.unity.ugui 2.6.0`)
- Unity Test Framework `1.8.0`
- Existing NutriPick runtime scripts and ScriptableObjects

**Storage**: Unity assets only — ScriptableObjects (`FoodData`, `LevelDefinition`, `NutritionRules`), prefabs, scenes, and generated icon textures. No database, save file, account, or network storage.

**Testing**: Unity Test Framework EditMode and PlayMode; existing `Assets/Tests/EditMode` and `Assets/Tests/PlayMode` suites plus new Scene 1 tests.

**Target Platform**: Windows 64-bit at 1920×1080, played with a Rexus gamepad; keyboard and mouse fallback. No VR headset or motion controller is required for this feature.

**Project Type**: Single Unity desktop game project.

**Performance Goals**:
- 60 fps at 1920×1080 on the target Windows machine.
- Food card grid binds eight cards without per-frame allocations.
- Status Makanan updates within 0.5 seconds of a plate change.
- Final Result appears within the existing one-second serve delay.

**Constraints**:
- UI information is presented through **UI Canvas** and TextMeshPro.
- The serving plate must use `Assets/Models/Props/ceramic_plate.fbx`.
- No new packages or 3D food models.
- Existing Levels 2 and 3, shared assets, and build settings must not break.
- The Figma board is readable only as a public thumbnail; exact text, spacing, and fonts may be approximated until a higher-resolution export is provided.
- Scene 1 uses 3 minimum foods and 4 plate positions.
- Protein type is limited to `Hewani` and `Nabati`.

**Scale/Scope**:
- 1 scene (`MainVR`), 8 foods, 4 plate positions.
- 1 new plate prefab, 1 new HUD prefab, 1 food card prefab.
- 6 new UI scripts/views, 3 modified data scripts, 2 modified flow/interaction scripts.
- 3 screens/states: Start, Gameplay, Final Result.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

The project constitution at `.specify/memory/constitution.md` is still the unfilled Spec Kit template; it contains no ratified project-specific principles or governance rules. The plan therefore uses the following project conventions as gates:

| Gate | Status | Evidence |
|------|--------|----------|
| Reuse existing architecture; do not duplicate nutrition or interaction logic | PASS | Reuses `MealEvaluator`, `PlateController`, `FoodItem`, `ReticleInteractor`, `LevelFlowController` |
| Preserve Levels 2 and 3, existing tests, and build settings | PASS | New prefabs and optional presenter references; old HUD/plate remain |
| Use existing packages and Unity 6000.6.0f1 | PASS | No new dependencies |
| Single source of truth for food and nutrition data | PASS | Cards bind to `FoodData`; status and result use `MealResult` |
| Scene 1 is the only content scope; later scenes reuse the setup | PASS | Shared prefabs + `LevelDefinition` configuration |
| UI Canvas and TextMeshPro for information | PASS | `FigmaHUD.prefab` is a UI Canvas |
| No manual-only blocker for UI | PASS | Editor builder is primary; manual fallback documented |

**Post-Phase 1 re-check**: PASS. The design keeps old prefabs and presenters as fallbacks, adds no packages, and isolates Scene 1 changes. No complexity violations require justification.

## Project Structure

### Documentation (this feature)

```text
specs/002-mainvr-food-service/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── contracts/
    ├── ui-contracts.md
    └── scene-flow.md
```

### Source Code (repository root)

```text
Assets/
├── Scripts/
│   ├── Nutrition/
│   │   ├── FoodData.cs                     # add ProteinType + hasProteinType
│   │   ├── MealResult.cs                   # add 3 positive + 3 improvement points
│   │   └── MealEvaluator.cs                # build Figma Final Result content
│   ├── UI/
│   │   ├── FoodCardModel.cs                # new: card view model
│   │   ├── FoodCardView.cs                 # new: one food card
│   │   ├── FoodCardGridPresenter.cs        # new: bind/highlight/focus 8 cards
│   │   ├── MealStatusBarPresenter.cs       # new: Figma Status Makanan bar
│   │   ├── StartScreenPresenter.cs         # new: Start / Profil Dev
│   │   ├── FinalResultPresenter.cs         # new: two-panel result
│   │   ├── FoodInfoCardPresenter.cs        # kept for Levels 2/3 fallback
│   │   ├── NutritionBar.cs                 # kept for Levels 2/3 fallback
│   │   └── HealthReportPresenter.cs        # kept for Levels 2/3 fallback
│   ├── Flow/
│   │   └── LevelFlowController.cs          # start screen + new UI references
│   └── Player/
│       └── ReticleInteractor.cs            # optional card highlight/focus
├── Editor/
│   ├── FoodIconGenerator.cs                # new: render prefab icons
│   ├── FigmaHudBuilder.cs                  # new: generate FoodCard + FigmaHUD
│   └── Scene1FigmaBuilder.cs               # new: wire MainVR Scene 1
├── Prefabs/
│   ├── UI/
│   │   ├── FoodCard.prefab                 # new
│   │   ├── FoodCardGrid.prefab             # new
│   │   ├── StartScreen.prefab              # new (nested in FigmaHUD)
│   │   ├── StatusMakanan.prefab            # new (nested in FigmaHUD)
│   │   ├── FinalResult.prefab              # new (nested in FigmaHUD)
│   │   └── FigmaHUD.prefab                 # new (composes the four UI prefabs)
│   ├── Interaction/
│   │   ├── ServingPlate.prefab             # new: ceramic_plate.fbx + 4 slots
│   │   ├── Plate.prefab                    # kept for Levels 2/3
│   │   └── ServeStation.prefab             # reused
│   ├── Food/                               # existing 8+ prefabs
│   └── Environment/                        # existing
├── Scenes/
│   ├── MainVR.unity                        # Scene 1 wiring
│   ├── Level02_Padang.unity                # unchanged
│   └── Level03_Bazaar.unity                # unchanged
├── ScriptableObjects/
│   ├── Food/FD_*.asset                     # add proteinType/icon
│   └── Levels/LD_MainVR.asset              # confirm 8-food pool, min 3
└── Tests/
    ├── EditMode/                           # data/UI model tests
    └── PlayMode/                           # Scene 1 flow tests
```

**Structure Decision**: Single Unity project. All new runtime code stays under `Assets/Scripts`; all editor-only generation stays under `Assets/Editor`; reusable UI and interaction objects live under `Assets/Prefabs`. Scene-specific content stays in `Assets/Scenes/MainVR.unity` and `LD_MainVR.asset`.

## Design Decisions

Detailed rationale and alternatives are in [research.md](./research.md). The key decisions are:

1. **New `ServingPlate.prefab`** using `ceramic_plate.fbx`; keep `Plate.prefab` for Levels 2/3 until migration.
2. **New `FigmaHUD.prefab`** composed of nested `StartScreen`, `FoodCardGrid`, `StatusMakanan`, and `FinalResult` prefabs; keep `HUD.prefab` for Levels 2/3.
3. **UI builder first, manual fallback documented** for the UI Canvas; the builder produces reproducible hierarchy, anchors, colors, and references.
4. **`FoodData.proteinType` + `hasProteinType`**; only `Hewani`/`Nabati` are displayed, with `—` for no meaningful protein.
5. **`FoodData.icon` + `FoodIconGenerator`**; existing textures where suitable, generated prefab renders otherwise, placeholder as fallback.
6. **Reuse `MealEvaluator`**; extend `MealResult` with exactly three positive and three improvement points plus status badge/summary.
7. **Optional presenter references in `LevelFlowController` and `ReticleInteractor`**; old behavior remains when the new references are null.
8. **Start screen in the existing scene flow**; no separate bootstrap scene.
9. **Reuse for Scenes 2/3 through prefabs + `LevelDefinition` configuration**, not scene-specific code.

## Data Model and Contracts

- Data model: [data-model.md](./data-model.md)
- UI contracts: [contracts/ui-contracts.md](./contracts/ui-contracts.md)
- Scene flow and replication contract: [contracts/scene-flow.md](./contracts/scene-flow.md)
- Validation guide: [quickstart.md](./quickstart.md)

## Implementation Phases

### Phase A — Data and asset foundation

1. Add `ProteinType` (`Hewani`, `Nabati`) and `hasProteinType` to `FoodData`; update the eight Scene 1 assets.
2. Extend `MealResult` with `positivePoints[3]`, `improvementPoints[3]`, `statusBadge`, `statusSummary`, and `isBalanced`.
3. Extend `MealEvaluator` to produce exactly three points per panel with supportive fallbacks.
4. Add `FoodIconGenerator`; generate/assign icons for the eight foods; keep a placeholder sprite fallback.
5. Add EditMode tests for protein type, point arrays, and card model formatting.

### Phase B — UI presenters and prefabs

1. Implement `FoodCardView` and `FoodCardGridPresenter`.
2. Implement `MealStatusBarPresenter` with neutral/preview/final states.
3. Implement `StartScreenPresenter` and `FinalResultPresenter`.
4. Implement `FigmaHudBuilder` to generate `FoodCard.prefab` and `FigmaHUD.prefab` with exact hierarchy, anchors, colors, and references.
5. Document the manual UI Canvas fallback in `quickstart.md`.
6. Add UI model/PlayMode tests where practical.

### Phase C — Serving plate and Scene 1 wiring

1. Create `ServingPlate.prefab` using `ceramic_plate.fbx`; normalize scale/pivot; add four slots and `PlateController`.
2. Add `Scene1FigmaBuilder` to wire `MainVR`: replace the old plate visual with `ServingPlate`, add `FigmaHUD`, wire `LevelFlow`, `TableFoodStand`, `ServeStation`, and `TableSurfaceCollider`.
3. Keep the environment, decorative props, player rig, and existing food prefabs.
4. Confirm `LD_MainVR.asset` has the eight foods, `minItems = 3`, and `requiresDrink = false`.
5. Preserve Build Settings order: `MainVR`, `Level02_Padang`, `Level03_Bazaar`.

### Phase D — Flow integration

1. Add optional `startScreen`, `foodCards`, `mealStatus`, and `finalResult` references to `LevelFlowController`.
2. Add `StartLevel()` and defer timer/player input until Start.
3. Update `OnPlateChanged()` to use `mealStatus` when assigned.
4. Update `ServeInternal()` to show `finalResult` when assigned.
5. Add optional `foodCards` to `ReticleInteractor`; highlight/focus/clear cards while preserving the old `FoodInfoCardPresenter` fallback.
6. Ensure pause, disconnect, retry, next, and recap behavior remain intact.

### Phase E — Tests and validation

1. EditMode:
   - `FoodData` protein type/hasProteinType validation.
   - `MealResult` exactly three positive and three improvement points.
   - `FoodCardModel` formatting and placeholder behavior.
   - Existing `LevelDefinition` tests still pass.
2. PlayMode:
   - Start screen -> Start -> gameplay.
   - Eight cards bind with correct values.
   - Plate accepts three foods, refuses a fifth, locks on serve.
   - Status Makanan updates on plate changes.
   - Final Result shows both panels and Retry/Next.
   - Existing Level 2/3 smoke tests still pass.
3. Manual:
   - V1–V7 from `quickstart.md`.
   - 1920×1080 visual review.
   - Rexus gamepad and keyboard/mouse fallback.
4. Build Windows and repeat the core flow.

### Phase F — Replication guide

1. Validate the replication checklist in `contracts/scene-flow.md`.
2. Duplicate `MainVR` to a temporary scene, swap the food pool, and confirm the setup works without code changes.
3. Document any manual UI Canvas steps that the builder cannot cover.
4. Leave Scenes 2 and 3 untouched but smoke-test them.

## Risks and Mitigations

| Risk | Mitigation |
|------|------------|
| Figma thumbnail only; exact spacing/fonts unknown | Use the documented design tokens and hierarchy; keep approximation explicit; update when a higher-resolution export arrives |
| `FoodData.icon` is null for all foods | `FoodIconGenerator` renders prefab thumbnails; placeholder sprite fallback |
| `ceramic_plate.fbx` scale/pivot differs from the primitive plate | Normalize to a ~0.26 m diameter, validate slot alignment, and test placement |
| Existing working tree has many unrelated changes | Touch only feature files; do not overwrite user assets; commit only Scene 1 artifacts |
| UI Canvas assembly is error-prone | Editor builder is primary; manual guide is the fallback |
| Levels 2/3 regress | New prefabs and optional presenter references; keep old HUD/plate/report; run existing smoke tests |
| `MealResult` point generation produces fewer than three points | Deterministic fallback messages; EditMode tests assert exactly three |
| UI grid rebuild causes allocations | Build once per level; reuse card views; eight cards only |
| Duplicate EventSystem | `FigmaHUD` owns one; scene validator checks for duplicates |
| Font/asset mismatch with Figma | Use project-consistent TMP font and colors; exact assets are a later polish item |

## Complexity Tracking

No constitution violations or unjustified complexity. The design adds presenters and prefabs rather than duplicating interaction or nutrition logic.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | — | — |

## Next Steps

1. Run `/speckit.tasks` to decompose the implementation phases into actionable tasks.
2. Implement Phase A data changes and tests first.
3. Build the UI prefabs and Scene 1 wiring only after the data model is stable.
4. Validate Scene 1 and confirm Levels 2/3 still pass before migrating them to the new HUD.
