# Feature Specification: NutriPick VR — Healthy Food Selection Game

**Feature Branch**: `001-nutripick-vr-game`

**Created**: 2026-11-15

**Status**: Draft

**Input**: User description: "Build a virtual reality game using Unity engine. The game will consist of three levels. The user will select healthy and non-healthy food. There will be status bar to track the user choice in selecting healthy and non-healthy. For asset use blender to create asset, there is already existing asset, use it. Also add a information on the food." Source document: *Dokumen Pengembangan Aplikasi Virtual Reality NutriPick Menggunakan Platform Unity untuk Mendukung Sustainable Development Goals: Zero Hunger dan Asta Cita*.

## Overview

NutriPick VR is an educational virtual reality experience that teaches players how to build a balanced meal. Players stand in front of a food counter, pick up real-looking food items, read nutrition information, and place their choices on a plate. A live **Health Balance Bar** shows how healthy the current selection is. After serving, the player receives a short, non-judgmental health report, then continues to the next of three everyday eating situations.

The experience supports SDG 2 (Zero Hunger) and Asta Cita by making nutrition education tangible, memorable, and free of risk.

## Clarifications

### Session 2026-11-15

- Q: How should the Health Balance Bar score a plate containing several foods — composition-aware or only individual item healthiness? → A: Composition-aware hybrid: combine each item's nutrition with a bonus for meal composition and variety.
- Q: How should each food's healthy or less-healthy label be determined for the information card and counts? → A: Rule-based from nutrition thresholds, with a documented per-food override for cultural dishes.
- Q: Which VR runtime should the game primarily target for performance and device testing? → A: Use the Unity XR Device Simulator as the primary runtime; treat physical-headset builds as optional and deferred until hardware is available.
- Q: What should the Health Balance Bar display before any food is placed on the plate? → A: Centered 50 in a neutral gray state labeled "Belum dinilai"; the first item moves it up or down from the middle.
- Q: After a level report, should the player be able to retry that same level before moving on? → A: Yes. The report offers "Ulangi level" and "Lanjut"; a retry replaces that level's result, and the recap uses the latest attempt.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Build a meal from a food counter (Priority: P1)

A player enters a virtual eating setting, looks at the food available on the counter, picks up items with their virtual hands, and places them on a plate. The player can put items back and choose differently before serving. The level ends when the player serves the plate.

**Why this priority**: This is the core interaction. Without grabbing, inspecting, and plating food, none of the educational feedback can happen. It alone delivers a playable demo.

**Independent Test**: A tester can enter Level 1, place three items on the plate, serve, and reach the result screen without using any other feature.

**Acceptance Scenarios**:

1. **Given** a player is in a level with food on the counter, **When** they reach out and grab a food item, **Then** the item attaches to their hand and can be moved freely.
2. **Given** a player is holding a food item above the plate, **When** they release it over an empty plate position, **Then** the item snaps neatly onto the plate and stays there.
3. **Given** a plate with food on it, **When** the player grabs a plated item and releases it away from the plate, **Then** the item leaves the plate and the meal updates.
4. **Given** a plate with fewer items than the level requires, **When** the player tries to serve, **Then** serving is unavailable and the player is told how many more items are needed.
5. **Given** a plate with the required number of items, **When** the player presses Serve, **Then** the plate is locked and the result sequence begins.

---

### User Story 2 - See whether choices are healthy in real time (Priority: P2)

While building a meal, the player watches a status bar that moves as food is added or removed. The bar clearly separates healthy choices from less healthy choices, shows the current balance as a number, and gives an immediate, easy-to-read verdict.

**Why this priority**: The bar is the main teaching device. It turns an invisible nutrition concept into instant visual cause and effect, and it is the feature the player remembers.

**Independent Test**: A tester can add a healthy item and see the bar rise, remove it and see the bar fall, with the number and color updating each time.

**Acceptance Scenarios**:

1. **Given** an empty plate, **When** the level starts, **Then** the bar shows 50 in a neutral gray state labeled "Belum dinilai".
2. **Given** a partially filled plate, **When** the player adds a healthy item, **Then** the bar moves toward the healthy end and the healthy-choice count increases.
3. **Given** a partially filled plate, **When** the player adds a less healthy item, **Then** the bar moves toward the less healthy end and the less-healthy count increases.
4. **Given** a plated item is removed, **When** the removal completes, **Then** the bar recalculates and animates to the new value.
5. **Given** the player serves the meal, **When** the final evaluation runs, **Then** the bar animates to its final value in about one second and locks.
6. **Given** the bar is at any value, **When** the player looks at it, **Then** the numeric value, color, and text label all agree and are readable from a standing position.

