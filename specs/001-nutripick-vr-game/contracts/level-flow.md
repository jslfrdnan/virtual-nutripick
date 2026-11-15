# Contract: Level Flow

**Feature**: `001-nutripick-vr-game`

This contract defines scene names, the persistent session, the per-level state machine, and the retry/next/recap behaviour for the gamepad-first build.

## Scenes

| Order | Scene path | Level | Final |
|------:|------------|-------|-------|
| 1 | `Assets/Scenes/MainVR.unity` | 1 - Warteg breakfast | No |
| 2 | `Assets/Scenes/Level02_Padang.unity` | 2 — Padang lunch | No |
| 3 | `Assets/Scenes/Level03_Bazaar.unity` | 3 — Bazaar dinner | Yes |

`Assets/Scenes/MainVR.unity` is a sandbox and must not be in Build Settings.

Every level scene must contain:

- `PlayerRig` with `CharacterController`, camera, reticle, `FirstPersonController`, `GamepadInput`, `ReticleInteractor`, and `HeldFoodCarrier`
- `LevelFlowController`
- `LevelDefinition` reference
- One or more `FoodStand` components
- `PlateController` with four `PlateSlot`s
- `ServeStation`
- `NutritionBar`
- `FoodInfoCardPresenter`
- `LevelTimerDisplay`
- `InteractionPrompt`
- `HealthReportPresenter`
- `RecapPresenter` (Level 3 only)
- Screen-space HUD canvas and TMP text
- EventSystem with `InputSystemUIInputModule`
- Environment root and lighting

## GameSession

`GameSession` is a `MonoBehaviour` singleton that survives scene loads with `DontDestroyOnLoad`.

```csharp
public static GameSession EnsureExists();
public void BeginLevel(int levelIndex);
public void RecordResult(int levelIndex, MealResult result);
public LevelResult GetResult(int levelIndex);
public void ResetSession();
public float AverageScore();
```

Rules:

- `EnsureExists()` creates a new session object if none exists.
- `BeginLevel(1)` resets the session only when no session is active. This makes a fresh launch clean but preserves results when retrying Level 1.
- `BeginLevel(2 or 3)` never resets.
- `RecordResult` replaces the existing result for that level and increments `attemptCount`.
- `GetResult` returns null for an unplayed level.
- `ResetSession` clears all results and deactivates the session.
- `AverageScore` averages only recorded levels.

## LevelFlowController

### States

`Explore` → `Plating` → `Ready` → `Served` → `Report` → `Recap` (Level 3 only).

| State | Player can | System does |
|-------|------------|-------------|
| Explore | Move, look, target food, read cards | Timer running; bar shows 50 neutral |
| Plating | Pick up, carry, place, remove food | Bar preview updates on every plate change |
| Ready | Add/remove food, serve | Serve prompt enabled |
| Grace | Add food only; serve if minimum met | Grace countdown visible |
| Served | Nothing; plate locked | Final evaluation, bar animation ~1.0 s |
| Report | Navigate Retry, Next, or Recap | Report visible; timer stopped |
| Recap | Navigate Main lagi | Three level results and closing message |

### Event sequence on serve

1. Player targets the `ServeStation` and presses A, or presses Start.
2. `PlateController.CanServe` confirms the minimum item count and, for Level 3, a drink.
3. `PlateController.Lock()` disables picking up plated items and all slots.
4. `LevelFlowController` stops the timer and calls `MealEvaluator.Evaluate`.
5. `GameSession.RecordResult(levelIndex, result)`.
6. `NutritionBar.SetFinal(result)` animates for approximately 1.0 s.
7. `HealthReportPresenter.Show(result)` appears after the animation.
8. Report buttons become active and the first button is selected for gamepad navigation.

### Timer

- `timeLimitSeconds` defaults to 180 seconds per level.
- The timer starts when the scene is ready and the player can interact (after the fade-in).
- At 0:
  - If the plate meets the minimum, auto-serve.
  - If below the minimum but not empty, enter `Grace` for `graceSeconds` (default 30).
  - At the end of grace, auto-serve with `isIncomplete = true` and a warning.
  - If the plate is empty at the end of grace, reload the current level and show a short message. No result is recorded.
- The timer turns red under 30 seconds.
- The timer stops when the plate is locked.

### Serve station

- A 3D station on the serving table with a collider on the `ServeStation` layer.
- The reticle shows "A: Sajikan" when the plate can be served.
- If the plate cannot be served, the prompt shows the reason: "Tambahkan 1 makanan lagi" or "Pilih satu minuman".
- The Start button always attempts the same action.
- The station is disabled and grayed out after serving.

### Pause and gamepad disconnect

- Select/Escape pauses the game and shows a pause panel with "Lanjut" and "Keluar".
- If `Gamepad.current` becomes null, pause automatically and show "Controller terputus. Sambungkan kembali atau tekan Enter untuk keyboard."
- Reconnecting the gamepad resumes from the pause panel.
- Keyboard and mouse fallback remains available while paused.

### Retry

- `Ulangi level` reloads the current scene.
- `GameSession` remains active; the previous result stays until the new result is recorded.
- On reload, the plate is empty, the timer resets, and the food stands respawn.

### Next

- `Lanjut` loads the next scene by `LevelDefinition.sceneName`.
- The current result is already recorded.
- Use `SceneManager.LoadSceneAsync` with a short fade.

### Recap

- Level 3 report shows `Lihat rekap` instead of `Lanjut`.
- The recap panel shows:
  - three level rows with name, score, and band,
  - average score and band,
  - two strengths and one improvement across the session,
  - closing message and disclaimer,
  - `Main lagi` button.
- `Main lagi` calls `GameSession.ResetSession()` and loads Level 1.

### Restart

- Restart clears the session and loads Level 1.
- Quitting and relaunching the application also starts cleanly at Level 1.

## Level 3 Bazaar Flow

1. Level 3 loads with the player at the bazaar entrance, facing the central serving table.
2. Four stands are visible and signposted: Mie & Jajanan, Lauk, Sayur & Buah, Minuman.
3. The player walks with the left stick to a stand, aims at food, and picks it up.
4. The player can carry only one item at a time; each pick-up requires a separate trip or a nearby stand.
5. The player returns to the central table, places the item on the plate, and repeats.
6. The player must collect four items including one drink.
7. When the plate is ready, the player targets the `ServeStation` and presses A, or presses Start.
8. The report appears; the player can retry the bazaar or view the recap.

The bazaar path must be at least 1.2 m wide, with no collider blocking the player from reaching any stand.

## Failure and Edge Behaviour

| Situation | Behaviour |
|-----------|-----------|
| Empty plate, player presses Serve | Button disabled; reason shown |
| Empty plate at time-out | Grace period; if still empty, reload level |
| Under-filled plate at grace end | Auto-serve with `isIncomplete` warning |
| Player carries food at time-out | Return item to its stand, then evaluate |
| Item dropped below the floor | Return to its home stand after 1 s |
| Item dropped outside the play area | Return to its home stand after 1 s |
| Plate is full | Placement refused with "Piring penuh" |
| Reticle over nothing | No prompt; interact does nothing |
| Gamepad disconnects | Pause; reconnect or use keyboard/mouse |
| Retry after a result | Old result remains until the new one is recorded |
| Level scene loaded directly in the editor | `EnsureExists` creates a session; Level 1 begins cleanly |
| Level 3 stand unreachable | Validation error; fix the path or stand collider |

## Build Settings

Build Settings scene order must be:

1. `MainVR`
2. `Level02_Padang`
3. `Level03_Bazaar`

`MainVR` and `SampleScene` must not be enabled in the build list.
