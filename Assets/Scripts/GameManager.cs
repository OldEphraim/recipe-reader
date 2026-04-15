using UnityEngine;
using Board.Core;

public enum GameState
{
    Title,
    DifficultySelect,
    Gameplay,
    Results
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Data")]
    [Tooltip("Drag recipes.json here, or leave empty to load from Resources/recipes.")]
    public TextAsset recipesJson;
    public string resourcesRecipesPath = "recipes";

    [Header("References")]
    public UIManager uiManager;
    public RecipeManager recipeManager;

    public GameState State { get; private set; } = GameState.Title;
    public RecipeCollection Recipes { get; private set; }
    public string CurrentDifficulty { get; private set; }
    public Recipe CurrentRecipe { get; private set; }

    private readonly System.Collections.Generic.Dictionary<string, int> difficultyCursor = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Application.targetFrameRate = 60;

        LoadRecipes();
    }

    void Start()
    {
        BoardApplication.SetPauseScreenContext(
            applicationName: "Recipe Reader",
            showSaveOptionUponExit: false
        );
        BoardApplication.pauseScreenActionReceived += OnPauseScreenAction;

        SetState(GameState.Title);
    }

    void OnDestroy()
    {
        BoardApplication.pauseScreenActionReceived -= OnPauseScreenAction;
        if (Instance == this) Instance = null;
    }

    private void LoadRecipes()
    {
        if (recipesJson != null)
        {
            Recipes = RecipeCollection.LoadFromTextAsset(recipesJson);
        }
        else
        {
            Recipes = RecipeCollection.LoadFromResources(resourcesRecipesPath);
        }

        if (Recipes == null || Recipes.recipes == null || Recipes.recipes.Count == 0)
        {
            Debug.LogError("[GameManager] No recipes loaded. Assign recipesJson or place recipes.json in a Resources folder.");
        }
    }

    public void SetState(GameState newState)
    {
        State = newState;
        if (uiManager == null) return;

        switch (newState)
        {
            case GameState.Title:
                uiManager.ShowTitle();
                break;
            case GameState.DifficultySelect:
                uiManager.ShowDifficultySelect();
                break;
            case GameState.Gameplay:
                if (CurrentRecipe != null) uiManager.ShowGameplay(CurrentRecipe);
                break;
            case GameState.Results:
                break;
        }
    }

    public void ShowDifficultySelect()
    {
        SetState(GameState.DifficultySelect);
    }

    public void ReturnToTitle()
    {
        CurrentRecipe = null;
        SetState(GameState.Title);
    }

    public void StartGame(string difficulty)
    {
        CurrentDifficulty = difficulty;
        CurrentRecipe = PickNextRecipe(difficulty);

        if (CurrentRecipe == null)
        {
            Debug.LogWarning($"[GameManager] No recipes for difficulty '{difficulty}'.");
            return;
        }

        if (recipeManager != null)
        {
            recipeManager.BeginRecipe(CurrentRecipe, difficulty);
        }
        SetState(GameState.Gameplay);
    }

    private Recipe PickNextRecipe(string difficulty)
    {
        if (Recipes == null) return null;
        var list = Recipes.GetByDifficulty(difficulty);
        if (list.Count == 0) return null;

        if (!difficultyCursor.TryGetValue(difficulty, out int idx)) idx = 0;
        var recipe = list[idx % list.Count];
        difficultyCursor[difficulty] = (idx + 1) % list.Count;
        return recipe;
    }

    public void CompleteRecipe()
    {
        SetState(GameState.Results);
        if (uiManager != null && recipeManager != null)
        {
            uiManager.ShowResults(
                recipeManager.CorrectCount,
                recipeManager.TotalSteps,
                recipeManager.WrongAttempts,
                recipeManager.ElapsedTime
            );
        }
    }

    public void PlayAnother()
    {
        if (!string.IsNullOrEmpty(CurrentDifficulty))
        {
            StartGame(CurrentDifficulty);
        }
        else
        {
            ShowDifficultySelect();
        }
    }

    private void OnPauseScreenAction(BoardPauseAction action, BoardPauseAudioTrack[] audioTracks)
    {
        switch (action)
        {
            case BoardPauseAction.Resume:
                break;
            case BoardPauseAction.ExitGameUnsaved:
            case BoardPauseAction.ExitGameSaved:
                BoardApplication.Exit();
                break;
        }
    }
}
