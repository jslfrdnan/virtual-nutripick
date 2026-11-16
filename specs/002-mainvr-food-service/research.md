# Research: Scene 1 MainVR — Food Plating, Figma-Style Food Cards, and Final Result

**Feature**: `002-mainvr-food-service`
**Date**: 2026-11-16
**Spec**: `specs/002-mainvr-food-service/spec.md`

## 1. Serving plate model integration

**Decision**: Create a new shared `ServingPlate.prefab` using `Assets/Models/Props/ceramic_plate.fbx` as the visual child, with four `PlateSlot` children and `PlateController`. Use it in `MainVR` and leave the existing `Plate.prefab` untouched for Levels 2 and 3 until they are migrated.

**Rationale**: The current `Plate.prefab` uses a primitive cylinder (`PlateVisual` scaled `0.26 × 0.01 × 0.26`). Replacing its visual inside the shared prefab would silently change Levels 2 and 3. A new prefab isolates the Scene 1 change while giving Scenes 2 and 3 a reusable plate later.

**Alternatives considered**:
- Update `Plate.prefab` directly: rejected because it would affect Levels 2 and 3 before they are tested with the new visual.
- Keep the primitive plate: rejected because the user explicitly requested `ceramic_plate.fbx`.
- Build the plate per scene: rejected because it duplicates work and breaks the reuse goal.

**Implementation notes**:
- Normalize the FBX import so the plate diameter is approximately `0.26 m`, matching the current slot layout (`±0.06` on X/Z).
- Keep the plate pivot at the base center; place the visual so its top surface is around local `y = 0.01`.
- Preserve the four existing slot positions and trigger sizes.
- Add a simple non-trigger collider only if raycast targeting needs it; the slot triggers remain the interaction targets.

**Measured FBX bounds (2026-11-16)**:
- Imported root scale: `(100, 100, 100)`.
- Renderer bounds at identity rotation: center `(0.00, 0.00, 0.01)`, size `(0.26, 0.26, 0.03)`.
- The model is Z-up: the plate lies in the X/Y plane and is `0.03 m` thick along Z.
- Use local rotation `(90, 0, 0)` to lay it flat in Unity's X/Z plane; the resulting thickness is along Y.
- Keep the imported root scale so the diameter stays `0.26 m`; position the visual so its top surface is around local `y = 0.01`.

## 2. Figma-style UI Canvas architecture

**Decision**: Create a new `FigmaHUD.prefab` (Screen Space Overlay, 1920×1080, CanvasScaler match `0.5`) containing:
- `StartScreen`
- `GameplayHUD` with `FoodCardGrid`, `StatusMakanan`, `Prompt`, `Reticle`, and `PausePanel`
- `FinalResult`

Keep the existing `HUD.prefab` for Levels 2 and 3. Build the new prefab through an editor builder (`FigmaHudBuilder`) so the hierarchy, anchors, colors, and references are reproducible; document a manual UI Canvas fallback in `quickstart.md`.

**Rationale**: The user explicitly requested UI Canvas and a Figma-style layout. A separate prefab prevents the Scene 1 redesign from changing Levels 2 and 3, and the editor builder makes the UI repeatable for later scenes.

**Alternatives considered**:
- Modify `HUD.prefab` in place: rejected because Levels 2 and 3 would inherit an untested UI.
- Build the cards as world-space objects: rejected because the user asked for UI Canvas and the Figma board is screen-space.
- Manual-only UI: rejected as the primary path because it is error-prone and hard to replicate; manual steps remain a fallback.

**Design tokens**:
- Background: warm off-white `#F7F7F2`
- Primary text: `#2B2B2B`
- Muted text: `#6B6B6B`
- Positive green: `#6DBE45`
- Negative red/coral: `#F26B5E`
- Card surface: `#FFFFFF`
- Focus outline: `#2E7D32`
- Font: existing TMP font (LiberationSans SDF or project font)

## 3. Food card data and images

**Decision**: Add `ProteinType` to `FoodData` with two display categories (`Hewani`, `Nabati`) plus a `hasProteinType` flag for later foods with no meaningful protein. Use the existing `FoodData.icon` field for card images. Add an editor `FoodIconGenerator` that renders each food prefab to a 256×256 transparent PNG and assigns the sprite when `icon` is missing. Use existing textures (for example `Rendang.jpg`, `Kerupuk.jpg`, `Indomie Mi Goreng Packet Render.png`) where they are already suitable.

**Rationale**: `FoodData.icon` is currently null for every food, but the Figma cards require images. Generating icons from the existing 3D prefabs keeps one source of truth and avoids hand-authoring eight new images.

**Alternatives considered**:
- Hardcode card data: rejected because it duplicates `FoodData` and breaks the single-source rule.
- Use only text cards: rejected because the Figma direction is image-led.
- Create new 2D food art: rejected because it adds unnecessary asset work.

**Data contract**:
- `proteinType`: `Hewani` or `Nabati`.
- `hasProteinType`: false shows a neutral placeholder (`—`) on the card.
- `icon`: sprite used by `FoodCardView`; fallback to a neutral placeholder sprite if null.

## 4. Status Makanan bar

