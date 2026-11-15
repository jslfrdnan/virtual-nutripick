# Phase 0 Research: NutriPick VR

**Feature**: `001-nutripick-vr-game` | **Date**: 2026-11-16

This document records the technical decisions that shape the plan. Each decision states what was chosen, why, and what was rejected.

## Decision 1 — Runtime and input model

**Chosen**: Windows desktop, gamepad-first, played with a Rexus gamepad. A standard `CharacterController` first-person rig with Unity Input System bindings. No VR headset, no motion controller, and no XR Interaction Toolkit runtime.

**Rationale**:

- No VR headset or VR controller is available. The user explicitly chose a gamepad-only build.
- A first-person `CharacterController` is simpler, more reliable, and easier to test than an XR rig without hardware.
- Unity Input System 1.20.0 is already installed and supports XInput gamepads, including the Rexus controller.
- The game keeps a first-person, real-world-scale design so a VR version can be added later, but VR is out of scope for this version.

**Default gamepad mapping**:

| Input | Action |
|-------|--------|
| Left stick | Move |
| Right stick | Look |
| A / South | Interact: pick up, place, confirm |
| B / East | Cancel: drop carried food, close card |
| X / West | Inspect: open the full food card |
| Start | Serve |
| Select | Pause/resume |
| D-pad / left stick | Navigate report and recap buttons |
| Mouse + WASD | Fallback when no gamepad is detected |

**Alternatives considered**:

- **XR Device Simulator with keyboard/mouse**: Rejected. The user wants the Rexus gamepad, and the simulator does not map naturally to a gamepad.
- **XR hand grab**: Rejected. No headset or motion controller is available.
- **Gamepad-only reticle** (chosen): Works on any Windows PC and keeps the first-person immersion.

**Implementation notes**:

- Create `Assets/Input/NutriPick.inputactions` with `Gameplay` and `Menu` action maps.
- Use `InputSystemUIInputModule` on the EventSystem so UGUI buttons work with the gamepad.
- Detect the gamepad with `Gamepad.current`; if null, enable the keyboard/mouse fallback and show a notice.
- Replace the broken `RexusInputProbe.cs` with `GamepadInput.cs` and a small input debug overlay in the editor.
- Verify the exact Rexus model's button labels in M0; the Input System treats it as a standard gamepad.

## Decision 2 — Scene and session architecture

**Chosen**: One scene per level plus one persistent `GameSession` created on demand with `DontDestroyOnLoad`. No boot scene. The final recap is a panel inside Level 3, not a fourth scene.

**Rationale**:

- One scene per level is simple for a group project: one owner per scene, fewer merge conflicts.
- A boot scene adds a loading step and an extra failure point for a short course project.
- The recap is a UI state after the Level 3 report; a separate scene would duplicate the report presentation.

**Scene names** (must be in Build Settings, in this order):

1. `Assets/Scenes/Level01_Warteg.unity`
2. `Assets/Scenes/Level02_Padang.unity`
3. `Assets/Scenes/Level03_Bazaar.unity`

`Assets/Scenes/MainVR.unity` stays a sandbox and is not in Build Settings.

## Decision 3 — Interaction model

**Chosen**: Center-reticle targeting with a pick/carry/place interaction. No physics while the food is carried. Custom plate slots for placement. A `ServeStation` in the world plus the Start button for serving.

**Rationale**:

- A reticle is the most legible gamepad interaction: the player always knows what is targeted.
- Carrying the food in front of the camera avoids physics instability and makes placement predictable.
- Custom slots avoid overlapping food and make serving reliable.
- A world-space `ServeStation` gives the player a physical place to serve; Start is a shortcut.

**Interaction states**:

```text
Idle → HoveringFood → Carrying → HoveringSlot → Placing → Idle
Carrying → Dropping → Idle
Idle → Serving → Report
```

**Rules**:

