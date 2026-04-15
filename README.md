# Recipe Reader

A two-player collaborative reading comprehension game for the [Board.fun](https://board.fun) tabletop console, built for the **Board of Education** game design challenge.

Two players read a recipe together and take turns placing the correct Chop Chop utensil on each cooking step. The fun lives in *reading out loud, discussing what a word means, and handing a physical piece to your partner*. The game teaches vocabulary, reading comprehension, and contextual inference through physical-digital play.

**Target audience:** Ages 6–12
**Players:** 2 (collaborative)
**Prototype timeline:** 1-week sprint

---

## How It Teaches

Reading comprehension is hard to make fun. Most "educational" reading games quiz a child in isolation and reward them with generic feedback. Recipe Reader flips this:

- **Kids must read aloud** to each other because only one player holds the Little Chef selector at a time. Reading becomes the gameplay, not a chore between gameplay.
- **Context-to-action mapping** trains comprehension. When a child reads "Julienne the carrots into thin matchstick strips" and then picks up the Knife piece, they are demonstrating comprehension through a physical action, not a multiple-choice bubble.
- **Hard mode hides the verb** (`_____ the onion until you have tiny, even pieces.`) and forces inference from surrounding context — the core skill tested on every standardized reading assessment from 2nd grade up.
- **Collaboration > competition.** The game never pits players against each other. Disagreements about an unfamiliar word ("what does *zest* mean?") become the learning moment, and the physical handoff of pieces creates natural turn-taking.

---

## Tech Stack

- **Unity** 6.4 (6000.4.2f1)
- **Board SDK** v3.3.0 (`fun.board` package)
- **C#** for all gameplay scripts
- **Chop Chop** Piece Set (`chop_chop_v1.3.0.tflite`)
- **TextMesh Pro** for all UI text
- **CSV** file logging (local, no backend)

---

## Setup

### 1. Unity
Open the project in **Unity 6.4 (6000.4.2f1)** or newer. The `Library/` folder will rebuild on first open — expect a few minutes.

### 2. Board SDK
The project uses the `fun.board` package at v3.3.0. It is referenced via `Packages/manifest.json` and should resolve automatically on first open. If it does not, add it manually via `Window > Package Manager > Add package from git URL…`.

### 3. Chop Chop piece model
Confirm `Assets/StreamingAssets/chop_chop_v1.3.0.tflite` exists. Without it, the Board Simulator cannot recognize any of the five Chop Chop pieces.

### 4. ⚠️ Disable `BoardUIInputModule` for editor testing
Open the scene, select **Canvas > EventSystem** in the hierarchy, and **uncheck** `BoardUIInputModule` in the Inspector. **Re-enable it before building for the Board hardware or an Android device.**

This is required because, under SDK v3.3.0, `BoardUIInputModule` blocks mouse/keyboard `InputSystemUIInputModule` events in the Editor even though the SDK docs say the two coexist. Without disabling it, buttons on the Title and Difficulty screens will not respond to Editor mouse clicks. See **Known Issues** below.

### 5. Build the scene (optional helper)
The project ships with an editor script that regenerates the full scene from code so you never have to hand-wire references:

```
Tools > Build Recipe Reader Scene
```

This creates `Assets/Scenes/RecipeReader.unity` with every GameObject, component reference, panel, and the `StepCard.prefab` wired correctly. Run it once after pulling the project.

---

## Running in the Simulator

1. Open `Assets/Scenes/RecipeReader.unity`.
2. In the **Game** view, set the aspect ratio to **1920×1080 Landscape**.
3. Select the EventSystem and disable `BoardUIInputModule` (see Setup step 4).
4. Enter **Play Mode**.
5. The Board Simulator window appears alongside the Game view. Use it to drag virtual Chop Chop pieces onto the screen — the cursor icon in the Simulator switches between the 5 pieces.
6. Click **Play** on the title screen, pick a difficulty, and test.

---

## How to Play

### The Pieces
| Glyph ID | Piece         | Role                                                                     |
|----------|---------------|--------------------------------------------------------------------------|
| 0        | Spoon         | Answer piece for **stir, mix, scoop, fold**                              |
| 1        | Sponge        | Answer piece for **wash, scrub, rinse, wipe**                            |
| 2        | Spice Mill    | Answer piece for **grind, crush, season, sprinkle**                      |
| 3        | Knife         | Answer piece for **slice, chop, dice, cut, mince**                       |
| 4        | Little Chef   | **Selector** — place on a step card to choose which step you are solving |

### The Loop
1. A recipe appears as a vertical stack of step cards on the left side of the screen.
2. **Read a step aloud together.** Decide which utensil it calls for.
3. Player A places the **Little Chef** on the chosen step. The card highlights with a golden border — the step is now *selected*.
4. Player B places the matching **utensil** (Knife / Spoon / Sponge / Spice Mill) anywhere — the game checks the selected step.
5. **Correct** → the card turns green, a checkmark appears, and the selection auto-advances to the next step.
6. **Wrong** → the card flashes red and shakes. Lift the utensil and try again. Talk about why.
7. Complete every step to finish the recipe and see the Results screen with stars, accuracy, and time.

### Difficulty Levels
- **Junior Chef (ages 6–8)** — 4 steps, simple verbs, one short sentence each.
- **Sous Chef (ages 8–10)** — 5 steps, richer sentences with varied vocabulary.
- **Head Chef (ages 10–12)** — 6 steps, the verb is replaced with `_____` and must be inferred from context. When the correct piece is placed, the missing word fills in and the child sees the vocabulary they just figured out.

### The Little Chef Rule
The first step is **not** auto-selected. Players must physically place the Little Chef on a step to begin — an on-screen prompt ("Place the Little Chef on a step to begin!") appears until they do. This enforces turn-taking from the very first move.

---

## Interaction Log (CSV)

Every piece placement, move, and lift is logged to a CSV file on disk for post-playtest analysis.

**Location:** `Application.persistentDataPath/interaction_log_<timestamp>.csv`

On macOS: `~/Library/Application Support/<Company>/<Product>/interaction_log_YYYYMMDD_HHMMSS.csv`
On Windows: `%UserProfile%\AppData\LocalLow\<Company>\<Product>\interaction_log_YYYYMMDD_HHMMSS.csv`

**Columns:**
```
timestamp,event_type,piece_glyph_id,piece_name,contact_id,screen_x,screen_y,rotation_deg,target_step_index,was_correct,difficulty,recipe_name
```

**Event types:**
- `placed` / `moved` / `lifted` — raw piece lifecycle events
- `correct` / `wrong` — utensil answer was checked against a selected step
- `utensil_no_selection` — utensil placed while no Little Chef selection existed

Every write is flushed immediately so a Unity crash never loses the last interaction.

---

## Architecture

```
Assets/
├── Scripts/
│   ├── GameManager.cs         Singleton state machine, pause-menu wiring
│   ├── RecipeManager.cs       Core gameplay: selection, validation, advance
│   ├── PieceHandler.cs        Polls Board SDK, emits Placed/Moved/Lifted events
│   ├── PieceIdentifier.cs     Glyph-ID constants + piece-name helpers
│   ├── InteractionLogger.cs   CSV logger, flush-per-write
│   ├── UIManager.cs           Panel switching, button wiring, step-card spawning
│   ├── StepCardUI.cs          Per-card visuals + shake/flash wrong feedback
│   └── RecipeData.cs          [Serializable] classes + JSON loader
├── Editor/
│   └── RecipeReaderSceneBuilder.cs   Menu item that rebuilds the full scene
├── Data/
│   └── recipes.json           All 6 recipes (2 easy, 2 medium, 2 hard)
├── Prefabs/
│   └── StepCard.prefab        One step card — instantiated per step at runtime
├── Scenes/
│   └── RecipeReader.unity
└── StreamingAssets/
    └── chop_chop_v1.3.0.tflite
```

### Data flow
```
BoardInput ──► PieceHandler ──► RecipeManager ──► UIManager ──► StepCardUI
                    │                  │
                    └──► InteractionLogger (CSV)
```

`PieceHandler` is the only component that talks to the Board SDK input API. It tracks pieces by `contactId` across frames (never `glyphId`, since multiple pieces of the same type would collide) and emits plain C# events. `RecipeManager` subscribes to those events, runs a hit test against the active `StepCardUI` rect transforms, and routes the result into the game logic.

---

## Known Issues

### `BoardUIInputModule` blocks Editor mouse input (SDK v3.3.0)
**Symptom:** Buttons on the Title and Difficulty screens do not respond to mouse clicks in the Unity Editor Game view.
**Cause:** `BoardUIInputModule` appears to swallow `InputSystemUIInputModule` pointer events in the Editor even though the SDK docs state the two are supposed to coexist. Likely an SDK v3.3.0 bug.
**Workaround:** Disable `BoardUIInputModule` on the EventSystem GameObject while testing in the Editor. **Re-enable it before building for Board hardware or Android** — piece touch input still works with it disabled, but finger-touch on UI will not work on the real device.

### Auto-advance highlights a step the physical Little Chef is not on
**Symptom:** After a correct answer, the next incomplete step highlights even though the physical Little Chef piece is still sitting on the just-completed step.
**Behavior:** This is intentional — it serves as a visual "next up" hint. The player is expected to physically move the Little Chef to the highlighted step (or to wherever they prefer).

### Hard recipe `completedText` for "quarter"
Hard recipes fill in the blank verb when answered correctly. A recipe step that uses the verb "quarter" would read "Quarter the potatoes into quarters…" after completion. This is intentional — removed from the current recipe set, but worth noting if new recipes are added.

---

## Credits

Built for Board.fun's **Board of Education** challenge. Game design, Unity implementation, and documentation written during a 1-week prototype sprint. See `GAME_DESIGN.md` for the full design document and `DECISION_LOG.md` for the running record of decisions and tradeoffs.
