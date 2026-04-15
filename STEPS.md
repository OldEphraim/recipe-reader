# Recipe Reader — Implementation Steps

Follow these steps in order. Complete each before moving to the next.
After each step, append what you built and why to DECISION_LOG.md.

## Step 1: Project Foundation
Create the core data structures and manager scripts.

1. Create `Assets/Scripts/RecipeData.cs`:
   - `[System.Serializable]` classes: `RecipeCollection`, `Recipe`, `RecipeStep`
   - Fields matching the JSON schema in GAME_DESIGN.md
   - Utility method to load from JSON (TextAsset or Resources)

2. Create `Assets/Scripts/GameManager.cs`:
   - Singleton MonoBehaviour
   - Game state enum: `Title`, `DifficultySelect`, `Gameplay`, `Results`
   - Methods: `StartGame(string difficulty)`, `CompleteRecipe()`, `ReturnToTitle()`
   - Set `Application.targetFrameRate = 60` in Awake
   - Set up `BoardApplication.SetPauseScreenContext()` for pause menu
   - Subscribe to `BoardApplication.pauseScreenActionReceived`

3. Create `Assets/Data/recipes.json` with the recipe data from GAME_DESIGN.md

## Step 2: Piece Input Handler
Create the script that reads Board SDK input and maps pieces to game actions.

1. Create `Assets/Scripts/PieceHandler.cs`:
   - MonoBehaviour that runs in Update()
   - Gets active Glyph contacts via `BoardInput.GetActiveContacts(BoardContactType.Glyph)`
   - Tracks pieces across frames using Dictionary<int, PieceState>
   - PieceState stores: glyphId, contactId, screenPosition, orientation, isActive
   - On Began phase: determine piece type, fire events
   - On Ended/Canceled phase: clean up tracking
   - Expose C# events: `OnPiecePlaced(int glyphId, Vector2 screenPos, int contactId)`
   - Expose C# events: `OnPieceLifted(int glyphId, Vector2 screenPos, int contactId)`
   - Expose C# events: `OnPieceMoved(int glyphId, Vector2 screenPos, int contactId)`

2. Create `Assets/Scripts/PieceIdentifier.cs` (static helper):
   - `GetPieceName(int glyphId)` returns "spoon", "sponge", "spice_mill", "knife", "little_chef"
   - `IsUtensil(int glyphId)` true for 0-3, false for 4
   - `IsSelector(int glyphId)` true for 4 (Little Chef)

## Step 3: Interaction Logger
Create CSV logging before gameplay so every interaction is captured from the start.

1. Create `Assets/Scripts/InteractionLogger.cs`:
   - Singleton MonoBehaviour (DontDestroyOnLoad)
   - Creates CSV file at `Application.persistentDataPath/interaction_log_{timestamp}.csv`
   - Writes header row on creation
   - Public method: `LogEvent(string eventType, int glyphId, string pieceName, int contactId, Vector2 screenPos, float rotation, int targetStepIndex, bool wasCorrect, string difficulty, string recipeName)`
   - Subscribes to PieceHandler events and logs every placement, lift, and move
   - Flushes file on each write (don't lose data on crash)

## Step 4: UI Framework
Build the UI screens using Unity Canvas (uGUI).

1. Create `Assets/Scripts/UIManager.cs`:
   - Manages four screen panels: TitlePanel, DifficultyPanel, GameplayPanel, ResultsPanel
   - Only one panel active at a time
   - Methods: `ShowTitle()`, `ShowDifficultySelect()`, `ShowGameplay(Recipe recipe)`, `ShowResults(int score, int totalSteps, int wrongAttempts, float timeElapsed)`
   - Wire buttons via onClick events

2. Design each panel (programmatically or describe for manual setup):
   - **TitlePanel**: Title text, subtitle, "Play" button
   - **DifficultyPanel**: Three buttons (Junior/Sous/Head Chef) with descriptions
   - **GameplayPanel**: Left side vertical step cards, right side cooking scene, bottom piece reminder, top recipe banner
   - **ResultsPanel**: Stars, stats, replay buttons

3. Create `Assets/Scripts/StepCardUI.cs`:
   - MonoBehaviour on each step card prefab
   - Fields: Text displayText, Image background, Image checkmark, Image glowBorder
   - Methods: `SetStep(RecipeStep step)`, `SetSelected(bool)`, `SetCompleted(bool)`, `FlashWrong()`
   - Has a RectTransform that defines the hit area for piece detection

## Step 5: Recipe Manager (Core Game Logic)
Wire everything together.

1. Create `Assets/Scripts/RecipeManager.cs`:
   - Holds current Recipe, selected step index (-1 if none)
   - Tracks completion state per step (bool array)
   - Tracks wrong attempt count and elapsed time
   
   - `SelectStep(int stepIndex)`: highlight step, deselect previous
   - `DeselectStep()`: called when Little Chef lifted
   - `TryPlaceUtensil(int glyphId, Vector2 screenPos)`:
     - No step selected: show prompt
     - Correct piece: mark done, log, animate, advance
     - Wrong piece: log, flash wrong, increment wrong count
   - `CheckCompletion()`: all steps done triggers Results

2. Connect PieceHandler to RecipeManager:
   - PiecePlaced + Little Chef → find step card via hit test → SelectStep
   - PiecePlaced + utensil → TryPlaceUtensil
   - PieceLifted + Little Chef → DeselectStep

3. Hit testing: convert screenPosition to Canvas space, check each StepCardUI RectTransform using `RectTransformUtility.ScreenPointToLocalPointInRectangle`

## Step 6: Polish and Feedback
1. Correct answer: green background, checkmark, scale bounce
2. Wrong answer: red flash (0.5s), gentle shake
3. Step selection: golden glow, slight scale up, other steps dim
4. Stretch: sound effects, background music

## Step 7: Integration Testing
1. Full flow: Title → Difficulty → Gameplay → Results
2. Little Chef select/deselect works
3. Each utensil validates correctly per recipe
4. Wrong answers allow retry
5. Completion triggers Results
6. CSV log written correctly
7. Multiple recipes across difficulties
8. Edge cases: simultaneous pieces, pieces outside cards

## Step 8: Deliverables
1. README.md with setup/play instructions
2. Sample CSV log from a playthrough
3. Record 3-5 min demo video

## File Checklist
```
Assets/
  Scripts/
    GameManager.cs
    RecipeManager.cs
    PieceHandler.cs
    PieceIdentifier.cs
    InteractionLogger.cs
    UIManager.cs
    RecipeData.cs
    StepCardUI.cs
  Data/
    recipes.json
  Scenes/
    RecipeReader.unity
  Prefabs/
    StepCard.prefab
  StreamingAssets/
    chop_chop_v1.3.0.tflite
CLAUDE.md
GAME_DESIGN.md
STEPS.md
DECISION_LOG.md
README.md
```