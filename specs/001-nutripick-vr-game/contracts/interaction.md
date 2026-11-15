# Contract: Interaction

**Feature**: `001-nutripick-vr-game`

This contract defines how the player moves, aims, picks up, carries, plates, inspects, and serves food with a Rexus gamepad.

## Input Map

| Input | Action |
|-------|--------|
| Left stick / WASD | Move |
| Right stick / mouse | Look |
| A / South / Enter / left mouse | Interact: pick up, place, confirm |
| B / East / Escape / right mouse | Cancel: drop carried food, close card |
| X / West / Tab | Inspect: open the full food card |
| Start | Serve |
| Select / Escape | Pause/resume |
| D-pad / left stick | Navigate report and recap buttons |

- The gamepad is primary; keyboard and mouse are the fallback.
- The mapping is defined in `Assets/Input/NutriPick.inputactions`.
- If `Gamepad.current` is null at startup, show "Controller tidak terdeteksi. Menggunakan keyboard dan mouse." and enable the fallback bindings.

## Player Rig

```text
PlayerRig
├── CharacterController
├── FirstPersonController
├── GamepadInput
├── CameraPivot
│   └── Main Camera
│       └── ReticleInteractor
│           └── HoldAnchor (0.5 m forward, 0.15 m down)
```

The reticle itself is drawn by the screen-space HUD, not by a world-space object.

- Eye height: 1.6 m.
- Move speed: 2.0 m/s; no sprint required.
- Look: right stick at 120°/s; mouse sensitivity configurable.
- Gravity: −9.81 m/s²; grounded check via `CharacterController.isGrounded`.
- Camera collision: the camera pivot stays inside the capsule; no head bob.
- Reticle: screen-space dot at the center, with a subtle ring when a target is available.

## Reticle Interaction

`ReticleInteractor` raycasts from the camera center:

- Max distance: 3.0 m.
- Layers: `Food`, `PlateSlot`, `ServeStation`, `Environment`.
- A hit on `Food` sets `TargetFood`.
- A hit on `PlateSlot` sets `TargetSlot` (only while carrying).
- A hit on `ServeStation` sets `TargetServe`.
- A hit on `Environment` clears all targets.

### State machine

```text
Idle → HoveringFood → Carrying → HoveringSlot → Idle
Carrying → Dropping → Idle
Idle → HoveringServe → Serving
```

### Rules

| State | Reticle | Prompt | Interact (A) | Cancel (B) |
|-------|---------|--------|--------------|------------|
| Idle | Default dot | None | Nothing | Nothing |
| HoveringFood | Ring + compact label | "A: Ambil" | Pick up | Nothing |
| Carrying | Dot + carried item | "A: Taruh · B: Lepas" | Place if over a free slot | Drop |
| HoveringSlot | Highlight slot | "A: Taruh" | Place | Drop |
| HoveringServe | Highlight station | "A: Sajikan" or reason | Serve if valid | Nothing |
| Dropping | Default dot | None | Nothing | Nothing |

- Picking up requires the food to be on a stand or the plate, not already carried.
- Carrying is limited to one item.
- While carrying, the food's collider is disabled so it does not block the reticle.
- Placing requires a free slot and an unlocked plate.
- If the plate is full, the prompt shows "Piring penuh".
- Pressing X while hovering a food opens the full card without picking it up.
- Pressing B while carrying drops the food. If the food is over its home stand, it returns to the anchor; otherwise it falls and returns home after 1 s.

## Food Prefab

```text
FoodRoot (layer Food)
├── FoodItem
├── Collider
├── Rigidbody
├── Model
└── InfoAnchor (optional)
```

- `FoodItem.data` points to the matching `FoodData` asset.
- `FoodItem.homeStand` is assigned by the stand at spawn.
- On stand: Rigidbody dynamic, collider enabled.
- Carried: Rigidbody kinematic, collider disabled, parented to `HoldAnchor`.
- Plated: Rigidbody kinematic, collider enabled, parented to the slot.
- Dropped: Rigidbody dynamic, collider enabled.
- If the item falls below y = −0.5 or moves more than 15 m from its home stand, it returns home.

