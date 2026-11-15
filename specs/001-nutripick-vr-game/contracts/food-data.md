# Contract: Food Data

**Feature**: `001-nutripick-vr-game`

This contract defines what a food asset must contain and how its healthy/less healthy label is resolved. The information card and the bar counts must use the same resolved label.

## Asset Contract

- Type: `FoodData : ScriptableObject`
- Menu: `NutriPick/Food Data`
- Location: `Assets/ScriptableObjects/Food/`
- Naming: `FD_<id>.asset`, for example `FD_nasi_putih.asset`
- One asset per unique food; levels reference the same asset when a food appears in multiple levels.

## Required Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `id` | string | Yes | Lowercase snake_case, unique across all foods |
| `displayName` | string | Yes | Indonesian, shown on the card |
| `description` | string | Yes | 1–2 supportive sentences; no medical claims |
| `composition` | flags | Yes | At least one of Vegetable, Protein, Carbohydrate, Fruit, Drink, Snack |
| `processing` | enum | Yes | Minimal, Processed, UltraProcessed |
| `isFried` | bool | Yes | Drives the fried penalty and classification |
| `isDrink` | bool | Yes | Must match the Drink flag |
| `calories` | int | Yes | ≥ 0 |
| `protein` | float | Yes | Grams, ≥ 0 |
| `carbs` | float | Yes | Grams, ≥ 0 |
| `fat` | float | Yes | Grams, ≥ 0 |
| `fiber` | float | Yes | Grams, ≥ 0 |
| `addedSugar` | float | Yes | Grams, ≥ 0 |
| `sodium` | float | Yes | Milligrams, ≥ 0 |
| `portionLabel` | string | Yes | e.g. "150 g", "1 butir (55 g)" |
| `icon` | Sprite | No | Optional for the first pass |
| `prefab` | GameObject | Yes | Must contain one `FoodItem` whose `data` points back to this asset |
| `healthyOverride` | enum | Yes | None, ForceHealthy, ForceLessHealthy |

## Classification Contract

`FoodClassifier.Resolve(FoodData food, NutritionRules rules)` returns `FoodHealthLabel`.

Order:

1. `ForceHealthy` → Healthy.
2. `ForceLessHealthy` → LessHealthy.
3. LessHealthy if any:
   - `addedSugar > rules.healthySugarMax`
   - `sodium > rules.healthySodiumMax`
   - `processing == UltraProcessed`
   - `isFried`
   - `composition` has `Snack` and does not have `Vegetable` or `Fruit`
   - `isDrink && addedSugar > rules.healthyDrinkSugarMax`
4. Otherwise Healthy.

The same function is used by:
- the information card badge,
- the plate's healthy/less healthy counts,
- the report's strength/improvement logic.

## Prefab Contract

```text
FoodRoot (layer Food)
├── FoodItem (data = this FoodData)
├── Collider (Box, Sphere, or Capsule)
├── Rigidbody
├── Model (visual mesh; no collider)
└── InfoAnchor (optional; used by the card presenter)
```

Rules:

- Layer: `Food`.
- Mass: 0.2–0.5 kg.
- Collider: Box, Sphere, or Capsule; convex if a MeshCollider is used.
- Pivot at the base center so the food sits on the plate.
- No non-convex MeshCollider on the Rigidbody.
- The prefab must work when instantiated by `FoodStand.SpawnFoods()`.

### Runtime states

| State | Rigidbody | Collider | Notes |
|-------|-----------|----------|-------|
| On stand | Dynamic, gravity on | Enabled | Reticle can target it |
| Carried | Kinematic, gravity off | Disabled | Follows `HeldFoodCarrier`; does not block the reticle |
| Plated | Kinematic, gravity off | Enabled | Parented to the slot; reticle can target it to pick it up |
| Dropped | Dynamic, gravity on | Enabled | Falls; returns home if it leaves the play area |

`FoodItem` exposes:

```csharp
public FoodData Data { get; }
public FoodStand HomeStand { get; }
public bool IsCarried { get; }
public void SetCarried(bool carried);
public void ReturnHome();
```

## Food Stand Contract

A `FoodStand` is a scene component, not a ScriptableObject.

- It has a `displayName`, a `foods` array, and an `anchors` array.
- `SpawnFoods()` instantiates each food prefab at the matching anchor.
- The union of all stands' foods must equal the level's `foodPool`.
- A food must not appear on two stands in the same level.
- A dropped item returns to its home stand anchor.

## Validation

Editor menu `NutriPick/Validate Food Data` checks:

- `id` is non-empty, lowercase snake_case, and unique.
- `displayName`, `description`, and `portionLabel` are non-empty.
- All numeric values are ≥ 0.
- `composition` is not None.
- `isDrink` matches the Drink flag.
- `prefab` is assigned and contains exactly one `FoodItem`.
- The prefab's `FoodItem.data` equals this asset.
- An override is documented in `content/food-catalog.md`.

Errors are logged with the asset path and are treated as blocking for content sign-off.

## Example

```text
id: nasi_putih
displayName: Nasi putih
composition: Carbohydrate
processing: Minimal
isFried: false
isDrink: false
calories: 195
protein: 4.0
carbs: 43.0
fat: 0.4
fiber: 0.6
addedSugar: 0
sodium: 2
portionLabel: 150 g
healthyOverride: None
resolved label: Sehat
```
