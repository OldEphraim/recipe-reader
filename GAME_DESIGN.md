# Recipe Reader — Game Design Document

## Core Concept
Players read recipe instructions together and place the correct kitchen utensil on each step. The fun is in reading, discussing, and physically interacting — the game teaches vocabulary and reading comprehension through the act of cooking.

## Core Loop
1. Screen displays a recipe with 4-6 steps arranged as cards
2. Players read each step aloud and discuss which utensil it requires
3. One player places the Little Chef (Glyph 4) on a step to SELECT it (the step highlights)
4. The other player places the matching utensil (Knife/Spoon/Sponge/Spice Mill) on the same step
5. If correct: satisfying animation, step marked complete, move to next
6. If wrong: gentle "oops" feedback (e.g., soup splashes, flour poof), piece must be lifted and try again
7. Complete all steps to finish the recipe and see results

## Why Two-Step Interaction (Little Chef + Utensil)
- Forces both players to participate (one selects, one answers)
- Creates natural turn-taking and discussion
- Players physically hand pieces to each other
- Little Chef acts as a shared cursor both players can see

## Piece Roles

### Utensil Pieces (Answer Pieces)
Each utensil matches a family of cooking action verbs:

**Knife (Glyph 3):** chop, slice, dice, cut, mince, julienne, carve, trim, quarter, halve
**Spoon (Glyph 0):** stir, mix, scoop, fold, ladle, taste, blend, combine, whisk, swirl
**Spice Mill (Glyph 2):** grind, crush, season, sprinkle, grate, zest, dust, pepper, spice, crack
**Sponge (Glyph 1):** clean, wash, scrub, rinse, wipe, soak, dry, polish, tidy, sanitize

### Little Chef (Glyph 4) — Selector Piece
- Place on a step card to select/highlight it
- When Little Chef is on a step, that step "activates" (glows, enlarges slightly)
- Lifting the Little Chef deselects the step
- Only one step can be selected at a time

## Difficulty Levels

### Easy (Ages 6-8) — "Junior Chef"
- Steps use simple, obvious verbs: "Chop the carrots"
- Each step is one short sentence
- Only one correct utensil per step (no ambiguity)
- Visual icon hints next to each step showing the utensil silhouette (can be toggled)
- 4 steps per recipe

### Medium (Ages 8-10) — "Sous Chef"
- Steps use varied vocabulary: "Julienne the bell peppers into thin strips"
- Sentences are longer with more context
- No icon hints
- 5 steps per recipe
- Some steps use less common synonyms that require discussion

### Hard (Ages 10-12) — "Head Chef"
- Steps are full sentences with the action verb MISSING (shown as a blank)
- Players must infer from context: "_____ the garlic until it becomes a fine paste"
- No hints at all
- 6 steps per recipe
- After placing the correct utensil, the missing word fills in (teaching the vocabulary)

