# Contract: Meal Evaluation

**Feature**: `001-nutripick-vr-game`

The evaluator is a pure, deterministic function. It has no scene references, no time dependence, and no randomness.

## API

```csharp
public static class FoodClassifier
{
    public static FoodHealthLabel Resolve(FoodData food, NutritionRules rules);
    public static float HealthValue(FoodData food, NutritionRules rules);
}

public static class MealEvaluator
{
    public static MealResult Evaluate(
        IReadOnlyList<FoodData> foods,
        NutritionRules rules,
        int minItems = 0,
        bool requiresDrink = false);
}
```

- `foods` may be empty.
- The order of `foods` must not affect the score.
- `minItems` and `requiresDrink` only affect `isIncomplete` and warnings; they do not change the score.

## Empty Plate

- `Evaluate(empty, rules)` returns `score = 50`, `band = Neutral`, `bandLabel = "Belum dinilai"`.
- No strengths or improvement are generated for the neutral state.
- The UI displays the neutral state, not a report.

## Item Health Value

```text
h = clamp(
    0.50
  + 0.15 * clamp01(fiber / fiberTargetGrams)
  + 0.15 * clamp01(protein / proteinTargetGrams)
  - 0.20 * clamp01(addedSugar / sugarTargetGrams)
  - 0.15 * clamp01(sodium / sodiumTargetMg)
  - 0.10 * processingPenalty
  - 0.05 * friedPenalty
  + 0.15 * unsweetenedDrinkBonus,
  itemHealthMin, itemHealthMax)
```

- `processingPenalty`: Minimal 0.0, Processed 0.5, UltraProcessed 1.0.
- `friedPenalty`: 1.0 when `isFried`, otherwise 0.0.
- `unsweetenedDrinkBonus`: 1.0 when `isDrink && addedSugar == 0`, otherwise 0.0.

## Composition

```text
coreCovered = count of {Carbohydrate, Protein, Vegetable} present in the union of item categories
distinctCategories = count of {Vegetable, Protein, Carbohydrate, Fruit, Drink} present
composition = clamp01(
    0.70 * (coreCovered / 3)
  + 0.30 * (distinctCategories / 5)
  - 0.15 * (3 - coreCovered))
```

- `Snack` is not counted in `distinctCategories`.
- An item with multiple flags contributes all of its flags.
- The union is used, so two proteins count as one protein category.

## Final Score

```text
averageItemHealth = mean(HealthValue(item) for item in foods)
score = round(100 * clamp01(0.50 * averageItemHealth + 0.50 * composition))
```

- Clamp the result to 0–100.
- Round to the nearest integer.
- The score is independent of item order.
- Empty plate is handled before this formula and returns 50 neutral.

## Bands

| Range | Band | Label |
|-------|------|-------|
| Empty | Neutral | "Belum dinilai" |
| 0–39 | NeedsImprovement | "Perlu perbaikan" |
| 40–69 | Adequate | "Cukup" |
| 70–100 | Balanced | "Seimbang" |

Bands come from `NutritionRules.bands`; the values above are the default.

## Result Contents

`MealResult` must include:

- `score`, `band`, `bandLabel`
- `healthyCount`, `lessHealthyCount`
- `itemScores` with each item's id, display name, resolved label, and health value
- `averageItemHealth`, `compositionScore`, `compositionCovered`
- `strengths` (two strings)
- `improvement` (one string)
- `dimensions` (energy, focus, long-term)
- `isIncomplete` and `warnings`

## Strengths and Improvement

Use the rules in [data-model.md](../data-model.md#strengths) and [data-model.md](../data-model.md#improvement). The evaluator must always return exactly two strengths and one improvement, using the fallbacks when no specific rule applies.

## Health Dimensions

Use the thresholds and sentence templates in [data-model.md](../data-model.md#health-dimensions). Each dimension returns a `DimensionRating`, an Indonesian label, and one sentence.

## Determinism and Purity

- No `UnityEngine.Time`, `Random`, scene lookups, or coroutines.
- No mutation of input `FoodData` assets.
- Same inputs always produce the same output.
- The evaluator may use `System.Math` or `UnityEngine.Mathf`; both are deterministic for these operations.

## Test Vectors

| Case | Expected |
|------|----------|
| Empty plate | score 50, Neutral, "Belum dinilai" |
| Rice + boiled egg + tempeh + broccoli | score ≥ 70, Balanced |
| Rice + boiled egg + tempeh + broccoli + banana | score ≥ 70, Balanced |
| Fried egg + fried noodles + crackers | score < 50, NeedsImprovement |
| Three bananas | score < 40, NeedsImprovement |
| Rice + rendang + cassava leaves + broccoli | score 65–69, Adequate |
| Rice + boiled egg + cassava leaves + broccoli | score ≥ 70, Balanced |
| Noodles + boiled egg + tempeh + broccoli + water | score ≥ 70, Balanced |
| Noodles + boiled egg + tempeh + broccoli + sweet tea | score 60–69, Adequate |
| Any plate | score clamped to 0–100 |
| Same items in reverse order | identical score |
| Level 3 plate without a drink | `isIncomplete` true, warning present |
| Level 3 plate with water | `isIncomplete` false |

## Integration Points

- `PlateController` exposes `IReadOnlyList<FoodData> Foods`.
- `LevelFlowController` calls `MealEvaluator.Evaluate` on every `PlateChanged` for preview and once on serve for final.
- `NutritionBar` only renders the result; it does not compute the score.
- `HealthReportPresenter` only renders the result; it does not generate strengths or dimensions.