- The reticle raycasts from the camera center up to 3.0 m, against `Food`, `PlateSlot`, `ServeStation`, and `Environment` layers.
- Hovering a food shows the compact card; pressing A picks it up.
- While carrying, the item follows a `HoldAnchor` at 0.5 m forward and 0.15 m down from the camera, with a small bob.
- While carrying, aiming at a free plate slot highlights it; pressing A places the food.
- Pressing B drops the food; if it is over a stand, it returns to its anchor; otherwise it falls and returns home after 1 s.
- Aiming at a plated item and pressing A picks it up again.
- Aiming at the `ServeStation` and pressing A, or pressing Start, attempts to serve.
- Aiming at a food and pressing X opens the full card without picking it up.

**Alternatives considered**:

- **Physics-based carry**: Rejected. Food jitters and can fall through the plate.
- **Drag-and-drop with the mouse**: Rejected. The user wants a gamepad.
- **Ray from the gamepad instead of the camera**: Rejected. The reticle is more stable and readable.

## Decision 4 — Healthy/less healthy classification

**Chosen**: A rule-based classifier with a documented per-food override.

**Rules** (evaluated in order):

1. If `healthyOverride` is `ForceHealthy`, return Healthy.
2. If `healthyOverride` is `ForceLessHealthy`, return LessHealthy.
3. Return LessHealthy if any of these is true:
   - added sugar > 10 g per portion
   - sodium > 500 mg per portion
   - processing level is UltraProcessed
   - the food is fried
   - the food is a snack and not also a vegetable or fruit
   - the food is a drink with added sugar > 5 g
4. Otherwise return Healthy.

**Rationale**:

- The rules are explainable to the player and consistent between the information card and the bar counts.
- The override handles cultural dishes where a strict threshold would be misleading.
- The rules are data-driven and can be tuned without code changes.

**Alternatives considered**:

- **Manual label only**: Rejected. It drifts between foods and makes the bar contradict the cards.
- **Per-item nutrition score only**: Rejected. The player asked for a clear healthy/less healthy distinction.

## Decision 5 — Meal scoring model

**Chosen**: Composition-aware hybrid. The plate score combines the average item health value with a composition score that rewards covering carbohydrate, protein, and vegetable, plus variety across the five meal categories.

### Item health value

```text
h = clamp(
    0.50
  + 0.15 * clamp01(fiber / 8 g)
  + 0.15 * clamp01(protein / 20 g)
  - 0.20 * clamp01(addedSugar / 15 g)
  - 0.15 * clamp01(sodium / 600 mg)
  - 0.10 * processingPenalty
  - 0.05 * friedPenalty
  + 0.15 * unsweetenedDrinkBonus,
  0.05, 0.95)
```

- `processingPenalty`: Minimal = 0, Processed = 0.5, UltraProcessed = 1.0.
- `friedPenalty`: 1 if fried, otherwise 0.
- `unsweetenedDrinkBonus`: 1 if the item is a drink with 0 g added sugar, otherwise 0.

### Plate composition

```text
coreCovered = number of core categories present among {carbohydrate, protein, vegetable}
distinctCategories = number of meal categories present among {vegetable, protein, carbohydrate, fruit, drink}
composition = clamp01(
    0.70 * (coreCovered / 3)
  + 0.30 * (distinctCategories / 5)
  - 0.15 * (3 - coreCovered)
)
```

`Snack` is a classification category, not a variety category; it does not earn a composition bonus.

### Final score

```text
score = round(100 * clamp01(0.50 * averageItemHealth + 0.50 * composition))
```

- Empty plate: display **50 neutral**, label **"Belum dinilai"**, not a scored result.
- Bands: 0–39 red **"Perlu perbaikan"**, 40–69 yellow **"Cukup"**, 70–100 green **"Seimbang"**.
- All weights, thresholds, and bands live in the `NutritionRules` ScriptableObject.

### Worked examples

