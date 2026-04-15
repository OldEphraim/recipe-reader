# Recipe Reader — Board of Education

## Project Overview
An educational reading comprehension game for the Board.fun tabletop console. Two players collaborate to complete recipes by reading cooking instructions and placing the correct Chop Chop utensil piece on each recipe step. The game teaches vocabulary, reading comprehension, and contextual inference through physical-digital interaction.

**Target audience:** Ages 6-12
**Players:** 2 (collaborative, NOT competitive)
**Platform:** Board.fun console (develop using Board Simulator in Unity Editor)
**Timeline:** 1-week prototype sprint

## Tech Stack
- Unity 6.4 (6000.4.2f1) with Board SDK v3.3.0
- C# scripting
- Chop Chop Piece Set (model: chop_chop_v1.3.0.tflite)
- CSV interaction logging (local file, no cloud backend)

## Chop Chop Piece Mapping

| Glyph ID | Piece       | Game Role                                          |
|----------|-------------|-----------------------------------------------------|
| 0        | Spoon       | Matches: stir, mix, scoop, fold, ladle, taste       |
| 1        | Sponge      | Matches: clean, wash, scrub, rinse, wipe, soak      |
| 2        | Spice Mill  | Matches: grind, crush, season, sprinkle, grate, zest|
| 3        | Knife       | Matches: chop, slice, dice, cut, mince, julienne    |
| 4        | Little Chef | Selector piece — place on a step to select it       |

## Board SDK Quick Reference

### Required Namespaces
```csharp
using Board.Core;      // BoardApplication, BoardPlayer
using Board.Input;     // BoardInput, BoardContact, BoardContactType, BoardContactPhase
using Board.Session;   // BoardSession, BoardSessionPlayer
using Board.Save;      // BoardSaveGameManager, BoardSaveGameMetadata
```
Do NOT use `using Board;` — each namespace must be imported individually.

### Touch Input
```csharp
// Get all active contacts
BoardContact[] contacts = BoardInput.GetActiveContacts();

// Filter by type
BoardContact[] pieces = BoardInput.GetActiveContacts(BoardContactType.Glyph);
BoardContact[] fingers = BoardInput.GetActiveContacts(BoardContactType.Finger);
```

### BoardContact Properties
| Property | Type | Description |
|----------|------|-------------|
| contactId | int | Unique identifier for this contact session |
| glyphId | int | Which Piece (0-4 for Chop Chop), -1 for fingers |
| screenPosition | Vector2 | Position in pixels (1920x1080) |
| orientation | float | Rotation in radians CCW from vertical |
| phase | BoardContactPhase | Began, Moved, Stationary, Ended, Canceled |
| isTouched | bool | Whether finger is touching the Piece |
| type | BoardContactType | Glyph (Piece) or Finger |

### Contact Phases
```csharp
void ProcessContact(BoardContact contact)
{
    switch (contact.phase)
    {
        case BoardContactPhase.Began:    // Piece just placed
            break;
        case BoardContactPhase.Moved:    // Piece moved/rotated
            break;
        case BoardContactPhase.Stationary: // No change
            break;
        case BoardContactPhase.Ended:    // Piece lifted
        case BoardContactPhase.Canceled: // Interrupted (pause screen)
            break;
    }
}
```

### Tracking Pieces Across Frames (CORE PATTERN)
```csharp
private Dictionary<int, GameObject> trackedPieces = new();

void Update()
{
    var contacts = BoardInput.GetActiveContacts(BoardContactType.Glyph);
    var activeIds = new HashSet<int>();

    foreach (var contact in contacts)
    {
        activeIds.Add(contact.contactId);

        if (contact.phase == BoardContactPhase.Began)
        {
            var piece = Instantiate(piecePrefabs[contact.glyphId]);
            trackedPieces[contact.contactId] = piece;
        }

        if (trackedPieces.TryGetValue(contact.contactId, out var obj))
        {
            obj.transform.position = ScreenToWorld(contact.screenPosition);
            obj.transform.rotation = Quaternion.Euler(0, 0, -contact.orientation * Mathf.Rad2Deg);
        }
    }

    foreach (var id in trackedPieces.Keys.ToList())
    {
        if (!activeIds.Contains(id))
        {
            Destroy(trackedPieces[id]);
            trackedPieces.Remove(id);
        }
    }
}
```

### Screen Coordinates
- Resolution: 1920x1080 pixels (landscape)
- Origin (0,0): bottom-left corner
- Convert to world: `Camera.main.ScreenToWorldPoint(new Vector3(pos.x, pos.y, 10f))`

### UI Input
Add `BoardUIInputModule` to EventSystem GameObjects so finger touches work for UI.
Use `Board > Input > Add BoardUIInputModule to EventSystems` for quick setup.

### Pause Menu
```csharp
BoardApplication.SetPauseScreenContext(
    applicationName: "Recipe Reader",
    showSaveOptionUponExit: false
);
BoardApplication.pauseScreenActionReceived += (action, audioTracks) => {
    switch (action) {
        case BoardPauseAction.Resume: ResumeGame(); break;
        case BoardPauseAction.ExitGameUnsaved: BoardApplication.Exit(); break;
    }
};
```

### Performance
```csharp
void Awake() { Application.targetFrameRate = 60; }
```

### Key SDK Rules
- Track by `contactId` (unique per contact), NOT `glyphId` (shared by same piece type)
- Only one Piece Set active at a time
- When pause screen opens, all contacts get Canceled — handle cleanup
- Screen orientation MUST be Landscape Left
- Call `BoardApplication.Exit()` when exiting

## Architecture

### Scripts (Assets/Scripts/)
| File | Responsibility |
|------|---------------|
| GameManager.cs | State machine: Title, DifficultySelect, Gameplay, Results. Singleton. |
| RecipeManager.cs | Loads recipes, tracks step completion, validates piece-to-step matches |
| PieceHandler.cs | Listens for SDK piece events, identifies type, routes to RecipeManager |
| InteractionLogger.cs | Writes CSV log of every piece event |
| UIManager.cs | Screen transitions, step highlighting, feedback, score display |
| RecipeData.cs | Data classes for recipes and steps |
| StepZone.cs | Component on each step UI element — defines hit area for piece detection |

### Data (Assets/Data/)
- recipes.json — All recipe definitions

## CSV Log Format
```
timestamp,event_type,piece_glyph_id,piece_name,contact_id,screen_x,screen_y,rotation_deg,target_step_index,was_correct,difficulty,recipe_name
```

## Constraints
- All dev uses Board Simulator — no hardware
- Collaborative, never competitive
- Learning must be intrinsic to gameplay
- Log every piece interaction to CSV
- Use existing Chop Chop pieces only
- Deliverables: prototype, CSV log, 3-5 min demo video, README

## Decision Log
Append all decisions to DECISION_LOG.md with timestamp and reasoning.

## References
- Board SDK Docs: https://docs.dev.board.fun
- See GAME_DESIGN.md for full game mechanics
- See STEPS.md for implementation order