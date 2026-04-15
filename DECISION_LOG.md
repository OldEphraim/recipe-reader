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