---

### User Story 3 - Learn about each food item (Priority: P2)

The player points at or picks up a food item and sees an information card describing what it is and what it contributes nutritionally, including whether it is a healthy or less healthy choice and why.

**Why this priority**: The game must teach, not just score. Information is what turns a sorting exercise into nutrition education.

**Independent Test**: A tester can focus on any food item and read a complete, accurate information card without placing the item on the plate.

**Acceptance Scenarios**:

1. **Given** a food item on the counter, **When** the player's hand or gaze focuses on it, **Then** a compact label appears with the food name and a healthy/less healthy indicator.
2. **Given** a focused food item, **When** the player grabs or holds it, **Then** the full information card appears with calories, protein, fiber, sugar, sodium, portion, and a one-sentence explanation.
3. **Given** a full information card is visible, **When** the player releases the item or looks away, **Then** the card dismisses without blocking the view of the counter.
4. **Given** any information card, **When** it is shown, **Then** it never labels a single food as "forbidden" or "bad", and it explains the contribution in context.

---

### User Story 4 - Progress through three levels and receive a recap (Priority: P3)

The player completes three levels, each set in a different everyday eating situation, and at the end sees a summary of all three meals with one practical next step.

**Why this priority**: Progression gives the experience structure and the recap reinforces learning, but a single level already delivers the core educational value.

**Independent Test**: A tester can play Level 1, advance to Level 2, advance to Level 3, and reach the final recap with all three results retained.

**Acceptance Scenarios**:

1. **Given** the player finishes Level 1, **When** they choose Retry, **Then** Level 1 reloads and the new result replaces the earlier one.
2. **Given** the player finishes Level 1, **When** they press Next, **Then** Level 2 loads and the Level 1 result is retained.
3. **Given** the player finishes Level 2, **When** they press Next, **Then** Level 3 loads with its own food set and requirement.
4. **Given** the player finishes Level 3, **When** the report is dismissed, **Then** a recap shows all three level scores, two strengths, one improvement, and a closing educational message.
5. **Given** the player is on the final recap, **When** they choose to restart, **Then** the session returns to Level 1 with cleared progress.
6. **Given** the player quits mid-session, **When** they relaunch, **Then** the game starts cleanly at Level 1 without corrupted progress.

---

### Edge Cases

- **Empty plate served**: Serving must be blocked and the player told the minimum number of items.
- **Under-filled plate at time-out**: The current plate is auto-served if it meets the minimum; otherwise the player is returned to selection with a short grace period.
- **Removing every item**: The bar returns to 50 in the "Belum dinilai" state and the healthy/less healthy counts reset.
- **All healthy choices**: The report praises balance but notes that variety and portion still matter.
- **All less healthy choices**: The report stays supportive, names two things that went well elsewhere, and gives one concrete swap.
- **Item dropped outside the play area**: The item respawns at its counter position so the player is never stuck.
- **Both hands grab the same item**: Only one hand owns the item; the other hand gets no duplicate.
- **Player cannot reach an item**: Every item is within comfortable arm's reach from the standing position.
- **Time expires while holding food**: The held item is returned to the counter before the result sequence.
- **Text too small or low contrast**: Information must remain legible at typical standing distance with a headset.
- **No headset available**: The full experience must be demonstrable on a desktop VR simulator with keyboard and mouse.
- **Player feels discomfort**: The experience uses a stationary viewpoint and avoids forced camera motion.

## Requirements *(mandatory)*

### Functional Requirements

**Core loop**

- **FR-001**: The experience MUST present exactly three levels, each set in a distinct everyday eating situation.
- **FR-002**: Each level MUST place between six and eight food items on a reachable counter, mixing healthy and less healthy choices.
- **FR-003**: Players MUST be able to grab, hold, move, and release food items using either hand.
- **FR-004**: Players MUST be able to place held food onto any free position on the plate, and the item MUST settle into a stable, non-overlapping arrangement.
- **FR-005**: Players MUST be able to remove a plated item and return it to the counter or place it elsewhere.
- **FR-006**: Each level MUST define a minimum number of items required before serving; the default is three for Levels 1 and 2 and four for Level 3.
- **FR-007**: The Serve action MUST be unavailable while the plate is below the minimum, and MUST explain the remaining requirement.
- **FR-008**: Serving MUST lock the plate and start the evaluation and report sequence.