| Plate | Average item health | Composition | Score | Band |
|-------|--------------------:|------------:|------:|------|
| Empty plate | — | — | 50 | Neutral, "Belum dinilai" |
| Rice + boiled egg + tempeh + broccoli | 0.537 | 0.88 | 71 | Seimbang |
| Rice + boiled egg + tempeh + broccoli + banana | 0.543 | 0.94 | 72 | Seimbang |
| Fried egg + fried noodles + crackers | 0.334 | 0.437 | 39 | Perlu perbaikan |
| Three bananas | 0.568 | 0.00 | 28 | Perlu perbaikan |
| Rice + rendang + cassava leaves + broccoli | 0.495 | 0.88 | 69 | Cukup |
| Rice + boiled egg + cassava leaves + broccoli | 0.527 | 0.88 | 70 | Seimbang |
| Noodles + boiled egg + tempeh + broccoli + water | 0.503 | 0.94 | 72 | Seimbang |
| Noodles + boiled egg + tempeh + broccoli + sweet tea | 0.423 | 0.94 | 68 | Cukup |

Scores are rounded; tests should assert ranges (for example, balanced ≥70, fried <50, three bananas <40), except the empty-plate value, which is exactly 50.

**Rationale**:

- The composition half of the formula prevents a one-note plate (three bananas) from scoring as balanced.
- The item half rewards fiber and protein and penalizes added sugar, sodium, frying, and ultra-processing.
- The formula is a pure function, so it is fully testable in EditMode.
- The weights are a starting hypothesis, not a nutritional truth; the catalog and weights are reviewed before the demo.

**Alternatives considered**:

- **Average item health only**: Rejected. It lets a fruit-only plate score well.
- **Healthy-item ratio only**: Rejected. It ignores nutrition detail and makes the bar jump in large steps.
- **Hard composition requirement**: Rejected as the only rule. It would block a valid but imperfect plate; the bonus is a gentler teaching signal.

## Decision 6 — UI approach

**Chosen**: Screen-space HUD with UGUI and TextMeshPro. A center reticle, compact food label, full food card, bar, timer, interaction prompt, report panel, and recap panel. All report and recap buttons are UGUI `Selectable`s navigated with the gamepad.

**Rationale**:

- A screen-space HUD is the most readable option at 1920×1080 and is independent of camera movement.
- UGUI supports gamepad navigation through the EventSystem and `InputSystemUIInputModule`.
- The reticle is the interaction anchor; the HUD does not need world-space canvases.
- A full-screen report is easier to read than a world-space panel on a monitor.

**Layout** (1920×1080 reference):

| Element | Anchor | Notes |
|---------|--------|-------|
| Health Balance Bar | Bottom center | 720 × 80 px, above the prompt |
| Timer | Top center | 220 × 60 px |
| Reticle | Center | 24 px dot with a subtle ring |
| Interaction prompt | Center, below reticle | "A: Ambil", "A: Taruh", "B: Lepas" |
| Compact food label | Right of reticle | Name, badge, calories |
| Full food card | Left side | 420 × 560 px |
| Report panel | Full screen | 1200 × 760 px, centered |
| Recap panel | Full screen | 1200 × 760 px, centered |

**Alternatives considered**:

- **World-space UI**: Rejected. It is harder to read on a monitor and requires camera-relative placement.
- **IMGUI debug UI**: Rejected. Not suitable for a polished demo.

## Decision 7 — Asset pipeline

**Chosen**: Reuse the existing FBX assets. The asset team supplies five missing foods and modular bazaar stands; this plan defines the integration contract and uses placeholder prefabs until delivery.

**Existing assets to reuse**:

- Foods: `brokoli.fbx`, `BURGER.fbx`, `mie.fbx`, `pisang.fbx`, `rendang.fbx`, `telur ceplok.fbx`, `telur.fbx`, `tempe.fbx`, `yamgoreng.fbx`, and the banana OBJ.
- Props: `ceramic_plate.fbx`, `wooden_table.fbx`, `wooden_chair.fbx`, `display_counter.fbx`, `warteg_stall.fbx`, `Wrng_padangggg.fbx`.
- Textures: `Rendang.jpg`, `Sayursingkong1.jpg`, `Kerupuk.jpg`, `PadangPNG.png`, wood, wall, and tile textures.

