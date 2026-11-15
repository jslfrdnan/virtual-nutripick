# Feature Specification: NutriPick VR — Healthy Food Selection Game

**Feature Branch**: `001-nutripick-vr-game`

**Created**: 2026-11-15

**Status**: Draft

**Input**: User description: "Build a virtual reality game using Unity engine. The game will consist of three levels. The user will select healthy and non-healthy food. There will be status bar to track the user choice in selecting healthy and non-healthy. For asset use blender to create asset, there is already existing asset, use it. Also add a information on the food." Additional decisions: play with a Rexus gamepad because no VR headset or VR controller is available; Level 3 is a bazaar with many food stands. Source document: *Dokumen Pengembangan Aplikasi Virtual Reality NutriPick Menggunakan Platform Unity untuk Mendukung Sustainable Development Goals: Zero Hunger dan Asta Cita*.

## Overview

NutriPick VR is an educational first-person 3D experience that teaches players how to build a balanced meal. The player walks or stands in front of food stands, aims a reticle at real-looking food, reads nutrition information, picks items up, and places them on a plate. A live **Health Balance Bar** shows how healthy the current selection is. After serving, the player receives a short, non-judgmental health report, then continues to the next of three everyday eating situations.

The current playable version targets **Windows with a Rexus gamepad**. No VR headset or VR motion controller is available, so the game uses a gamepad-first first-person control scheme. The design remains VR-style — immersive first-person view, real-world scale, spatial counters, and hands-free interaction replaced by a reticle and a gamepad — and can be extended to stereoscopic VR later. The experience supports SDG 2 (Zero Hunger) and Asta Cita by making nutrition education tangible, memorable, and free of risk.

## Clarifications

### Session 2026-11-15

- Q: How should the Health Balance Bar score a plate containing several foods — composition-aware or only individual item healthiness? → A: Composition-aware hybrid: combine each item's nutrition with a bonus for meal composition and variety.
- Q: How should each food's healthy or less-healthy label be determined for the information card and counts? → A: Rule-based from nutrition thresholds, with a documented per-food override for cultural dishes.
- Q: What should the Health Balance Bar display before any food is placed on the plate? → A: Centered 50 in a neutral gray state labeled "Belum dinilai"; the first item moves it up or down from the middle.
- Q: After a level report, should the player be able to retry that same level before moving on? → A: Yes. The report offers "Ulangi level" and "Lanjut"; a retry replaces that level's result, and the recap uses the latest attempt.

### Session 2026-11-16

- Q: Which runtime should the game primarily target for performance and device testing? → A: Windows desktop with a Rexus gamepad. No VR headset or VR controller is available; VR headset support is deferred. This supersedes the earlier XR Device Simulator decision.
- Q: What is the Level 3 setting? → A: A bazaar with many food stands. The player walks between stands to collect food, then returns to the plate to serve.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Build a meal with a gamepad (Priority: P1)

A player enters a virtual eating setting, moves with the left stick, looks with the right stick, and aims a center reticle at food on a counter or stand. Pressing the interact button picks up the food and carries it in front of the camera. Aiming at a free plate slot and pressing interact places it. The player can put items back and choose differently before serving. The level ends when the player serves the plate.

**Why this priority**: This is the core interaction. Without moving, aiming, picking up, and plating food, none of the educational feedback can happen. It alone delivers a playable demo.

**Independent Test**: A tester can enter Level 1 with a Rexus gamepad, pick up three items, place them on the plate, serve, and reach the result screen without using any other feature.

**Acceptance Scenarios**:

1. **Given** a player is in a level with food on the counter, **When** they move the left stick and right stick, **Then** the first-person camera moves and looks smoothly with collision against the environment.
2. **Given** the center reticle is over a food item, **When** the player presses the interact button, **Then** the item attaches to the carry position in front of the camera.
3. **Given** the player is carrying food and the reticle is over a free plate slot, **When** they press interact, **Then** the item snaps neatly onto the plate and stays there.
4. **Given** a plate with food on it, **When** the player aims at a plated item and presses interact, **Then** the item returns to the carry position and the meal updates.
5. **Given** the player is carrying food, **When** they press cancel, **Then** the food is dropped or returned to its stand without changing the plate.
6. **Given** a plate with fewer items than the level requires, **When** the player tries to serve, **Then** serving is unavailable and the player is told how many more items are needed.
7. **Given** a plate with the required number of items, **When** the player presses Serve, **Then** the plate is locked and the result sequence begins.

---