**Health status bar**

- **FR-009**: A persistent Health Balance Bar MUST show a 0–100 value representing the overall balance of the current plate, where 0 is the least healthy composition and 100 is the most balanced. The value MUST combine the nutrition of the individual items with a bonus for meal composition and variety, so a one-note plate cannot score as balanced.
- **FR-010**: The bar MUST update as a preview every time an item is added to or removed from the plate.
- **FR-011**: The bar MUST display the numeric value, a color, and a text label. The ranges are 0–39 red ("Perlu perbaikan"), 40–69 yellow ("Cukup"), and 70–100 green ("Seimbang").
- **FR-012**: The bar MUST visually separate and count healthy choices versus less healthy choices for the current plate.
- **FR-013**: On serve, the bar MUST animate from its preview value to the final value in approximately one second, then lock.
- **FR-014**: The bar's color MUST never be the only signal; a label and number MUST always accompany it.

**Food information**

- **FR-015**: Every food item MUST have an information card containing: display name, healthy/less healthy classification, calories, protein, fiber, added sugar, sodium, portion description, and a one-sentence educational explanation.
- **FR-016**: A compact label MUST appear when the player focuses on an item, showing at minimum the name and classification.
- **FR-017**: The full information card MUST appear while the player holds an item, positioned so it does not cover the item or the plate.
- **FR-018**: Information MUST use supportive language and MUST NOT describe any single food as forbidden, toxic, or morally bad.
- **FR-019**: Nutrition values MUST be presented as approximate and sourced from public Indonesian food-composition references.

**Report and progression**

- **FR-020**: After serving, the game MUST show a report containing the final score, two strengths of the meal, one recommended improvement, and an assessment of three dimensions: energy, focus, and long-term health.
- **FR-021**: The report MUST state that the experience is educational and not medical advice.
- **FR-022**: The report MUST offer a Retry action that reloads the current level and a Next action that loads the following level, or a Recap action after Level 3.
- **FR-023**: Level results MUST be retained across the session and shown in the final recap. A retry MUST replace that level's earlier result, and the recap MUST use the latest attempt.
- **FR-024**: The final recap MUST summarize all three levels and offer a restart.
- **FR-025**: Starting a new session MUST clear previous results.

**Time and pacing**

- **FR-026**: Each level MUST have a visible countdown timer; the default duration is three minutes.
- **FR-027**: When the timer reaches zero, the current plate MUST be auto-served if it meets the minimum, otherwise the player MUST receive a short grace period to finish.
- **FR-028**: There MUST be no fail state; every valid plate produces a report.

**Presentation and comfort**

- **FR-029**: All player-facing text MUST be in Indonesian and legible from a standing position at typical headset distance.
- **FR-030**: The player viewpoint MUST remain stationary; the experience MUST NOT force camera translation or rotation.
- **FR-031**: Grab, place, serve, and bar changes MUST each have distinct audio or haptic feedback.
- **FR-032**: Food items MUST be recognizable three-dimensional models shown at believable real-world scale, with consistent orientation and pivot points.
- **FR-033**: The full three-level experience MUST be playable using a desktop XR Device Simulator without a physical headset. The simulator is the primary acceptance runtime for this version; physical-headset builds are optional and may be validated later.
- **FR-034**: Every food item MUST belong to at least one meal-composition category (vegetable, protein, carbohydrate, fruit, drink, or snack), and the bar MUST reward a plate that covers multiple categories.
- **FR-035**: A food's healthy/less healthy label MUST be derived from nutrition thresholds for fiber, protein, added sugar, sodium, and processing level, with an explicit documented override allowed per food. The information card and the bar MUST use the same resolved label.
- **FR-036**: On an empty plate, the bar MUST display 50 in a neutral gray state labeled "Belum dinilai". The first placed item MUST move the value up or down from 50 according to the meal's composition.

### Level Content

| Level | Setting | Learning focus | Food pool | Minimum items |
|-------|---------|----------------|-----------|---------------|
| 1 | Warteg, breakfast | Building a basic balanced plate | Rice, egg, tempeh, broccoli, banana, fried noodles, crackers | 3 |
| 2 | Padang restaurant, lunch | Portion, fat, and sodium | Rendang, fried chicken, cassava-leaf vegetables, egg, broccoli, crackers | 3 |
| 3 | Home, dinner | Everyday habits and sweet drinks | Noodles, burger, egg, tempeh, broccoli, banana, water, sweet tea | 4, including one drink |

