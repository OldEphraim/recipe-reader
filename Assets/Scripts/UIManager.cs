using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject titlePanel;
    public GameObject difficultyPanel;
    public GameObject gameplayPanel;
    public GameObject resultsPanel;

    [Header("Title Panel")]
    public Button titlePlayButton;

    [Header("Difficulty Panel")]
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;
    public Button difficultyBackButton;

    [Header("Gameplay Panel")]
    public TMP_Text recipeNameText;
    public TMP_Text difficultyBadgeText;
    public TMP_Text scoreText;
    public TMP_Text littleChefPromptText;
    public RectTransform stepCardContainer;
    public StepCardUI stepCardPrefab;
    public Canvas gameplayCanvas;

    [Header("Results Panel")]
    public TMP_Text resultsHeaderText;
    public TMP_Text resultsStatsText;
    public Image[] starImages;
    public Button playAnotherButton;
    public Button changeDifficultyButton;
    public Button quitButton;

    private readonly List<StepCardUI> activeStepCards = new();

    public IReadOnlyList<StepCardUI> ActiveStepCards => activeStepCards;
    public Canvas GameplayCanvas => gameplayCanvas;

    void Start()
    {
        WireButtons();
        ShowTitle();
    }

    private void WireButtons()
    {
        Wire(titlePlayButton, () => GameManager.Instance?.ShowDifficultySelect());
        Wire(easyButton, () => GameManager.Instance?.StartGame("easy"));
        Wire(mediumButton, () => GameManager.Instance?.StartGame("medium"));
        Wire(hardButton, () => GameManager.Instance?.StartGame("hard"));
        Wire(difficultyBackButton, () => GameManager.Instance?.ReturnToTitle());
        Wire(playAnotherButton, () => GameManager.Instance?.PlayAnother());
        Wire(changeDifficultyButton, () => GameManager.Instance?.ShowDifficultySelect());
        Wire(quitButton, () => Board.Core.BoardApplication.Exit());
    }

    private static void Wire(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null) return;
        // Clear first so hot-reload / re-enable does not stack duplicate listeners.
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }

    private void SetOnlyActive(GameObject panel)
    {
        if (titlePanel != null) titlePanel.SetActive(panel == titlePanel);
        if (difficultyPanel != null) difficultyPanel.SetActive(panel == difficultyPanel);
        if (gameplayPanel != null) gameplayPanel.SetActive(panel == gameplayPanel);
        if (resultsPanel != null) resultsPanel.SetActive(panel == resultsPanel);
    }

    public void ShowTitle() => SetOnlyActive(titlePanel);
    public void ShowDifficultySelect() => SetOnlyActive(difficultyPanel);

    public void ShowGameplay(Recipe recipe)
    {
        SetOnlyActive(gameplayPanel);
        if (recipe == null) return;

        if (recipeNameText != null) recipeNameText.text = recipe.name;
        if (difficultyBadgeText != null) difficultyBadgeText.text = recipe.difficulty.ToUpper();

        BuildStepCards(recipe);
        UpdateScore(0, 0);
        ShowLittleChefPrompt();
    }

    public void ShowLittleChefPrompt()
    {
        if (littleChefPromptText != null)
        {
            littleChefPromptText.text = "Place the Little Chef on a step to begin!";
            littleChefPromptText.gameObject.SetActive(true);
        }
    }

    public void HideLittleChefPrompt()
    {
        if (littleChefPromptText != null)
        {
            littleChefPromptText.gameObject.SetActive(false);
        }
    }

    private void BuildStepCards(Recipe recipe)
    {
        foreach (var card in activeStepCards)
        {
            if (card != null) Destroy(card.gameObject);
        }
        activeStepCards.Clear();

        if (stepCardContainer == null || stepCardPrefab == null || recipe.steps == null) return;

        foreach (var step in recipe.steps)
        {
            var card = Instantiate(stepCardPrefab, stepCardContainer);
            card.SetStep(step);
            activeStepCards.Add(card);
        }
    }

    public StepCardUI GetCardForStepIndex(int stepIndex)
    {
        foreach (var c in activeStepCards)
        {
            if (c != null && c.StepIndex == stepIndex) return c;
        }
        return null;
    }

    public void UpdateScore(int correct, int wrong)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Correct: {correct}   Oops: {wrong}";
        }
    }

    public void ShowResults(int score, int totalSteps, int wrongAttempts, float timeElapsed)
    {
        SetOnlyActive(resultsPanel);

        int stars = 3;
        if (wrongAttempts >= 3) stars = 1;
        else if (wrongAttempts >= 1) stars = 2;

        if (resultsHeaderText != null)
            resultsHeaderText.text = "Recipe Complete!";

        float accuracy = 0f;
        int totalAttempts = score + wrongAttempts;
        if (totalAttempts > 0) accuracy = 100f * score / totalAttempts;

        if (resultsStatsText != null)
        {
            int mins = Mathf.FloorToInt(timeElapsed / 60f);
            int secs = Mathf.FloorToInt(timeElapsed % 60f);
            resultsStatsText.text =
                $"Steps: {score}/{totalSteps}\n" +
                $"Accuracy: {accuracy:F0}%\n" +
                $"Time: {mins:00}:{secs:00}";
        }

        if (starImages != null)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] != null) starImages[i].enabled = i < stars;
            }
        }
    }
}