### User Story 2 - See whether choices are healthy in real time (Priority: P2)

While building a meal, the player watches a status bar on the screen that moves as food is added or removed. The bar clearly separates healthy choices from less healthy choices, shows the current balance as a number, and gives an immediate, easy-to-read verdict.

**Why this priority**: The bar is the main teaching device. It turns an invisible nutrition concept into instant visual cause and effect, and it is the feature the player remembers.

**Independent Test**: A tester can add a healthy item and see the bar rise, remove it and see the bar fall, with the number and color updating each time.

**Acceptance Scenarios**:

1. **Given** an empty plate, **When** the level starts, **Then** the bar shows 50 in a neutral gray state labeled "Belum dinilai".
2. **Given** a partially filled plate, **When** the player adds a healthy item, **Then** the bar moves toward the healthy end and the healthy-choice count increases.
3. **Given** a partially filled plate, **When** the player adds a less healthy item, **Then** the bar moves toward the less healthy end and the less-healthy count increases.
4. **Given** a plated item is removed, **When** the removal completes, **Then** the bar recalculates and animates to the new value.
5. **Given** the player serves the meal, **When** the final evaluation runs, **Then** the bar animates to its final value in about one second and locks.
6. **Given** the bar is at any value, **When** the player looks at it, **Then** the numeric value, color, and text label all agree and are readable on a 1080p screen.

---

### User Story 3 - Learn about each food item (Priority: P2)

The player aims at a food item and sees a compact label, then picks it up or presses Inspect to see a complete information card describing what it is and what it contributes nutritionally, including whether it is a healthy or less healthy choice and why.

**Why this priority**: The game must teach, not just score. Information is what turns a sorting exercise into nutrition education.

**Independent Test**: A tester can aim at any food item and read a complete, accurate information card without placing the item on the plate.

**Acceptance Scenarios**:

1. **Given** a food item on a counter or stand, **When** the reticle rests on it, **Then** a compact label appears with the food name, a healthy/less healthy indicator, and calories.
2. **Given** a targeted food item, **When** the player picks it up or presses Inspect, **Then** the full information card appears with calories, protein, fiber, sugar, sodium, portion, and a one-sentence explanation.
3. **Given** a full information card is visible, **When** the player drops the item or presses Cancel, **Then** the card dismisses without blocking the view.
4. **Given** any information card, **When** it is shown, **Then** it never labels a single food as "forbidden" or "bad", and it explains the contribution in context.

---

### User Story 4 - Progress through three levels and receive a recap (Priority: P3)

The player completes three levels, each set in a different everyday eating situation: a warteg breakfast, a Padang lunch, and a bazaar dinner with many stands. At the end, the player sees a summary of all three meals with one practical next step.

**Why this priority**: Progression gives the experience structure and the recap reinforces learning, but a single level already delivers the core educational value.

**Independent Test**: A tester can play Level 1, advance to Level 2, walk through the Level 3 bazaar, and reach the final recap with all three results retained.

**Acceptance Scenarios**:

1. **Given** the player finishes Level 1, **When** they choose Retry, **Then** Level 1 reloads and the new result replaces the earlier one.
2. **Given** the player finishes Level 1, **When** they press Next, **Then** Level 2 loads and the Level 1 result is retained.
3. **Given** the player finishes Level 2, **When** they press Next, **Then** Level 3 loads as a bazaar with multiple food stands.
4. **Given** the player is in the bazaar, **When** they move between stands, **Then** each stand presents its own food and the player can pick food from any stand.
5. **Given** the player has collected four items including a drink, **When** they return to the plate and serve, **Then** the Level 3 report appears.
6. **Given** the player finishes Level 3, **When** the report is dismissed, **Then** a recap shows all three level scores, two strengths, one improvement, and a closing educational message.
7. **Given** the player is on the final recap, **When** they choose to restart, **Then** the session returns to Level 1 with cleared progress.
8. **Given** the player quits mid-session, **When** they relaunch, **Then** the game starts cleanly at Level 1 without corrupted progress.

---

### Edge Cases

