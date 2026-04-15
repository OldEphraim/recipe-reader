# Recipe Reader — Decision Log

Record all architectural and design decisions here with timestamp and reasoning.

---

## 2026-04-14 — Step 1: Project Foundation
- Added `RecipeData.cs` with `RecipeCollection`/`Recipe`/`RecipeStep` matching the JSON schema. `RecipeCollection` offers three load paths (raw JSON, `TextAsset`, `Resources`) plus a `GetByDifficulty` helper so the GameManager does not need to filter by hand.
- Added `GameManager.cs` as a singleton driving the `Title / DifficultySelect / Gameplay / Results` state machine. Sets `Application.targetFrameRate = 60` in `Awake`, wires `BoardApplication.SetPauseScreenContext("Recipe Reader", showSaveOptionUponExit: false)`, and subscribes to `pauseScreenActionReceived` with the SDK-correct `(BoardPauseAction, BoardPauseAudioTrack[])` signature (verified against `Library/PackageCache/fun.board.../BoardApplication.cs`). On Exit actions the game calls `BoardApplication.Exit()`.
- `StartGame(difficulty)` picks the next recipe for a difficulty via a round-robin cursor per difficulty — simple, zero-config variety across plays without needing a menu of specific recipes.
- Created `Assets/Data/recipes.json` with all four recipes (2 easy, 1 medium, 1 hard) copied verbatim from GAME_DESIGN.md. Note: the JSON file lives in `Assets/Data/`; GameManager supports either a direct `TextAsset` drag-in or loading from a `Resources` folder, so the user can move/copy it at scene-wire time without code changes.

## 2026-04-14 — Step 2: Piece Input Handler
- Added `PieceHandler.cs` that polls `BoardInput.GetActiveContacts(BoardContactType.Glyph)` every `Update()` and tracks pieces in a `Dictionary<int, PieceState>` keyed by `contactId` (never `glyphId`, per CLAUDE.md). Exposes three C# events: `OnPiecePlaced`, `OnPieceLifted`, `OnPieceMoved`.
- Dropouts (contact no longer present this frame) are treated as lifts — this catches the pause-screen Canceled case cleanly without a special path, since the next frame's contact list will simply omit the canceled IDs.
- `Stationary` and `Moved` phases defensively add-and-fire-Placed if the contact is new to us (e.g. if we missed its `Began` frame during a scene transition), so state stays consistent.
- Added `PieceIdentifier.cs` static helper with glyph-id constants, `GetPieceName`, `IsUtensil`, and `IsSelector`, so other systems never hardcode the 0–4 numbers.

## 2026-04-14 — Step 3: Interaction Logger
- Added `InteractionLogger.cs` as a `DontDestroyOnLoad` singleton writing to `Application.persistentDataPath/interaction_log_{yyyyMMdd_HHmmss}.csv`. Writes the exact header specified in CLAUDE.md and flushes after every line (crash resilience — we will not lose the last interaction if Unity crashes mid-session).
- Auto-subscribes to a `PieceHandler` reference (if assigned) and logs all place/lift/move events, stamping them with the current `GameManager.CurrentDifficulty` and `CurrentRecipe.name`. Game-logic events (`correct`, `wrong`, `utensil_no_selection`) are logged directly by `RecipeManager` via the public `LogEvent` method, which includes `targetStepIndex` and `wasCorrect`.
- CSV field escaping handles commas and quotes in recipe names so the log stays parseable.

## 2026-04-14 — Step 4: UI Framework
- Added `UIManager.cs` which owns four panel GameObjects and toggles exactly one active at a time via `SetOnlyActive`. Wires buttons in `Start` using `onClick.AddListener` so scene setup is just drag-and-drop in the Inspector. `ShowResults` computes stars (0 wrong = 3, 1–2 = 2, 3+ = 1) and an accuracy percent locally.
- Added `StepCardUI.cs`: serialized fields for `displayText` (TMP_Text), `background`, `checkmark`, `glowBorder`, plus a `RectTransform hitArea` used for piece hit-testing. `SetSelected/SetCompleted/FlashWrong` handle the visual states called out in GAME_DESIGN.md. `ContainsScreenPoint` wraps `RectTransformUtility.RectangleContainsScreenPoint`, which is the correct API for Screen-Space-Camera canvases and also works for Overlay canvases when passed a null camera.
- `UIManager.BuildStepCards` instantiates `StepCardUI` instances into `stepCardContainer` (a vertical layout group in the scene) so the UI adapts to recipes with 4/5/6 steps automatically. `GetCardForStepIndex` returns the card matching a given step for RecipeManager to address.
- Used TMPro (`TMP_Text`) rather than legacy UGUI `Text` — it is already in the project's package set and renders better at 1920×1080.

