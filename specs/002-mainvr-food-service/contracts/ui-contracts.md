# UI Contracts: Scene 1 MainVR

**Feature**: `002-mainvr-food-service`
**Date**: 2026-11-16

These contracts define the boundaries between the Scene 1 flow, the UI presenters, and the reusable prefabs. They are contracts, not implementation bodies.

## 1. FoodCardView

**Prefab**: `Assets/Prefabs/UI/FoodCard.prefab`

**Purpose**: Display one food's image, name, protein type, calories, protein, carbohydrates, and fat.

**Inspector bindings**:
- `Image icon`
- `TMP_Text nameText`
- `TMP_Text proteinTypeText`
- `TMP_Text caloriesText`
- `TMP_Text proteinText`
- `TMP_Text carbsText`
- `TMP_Text fatText`
- `Image cardBackground`
- `Image healthAccent`
- `GameObject focusOutline`
- `Image placeholderIcon`

**API**:
```csharp
void Bind(FoodData food, NutritionRules rules);
void SetFocused(bool focused);
void SetHighlighted(bool highlighted);
void Clear();
```

**Behavior**:
- `Bind` maps `FoodData` to the view using `NutritionFormat` for numbers.
- `proteinTypeText` is `Hewani`, `Nabati`, or `—` when `hasProteinType` is false.
- `healthAccent` uses the existing `FoodClassifier` result; the card must not invent a second health rule.
- `focusOutline` is active only for the focused card.
- A missing `icon` uses `placeholderIcon`; it never leaves the image blank.

## 2. FoodCardGridPresenter

**Prefab**: `Assets/Prefabs/UI/FoodCardGrid.prefab`

**Purpose**: Build and manage the eight food cards from the active level's food pool.

**Inspector bindings**:
- `RectTransform cardContainer`
- `FoodCardView cardPrefab`
- `NutritionRules rules`

**API**:
```csharp
void Bind(IReadOnlyList<FoodData> foods);
void Highlight(string foodId);
void Focus(string foodId);
void ClearFocus();
void ClearHighlight();
```

**Behavior**:
- `Bind` destroys/rebuilds cards or reuses a pool; the visible card count equals the food count.
- Only one card may be focused; only one may be highlighted.
- `Focus(foodId)` also highlights that card.
- Unknown food ids are ignored without throwing.
- The grid must remain usable when a food has no icon or missing optional data.

## 3. MealStatusBarPresenter

**Prefab**: `Assets/Prefabs/UI/StatusMakanan.prefab` (child of `FigmaHUD`)

**Purpose**: Render the Figma-style Status Makanan bar from `MealResult`.

**Inspector bindings**:
- `Image fill`
- `RectTransform marker`
- `TMP_Text statusLabel`
- `TMP_Text detailLabel`
- `TMP_Text countsLabel`
- `Color neutralColor`
- `Color balancedColor`
- `Color warningColor`

**API**:
```csharp
void SetNeutral();
void SetPreview(MealResult result);
void SetFinal(MealResult result);
```

**Behavior**:
- `SetNeutral` shows `BELUM DINILAI` and a neutral marker.
- `SetPreview` updates within 0.15 s.
- `SetFinal` updates within 1.0 s and keeps the final state visible.
- The label must always accompany the color.
- The presenter reads only `MealResult`; it must not re-evaluate nutrition.

## 4. StartScreenPresenter

**Prefab**: `Assets/Prefabs/UI/StartScreen.prefab` (child of `FigmaHUD`)

**Purpose**: Present the Figma start screen before gameplay.

**Inspector bindings**:
- `GameObject root`
- `Button startButton`
- `Button profileButton`
- `GameObject profilePanel`
- `Button profileCloseButton`

**API**:
```csharp
void Show(Action onStart, Action onProfile);
void Hide();
```

**Behavior**:
- Start invokes the supplied callback exactly once.
- Profil Dev opens a simple optional panel; it must not block Start.
- If no profile panel is configured, the button is hidden.
- The start screen must be dismissible with the gamepad and keyboard.

## 5. FinalResultPresenter

**Prefab**: `Assets/Prefabs/UI/FinalResult.prefab` (child of `FigmaHUD`)