## Recipe Data Format (recipes.json)
```json
{
  "recipes": [
    {
      "id": "pancakes",
      "name": "Fluffy Pancakes",
      "difficulty": "easy",
      "steps": [
        {
          "index": 0,
          "text": "Crack the eggs into a big bowl.",
          "verb": "crack",
          "correctPiece": 2,
          "displayText": "Crack the eggs into a big bowl.",
          "completedText": "Crack the eggs into a big bowl."
        },
        {
          "index": 1,
          "text": "Stir the flour and milk together.",
          "verb": "stir",
          "correctPiece": 0,
          "displayText": "Stir the flour and milk together.",
          "completedText": "Stir the flour and milk together."
        },
        {
          "index": 2,
          "text": "Chop some fresh strawberries.",
          "verb": "chop",
          "correctPiece": 3,
          "displayText": "Chop some fresh strawberries.",
          "completedText": "Chop some fresh strawberries."
        },
        {
          "index": 3,
          "text": "Wipe down the countertop before serving.",
          "verb": "wipe",
          "correctPiece": 1,
          "displayText": "Wipe down the countertop before serving.",
          "completedText": "Wipe down the countertop before serving."
        }
      ]
    },
    {
      "id": "pasta_sauce",
      "name": "Garden Pasta Sauce",
      "difficulty": "easy",
      "steps": [
        {
          "index": 0,
          "text": "Dice the tomatoes into small cubes.",
          "verb": "dice",
          "correctPiece": 3,
          "displayText": "Dice the tomatoes into small cubes.",
          "completedText": "Dice the tomatoes into small cubes."
        },
        {
          "index": 1,
          "text": "Mix the sauce with a big spoon.",
          "verb": "mix",
          "correctPiece": 0,
          "displayText": "Mix the sauce with a big spoon.",
          "completedText": "Mix the sauce with a big spoon."
        },
        {
          "index": 2,
          "text": "Sprinkle some cheese on top.",
          "verb": "sprinkle",
          "correctPiece": 2,
          "displayText": "Sprinkle some cheese on top.",
          "completedText": "Sprinkle some cheese on top."
        },
        {
          "index": 3,
          "text": "Rinse the vegetables under cold water.",
          "verb": "rinse",
          "correctPiece": 1,
          "displayText": "Rinse the vegetables under cold water.",
          "completedText": "Rinse the vegetables under cold water."
        }
      ]
    },
    {
      "id": "stir_fry",
      "name": "Veggie Stir Fry",
      "difficulty": "medium",
      "steps": [
        {
          "index": 0,
          "text": "Julienne the carrots into thin matchstick strips.",
          "verb": "julienne",
          "correctPiece": 3,
          "displayText": "Julienne the carrots into thin matchstick strips.",
          "completedText": "Julienne the carrots into thin matchstick strips."
        },
        {
          "index": 1,
          "text": "Fold the egg mixture gently into the vegetables.",
          "verb": "fold",
          "correctPiece": 0,
          "displayText": "Fold the egg mixture gently into the vegetables.",
          "completedText": "Fold the egg mixture gently into the vegetables."
        },
        {
          "index": 2,
          "text": "Zest a lime over the finished dish for brightness.",
          "verb": "zest",
          "correctPiece": 2,
          "displayText": "Zest a lime over the finished dish for brightness.",
          "completedText": "Zest a lime over the finished dish for brightness."
        },
        {
          "index": 3,
          "text": "Scrub the cutting board clean between ingredients.",
          "verb": "scrub",
          "correctPiece": 1,
          "displayText": "Scrub the cutting board clean between ingredients.",
          "completedText": "Scrub the cutting board clean between ingredients."
        },
        {
          "index": 4,
          "text": "Ladle the hot sauce into a serving bowl.",
          "verb": "ladle",
          "correctPiece": 0,
          "displayText": "Ladle the hot sauce into a serving bowl.",
          "completedText": "Ladle the hot sauce into a serving bowl."
        }
      ]
    },
    {
      "id": "soup",
      "name": "Mystery Soup",
      "difficulty": "hard",
      "steps": [
        {
          "index": 0,
          "text": "_____ the onion until you have tiny, even pieces.",
          "verb": "mince",
          "correctPiece": 3,
          "displayText": "_____ the onion until you have tiny, even pieces.",
          "completedText": "Mince the onion until you have tiny, even pieces."
        },
        {
          "index": 1,
          "text": "_____ the pot slowly so nothing sticks to the bottom.",
          "verb": "stir",
          "correctPiece": 0,
          "displayText": "_____ the pot slowly so nothing sticks to the bottom.",
          "completedText": "Stir the pot slowly so nothing sticks to the bottom."
        },
        {
          "index": 2,
          "text": "_____ the peppercorns until they are a fine powder.",
          "verb": "grind",
          "correctPiece": 2,
          "displayText": "_____ the peppercorns until they are a fine powder.",
          "completedText": "Grind the peppercorns until they are a fine powder."
        },
        {
          "index": 3,
          "text": "_____ the celery stalks under running water before using them.",
          "verb": "wash",
          "correctPiece": 1,
          "displayText": "_____ the celery stalks under running water before using them.",
          "completedText": "Wash the celery stalks under running water before using them."
        },
        {
          "index": 4,
          "text": "_____ the potatoes into quarters before adding to the pot.",
          "verb": "quarter",
          "correctPiece": 3,
          "displayText": "_____ the potatoes into quarters before adding to the pot.",
          "completedText": "Quarter the potatoes into quarters before adding to the pot."
        },
        {
          "index": 5,
          "text": "_____ everything together one final time before serving.",
          "verb": "blend",
          "correctPiece": 0,
          "displayText": "_____ everything together one final time before serving.",
          "completedText": "Blend everything together one final time before serving."
        }
      ]
    }
  ]
}
```