- **Empty plate served**: Serving must be blocked and the player told the minimum number of items.
- **Under-filled plate at time-out**: The current plate is auto-served if it meets the minimum; otherwise the player is returned to selection with a short grace period.
- **Removing every item**: The bar returns to 50 in the "Belum dinilai" state and the healthy/less healthy counts reset.
- **All healthy choices**: The report praises balance but notes that variety and portion still matter.
- **All less healthy choices**: The report stays supportive, names two things that went well elsewhere, and gives one concrete swap.
- **Food dropped outside the play area**: The item respawns at its stand so the player is never stuck.
- **Player carries food away from the plate**: The food stays in the carry position; the player can return and place it.
- **Plate is full**: Placement is refused with a short prompt; the player can remove an item first.
- **Reticle over nothing**: No interaction prompt appears; pressing interact does nothing.
- **Gamepad disconnects**: The game pauses and shows "Controller terputus"; reconnecting resumes without losing the plate.
- **Gamepad not recognized**: The game falls back to keyboard and mouse and tells the player.
- **Time expires while carrying food**: The held item is returned to its stand before the result sequence.
- **Player cannot reach a stand**: Every stand has a clear approach path; the reticle range is long enough to target food from the walkway.
- **Text too small or low contrast**: Information must remain legible on a 1920×1080 screen at normal viewing distance.
- **No headset available**: The full experience must be playable with a Rexus gamepad and no VR hardware.

## Requirements *(mandatory)*

### Functional Requirements

**Core loop and input**

- **FR-001**: The experience MUST present exactly three levels, each set in a distinct everyday eating situation. Level 3 MUST be a bazaar with multiple food stands.
- **FR-002**: Each level MUST present between six and eight food items, mixing healthy and less healthy choices. Level 3 MUST distribute its food across at least three stands.
- **FR-003**: Players MUST be able to move with the left stick and look with the right stick. Camera movement MUST collide with the environment and MUST NOT be forced or scripted.
- **FR-004**: A center reticle MUST show what the player is aiming at. When the reticle is over a food item, a compact label MUST appear. When it is over nothing, no interaction prompt may appear.
- **FR-005**: Players MUST be able to pick up a targeted food item with the interact button, and the item MUST attach to a carry position in front of the camera.
- **FR-006**: While carrying food and aiming at a free plate slot, the player MUST be able to place the item with the interact button. The item MUST snap into a stable, non-overlapping position.
- **FR-007**: While carrying food, the player MUST be able to drop or return it with the cancel button. Dropping MUST NOT change the plate.
- **FR-008**: Players MUST be able to pick up a plated item again by aiming at it and pressing interact. The plate MUST update immediately.
- **FR-009**: Each level MUST define a minimum number of items required before serving: three for Levels 1 and 2, and four for Level 3.
- **FR-010**: Level 3 MUST require at least one drink. Serving MUST be blocked until a drink is on the plate.
- **FR-011**: The Serve action MUST be unavailable while the plate is below the minimum, and MUST explain the remaining requirement.
- **FR-012**: Serving MUST lock the plate and start the evaluation and report sequence.
- **FR-013**: The player MUST be able to pause and resume with the gamepad. A disconnected gamepad MUST pause the game and preserve the current plate.

**Health status bar**

- **FR-014**: A persistent Health Balance Bar MUST show a 0–100 value representing the overall balance of the current plate, where 0 is the least healthy composition and 100 is the most balanced. The value MUST combine individual nutrition with a bonus for meal composition and variety.
- **FR-015**: The bar MUST update as a preview every time an item is added to or removed from the plate.
- **FR-016**: The bar MUST display the numeric value, a color, and a text label. The ranges are 0–39 red ("Perlu perbaikan"), 40–69 yellow ("Cukup"), and 70–100 green ("Seimbang").
- **FR-017**: The bar MUST visually separate and count healthy choices versus less healthy choices for the current plate.
- **FR-018**: On serve, the bar MUST animate from its preview value to the final value in approximately one second, then lock.
- **FR-019**: The bar's color MUST never be the only signal; a label and number MUST always accompany it.

**Food information**

- **FR-020**: Every food item MUST have an information card containing: display name, healthy/less healthy classification, calories, protein, fiber, added sugar, sodium, portion description, and a one-sentence educational explanation.
- **FR-021**: A compact label MUST appear when the reticle rests on an item, showing at minimum the name, classification, and calories.
- **FR-022**: The full information card MUST appear when the player picks up an item or presses the Inspect button, positioned so it does not cover the plate or the reticle target.
- **FR-023**: Information MUST use supportive language and MUST NOT describe any single food as forbidden, toxic, or morally bad.
- **FR-024**: Nutrition values MUST be presented as approximate and sourced from public Indonesian food-composition references.

**Report and progression**