Each level's pool MUST contain at least two clearly healthy options and at least two clearly less healthy options so that a meaningful choice always exists.

### Key Entities *(include if feature involves data)*

- **Food Item**: A single selectable food or drink. Key attributes: display name, resolved healthy/less healthy classification (threshold-derived with optional documented override), meal-composition category, calories, protein, fiber, added sugar, sodium, portion description, educational explanation, and a 3D representation.
- **Meal**: The set of items currently on the plate, the composition categories they cover, plus the derived healthy count, less healthy count, and balance value.
- **Health Balance Value**: A 0–100 measure of the current meal's overall nutritional balance, with color and label bands.
- **Information Card**: The name, classification, nutrition values, and explanation shown for a food item.
- **Level**: A self-contained eating situation with its own setting, food pool, minimum item count, time limit, and target learning focus.
- **Result Report**: The per-level outcome containing score, two strengths, one improvement, and the three health dimensions.
- **Session**: The player's run across all three levels, retaining each level result and powering the final recap.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A first-time player can complete Level 1 from entry to report in under five minutes without assistance.
- **SC-002**: At least 90% of testers can explain, in their own words, why the bar moved after adding or removing a food item.
- **SC-003**: A full three-level session takes between 8 and 12 minutes.
- **SC-004**: At least 90% of testers correctly classify two or more foods as healthy or less healthy after playing.
- **SC-005**: At least 90% of testers report no discomfort during a full session in the simulator.
- **SC-006**: All five testers can read the bar value, food name, and report text without leaning in.
- **SC-007**: A complete three-level session runs start to finish with zero blocking errors on the simulator.
- **SC-008**: Every food item in every level can be reached, grabbed, and plated by both left- and right-handed testers.
- **SC-009**: Every level can be completed with either a mostly healthy or mostly less healthy plate, and both routes produce a complete report.
- **SC-010**: All three level reports and the final recap retain correct scores after advancing between levels.

## Assumptions

- **Target platform**: The experience runs inside the existing Unity VR project. The Unity XR Device Simulator is the primary runtime and acceptance environment for this version. Physical-headset builds are optional and deferred until hardware is available, so headset-specific performance tuning is not a release gate for this course project.
- **Player posture**: Players remain seated or standing in one place; no artificial locomotion is required.
- **Level themes**: The three settings are warteg breakfast, Padang lunch, and home dinner, following the project's existing interaction plan. Level 3 includes one drink because drinks are part of everyday Indonesian meals.
- **Bar model**: The status bar is a single 0–100 balance value supplemented by separate healthy and less healthy item counts. It starts at 50 with a neutral "Belum dinilai" state on an empty plate. This satisfies both "track healthy and non-healthy choices" and the existing nutrition-score plan without adding a second competing meter.
- **Scoring basis**: Scores are driven by food classification plus fiber, protein, added sugar, sodium, and processing level, combined with a composition bonus for covering vegetable, protein, carbohydrate, fruit, and drink categories. Healthy/less healthy labels are threshold-derived with documented per-food overrides so the cards and the bar always agree. Exact weights are calibrated during development, not fixed by this specification.
- **Time limit**: The three-minute per-level default is a pacing choice; it can be tuned after playtesting without changing the experience design.
- **Assets**: Existing Blender food and environment models (broccoli, burger, noodles, banana, rendang, fried egg, egg, tempeh, fried chicken, plate, table, chair, stalls) are reused. Missing items are modeled in Blender to match their scale, pivot, and visual style.
- **Language**: Indonesian is the only supported language for this version.
- **Nutrition data**: Values are approximate and drawn from public sources such as Panganku/TKPI, Kemenkes, and WHO. The game is an educational course project, not a medical tool.
- **Session scope**: One player, no accounts, no multiplayer, no online services, and no persistent profile between separate launches.
- **Retry policy**: A retry reloads the current level and replaces that level's stored result. The final recap reflects the latest attempt, not the best attempt, so the recap always matches the last meal the player actually built.

### Out of Scope

- Calorie tracking over days or weeks.
- Medical diagnosis, personalized diet prescriptions, or allergen safety guarantees.
- Multiplayer, social features, or leaderboards.
- Hand tracking without controllers.
- Localization beyond Indonesian.
- Level editor or user-generated content.

## Dependencies

- The existing Unity VR project must compile and provide a working XR rig plus simulator.
- The three level environments and all food models must be available before content sign-off.
- Nutrition reference values must be reviewed against a public Indonesian food-composition table.