**Team-owned assets to be supplied**:

1. `nasi_putih` — a bowl/portion of white rice.
2. `kerupuk` — a small pile of crackers (use `Kerupuk.jpg`).
3. `sayur_singkong` — cassava-leaf vegetable (use `Sayursingkong1.jpg`).
4. `air_putih` — a clear water glass.
5. `teh_manis` — a glass of sweet tea.
6. `bazaar_stand` — a modular market stand with a counter, posts, and a canopy.
7. `bazaar_canopy` — a cloth canopy/tent piece and banner, reusable across stands.

The implementation uses placeholder prefabs with the same `FoodData` and prefab contract until these assets are delivered.

**Level 3 bazaar layout**:

- A 12 × 12 m open market area with a central serving table.
- Four stands arranged around the walkway:
  - Mie & Jajanan: fried noodles, burger.
  - Lauk: boiled egg, tempeh.
  - Sayur & Buah: broccoli, banana.
  - Minuman: water, sweet tea.
- Each stand has a sign, a counter, and two to three food anchors.
- Reuse `warteg_stall.fbx` and `Wrng_padangggg.fbx` as the two larger stands; use the new modular stand for the other two.
- Add string lights and banners for atmosphere, but keep draw calls low.

**Handoff requirements for the asset team** (Blender 4.x, FBX 7.4 binary):
- Apply transforms; 1 Unity unit = 1 meter; forward −Z; up Y.
- Triangulate meshes; no animation, cameras, or lights in food exports.
- Pivot at the base center for food and drinks so they sit naturally on a plate or counter.
- Real-world sizes: plate diameter 0.26 m; rice portion 0.12 m wide; glass 0.08 m diameter × 0.12 m tall; cracker pile 0.10 m wide; stand counter 1.2 m wide × 0.9 m tall.
- Materials: URP/Lit; texture size ≤1024 px.

**Mesh budget**:

- Each food ≤5k triangles; heavy imports (rendang 10 MB, fried chicken 5.4 MB, burger 3.4 MB) must be audited and decimated.
- Add one LOD level (50%) for any food above 3k triangles.
- Colliders: primitive Box/Sphere/Capsule on the prefab; never a non-convex MeshCollider on a Rigidbody.
- Bazaar stands: ≤3k triangles each; reuse materials and textures.

**Rationale**:

- The existing assets already cover most foods and all major props.
- The asset team owns the five missing foods and the modular bazaar; this plan defines the integration contract so delivery is predictable.
- A strict scale/pivot contract prevents the common "food floats above the plate" bug.
- Placeholder prefabs keep M2 and M3 moving while the team finishes the art.

**Alternatives considered**:

- **Primitives for all missing foods**: Rejected as the final art. Placeholder primitives are allowed only to unblock M2/M3.
- **Asset Store packs**: Rejected. The user asked for Blender-created assets, and external packs add style and license inconsistency.
- **Home dining room**: Rejected. The user changed Level 3 to a bazaar.

## Decision 8 — Audio and gamepad rumble

**Chosen**: Small generated/placeholder SFX clips plus gamepad rumble.

**Event map**:

| Event | Audio | Rumble |
|-------|-------|--------|
| Hover food | Soft tick | None |
| Pick up | Cloth/wood pickup | 0.4 amplitude, 0.2 s |
| Place on plate | Ceramic tap | 0.3 amplitude, 0.1 s |
| Drop/return | Soft lift | 0.2 amplitude, 0.08 s |
| Serve | Whoosh + confirmation | 0.5 amplitude, 0.15 s |
| Bar milestone (40/70) | Light chime | None |
| Report reveal | Page/soft chime | None |

