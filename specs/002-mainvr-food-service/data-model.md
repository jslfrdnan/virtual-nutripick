# Data Model: Scene 1 MainVR — Food Plating, Figma-Style Food Cards, and Final Result

**Feature**: `002-mainvr-food-service`
**Date**: 2026-11-16
**Spec**: `specs/002-mainvr-food-service/spec.md`

## 1. Existing entities reused

### FoodData (ScriptableObject)

Current fields are reused as the single source of truth for the scene food pool and card values.

| Field | Type | Notes |
|-------|------|-------|
| `id` | string | lowercase snake_case, unique |
| `displayName` | string | shown on the card |
| `description` | string | supportive educational copy |
| `composition` | flags enum | vegetable, protein, carbohydrate, fruit, drink, snack |
| `processing` | enum | minimal, processed, ultra-processed |
| `isFried` | bool | used by meal evaluation |
| `isDrink` | bool | must match the Drink flag |
| `healthyOverride` | enum | none, force healthy, force less healthy |
| `calories` | int | per portion |
| `protein` | float | grams |
| `carbs` | float | grams |
| `fat` | float | grams |
| `fiber` | float | grams |
| `addedSugar` | float | grams |
| `sodium` | float | milligrams |
| `portionLabel` | string | shown as secondary card text |
| `icon` | Sprite | card image; may be generated or fallback |
| `prefab` | GameObject | 3D food prefab |

### LevelDefinition (ScriptableObject)

| Field | Type | Notes |
|-------|------|-------|
| `levelId` | string | stable level id |
| `displayName` | string | e.g. `Level 1 — Warteg` |
| `sceneName` | string | `MainVR` |
| `nextSceneName` | string | `Level02_Padang` |
| `levelIndex` | int | `1` for Scene 1 |
| `settingDescription` | string | short setting copy |
| `foodPool` | FoodData[] | 8 foods for Scene 1; also drives the card grid |
| `minItems` | int | `3` |
| `requiresDrink` | bool | `false` for Scene 1 |
| `minimumStands` | int | `1` |
| `timeLimitSeconds` | float | existing pacing; visible timer may be hidden |
| `graceSeconds` | float | existing grace behavior |
| `isFinalLevel` | bool | `false` |
| `learningFocus` | string[] | educational focus |

## 2. New or extended entities

### ProteinType (enum)

| Value | Meaning | Card display |
|-------|---------|--------------|
| `Hewani` | animal protein | `Hewani` |
| `Nabati` | plant protein | `Nabati` |

`FoodData` also gets:

| Field | Type | Default | Notes |
|-------|------|---------|-------|
| `hasProteinType` | bool | `false` | When false, the card shows `—` instead of a category. All eight Scene 1 foods must be true. |

**Validation rules**:
- Every Scene 1 food must have `hasProteinType = true` and a valid `proteinType`.
- `proteinType` is only meaningful when `hasProteinType` is true.
- The card must never show a third protein category.

### MealResult (extended)

Existing fields remain for compatibility with Levels 2 and 3. New fields support the Figma Final Result.

| Field | Type | Notes |
|-------|------|-------|
| `positivePoints` | string[3] | exactly three supportive points |
| `improvementPoints` | string[3] | exactly three next-step points |
| `statusBadge` | string | `LEBIH SEHAT` or `PERLU DIUBAH` |
| `statusSummary` | string | one short sentence for the result header |
| `isBalanced` | bool | derived from `score` and band |

**Validation rules**:
- `positivePoints` and `improvementPoints` must each contain exactly three non-empty strings.
- `statusBadge` must be one of the two canonical labels.
- `isBalanced` must agree with `score`/`band` (balanced when the score is in the balanced band).
- Existing `strengths`, `improvement`, `dimensions`, `warnings`, and `isIncomplete` remain populated for backward compatibility.

### FoodCardModel (view model, runtime only)

| Field | Type | Notes |
|-------|------|-------|
| `foodId` | string | stable key for highlighting |
| `displayName` | string | card title |
| `icon` | Sprite | food image or placeholder |
| `proteinTypeLabel` | string | `Hewani`, `Nabati`, or `—` |
| `caloriesText` | string | e.g. `250 kkal` |
| `proteinText` | string | e.g. `15 g` |
| `carbsText` | string | e.g. `5 g` |
| `fatText` | string | e.g. `19 g` |
| `isHealthy` | bool | drives the health accent |
| `isFocused` | bool | drives the focused card outline |

**Validation rules**:
- Text fields must use the project's Indonesian number formatting (`NutritionFormat`).
- A missing icon uses the placeholder sprite, never a null Image.
- A missing value uses `—`, never an empty string.