## UI Layout (1920x1080 Landscape)

### Title Screen
- Game title "Recipe Reader" centered, large friendly font
- Subtitle "A Cooking Adventure for Two!"
- Three difficulty buttons stacked: Junior Chef / Sous Chef / Head Chef
- All interaction via finger touch (no pieces needed on this screen)

### Gameplay Screen
- TOP: Recipe name banner (e.g., "Fluffy Pancakes") with difficulty badge
- LEFT HALF (960x~800): Recipe steps displayed as vertical card stack
  - Each step is a rounded card with the instruction text
  - Incomplete steps: white/cream background
  - Selected step (Little Chef on it): golden glow border, slightly enlarged
  - Completed step: green background with checkmark
  - Wrong answer: brief red flash, then back to white
- RIGHT HALF (960x~800): Visual "cooking scene" area
  - Simple illustration of a kitchen counter/cutting board
  - Shows animated feedback when pieces are correctly placed
  - Completed dishes build up here as steps finish
- BOTTOM BAR: Shows all 5 piece silhouettes as reminders of what's available
- SCORE: Running count of correct/incorrect attempts in corner

### Results Screen
- "Recipe Complete!" header with celebratory animation
- Stars rating (1-3 based on incorrect attempts: 0 wrong = 3 stars, 1-2 = 2 stars, 3+ = 1 star)
- Stats: time taken, accuracy percentage
- Buttons: "Cook Another!" (new recipe) / "Change Difficulty" / "Quit"
- Finger touch interaction only

## Piece Detection Logic

### Hit Testing
When a piece is placed (phase == Began), convert its screenPosition to UI coordinates and check which step card's RectTransform contains that position. If the piece is the Little Chef, select that step. If it's a utensil, validate against the selected step's correctPiece.

### Validation Flow
1. Little Chef placed on Step X → Step X becomes "selected" (highlighted)
2. Utensil placed while Step X is selected:
   - If utensil.glyphId == step.correctPiece → CORRECT
   - Else → INCORRECT
3. If utensil placed with NO step selected → gentle prompt "Place the Little Chef on a step first!"
4. If Little Chef moved to different step → old step deselects, new step selects
5. After correct answer, Little Chef automatically "bounces" to next incomplete step (or player moves it)

### Edge Cases
- Two pieces placed simultaneously: process Little Chef first, then utensil
- Piece placed outside any step zone: ignore, no feedback
- All steps complete: trigger Results screen
- Piece lifted and re-placed: treat as new interaction for logging

## Collaborative Design Elements
- Only one Little Chef means players must negotiate who holds it
- Players should read steps aloud to each other
- Discussion about unfamiliar vocabulary is the learning moment
- The physical handoff of pieces creates natural turn-taking
- On Hard difficulty, both players puzzle over the blank together

## Audio (stretch goal)
- Correct placement: satisfying "ding" + cooking sound effect (sizzle, chop, etc.)
- Wrong placement: gentle "whoops" sound
- Recipe complete: celebratory jingle
- Background: light kitchen ambiance