- **FR-025**: After serving, the game MUST show a report containing the final score, two strengths of the meal, one recommended improvement, and an assessment of three dimensions: energy, focus, and long-term health.
- **FR-026**: The report MUST state that the experience is educational and not medical advice.
- **FR-027**: The report MUST offer a Retry action that reloads the current level and a Next action that loads the following level, or a Recap action after Level 3.
- **FR-028**: Level results MUST be retained across the session and shown in the final recap. A retry MUST replace that level's earlier result, and the recap MUST use the latest attempt.
- **FR-029**: The final recap MUST summarize all three levels and offer a restart.
- **FR-030**: Starting a new session MUST clear previous results.

**Time and pacing**

- **FR-031**: Each level MUST have a visible countdown timer; the default duration is three minutes.
- **FR-032**: When the timer reaches zero, the current plate MUST be auto-served if it meets the minimum, otherwise the player MUST receive a short grace period to finish.
- **FR-033**: There MUST be no fail state; every valid plate produces a report.

**Presentation, input, and accessibility**

- **FR-034**: All player-facing text MUST be in Indonesian and legible on a 1920×1080 display at normal viewing distance.
- **FR-035**: The full three-level experience MUST be playable with a Rexus gamepad on Windows. Keyboard and mouse MUST be available as a fallback if the gamepad is not recognized.
- **FR-036**: Pick up, place, serve, and bar changes MUST each have distinct audio feedback and, where supported, gamepad rumble.
- **FR-037**: Food items MUST be recognizable three-dimensional models shown at believable real-world scale, with consistent orientation and pivot points.
- **FR-038**: The Level 3 bazaar MUST have at least three distinct stands with clear signage, a walkable path between them, and a drink stand.
- **FR-039**: The player MUST be able to reach every stand and target its food with the reticle from the walkway.
- **FR-040**: All report and recap buttons MUST be navigable with the D-pad or left stick and confirmed with the interact button.

### Level Content

| Level | Setting | Learning focus | Food pool | Minimum items |
|-------|---------|----------------|-----------|---------------|
| 1 | Warteg, breakfast | Building a basic balanced plate | Rice, boiled egg, fried egg, tempeh, broccoli, banana, fried noodles, crackers | 3 |
| 2 | Padang restaurant, lunch | Portion, fat, and sodium | Rice, rendang, fried chicken, cassava-leaf vegetables, boiled egg, broccoli, crackers | 3 |
| 3 | Bazaar, dinner — multiple stands | Everyday habits, variety, and sweet drinks | Fried noodles, burger, boiled egg, tempeh, broccoli, banana, water, sweet tea | 4, including one drink |

Each level's pool MUST contain at least two clearly healthy options and at least two clearly less healthy options so that a meaningful choice always exists.

### Level 3 Stand Layout

| Stand | Foods |
|-------|-------|
| Mie & Jajanan | Fried noodles, burger |
| Lauk | Boiled egg, tempeh |
| Sayur & Buah | Broccoli, banana |
| Minuman | Water, sweet tea |

The plate sits on a central table in the bazaar. The player walks between stands, picks food, and returns to the plate to serve. All stands are within a short walk and are clearly signposted.

### Key Entities *(include if feature involves data)*

- **Food Item**: A single selectable food or drink. Key attributes: display name, resolved healthy/less healthy classification (threshold-derived with optional documented override), meal-composition category, calories, protein, fiber, added sugar, sodium, portion description, educational explanation, and a 3D representation.
- **Meal**: The set of items currently on the plate, the composition categories they cover, plus the derived healthy count, less healthy count, and balance value.
- **Health Balance Value**: A 0–100 measure of the current meal's overall nutritional balance, with color and label bands.
- **Information Card**: The name, classification, nutrition values, and explanation shown for a food item.
- **Level**: A self-contained eating situation with its own setting, food pool, minimum item count, time limit, and target learning focus.
- **Food Stand**: A physical counter in a level with a name, a set of foods, and one or more placement anchors. Levels 1 and 2 have one stand; Level 3 has at least three.
- **Result Report**: The per-level outcome containing score, two strengths, one improvement, and the three health dimensions.
- **Session**: The player's run across all three levels, retaining each level result and powering the final recap.
- **Carry State**: Whether the player is empty-handed, carrying a food item, or aiming at a valid placement target.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A first-time player can complete Level 1 from entry to report in under five minutes using a Rexus gamepad, without assistance.
- **SC-002**: At least 90% of testers can explain, in their own words, why the bar moved after adding or removing a food item.
- **SC-003**: A full three-level session takes between 8 and 12 minutes.
- **SC-004**: At least 90% of testers correctly classify two or more foods as healthy or less healthy after playing.
- **SC-005**: At least 90% of testers report no discomfort during a full session.
- **SC-006**: All five testers can read the bar value, food name, and report text on a 1920×1080 display without leaning in.
- **SC-007**: A complete three-level session runs start to finish with zero blocking errors using the Rexus gamepad.
- **SC-008**: Every food item in every level can be reached, targeted, picked up, and plated with the gamepad.
- **SC-009**: Every level can be completed with either a mostly healthy or mostly less healthy plate, and both routes produce a complete report.
- **SC-010**: All three level reports and the final recap retain correct scores after advancing between levels.
- **SC-011**: In Level 3, the player can visit every stand, collect four items including a drink, and reach the report.
- **SC-012**: Disconnecting and reconnecting the gamepad mid-level pauses and resumes the game without losing the current plate.
- **SC-013**: If no gamepad is recognized, the full experience remains playable with keyboard and mouse.

