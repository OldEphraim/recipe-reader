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
      "id": "fruit_salad",
      "name": "Rainbow Fruit Salad",
      "difficulty": "easy",
      "steps": [
        {
          "index": 0,
          "text": "Slice the bananas into coins.",
          "verb": "slice",
          "correctPiece": 3,
          "displayText": "Slice the bananas into coins.",
          "completedText": "Slice the bananas into coins."
        },
        {
          "index": 1,
          "text": "Stir the yogurt until it's smooth.",
          "verb": "stir",
          "correctPiece": 0,
          "displayText": "Stir the yogurt until it's smooth.",
          "completedText": "Stir the yogurt until it's smooth."
        },
        {
          "index": 2,
          "text": "Sprinkle sugar over the berries.",
          "verb": "sprinkle",
          "correctPiece": 2,
          "displayText": "Sprinkle sugar over the berries.",
          "completedText": "Sprinkle sugar over the berries."
        },
        {
          "index": 3,
          "text": "Wash the apples under the tap.",
          "verb": "wash",
          "correctPiece": 1,
          "displayText": "Wash the apples under the tap.",
          "completedText": "Wash the apples under the tap."
        }
      ]
    },
    {
      "id": "cheesy_toast",
      "name": "Cheesy Garlic Toast",
      "difficulty": "easy",
      "steps": [
        {
          "index": 0,
          "text": "Chop the chives into tiny bits.",
          "verb": "chop",
          "correctPiece": 3,
          "displayText": "Chop the chives into tiny bits.",
          "completedText": "Chop the chives into tiny bits."
        },
        {
          "index": 1,
          "text": "Mix the butter and garlic together.",
          "verb": "mix",
          "correctPiece": 0,
          "displayText": "Mix the butter and garlic together.",
          "completedText": "Mix the butter and garlic together."
        },
        {
          "index": 2,
          "text": "Grind black pepper over the cheese.",
          "verb": "grind",
          "correctPiece": 2,
          "displayText": "Grind black pepper over the cheese.",
          "completedText": "Grind black pepper over the cheese."
        },
        {
          "index": 3,
          "text": "Wipe the crumbs off the cutting board.",
          "verb": "wipe",
          "correctPiece": 1,
          "displayText": "Wipe the crumbs off the cutting board.",
          "completedText": "Wipe the crumbs off the cutting board."
        }
      ]
    },
    {
      "id": "veggie_tacos",
      "name": "Crunchy Veggie Tacos",
      "difficulty": "medium",
      "steps": [
        {
          "index": 0,
          "text": "Dice the red onion into small squares.",
          "verb": "dice",
          "correctPiece": 3,
          "displayText": "Dice the red onion into small squares.",
          "completedText": "Dice the red onion into small squares."
        },
        {
          "index": 1,
          "text": "Scoop the black beans into a warm pan.",
          "verb": "scoop",
          "correctPiece": 0,
          "displayText": "Scoop the black beans into a warm pan.",
          "completedText": "Scoop the black beans into a warm pan."
        },
        {
          "index": 2,
          "text": "Season the filling with a pinch of cumin.",
          "verb": "season",
          "correctPiece": 2,
          "displayText": "Season the filling with a pinch of cumin.",
          "completedText": "Season the filling with a pinch of cumin."
        },
        {
          "index": 3,
          "text": "Rinse the lettuce leaves under cool water.",
          "verb": "rinse",
          "correctPiece": 1,
          "displayText": "Rinse the lettuce leaves under cool water.",
          "completedText": "Rinse the lettuce leaves under cool water."
        },
        {
          "index": 4,
          "text": "Slice the ripe avocado into thin wedges.",
          "verb": "slice",
          "correctPiece": 3,
          "displayText": "Slice the ripe avocado into thin wedges.",
          "completedText": "Slice the ripe avocado into thin wedges."
        }
      ]
    },
    {
      "id": "smoothie_bowl",
      "name": "Sunrise Smoothie Bowl",
      "difficulty": "medium",
      "steps": [
        {
          "index": 0,
          "text": "Scrub the strawberries to get them squeaky clean.",
          "verb": "scrub",
          "correctPiece": 1,
          "displayText": "Scrub the strawberries to get them squeaky clean.",
          "completedText": "Scrub the strawberries to get them squeaky clean."
        },
        {
          "index": 1,
          "text": "Chop the frozen mango into big chunks.",
          "verb": "chop",
          "correctPiece": 3,
          "displayText": "Chop the frozen mango into big chunks.",
          "completedText": "Chop the frozen mango into big chunks."
        },
        {
          "index": 2,
          "text": "Fold the granola gently into the yogurt.",
          "verb": "fold",
          "correctPiece": 0,
          "displayText": "Fold the granola gently into the yogurt.",
          "completedText": "Fold the granola gently into the yogurt."
        },
        {
          "index": 3,
          "text": "Crush a handful of almonds for the topping.",
          "verb": "crush",
          "correctPiece": 2,
          "displayText": "Crush a handful of almonds for the topping.",
          "completedText": "Crush a handful of almonds for the topping."
        },
        {
          "index": 4,
          "text": "Mix the honey into the smoothie base.",
          "verb": "mix",
          "correctPiece": 0,
          "displayText": "Mix the honey into the smoothie base.",
          "completedText": "Mix the honey into the smoothie base."
        }
      ]
    },
    {
      "id": "mystery_stew",
      "name": "Mystery Winter Stew",
      "difficulty": "hard",
      "steps": [
        {
          "index": 0,
          "text": "_____ the carrots into thick coins.",
          "verb": "slice",
          "correctPiece": 3,
          "displayText": "_____ the carrots into thick coins.",
          "completedText": "Slice the carrots into thick coins."
        },
        {
          "index": 1,
          "text": "_____ the broth while it simmers on the stove.",
          "verb": "stir",
          "correctPiece": 0,
          "displayText": "_____ the broth while it simmers on the stove.",
          "completedText": "Stir the broth while it simmers on the stove."
        },
        {
          "index": 2,
          "text": "_____ a pinch of rosemary over the top.",
          "verb": "sprinkle",
          "correctPiece": 2,
          "displayText": "_____ a pinch of rosemary over the top.",
          "completedText": "Sprinkle a pinch of rosemary over the top."
        },
        {
          "index": 3,
          "text": "_____ the mushrooms under cool running water.",
          "verb": "rinse",
          "correctPiece": 1,
          "displayText": "_____ the mushrooms under cool running water.",
          "completedText": "Rinse the mushrooms under cool running water."
        },
        {
          "index": 4,
          "text": "_____ the garlic cloves into tiny, even pieces.",
          "verb": "mince",
          "correctPiece": 3,
          "displayText": "_____ the garlic cloves into tiny, even pieces.",
          "completedText": "Mince the garlic cloves into tiny, even pieces."
        },
        {
          "index": 5,
          "text": "_____ the dumplings into the pot one at a time.",
          "verb": "scoop",
          "correctPiece": 0,
          "displayText": "_____ the dumplings into the pot one at a time.",
          "completedText": "Scoop the dumplings into the pot one at a time."
        }
      ]
    },
    {
      "id": "garden_salad",
      "name": "Secret Garden Salad",
      "difficulty": "hard",
      "steps": [
        {
          "index": 0,
          "text": "_____ the cucumber into round slices.",
          "verb": "cut",
          "correctPiece": 3,
          "displayText": "_____ the cucumber into round slices.",
          "completedText": "Cut the cucumber into round slices."
        },
        {
          "index": 1,
          "text": "_____ the dressing until it looks creamy.",
          "verb": "mix",
          "correctPiece": 0,
          "displayText": "_____ the dressing until it looks creamy.",
          "completedText": "Mix the dressing until it looks creamy."
        },
        {
          "index": 2,
          "text": "_____ the peppercorns for a fresh kick.",
          "verb": "grind",
          "correctPiece": 2,
          "displayText": "_____ the peppercorns for a fresh kick.",
          "completedText": "Grind the peppercorns for a fresh kick."
        },
        {
          "index": 3,
          "text": "_____ the lettuce leaves to get rid of grit.",
          "verb": "wash",
          "correctPiece": 1,
          "displayText": "_____ the lettuce leaves to get rid of grit.",
          "completedText": "Wash the lettuce leaves to get rid of grit."
        },
        {
          "index": 4,
          "text": "_____ the tomatoes into bite-sized cubes.",
          "verb": "dice",
          "correctPiece": 3,
          "displayText": "_____ the tomatoes into bite-sized cubes.",
          "completedText": "Dice the tomatoes into bite-sized cubes."
        },
        {
          "index": 5,
          "text": "_____ the croutons gently into the bowl last.",
          "verb": "fold",
          "correctPiece": 0,
          "displayText": "_____ the croutons gently into the bowl last.",
          "completedText": "Fold the croutons gently into the bowl last."
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