**Purpose**: Present the two-panel Figma Final Result.

**Inspector bindings**:
- `GameObject root`
- `TMP_Text titleText`
- `TMP_Text balancedTitleText`
- `TMP_Text lessBalancedTitleText`
- `TMP_Text[] balancedPointTexts` (3)
- `TMP_Text[] lessBalancedPointTexts` (3)
- `TMP_Text balancedBadgeText`
- `TMP_Text lessBalancedBadgeText`
- `TMP_Text closingText`
- `TMP_Text disclaimerText`
- `Button retryButton`
- `Button nextButton`

**API**:
```csharp
void Show(MealResult result, LevelDefinition level, Action onRetry, Action onNext);
void Hide();
```

**Behavior**:
- Populates exactly three points in each panel.
- Highlights the panel that matches the meal outcome.
- `Retry` invokes `onRetry`; `Next` invokes `onNext`.
- If the level is final, the Next button is replaced by the existing Recap action or hidden according to the existing flow.
- The presenter does not compute nutrition; it only formats `MealResult` and `FinalResultContent`.

## 6. Scene flow contract

**Controller**: `LevelFlowController`

**New optional references**:
- `StartScreenPresenter startScreen`
- `FoodCardGridPresenter foodCards`
- `MealStatusBarPresenter mealStatus`
- `FinalResultPresenter finalResult`

**Backward compatibility**:
- If `startScreen` is null, the level starts immediately as before.
- If `foodCards` is null, `ReticleInteractor` falls back to `FoodInfoCardPresenter`.
- If `mealStatus` is null, `LevelFlowController` falls back to `NutritionBar`.
- If `finalResult` is null, `LevelFlowController` falls back to `HealthReportPresenter`.

**Flow**:
```text
Start()
  → prepare level
  → show start screen (if assigned)
  → wait for Start

StartLevel()
  → hide start screen
  → enable player/reticle
  → start timer
  → bind food cards from level.foodPool
  → set Status Makanan neutral

PlateChanged
  → MealEvaluator.Evaluate(...)
  → mealStatus.SetPreview(result)

TryServe()
  → validate plate
  → lock plate
  → mealStatus.SetFinal(result)
  → show FinalResult after the existing delay
  → record GameSession result

FinalResult Retry
  → reload active scene

FinalResult Next
  → load level.nextSceneName
```

**Inspector bindings for Scene 1**:
- `level`: `LD_MainVR.asset`
- `rules`: `NutritionRules.asset`
- `plate`: `ServingPlate` → `PlateController`
- `stands`: `TableFoodStand` → `FoodStand`
- `serveStation`: `ServeStation` → `ServeStation`
- `player`: `PlayerRig` → `FirstPersonController`
- `reticle`: `PlayerRig` → `ReticleInteractor`
- `input`: `PlayerRig` → `GamepadInput`
- `startScreen`: `FigmaHUD/StartScreen`
- `foodCards`: `FigmaHUD/GameplayHUD/FoodCardGrid`
- `mealStatus`: `FigmaHUD/GameplayHUD/StatusMakanan`
- `finalResult`: `FigmaHUD/FinalResult`
- `prompt`: `FigmaHUD/GameplayHUD/Prompt`
- `pausePanel` and pause buttons: existing `PausePanel` references

## 7. Reticle contract

`ReticleInteractor` keeps its current behavior and adds optional UI hooks:

```csharp
public FoodCardGridPresenter foodCards;
```

- On food hover: `foodCards.Highlight(food.data.id)`.
- On pickup or Inspect: `foodCards.Focus(food.data.id)`.
- On look-away, drop, place, or pause: `foodCards.ClearFocus()` and `ClearHighlight()`.
- If `foodCards` is null, use the existing `FoodInfoCardPresenter` calls.

## 8. Error handling contract

- Missing `FoodData.icon`: show `placeholderIcon`.
- Missing `proteinType`: show `—`; do not throw.
- Missing UI reference: log one warning and degrade gracefully.
- Missing plate/level reference: `LevelFlowController` logs an error and disables the flow, as it does today.
- Duplicate EventSystem: validation must fail with a clear message.
- More than four foods on the plate: impossible by slot count; placement is refused.