**Decision**: Reuse `MealEvaluator.Evaluate()` and `MealResult` as the single source of truth. Add a `MealStatusBarPresenter` that accepts the same preview/final calls as `NutritionBar` but renders the Figma style: a horizontal green-to-red track, a marker, a bold status label (`SEIMBANG` / `PERLU PERBAIKAN`), and a short detail label.

**Rationale**: The existing scoring already produces a 0–100 score, band, healthy/less-healthy counts, and feedback. Reusing it avoids a second scoring model and keeps the card, bar, and Final Result consistent.

**Alternatives considered**:
- Create a new scoring formula for the bar: rejected because it would diverge from the report and tests.
- Keep `NutritionBar` visuals unchanged: rejected because the Figma bar is a different visual language.

**Integration**:
- `LevelFlowController.OnPlateChanged()` calls `mealStatus.SetPreview(result)`.
- `ServeInternal()` calls `mealStatus.SetFinal(result)`.
- If `mealStatus` is not assigned, fall back to the existing `NutritionBar` for Levels 2 and 3.

## 5. Final Result with three feedback points per panel

**Decision**: Extend `MealResult` with:
- `string[] positivePoints` (exactly 3)
- `string[] improvementPoints` (exactly 3)
- `string statusBadge` (`LEBIH SEHAT` / `PERLU DIUBAH`)
- `string statusSummary`

Extend `MealEvaluator` to build these arrays from existing nutrition/composition signals. Add a `FinalResultPresenter` with a green balanced panel and a red less-balanced panel, each with three bullets and a badge. Keep the existing `HealthReportPresenter` for Levels 2 and 3.

**Rationale**: The existing report provides two strengths and one improvement; the Figma Final Result requires three points per panel. Extending the result model keeps the content logic testable and reusable.

**Alternatives considered**:
- Reuse `HealthReportPresenter` as-is: rejected because it has the wrong layout and point count.
- Build the three points in the UI presenter from raw nutrition: rejected because it would duplicate evaluation logic and make tests harder.

**Fallback rules**:
- If fewer than three positive signals exist, add supportive fallback messages.
- If fewer than three improvement signals exist, add neutral next-step messages.
- The arrays must always contain exactly three non-empty strings.

## 6. Start screen and level flow

> **Update 2026-11-17**: The Start screen was removed by user request. Scene 1 starts immediately; the `StartScreenPresenter` code remains as optional legacy support only.

**Decision**: Add `StartScreenPresenter` and an optional `startScreen` reference in `LevelFlowController`. `Start()` prepares the level, disables player/reticle input, and shows the start screen. `StartLevel()` hides the start screen, enables input, and starts the timer. `Profil Dev` is an optional simple information panel; it does not block Start.

**Rationale**: The Figma board includes a start screen. Adding it to the existing flow avoids a separate bootstrap scene and keeps Scene 1 self-contained.

**Alternatives considered**:
- Separate start scene: rejected because it adds a scene transition and complicates build settings.
- Auto-start without a start screen: rejected because it would not follow the Figma direction.

## 7. Reuse in Scenes 2 and 3

**Decision**: Make the reusable pieces explicit prefabs and data:
- `ServingPlate.prefab`
- `FoodCard.prefab`
- `FigmaHUD.prefab`
- `FinalResult` and `StartScreen` inside `FigmaHUD.prefab`
- `LevelDefinition` food pool and plate placement as scene configuration

Scene-specific code must not be required. A scene builder or documented checklist wires the prefabs and level data.

**Rationale**: The user explicitly asked for easy replication in Scenes 2 and 3. Prefabs plus ScriptableObject configuration are the smallest reusable units in this project.

**Alternatives considered**:
- Copy the entire `MainVR` scene: rejected because it duplicates environment content and makes maintenance harder.
- Put all logic in one scene-specific controller: rejected because it cannot be reused.

## 8. Testing and validation

**Decision**: Use Unity Test Framework EditMode and PlayMode tests.
- EditMode: `FoodData` protein type validation, `MealResult` point arrays, card formatting, level definition validity.
- PlayMode: Scene 1 start screen, card grid values, plate/serve flow, status bar updates, Final Result, retry/next.
- Existing Level 2 and Level 3 smoke tests must continue to pass.
- Manual validation: 1920×1080 visual review and Rexus gamepad playtest.

**Rationale**: The project already has EditMode and PlayMode tests; extending them is the lowest-risk validation path.

**Alternatives considered**:
- Manual-only testing: rejected because the feature touches shared data and flow.
- New test framework: rejected because Unity Test Framework is already installed.

## 9. Figma fidelity constraint

**Decision**: Treat the public Figma thumbnail as the visual direction, not a pixel-perfect specification. Implement the hierarchy, color semantics, card grid, bottom status bar, and two-panel Final Result. Document that exact text, spacing, and fonts may be adjusted until a higher-resolution export is provided.

**Rationale**: The interactive Figma file is not readable through the available public URL; only the thumbnail is accessible. The spec explicitly allows approximation.

**Alternatives considered**:
- Block implementation until a Figma export arrives: rejected because the functional layout is clear enough to proceed.
- Ignore Figma and keep the old HUD: rejected because the user asked to follow it.