**Rationale**:

- The project has no audio assets. Generated placeholder WAVs (short sine/noise envelopes) are enough for a course demo and avoid licensing risk.
- Gamepad rumble uses the Input System's `Gamepad.SetMotorSpeeds`; if the Rexus controller does not support rumble, the call no-ops.
- No VR haptics are needed because there is no VR controller.

**Alternatives considered**:

- **No audio**: Rejected. The spec requires distinct feedback for pick, place, serve, and bar changes.
- **Downloaded SFX pack**: Allowed later, but not required; licensing must be documented.

## Decision 9 — Testing strategy

**Chosen**: EditMode tests for pure logic, plus a manual PlayMode acceptance checklist with the Rexus gamepad.

**Automated coverage**:

- `FoodClassifierTests`: thresholds, overrides, boundary values (10 g sugar, 500 mg sodium, ultra-processed, fried).
- `MealEvaluatorTests`: empty plate 50, balanced ≥70, fried <50, three bananas <40, clamp 0–100, composition coverage, drink requirement, deterministic output.
- `GameSessionTests`: begin/reset, record/replace, result retention, latest-attempt recap.
- `LevelDefinitionTests`: 6–8 foods, at least two healthy and two less healthy, drink requirement, stand count, scene name present.

**Manual coverage** (in `quickstart.md`):

- Gamepad detection, movement, look, reticle, pick, carry, place, drop, inspect, serve, retry, next, recap, time-out, lost item, bazaar stands, drink requirement, text legibility, comfort.

**Rationale**:

- The pure logic is where the educational rules live, so it gets automated coverage.
- The manual checklist maps directly to the spec's acceptance scenarios.
- Gamepad interaction is best verified by a human because device mappings vary.

**Alternatives considered**:

- **Automated PlayMode tests with a virtual gamepad**: Deferred. High setup cost for a course project; the manual checklist is sufficient.

## Decision 10 — Performance and quality

**Chosen**: A 1080p, 60 fps budget with headroom for a later VR build.

- 60 fps at 1920×1080 on a mid-range laptop.
- ≤150k triangles per level; ≤100 draw calls.
- Textures ≤1024 px; no real-time shadow cascades beyond one directional light.
- Baked or mixed lighting for static environment geometry; real-time lights only for the player area.
- MSAA 4x; HDR off; render scale 1.0.
- Static batching for environment props; GPU instancing for repeated stands/chairs where possible.
- Food meshes ≤5k triangles each; LODs for heavy foods.
- Bazaar: occlusion culling and a short far plane to keep the stand count cheap.

**Rationale**:

- The existing rendang and fried chicken models are heavy; without decimation the scene will not hold 60 fps.
- The bazaar adds more geometry than the two counter levels; culling and modular stands keep it manageable.

**Alternatives considered**:

- **Target 72 fps on Quest now**: Rejected. No device is available and VR is deferred.
- **Ignore performance until the end**: Rejected. Heavy FBX imports would force rework after content is placed.

## Open risks and deferred decisions

| Item | Status | Notes |
|------|--------|-------|
| Unity 6000.6.0f1 installation | Required | The project pins 6000.6.0f1; install it via Unity Hub. The local Hub listing is a setup issue, not a version choice. |
| Rexus gamepad detection | Risk | Test in M0; document the device name; keep keyboard/mouse fallback |
| Asset team delivery | Risk | The team supplies the five foods and bazaar modules; placeholders keep M2/M3 unblocked |
| VR headset performance and comfort | Deferred | Gamepad-first is the agreed scope; VR build is optional |
| Exact nutrition values | Review needed | Catalog values are approximations; review against TKPI/Panganku before the demo |
| Level 3 bazaar scope | Managed | Four stands only; reuse existing stalls; no new architecture |
| Retry policy | Fixed | Retry replaces the current level result; recap uses the latest attempt |
| Empty plate | Fixed | 50 neutral, "Belum dinilai" |