## Plate

- Four `PlateSlot` components on a 0.26 m plate.
- Each slot has a snap point and a trigger radius of approximately 0.06 m.
- The reticle targets a slot's trigger collider.
- `PlateSlot.TryPlace(food)`:
  - returns false if occupied or the plate is locked,
  - disables the food Rigidbody,
  - parents the food to the slot,
  - sets local position and rotation to the snap point,
  - marks the slot occupied.
- `PlateSlot.Remove(food)`:
  - unparents the food,
  - re-enables the Rigidbody,
  - clears the slot.
- `PlateController` listens to place/remove and raises `PlateChanged`.
- `PlateController.CanServe(out string reason)` checks:
  - minimum item count from `LevelDefinition`,
  - Level 3 drink requirement.
- `PlateController.Lock()` disables picking up plated items and all slots.
- Plate highlight: the targeted free slot glows when the carried food can be placed.

## Food Stand

- A `FoodStand` has a `displayName`, `FoodData[] foods`, and `Transform[] anchors`.
- `SpawnFoods()` instantiates each food prefab at its anchor.
- Each stand has a sign with its name, readable from the walkway.
- Levels 1 and 2 use one stand; Level 3 uses four:
  - Mie & Jajanan: fried noodles, burger.
  - Lauk: boiled egg, tempeh.
  - Sayur & Buah: broccoli, banana.
  - Minuman: water, sweet tea.
- The player must be able to target every food from the walkway; the reticle range is 3.0 m and the stand anchors are placed within 2.5 m of the approach point.
- A dropped item returns to its home stand.

## Serve Station

- A 3D station on the serving table with a collider on the `ServeStation` layer.
- Hover shows "A: Sajikan" when the plate is valid.
- Hover shows the reason when invalid: "Tambahkan 1 makanan lagi" or "Pilih satu minuman".
- Pressing A or Start triggers `LevelFlowController.Serve()`.
- After serving, the station is disabled and grayed out.

## HUD Prompts

- Center-bottom prompt: "A: Ambil", "A: Taruh", "A: Sajikan", "B: Lepas", "X: Lihat info".
- The prompt changes with the interaction state.
- The prompt is hidden when the reticle is over nothing.
- The prompt uses a gamepad glyph or the letter label; include a text fallback for keyboard.

## Feedback

| Event | Audio | Rumble |
|-------|-------|--------|
| Hover | Soft tick | None |
| Pick up | Pickup | 0.4 / 0.2 s |
| Place | Ceramic tap | 0.3 / 0.1 s |
| Drop/return | Soft lift | 0.2 / 0.08 s |
| Serve | Whoosh + confirmation | 0.5 / 0.15 s |
| Bar crosses 40 or 70 | Light chime | None |
| Report reveal | Soft chime | None |

- Rumble uses `Gamepad.SetMotorSpeeds` via the Input System.
- If the Rexus controller does not support rumble, the call no-ops without an error.
- Audio clips live in `Assets/Audio/SFX/`; volumes are tuned in M6.

## Level 3 Bazaar Movement

- The bazaar is a 12 × 12 m area with a central serving table.
- The walkway is at least 1.2 m wide.
- The player starts at the entrance facing the table.
- Stands are arranged around the walkway, each with a sign.
- The player can visit stands in any order.
- The drink stand is clearly marked and is required for the Level 3 plate.
- The far plane and occlusion culling keep the bazaar at 60 fps.

## Edge Cases

| Case | Required behaviour |
|------|--------------------|
| Reticle over nothing | No prompt; interact does nothing |
| Plate full | "Piring penuh"; no placement |
| Plate locked | No pick-up, placement, or removal |
| Item dropped outside the play area | Returns to its home stand |
| Carrying at time-out | Return item to its stand before evaluation |
| Gamepad disconnects | Pause; reconnect or use keyboard/mouse |
| Gamepad has no rumble | Skip silently |
| Player walks away while carrying | Item stays in the carry position |
| Player targets a stand from too far | No target; walk closer |
| Level 3 stand unreachable | Validation error; fix the path or collider |