## Assumptions

- **Target platform**: Windows PC at 1920×1080, played with a Rexus gamepad. The current version does not use a VR headset or VR motion controllers.
- **Input model**: Left stick moves, right stick looks, interact picks up/places, cancel drops, Inspect opens the full card, Start serves, Select pauses. The mapping is configurable through Unity's Input System; the Rexus controller is treated as a standard XInput gamepad.
- **VR scope**: Stereoscopic VR, hand tracking, and motion-controller support are deferred. The game keeps a first-person, real-world-scale design so a VR version can be added later.
- **Level themes**: Warteg breakfast, Padang lunch, and bazaar dinner. Level 3 has four stands and a central serving table.
- **Bar model**: A single 0–100 balance value supplemented by separate healthy and less healthy item counts. It starts at 50 with a neutral "Belum dinilai" state on an empty plate.
- **Scoring basis**: Scores combine item nutrition with a composition bonus for covering vegetable, protein, carbohydrate, fruit, and drink categories. Healthy/less healthy labels are threshold-derived with documented per-food overrides so the cards and the bar always agree. Exact weights are calibrated during development.
- **Time limit**: Three minutes per level is a pacing choice; it can be tuned after playtesting without changing the experience design.
- **Assets**: Existing Blender food and environment models (broccoli, burger, noodles, banana, rendang, fried egg, egg, tempeh, fried chicken, plate, table, chair, stalls) are reused. The asset team supplies the five missing foods and the bazaar stand/canopy pieces; this spec lists them and their integration requirements.
- **Language**: Indonesian is the only supported language for this version.
- **Nutrition data**: Values are approximate and drawn from public sources such as Panganku/TKPI, Kemenkes, and WHO. The game is an educational course project, not a medical tool.
- **Session scope**: One player, no accounts, no multiplayer, no online services, and no persistent profile between separate launches.
- **Retry policy**: A retry reloads the current level and replaces that level's stored result. The final recap reflects the latest attempt, not the best attempt.

### Out of Scope

- Stereoscopic VR rendering, VR headset builds, hand tracking, and motion controllers.
- Calorie tracking over days or weeks.
- Medical diagnosis, personalized diet prescriptions, or allergen safety guarantees.
- Multiplayer, social features, or leaderboards.
- Localization beyond Indonesian.
- Level editor or user-generated content.

## Dependencies

- A Rexus gamepad recognized by Unity's Input System as a standard gamepad/XInput device. If it is not recognized, keyboard and mouse are the fallback.
- The existing Unity project must compile and provide a working first-person player rig.
- The three level environments and all food models must be available before content sign-off.
- Nutrition reference values must be reviewed against a public Indonesian food-composition table.
- The asset team supplies the missing assets listed below. Blender 4.x is their authoring tool; the game runtime does not require Blender.

### Asset Handoff (Team-Owned)

The asset team supplies:

- `nasi_putih` — a bowl/portion of white rice.
- `kerupuk` — a small pile of crackers (use `Kerupuk.jpg`).
- `sayur_singkong` — cassava-leaf vegetables (use `Sayursingkong1.jpg`).
- `air_putih` — a clear water glass.
- `teh_manis` — a glass of sweet tea.
- `bazaar_stand` — a modular market stand with a counter, posts, and canopy.
- `bazaar_canopy` — a cloth canopy/tent and banner piece.

Integration requirements: FBX, 1 Unity unit = 1 meter, pivot at the base center, URP/Lit materials, ≤5k triangles per food, ≤3k triangles per stand, textures ≤1024 px, no animation/cameras/lights in food exports. The implementation uses placeholder prefabs with the same `FoodData` and prefab contract until these assets are delivered.