## 2026-04-14 — Step 5: Recipe Manager (Core Game Logic)
- Added `RecipeManager.cs`. Tracks `CurrentRecipe`, `SelectedStepIndex` (-1 when none), a `bool[] completion`, `WrongAttempts`, `CorrectCount`, and `ElapsedTime` (ticking in `Update` while `isActive`).
- Subscribes to `PieceHandler` events in `OnEnable`/`OnDisable`. On `PiecePlaced`: Little Chef runs a hit test and calls `SelectStep`; utensils call `TryPlaceUtensil`. On `PieceMoved` for the same Little Chef `contactId`, selection follows the piece card-to-card — this makes the "Little Chef as cursor" feel promised in GAME_DESIGN.md work without extra input. On `PieceLifted` for the Little Chef, the selection is cleared.
- We remember `littleChefContactId` rather than `glyphId == 4` so a second Little Chef contact (e.g. piece lifted and re-placed) is a new selection event cleanly — matches CLAUDE.md's rule of tracking by `contactId`.
- `TryPlaceUtensil` enforces "select first" (logs a `utensil_no_selection` event but doesn't penalize), handles correct (mark done, advance to next incomplete step via `AutoAdvanceSelection`, call `GameManager.CompleteRecipe` when finished) and wrong (increment counter, `FlashWrong`, log as `wrong`). Every outcome is routed through `InteractionLogger.LogEvent` with `targetStepIndex` and `wasCorrect`, using the piece's actual rotation fetched from `PieceHandler.GetOrientationDegrees`.
- Hit testing (`HitTest`) iterates `UIManager.ActiveStepCards` and uses `RectTransformUtility.RectangleContainsScreenPoint`, passing the canvas camera only for non-Overlay canvases — this is the Unity-correct way to hit-test UI from a screen-space pixel coordinate (the Board SDK hands us pixel coords in 1920×1080 screen space).
- No scenes or prefabs authored — all wiring is Inspector-side per instructions.

## 2026-04-15 — BoardUIInputModule Editor workaround
- BoardUIInputModule must be disabled in the Editor for mouse-based UI interaction to work. Re-enable before building for Android/Board hardware. Likely SDK v3.3.0 bug — the module appears to block InputSystemUIInputModule even in the Editor despite docs saying they coexist.

## 2026-04-15 — Recipe content, wrong-feedback polish, Little Chef gating
- **Recipes rewritten** (`Assets/Data/recipes.json` + GAME_DESIGN.md). Replaced all 4 old recipes with 6 new ones — 2 easy (4 steps), 2 medium (5 steps), 2 hard (6 steps). Every verb-to-utensil pairing is now the "obvious" one a 6-year-old would reach for: knife = slice/chop/dice/cut/mince, spoon = stir/mix/scoop/fold, spice mill = grind/crush/season/sprinkle, sponge = wash/scrub/rinse/wipe. Dropped the old `crack` step because eggs could plausibly be cracked with any tool — too ambiguous for the target audience. Every recipe uses at least 3 different utensils (most use all 4). Hard recipes keep the blank-verb format so the teaching moment survives.
- **Wrong feedback is now loud.** `StepCardUI.FlashWrong()` no longer just recolors the background briefly. It now: (a) sets a bright-red background (0.15, 0.15), (b) shakes the card horizontally via `anchoredPosition` for ~0.6s (3 sine oscillations with linear falloff so it ends cleanly back at center), (c) holds the red flash for the remainder of a full 1-second window before resolving back to the correct state (selected / completed / default). Added a `Debug.Log` at the start of `FlashWrong` that includes the step index and verb so playtest logs confirm the handler actually fired. Made the duration/amplitude/shake-count serialized fields so we can tune without recompiling.
  - Why `anchoredPosition` rather than `localPosition`: the cards live inside a `VerticalLayoutGroup`, which rewrites `anchoredPosition` each layout pass. We capture the base position in `Awake` (and re-capture lazily if the coroutine fires before Awake finishes) and always restore it at the end, so the shake never desyncs from the layout.
- **Little Chef gating.** The first step no longer auto-selects — `BeginRecipe` leaves `SelectedStepIndex = -1` (already true) and now also calls `uiManager.ShowLittleChefPrompt()` to display the on-screen message "Place the Little Chef on a step to begin!". A new `firstSelectionMade` bool on `RecipeManager` guards the hide: the prompt goes away the first time `SelectStep` runs successfully and never comes back for the rest of the recipe, even if the Little Chef is lifted and re-placed between steps. Added `littleChefPromptText` (TMP_Text) serialized field + `ShowLittleChefPrompt`/`HideLittleChefPrompt` methods on `UIManager`, and updated `RecipeReaderSceneBuilder` to create the prompt text centered on the gameplay panel so the scene builder still produces a fully wired scene out of the box. Auto-advance after correct answers is unchanged — it was already doing the right thing.

## 2026-04-15 — README + script bug sweep
- Wrote `README.md` at the project root covering the pitch, how the game teaches reading comprehension, Unity/SDK setup (including the mandatory "disable BoardUIInputModule in the Editor" step), play instructions, the piece-to-verb mapping, CSV log location + format, architecture diagram, and known issues.
- Audited every script in `Assets/Scripts/` and fixed six real bugs. None of these were blocking in a happy-path playthrough, but each one would have surfaced in playtest logs or hot-reload sessions. **Did not** touch `Assets/Editor/` or the `.unity` scene per instructions.

**Fix 1 — `StepCardUI.WrongFlash` captured the wrong base position.**
The old code called `CaptureBasePos()` in `Awake`, which runs before the parent `VerticalLayoutGroup` has laid out the card. At that moment `anchoredPosition` is (0,0) (whatever the prefab ships with), so when the shake animation finished and restored `baseAnchoredPos`, it snapped the card to the top of the container instead of wherever the layout group had placed it. Fix: capture `anchoredPosition` at the start of the coroutine — by then the first layout pass has run and the value is correct. Removed the stale `baseAnchoredPos` / `baseAnchoredPosCaptured` fields entirely.

**Fix 2 — `InteractionLogger` leaked a log file on duplicate instances.**
`Awake` called `OpenFile()` *before* the singleton dedup check, so a second logger instance would open a second CSV on disk and then `Destroy` itself without disposing the `StreamWriter`. Two symptoms in the wild: (a) an empty "ghost" log file appears on every session where a second instance briefly exists (e.g., domain reload in the editor), (b) the file handle leaks until GC runs. Fix: run the `Instance != null` check *first* and early-return before opening any file.

**Fix 3 — `InteractionLogger` logged rotation as a hardcoded 0.**
`HandlePlaced` / `HandleLifted` / `HandleMoved` passed `0f` for the rotation column of the CSV. The logger has a `PieceHandler` reference right there — it should query the real orientation. Added a small `CurrentRotation(contactId)` helper that calls `pieceHandler.GetOrientationDegrees(contactId)` and pipes it into all three handlers. Now every `placed`/`moved`/`lifted` row in the CSV carries the actual piece rotation, not just the game-logic `correct`/`wrong` rows.

**Fix 4 — `UIManager.WireButtons` stacked duplicate listeners on hot-reload.**
`AddListener` was called without first clearing existing listeners. In normal play this never fires, but during Editor hot-reload (or if `Start` ever runs twice for any reason) each button would accumulate listeners and multi-fire. Refactored the 8 calls through a tiny `Wire(Button, UnityAction)` helper that runs `onClick.RemoveAllListeners()` before `AddListener`. Also tightens the call site.

**Fix 5 — `RecipeManager.TryPlaceUtensil` had no `isActive` guard.**
The public method (callable from anywhere, not just from our event routing) did not check `isActive`, so a stray call during the Results screen, or between `EndRecipe` and `BeginRecipe`, could still mutate `CorrectCount` / `WrongAttempts` or call `FlashWrong` on stale cards. Added a single-line early-return at the top that also guards `CurrentRecipe.steps == null`.

**Fix 6 — `RecipeManager.HandlePieceMoved` left a stale highlight when dragged over a completed step.**
When the Little Chef dragged from step A into an already-completed step B, the old logic called `SelectStep(B)`. `SelectStep` refuses completed indices and early-returns — but the caller had not cleared the previous highlight, so step A stayed visually selected even though the physical chef was no longer there. Fix: in the move handler, detect the completed-target case explicitly and route it through `DeselectStep()` instead.

**Things I looked at and intentionally did not change:**
- `PieceHandler` allocates a new `HashSet<int>` every `Update` frame. Real but minor GC at 60fps with ≤5 contacts. Not worth the complexity to reuse.
- `RecipeManager.AutoAdvanceSelection` highlights a step the physical Little Chef is not on. Documented as intentional "next-up hint" in the README's Known Issues.
- `RecipeData` uses `JsonUtility`, which silently ignores unknown fields and gives no error on malformed JSON. Swapping for a stricter parser would mean a new dependency for a prototype — not justified.

