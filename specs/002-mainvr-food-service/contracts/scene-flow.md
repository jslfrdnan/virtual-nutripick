# Scene Flow Contract: Scene 1 MainVR

**Feature**: `002-mainvr-food-service`
**Date**: 2026-11-16

This contract defines the reusable Scene 1 wiring and the minimum steps to reproduce it in Scenes 2 and 3.

## 1. Required scene objects

| Object | Component | Purpose |
|--------|-----------|---------|
| `MainVR` | Scene | Scene 1 environment |
| `PlayerRig` | `FirstPersonController`, `GamepadInput`, `HeldFoodCarrier`, `ReticleInteractor` | Player movement, carry, aim |
| `ServingPlate` | `PlateController` + 4 `PlateSlot` | Plate and placement positions |
| `TableFoodStand` | `FoodStand` | Food source / anchors |
| `ServeStation` | `ServeStation` | Serve interaction |
| `FigmaHUD` | Canvas + presenters | Card grid, Status Makanan, Final Result, prompt, pause |
| `LevelFlow` | `LevelFlowController` | Level orchestration |
| `TableSurfaceCollider` | `BoxCollider` | Keeps food on the table |

## 2. Required prefabs

| Prefab | Path | Reuse |
|--------|------|-------|
| Serving plate | `Assets/Prefabs/Interaction/ServingPlate.prefab` | Scene 1, 2, 3 |
| Food card | `Assets/Prefabs/UI/FoodCard.prefab` | Scene 1, 2, 3 |
| Food card grid | `Assets/Prefabs/UI/FoodCardGrid.prefab` | Scene 1, 2, 3 |
| Status Makanan | `Assets/Prefabs/UI/StatusMakanan.prefab` | Scene 1, 2, 3 |
| Final Result | `Assets/Prefabs/UI/FinalResult.prefab` | Scene 1, 2, 3 |
| Figma HUD | `Assets/Prefabs/UI/FigmaHUD.prefab` | Scene 1, 2, 3 |
| Serve station | `Assets/Prefabs/Interaction/ServeStation.prefab` | Scene 1, 2, 3 |
| Food prefabs | `Assets/Prefabs/Food/{id}.prefab` | Existing content |

## 3. Required data

| Asset | Purpose |
|-------|---------|
| `LD_MainVR.asset` | Scene 1 level definition, 8 foods, min 3 |
| `NutritionRules.asset` | Classification and meal evaluation thresholds |
| `FD_*.asset` | Food values, protein type, icon, prefab |

## 4. Start-to-finish sequence

```text
Scene loads
  → LevelFlow.Start()
  → LevelFlow.StartLevel()
  → FoodCardGrid.Bind(level.foodPool)
  → StatusMakanan.SetNeutral()
  → player picks food
  → ReticleInteractor highlights/focuses card
  → player places food on ServingPlate slot
  → PlateController.PlateChanged
  → MealEvaluator.Evaluate()
  → StatusMakanan.SetPreview()
  → player presses Serve (3+ foods)
  → PlateController.Lock()
  → StatusMakanan.SetFinal()
  → FinalResult.Show()
  → Retry or Next
```

## 5. Replication checklist for Scene 2 and 3

1. Duplicate the environment scene or create a new level scene.
2. Add `ServingPlate.prefab`; position it on the table.
3. Add `TableFoodStand` and assign the level's `foodPool`.
4. Add `ServeStation.prefab`.
5. Add `FigmaHUD.prefab`.
6. Add `LevelFlow` and assign the new `LevelDefinition`.
7. Assign `foodCards`, `mealStatus`, `finalResult`, `prompt`, and pause references. Do not assign `startScreen`; Scene 1 starts immediately.
8. Assign `plate`, `stands`, `serveStation`, `player`, `reticle`, and `input`.
9. Set the level's `foodPool`, `minItems`, `sceneName`, and `nextSceneName`.
10. Add the scene to Build Settings in the correct order.
11. Run the Scene 1 PlayMode validation scenarios.
12. Verify no duplicate EventSystem and no missing references.

## 6. Backward-compatibility contract

- Existing `HUD.prefab`, `Plate.prefab`, `HealthReportPresenter`, `FoodInfoCardPresenter`, and `NutritionBar` remain available for Levels 2 and 3.
- Scene 1 has no Start screen; `LevelFlowController.startScreen` is null and the level starts immediately.
- `LevelFlowController` uses the new presenters only when their references are assigned; otherwise it falls back to the old behavior.
- `ReticleInteractor` uses `FoodCardGridPresenter` only when assigned; otherwise it falls back to `FoodInfoCardPresenter`.
- No existing scene, prefab, or ScriptableObject is deleted.
- Existing EditMode and PlayMode tests must continue to pass.