### StatusMakananState (view model, runtime only)

| Field | Type | Notes |
|-------|------|-------|
| `score` | int | 0–100 |
| `band` | ScoreBand | neutral, balanced, adequate, needs improvement |
| `label` | string | `BELUM DINILAI`, `SEIMBANG`, `CUKUP`, `PERLU PERBAIKAN` |
| `detail` | string | short explanation |
| `healthyCount` | int | healthy items on the plate |
| `lessHealthyCount` | int | less healthy items on the plate |
| `isFinal` | bool | true after serving |

### FinalResultContent (view model, runtime only)

| Field | Type | Notes |
|-------|------|-------|
| `title` | string | `FINAL RESULT` |
| `balancedTitle` | string | green panel title |
| `lessBalancedTitle` | string | red panel title |
| `balancedPoints` | string[3] | three green-panel bullets |
| `lessBalancedPoints` | string[3] | three red-panel bullets |
| `balancedBadge` | string | `LEBIH SEHAT` |
| `lessBalancedBadge` | string | `PERLU DIUBAH` |
| `closingMessage` | string | `PILIHAN KECIL, DAMPAK BESAR!` |
| `disclaimer` | string | educational disclaimer |

## 3. Scene composition

### Scene 1 setup

| Element | Count | Notes |
|---------|-------|-------|
| Serving plate | 1 | `ServingPlate.prefab` using `ceramic_plate.fbx` |
| Plate positions | 4 | stable, non-overlapping |
| Food items | 8 | from `LD_MainVR.foodPool` |
| Food cards | 8 | one per food, UI Canvas |
| Status Makanan bar | 1 | bottom of gameplay HUD |
| Start screen | 1 | Start + optional Profil Dev |
| Final Result screen | 1 | two panels + Retry/Next |
| Serve station | 1 | existing `ServeStation` |
| Table surface collider | 1 | existing or rebuilt |

### UI hierarchy contract

```text
FigmaHUD (Canvas, Screen Space Overlay, 1920×1080)
├── EventSystem
├── StartScreen
│   ├── Backdrop
│   ├── Title
│   ├── StartButton
│   ├── ProfileButton
│   └── ProfilePanel (optional)
├── GameplayHUD
│   ├── Reticle
│   ├── FoodCardGrid
│   │   └── FoodCard × 8
│   ├── StatusMakanan
│   ├── Prompt
│   └── PausePanel
└── FinalResult
    ├── Backdrop
    ├── Title
    ├── BalancedPanel
    │   ├── Badge
    │   └── Points × 3
    ├── LessBalancedPanel
    │   ├── Badge
    │   └── Points × 3
    ├── ClosingMessage
    ├── RetryButton
    └── NextButton
```

## 4. State transitions

### Scene flow

```text
Boot
  → StartScreen
  → Gameplay (empty plate)
  → Gameplay (partially filled)
  → Gameplay (ready to serve: 3–4 foods)
  → FinalResult
  → Retry → Gameplay (empty plate)
  → Next → next scene
```

### Plate

```text
Empty
  → PartiallyFilled (1–2 foods)
  → Ready (3–4 foods)
  → Locked (served)
```

Rules:
- A food can be placed only in an empty, unlocked position.
- Removing a food returns the plate to the previous count state.
- Serving is allowed only in `Ready`.
- Serving locks all positions and all plated foods.

### Food card

```text
Idle
  → Highlighted (reticle over food)
  → Focused (picked up or inspected)
  → Idle (look away, drop, or place)
```

Rules:
- Only one card is highlighted or focused at a time.
- A focused card must remain readable and must not cover the reticle target or plate.
- If a food has no icon, the card uses the placeholder image.

### Status Makanan

```text
Neutral (empty plate)
  → Preview (plate changes; 0.15 s update)
  → Final (serve; 1.0 s update)
```

Rules:
- The label and color must agree.
- The bar must not rely on color alone.
- The final state remains visible until the Final Result screen opens.

## 5. Relationships

```text
LevelDefinition 1 ── * FoodData
FoodData 1 ── 1 FoodCardModel
FoodData 1 ── 1 FoodItem (scene representation)
PlateController 1 ── 4 PlateSlot
PlateSlot 0..1 ── FoodItem
MealEvaluator ── MealResult ── StatusMakananState
MealResult ── FinalResultContent
```

## 6. Persistence

No new persistent data. `FoodData`, `LevelDefinition`, and `NutritionRules` remain ScriptableObject assets. `GameSession` continues to hold per-run results in memory. The Scene 1 feature adds no save file, account, or database.
