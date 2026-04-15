using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    [Header("References")]
    public PieceHandler pieceHandler;
    public UIManager uiManager;

    public Recipe CurrentRecipe { get; private set; }
    public string CurrentDifficulty { get; private set; }

    public int SelectedStepIndex { get; private set; } = -1;
    public int WrongAttempts { get; private set; }
    public int CorrectCount { get; private set; }
    public float ElapsedTime { get; private set; }
    public int TotalSteps => CurrentRecipe != null && CurrentRecipe.steps != null ? CurrentRecipe.steps.Count : 0;

    private bool[] completion;
    private bool isActive;
    private int littleChefContactId = -1;

    void OnEnable()
    {
        if (pieceHandler != null)
        {
            pieceHandler.OnPiecePlaced += HandlePiecePlaced;
            pieceHandler.OnPieceLifted += HandlePieceLifted;
            pieceHandler.OnPieceMoved += HandlePieceMoved;
        }
    }

    void OnDisable()
    {
        if (pieceHandler != null)
        {
            pieceHandler.OnPiecePlaced -= HandlePiecePlaced;
            pieceHandler.OnPieceLifted -= HandlePieceLifted;
            pieceHandler.OnPieceMoved -= HandlePieceMoved;
        }
    }

    void Update()
    {
        if (isActive) ElapsedTime += Time.deltaTime;
    }

    public void BeginRecipe(Recipe recipe, string difficulty)
    {
        CurrentRecipe = recipe;
        CurrentDifficulty = difficulty;
        SelectedStepIndex = -1;
        WrongAttempts = 0;
        CorrectCount = 0;
        ElapsedTime = 0f;
        littleChefContactId = -1;
        completion = new bool[TotalSteps];
        isActive = true;

        if (uiManager != null) uiManager.UpdateScore(0, 0);
    }

    public void EndRecipe()
    {
        isActive = false;
    }

    private void HandlePiecePlaced(int glyphId, Vector2 screenPos, int contactId)
    {
        if (!isActive || CurrentRecipe == null) return;

        if (PieceIdentifier.IsSelector(glyphId))
        {
            int idx = HitTest(screenPos);
            littleChefContactId = contactId;
            if (idx >= 0) SelectStep(idx);
            return;
        }

        if (PieceIdentifier.IsUtensil(glyphId))
        {
            TryPlaceUtensil(glyphId, screenPos, contactId);
        }
    }

    private void HandlePieceMoved(int glyphId, Vector2 screenPos, int contactId)
    {
        if (!isActive || CurrentRecipe == null) return;

        if (PieceIdentifier.IsSelector(glyphId) && contactId == littleChefContactId)
        {
            int idx = HitTest(screenPos);
            if (idx != SelectedStepIndex)
            {
                if (idx >= 0) SelectStep(idx);
                else DeselectStep();
            }
        }
    }

    private void HandlePieceLifted(int glyphId, Vector2 screenPos, int contactId)
    {
        if (PieceIdentifier.IsSelector(glyphId) && contactId == littleChefContactId)
        {
            littleChefContactId = -1;
            DeselectStep();
        }
    }

    public void SelectStep(int stepIndex)
    {
        if (stepIndex < 0 || stepIndex >= TotalSteps) return;
        if (completion != null && stepIndex < completion.Length && completion[stepIndex]) return;

        if (SelectedStepIndex == stepIndex) return;

        if (SelectedStepIndex >= 0 && uiManager != null)
        {
            var prev = uiManager.GetCardForStepIndex(SelectedStepIndex);
            if (prev != null) prev.SetSelected(false);
        }

        SelectedStepIndex = stepIndex;
        if (uiManager != null)
        {
            var card = uiManager.GetCardForStepIndex(stepIndex);
            if (card != null) card.SetSelected(true);
        }
    }

    public void DeselectStep()
    {
        if (SelectedStepIndex < 0) return;
        if (uiManager != null)
        {
            var card = uiManager.GetCardForStepIndex(SelectedStepIndex);
            if (card != null) card.SetSelected(false);
        }
        SelectedStepIndex = -1;
    }

    public void TryPlaceUtensil(int glyphId, Vector2 screenPos, int contactId = -1)
    {
        if (CurrentRecipe == null) return;

        if (SelectedStepIndex < 0)
        {
            Debug.Log("[RecipeManager] No step selected — place the Little Chef on a step first.");
            LogAttempt("utensil_no_selection", glyphId, screenPos, contactId, -1, false);
            return;
        }

        var step = CurrentRecipe.steps[SelectedStepIndex];
        bool correct = step.correctPiece == glyphId;

        if (correct)
        {
            completion[SelectedStepIndex] = true;
            CorrectCount++;

            if (uiManager != null)
            {
                var card = uiManager.GetCardForStepIndex(SelectedStepIndex);
                if (card != null) card.SetCompleted(true);
                uiManager.UpdateScore(CorrectCount, WrongAttempts);
            }

            LogAttempt("correct", glyphId, screenPos, contactId, SelectedStepIndex, true);

            int completedIdx = SelectedStepIndex;
            SelectedStepIndex = -1;

            if (CheckCompletion())
            {
                EndRecipe();
                if (GameManager.Instance != null) GameManager.Instance.CompleteRecipe();
            }
            else
            {
                AutoAdvanceSelection(completedIdx);
            }
        }
        else
        {
            WrongAttempts++;
            if (uiManager != null)
            {
                var card = uiManager.GetCardForStepIndex(SelectedStepIndex);
                if (card != null) card.FlashWrong();
                uiManager.UpdateScore(CorrectCount, WrongAttempts);
            }
            LogAttempt("wrong", glyphId, screenPos, contactId, SelectedStepIndex, false);
        }
    }

    private void AutoAdvanceSelection(int fromIndex)
    {
        for (int i = fromIndex + 1; i < TotalSteps; i++)
        {
            if (!completion[i]) { SelectStep(i); return; }
        }
        for (int i = 0; i < fromIndex; i++)
        {
            if (!completion[i]) { SelectStep(i); return; }
        }
    }

    public bool CheckCompletion()
    {
        if (completion == null) return false;
        for (int i = 0; i < completion.Length; i++)
        {
            if (!completion[i]) return false;
        }
        return true;
    }

    private int HitTest(Vector2 screenPos)
    {
        if (uiManager == null) return -1;
        Camera cam = null;
        var canvas = uiManager.GameplayCanvas;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cam = canvas.worldCamera;
        }

        foreach (var card in uiManager.ActiveStepCards)
        {
            if (card == null) continue;
            if (card.ContainsScreenPoint(screenPos, cam))
            {
                return card.StepIndex;
            }
        }
        return -1;
    }

    private void LogAttempt(string eventType, int glyphId, Vector2 screenPos, int contactId, int targetStepIndex, bool wasCorrect)
    {
        if (InteractionLogger.Instance == null) return;
        float rotation = 0f;
        if (pieceHandler != null && contactId >= 0)
        {
            rotation = pieceHandler.GetOrientationDegrees(contactId);
        }
        InteractionLogger.Instance.LogEvent(
            eventType,
            glyphId,
            PieceIdentifier.GetPieceName(glyphId),
            contactId,
            screenPos,
            rotation,
            targetStepIndex,
            wasCorrect,
            CurrentDifficulty ?? "",
            CurrentRecipe != null ? CurrentRecipe.name : ""
        );
    }
}
