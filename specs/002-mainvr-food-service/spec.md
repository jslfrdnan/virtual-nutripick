# Feature Specification: Scene 1 MainVR — Food Plating, Figma-Style Food Cards, and Final Result

**Feature Branch**: `002-mainvr-food-service`

**Created**: 2026-11-16

**Status**: Draft

**Input**: User description: "scene 1 yang bernama MainVR. buatkan agar makanan - makanannya tersaji dalam piring menggunakan object Assets/Models/Props/ceramic_plate.fbx. Untuk informasi makanannya gunakan UI Canvas jika hanya bisa dibuatkan manual infokan. Kita akan fokus membuatkan scene 1 dan interaksinya dan informasi makanannya menjadi bisa tetapi sederhana mungkin. Bangunlah agar mudah saya mereplikasinya di scene 2 dan scene 3 yang akan dibuatkan di sesi selanjutnya"

Additional design reference: [Game UI – Scene 1 & 2](https://www.figma.com/design/8YoNSUQoNotIqp9Y9eP7uK/Game-UI-%E2%80%93-Scene-1---2?node-id=0-1).

## Overview

Scene 1 (`MainVR`) becomes the reference implementation for the rest of NutriPick VR. The player starts from a simple start screen, enters the table scene, aims at food with a center reticle, picks food up with a gamepad, and places it onto a single ceramic serving plate. When enough food is on the plate, the player serves the meal and receives a final result.

The food information is presented through a UI Canvas that follows the public Figma board **"Game UI – Scene 1 & 2"**: a food card grid with food images, names, protein type, calories, and nutrition values; a **Status Makanan** bar; and a **Final Result** screen with balanced and less balanced panels. Only the public thumbnail of the Figma board is currently readable; exact text, spacing, and typography may be approximated until a higher-resolution export is provided.

The most important architectural outcome is reuse. The serving plate, food interaction, food card grid, Status Makanan bar, Final Result screen, and interaction wiring must be separable from the `MainVR` environment so Scenes 2 and 3 can reuse them by changing only their environment, food selection, plate placement, and level configuration.

## Clarifications

### Session 2026-11-16

- Q: How many foods should the plate require before serving, and how many placement positions should it have? → A: 3 foods required, 4 placement positions.
- Q: Which protein type categories should the food information card use? → A: Hewani and Nabati only; the card still shows calories, protein, carbohydrates, and fat.
- Q: Should Scene 1 keep the existing HUD/report or follow the Figma UI? → A: Follow the full Figma direction: start screen, food card grid, Status Makanan bar, and Final Result panels; use placeholders where exact assets, text, or fonts are unavailable.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Start, plate, and serve food in MainVR (Priority: P1)

A player opens Scene 1, sees a simple start screen, presses Start, and enters the table scene. The player sees eight food items and one ceramic serving plate, aims at a food item, picks it up, and places it onto the plate. The player can remove or replace food before serving. A Status Makanan bar updates as food is added or removed. Once at least three foods are on the plate, the player serves the meal; the plate locks and the existing report/fallback appears until the Figma Final Result is delivered in US3.

**Why this priority**: Starting, plating, and serving are the core interaction of Scene 1. Without them, the food information UI has no purpose and the level cannot be completed.

**Independent Test**: A tester can open `MainVR`, press Start, pick up three different foods, place them on the ceramic plate, serve, and confirm the plate locks and the existing report/fallback appears using only a gamepad.

**Acceptance Scenarios**:

1. **Given** Scene 1 is opened, **When** the start screen appears, **Then** the player can press Start to enter the table scene.
2. **Given** Scene 1 has started, **When** the player looks at the table, **Then** eight food items and one ceramic serving plate are visible at a believable dining scale and reachable.
3. **Given** the reticle is over a non-food prop, **When** the player presses interact, **Then** nothing is picked up and no placement prompt appears.
4. **Given** the reticle is over a food item, **When** the player presses the interact button, **Then** the food attaches to the carry position in front of the camera.
5. **Given** the player is carrying food and the reticle is over a free plate position, **When** the player presses interact, **Then** the food snaps onto the plate and remains stable.
6. **Given** the player is carrying food and the reticle is over a free plate position, **When** the position is targeted, **Then** that position visibly highlights.
7. **Given** the plate contains at least three foods, **When** the player presses Serve, **Then** the plate locks and the result sequence begins with the existing report/fallback.
8. **Given** the plate contains fewer than three foods, **When** the player presses Serve, **Then** serving is refused and the player is told how many more foods are needed.
9. **Given** a food is already on the plate, **When** the player aims at it and presses interact, **Then** the food can be picked up again and the plate updates.
10. **Given** the player is carrying food, **When** the player presses cancel, **Then** the food returns to its starting position without changing the plate.

---

### User Story 2 - Read food information in the Figma-style card grid (Priority: P2)

During Scene 1, the player sees a UI Canvas food card grid showing the available foods. Each card shows a food image, name, protein type, calories, protein, carbohydrates, and fat. Targeting a food highlights its card; picking it up or pressing Inspect focuses the card. A Status Makanan bar at the bottom shows whether the current plate is balanced or needs improvement.

**Why this priority**: The game is educational. The player must be able to understand what each food contributes before deciding what to plate, and the Figma-style cards are the main information surface.

**Independent Test**: A tester can open the food card grid, target each of the eight foods, read its protein type, calories, protein, carbohydrates, and fat, and verify that the Status Makanan bar changes when food is added or removed.

**Acceptance Scenarios**:

1. **Given** Scene 1 is running, **When** the food card grid is visible, **Then** it shows a card for each of the eight foods.
2. **Given** a food card is visible, **When** the player reads it, **Then** it shows the food image, name, protein type, calories, protein, carbohydrates, and fat.
3. **Given** the reticle rests on a food item, **When** the food is targeted, **Then** its card is highlighted or focused.
4. **Given** a food is picked up or inspected, **When** the focused card is shown, **Then** the card remains readable and does not cover the reticle target or the serving plate.
5. **Given** a food is added to or removed from the plate, **When** the plate changes, **Then** the Status Makanan bar updates to reflect the new balance.
6. **Given** a food has no image, protein type, or optional value, **When** its card is shown, **Then** the card displays a clear placeholder instead of a blank field or an error.
7. **Given** any food card is visible, **When** the player reads it, **Then** it uses supportive language and does not call the food forbidden, toxic, or morally bad.

---

### User Story 3 - See a Figma-style Final Result and reuse the setup later (Priority: P3)

After serving, the player sees a Final Result screen with a balanced panel and a less balanced panel, each with a short explanation and three feedback points. The player can retry the level or continue. The same plate, food cards, Status Makanan bar, and Final Result screen can be reused in Scenes 2 and 3 by changing only the environment, food selection, plate position, and level configuration.

**Why this priority**: The Final Result turns the meal choice into a learning moment, and reuse prevents three separate implementations of the same UI and interaction.

**Independent Test**: A tester can serve a meal, read both Final Result panels, choose Retry or Next, and then copy the reusable setup into a test scene with a different food selection and confirm that the flow still works.

**Acceptance Scenarios**:

1. **Given** the player serves a valid meal, **When** the Final Result screen appears, **Then** it shows a balanced panel and a less balanced panel.
2. **Given** the Final Result screen is visible, **When** the player reads each panel, **Then** each panel shows a status badge and three feedback points.
3. **Given** the Final Result screen is visible, **When** the player chooses Retry, **Then** Scene 1 reloads with an empty plate.
4. **Given** the Final Result screen is visible, **When** the player chooses Next, **Then** the next level loads.
5. **Given** the Scene 1 setup, **When** a designer copies the reusable plate, food cards, Status Makanan bar, Final Result screen, and interaction elements into a new scene, **Then** the copied setup works with a different food selection.
6. **Given** a new scene uses the reusable setup, **When** the designer changes only the environment, food selection, plate position, and level configuration, **Then** no interaction or card logic needs to be rewritten.
7. **Given** the existing Scenes 2 and 3, **When** the Scene 1 changes are applied, **Then** those scenes still open, load, and build without new blocking errors.
8. **Given** the delivery, **When** the designer opens the replication guide, **Then** it lists the exact reusable elements, required references, and manual UI Canvas steps needed to reproduce Scene 1 in another scene.

---

### Edge Cases

- **Start screen is skipped or dismissed**: The player enters the table scene without a broken state; Start remains the only required action.
- **Plate is full**: Placement is refused with a short prompt; the player can remove a food first.
- **Food is dropped outside the table**: The food returns to its starting position and remains recoverable.
- **Food falls through or leaves the play area**: The food is restored automatically; the player is never permanently stuck.
- **Serving with exactly three foods**: Serving is allowed.
- **Serving with four foods**: Serving is allowed; the plate is full.
- **Serving with fewer than three foods**: Serving is blocked and the remaining count is shown.
- **Player aims at the plate while empty-handed**: No placement occurs; the player sees a neutral prompt.
- **Player aims at food while already carrying food**: The game does not pick up a second item; the player is prompted to place or drop the carried item.
- **Reticle is over nothing**: No interaction prompt appears and pressing interact does nothing.
- **Non-food prop is targeted**: No prompt appears and pressing interact does nothing.
- **Gamepad is not recognized**: Keyboard and mouse remain usable for the full Scene 1 flow.
- **Player-facing text language**: All prompts, labels, and card text are in Indonesian.
- **Long food names or large values**: The card remains readable without text overflowing its panel.
- **Food image is missing**: The card shows a neutral placeholder image or icon.
- **Missing nutrition or protein type**: The card shows a clear placeholder such as "—" or "Tidak ada".
- **UI is viewed at 1920×1080**: All card, status, and result text remains legible without leaning in.
- **Status Makanan is at an extreme**: The bar shows a clear label and is not color-only.
- **Final Result is shown for a mostly healthy or mostly less healthy meal**: Both routes produce both panels with the correct highlighted outcome.
- **Gamepad disconnects**: Existing pause behavior preserves the current plate; reconnecting resumes play.
- **A copied scene has a missing reference**: The replication checklist includes a validation step that catches the missing reference before playtesting.

## Requirements *(mandatory)*

### Scene 1 Content

| Food | Protein type |
|------|--------------|
| Rendang | Hewani |
| Burger | Hewani |
| Boiled egg | Hewani |
| Fried egg | Hewani |
| Tempeh | Nabati |
| Broccoli | Nabati |
| Banana | Nabati |
| Fried noodles | Nabati |

The table defines the initial card classification. Burger and fried noodles follow their dominant protein ingredient; adjust the classification if the recipe changes. It can be adjusted without changing the interaction design.

### Functional Requirements

**Scene 1 content and presentation**

- **FR-001**: `MainVR` MUST be the only scene in scope for this feature and MUST be playable as Scene 1.
- **FR-002**: Scene 1 MUST contain exactly one serving plate, using the provided `Assets/Models/Props/ceramic_plate.fbx` model.
- **FR-003**: The serving plate MUST be scaled and positioned like a believable dining plate on the table, with a clear area where food can be placed.
- **FR-004**: The plate MUST provide four stable placement positions; placed foods MUST NOT overlap or float above the plate surface.
- **FR-005**: Scene 1 MUST present eight food items from the existing food set, and every food item MUST be reachable and interactable.
- **FR-006**: Non-food props in the scene MUST remain non-interactable.
- **FR-007**: Food and the plate MUST remain visible and stable during play; food MUST NOT fall through the table or disappear when placed.
- **FR-008**: Scene 1 MUST be playable with a gamepad and MUST retain keyboard and mouse as a fallback.

**Plating and serving interaction**

- **FR-009**: A center reticle MUST indicate what the player is aiming at.
- **FR-010**: When the reticle rests on an interactable food item, an interaction prompt MUST appear and the corresponding food card MUST be highlighted.
- **FR-011**: The player MUST be able to pick up a targeted food item, and the item MUST attach to a carry position in front of the camera.
- **FR-012**: While carrying food and aiming at a free plate position, the player MUST be able to place the food with the interact button; the food MUST snap into the position and the plate MUST update immediately.
- **FR-013**: While carrying food and aiming at a free plate position, that position MUST visibly highlight.
- **FR-014**: The player MUST be able to pick up a plated food item again, and the plate MUST update immediately.
- **FR-015**: While carrying food, the player MUST be able to drop or cancel the carried item; dropping MUST NOT change the plate.
- **FR-016**: A food that leaves the play area MUST return to its starting position and remain recoverable.
- **FR-017**: The Serve action MUST be available only when at least three foods are on the plate.
- **FR-018**: When serving is refused, the player MUST see a short Indonesian message explaining how many more foods are needed.
- **FR-019**: Serving MUST lock the plate and start the Figma-style Final Result sequence.
- **FR-020**: All interaction prompts and player-facing messages MUST be in Indonesian.

**Figma-style UI and food information**

- **FR-021**: Scene 1 MUST present a start screen with a Start action before gameplay begins. A `Profil Dev` action MAY be included as a simple optional information action.
- **FR-022**: During gameplay, the UI MUST show a food card grid containing a card for each of the eight foods.
- **FR-023**: Each food card MUST show a food image, food name, protein type, calories, protein, carbohydrates, and fat.
- **FR-024**: Protein type MUST use one of two values: `Hewani` or `Nabati`. Foods with no meaningful protein in later scenes MUST show a neutral placeholder rather than a third category.
- **FR-025**: Targeting a food item MUST highlight or focus its card; picking up or inspecting the food MUST keep the focused card readable.
- **FR-026**: Card values MUST come from the same food information used by the level logic; the card MUST NOT maintain a separate hardcoded copy.
- **FR-027**: The food card grid and focused card MUST be readable at 1920×1080 and MUST NOT cover the reticle target or the serving plate.
- **FR-028**: The food card UI MUST use supportive language and MUST NOT describe a food as forbidden, toxic, or morally bad.
- **FR-029**: If a food has missing optional information, its card MUST show a clear placeholder rather than an empty field or an error.
- **FR-030**: A Status Makanan bar MUST show the current meal balance with clear labels for balanced and needs-improvement states.
- **FR-031**: The Status Makanan bar MUST update when food is added to or removed from the plate.
- **FR-032**: The Status Makanan bar MUST NOT rely on color alone; a text label MUST accompany the color.
- **FR-033**: After serving, the UI MUST show a Final Result screen with a balanced panel and a less balanced panel.
- **FR-034**: Each Final Result panel MUST show a status badge and three feedback points.
- **FR-035**: The Final Result screen MUST provide `Ulangi Level` and `Next Level` actions.
- **FR-036**: The UI MUST follow the Figma board's visual direction: green for balanced or positive, red for less balanced or needs improvement, a food card grid, a bottom Status Makanan bar, and a two-panel Final Result.
- **FR-037**: The UI MUST be built with UI Canvas and MUST be reusable across scenes.
- **FR-038**: Exact Figma text, spacing, and fonts MAY be approximated until a higher-resolution export is provided; the functional layout and color semantics MUST match the Figma direction.

**Reuse and regression safety**

- **FR-039**: The serving plate, food interaction, food card grid, Status Makanan bar, Final Result screen, and interaction wiring MUST be reusable independently of the `MainVR` environment.
- **FR-040**: Scene-specific content — environment, food selection, plate position, and level configuration — MUST be configurable without changing interaction or UI behavior.
- **FR-041**: Copying the Scene 1 setup into a new scene with a different food selection MUST work without rewriting interaction or UI logic.
- **FR-042**: If the UI Canvas screens can only be created through manual assembly, the delivery MUST include a step-by-step guide with exact hierarchy, anchors, sizes, colors, and field assignments.
- **FR-043**: The Scene 1 changes MUST NOT break the existing Level 2 and Level 3 scenes, shared assets, or build settings.
- **FR-044**: The delivery MUST include a short replication checklist for building Scenes 2 and 3 from the Scene 1 setup.

### Key Entities *(include if feature involves data)*

- **Scene 1 Setup**: The reusable combination of one serving plate, eight food items, a serve interaction, a food card grid, a Status Makanan bar, a Final Result screen, and the table environment.
- **Serving Plate**: The ceramic plate model, its four placement positions, and its occupancy state.
- **Plate Position**: One of four stable places on the plate; it can be empty or occupied.
- **Food Item**: A selectable food with a display name, protein type (`Hewani` or `Nabati`), calories, protein, carbohydrates, fat, healthy/less healthy indicator, and a three-dimensional representation.
- **Food Card**: A UI card containing the food image, name, protein type, calories, protein, carbohydrates, and fat.
- **Status Makanan**: The meal balance indicator with a numeric or positional state, a color, and a text label.
- **Final Result**: The post-serve screen with a balanced panel, a less balanced panel, three feedback points per panel, status badges, and Retry/Next actions.
- **Interaction State**: Whether the player is empty-handed, carrying food, or aiming at a valid placement position.
- **Replication Template**: The reusable plate, food, card grid, Status Makanan bar, Final Result screen, and interaction setup plus the written checklist for applying it to another scene.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A first-time player can press Start, pick up three foods, place them on the plate, and serve in under three minutes using a gamepad.
- **SC-002**: All eight food items can be picked up, placed on the plate, removed, and served with zero unrecoverable failures.
- **SC-003**: Placed food remains stable for at least ten seconds without falling through the plate, floating, or overlapping another food.
- **SC-004**: The food card grid shows the correct image, name, protein type, calories, protein, carbohydrates, and fat for all eight foods, with 100% accuracy against the stored food data.
- **SC-005**: At least 90% of testers can read the food cards, Status Makanan bar, and Final Result text at 1920×1080 without leaning in or zooming.
- **SC-006**: A designer can replicate the Scene 1 setup into a copy scene by changing only the environment, food selection, plate position, and level configuration in under fifteen minutes, with no interaction or UI logic changes.
- **SC-007**: Serving is blocked 100% of the time with fewer than three foods and allowed 100% of the time with three or four foods.
- **SC-008**: A five-minute Scene 1 session completes with zero blocking errors.
- **SC-009**: A food dropped outside the play area returns to its starting position 100% of the time.
- **SC-010**: Existing Level 2 and Level 3 scenes still load and build without new blocking errors after the Scene 1 changes.
- **SC-011**: At least 90% of testers can correctly explain the protein type (`Hewani` or `Nabati`) of two foods after using the food cards.
- **SC-012**: The Start action enters the table scene in under five seconds.
- **SC-013**: The Status Makanan bar updates within 0.5 seconds of a food being added to or removed from the plate.
- **SC-014**: The Final Result screen shows both panels, three feedback points per panel, the correct highlighted outcome, and both Retry and Next actions.
- **SC-015**: At least 90% of testers can identify the balanced panel, the less balanced panel, and the next action without assistance.
- **SC-016**: A visual review confirms that the UI follows the Figma direction — green/red semantics, food card grid, bottom Status Makanan bar, and two-panel Final Result. Pixel-perfect fidelity is not required until a higher-resolution Figma export is provided.

## Assumptions

- `MainVR` is Scene 1. The existing table environment, player rig, and eight-food selection are reused.
- The serving plate uses `Assets/Models/Props/ceramic_plate.fbx` as the authoritative plate model.
- The plate has four placement positions and the level requires at least three foods before serving.
- The Figma board **"Game UI – Scene 1 & 2"** is the visual reference. Only the public thumbnail is currently readable; exact text, spacing, and fonts may be approximated until a higher-resolution export is provided.
- Food card images may use existing food icons or renders. If a food image is unavailable, the card uses a neutral placeholder rather than an empty area.
- Exact Figma assets and fonts may be unavailable; project-consistent substitutes are acceptable as long as the layout, green/red semantics, and information hierarchy match the Figma direction.
- The food card UI is presented through the project's existing UI Canvas. If manual scene assembly is required, a concise manual guide is an acceptable and required deliverable.
- The card is intentionally simple: image, name, protein type, calories, protein, carbohydrates, and fat. Fiber, added sugar, and sodium are not required on the card in this iteration.
- The existing level flow, timer, health bar, and end-of-level report may be reused as a base, but the visible HUD and result screen are adapted to the Figma direction: a food card grid, a Status Makanan bar, and a two-panel Final Result. The visible timer may be hidden in Scene 1.
- The start screen includes Start; `Profil Dev` may be a simple optional information action.
- The target display is 1920×1080 on Windows, played with a Rexus gamepad; keyboard and mouse remain available as a fallback.
- No VR headset or motion controller is required.
- Scenes 2 and 3 are out of scope for content creation, but their existing scenes and build entries must remain functional.
- Nutrition values and protein types come from the existing food data and may be refined later without changing the interaction design. Protein type uses two categories: `Hewani` and `Nabati`; foods with no meaningful protein in later scenes show a neutral placeholder.
- One player, no accounts, no multiplayer, and no persistent profile between launches.

### Out of Scope

- Building the full content for Scenes 2 and 3.
- Creating new three-dimensional food models or a new plate model.
- VR headset builds, hand tracking, and motion controllers.
- Advanced nutrition fields on the card such as fiber, added sugar, and sodium.
- Redesigning the underlying scoring algorithm, pause system, or recap beyond the Scene 1 Final Result screen.
- Multiplayer, leaderboards, accounts, or cloud persistence.
- Localization beyond Indonesian.

## Dependencies

- `Assets/Models/Props/ceramic_plate.fbx` must be available and importable.
- The existing eight food items, food images, and nutrition data must be available.
- The `MainVR` table environment and player rig must be usable.
- The existing HUD must provide, or allow the addition of, the UI Canvas start screen, food card grid, Status Makanan bar, and Final Result screen.
- The Figma board or a higher-resolution export must be available for exact visual review; the public thumbnail is sufficient for the initial direction.
- Unity 6000.6.0f1 must open the project without blocking compile errors.
- A Rexus gamepad must be recognized as a standard gamepad; keyboard and mouse are the fallback.
