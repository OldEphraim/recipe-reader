using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Board.Input;

public static class RecipeReaderSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/RecipeReader.unity";
    private const string PrefabPath = "Assets/Prefabs/StepCard.prefab";
    private const string RecipesJsonPath = "Assets/Data/recipes.json";

    private static readonly Color BgColor = new Color(0.96f, 0.93f, 0.85f);
    private static readonly Color PanelColor = new Color(1f, 0.99f, 0.95f, 1f);
    private static readonly Color ButtonColor = new Color(1f, 0.82f, 0.36f);
    private static readonly Color CardColor = new Color(1f, 0.98f, 0.92f);
    private static readonly Color GlowColor = new Color(1f, 0.85f, 0.2f, 0.8f);
    private static readonly Color CheckColor = new Color(0.2f, 0.7f, 0.25f, 1f);

    [MenuItem("Tools/Build Recipe Reader Scene")]
    public static void Build()
    {
        EnsureFolder("Assets/Scenes");
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Editor");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // --- Camera already exists from default scene setup; grab it and tint it. ---
        var mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = BgColor;
            mainCam.orthographic = true;
        }

        // --- Step card prefab (created first so UIManager can reference it) ---
        var stepCardPrefab = CreateStepCardPrefab();

        // --- Manager GameObjects ---
        var gameManagerGO = new GameObject("GameManager");
        var gameManager = gameManagerGO.AddComponent<GameManager>();

        var pieceHandlerGO = new GameObject("PieceHandler");
        var pieceHandler = pieceHandlerGO.AddComponent<PieceHandler>();

        var interactionLoggerGO = new GameObject("InteractionLogger");
        var interactionLogger = interactionLoggerGO.AddComponent<InteractionLogger>();
        interactionLogger.pieceHandler = pieceHandler;

        var recipeManagerGO = new GameObject("RecipeManager");
        var recipeManager = recipeManagerGO.AddComponent<RecipeManager>();
        recipeManager.pieceHandler = pieceHandler;

        // --- Canvas + EventSystem ---
        var canvasGO = new GameObject("Canvas", typeof(RectTransform));
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        var eventSystemGO = new GameObject("EventSystem", typeof(EventSystem));
        eventSystemGO.AddComponent<BoardUIInputModule>();

        // --- UIManager lives on Canvas ---
        var uiManager = canvasGO.AddComponent<UIManager>();
        uiManager.gameplayCanvas = canvas;
        uiManager.stepCardPrefab = stepCardPrefab.GetComponent<StepCardUI>();

        // --- Build panels ---
        var titlePanel = CreateTitlePanel(canvasGO.transform, uiManager);
        var difficultyPanel = CreateDifficultyPanel(canvasGO.transform, uiManager);
        var gameplayPanel = CreateGameplayPanel(canvasGO.transform, uiManager);
        var resultsPanel = CreateResultsPanel(canvasGO.transform, uiManager);

        uiManager.titlePanel = titlePanel;
        uiManager.difficultyPanel = difficultyPanel;
        uiManager.gameplayPanel = gameplayPanel;
        uiManager.resultsPanel = resultsPanel;

        titlePanel.SetActive(true);
        difficultyPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        resultsPanel.SetActive(false);

        // --- Wire cross-references ---
        gameManager.uiManager = uiManager;
        gameManager.recipeManager = recipeManager;
        recipeManager.uiManager = uiManager;

        var recipesAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(RecipesJsonPath);
        if (recipesAsset != null)
        {
            gameManager.recipesJson = recipesAsset;
        }
        else
        {
            Debug.LogWarning($"[RecipeReaderSceneBuilder] {RecipesJsonPath} not found — GameManager.recipesJson left unassigned.");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"[RecipeReaderSceneBuilder] Built scene at {ScenePath}");
        EditorUtility.DisplayDialog("Recipe Reader", $"Scene built at\n{ScenePath}", "OK");
    }

    // ================================================================
    // Prefab
    // ================================================================

    private static GameObject CreateStepCardPrefab()
    {
        var root = new GameObject("StepCard", typeof(RectTransform));
        var rt = (RectTransform)root.transform;
        rt.sizeDelta = new Vector2(900, 140);

        var bg = root.AddComponent<Image>();
        bg.color = CardColor;
        bg.raycastTarget = false;

        var layout = root.AddComponent<LayoutElement>();
        layout.preferredHeight = 140;
        layout.minHeight = 120;

        // Glow border (child, disabled by default)
        var glow = new GameObject("GlowBorder", typeof(RectTransform));
        glow.transform.SetParent(root.transform, false);
        var glowRt = (RectTransform)glow.transform;
        glowRt.anchorMin = Vector2.zero;
        glowRt.anchorMax = Vector2.one;
        glowRt.offsetMin = new Vector2(-8, -8);
        glowRt.offsetMax = new Vector2(8, 8);
        var glowImg = glow.AddComponent<Image>();
        glowImg.color = GlowColor;
        glowImg.raycastTarget = false;
        glow.SetActive(true);
        glowImg.enabled = false;
        glow.transform.SetAsFirstSibling();

        // Text
        var textGO = new GameObject("DisplayText", typeof(RectTransform));
        textGO.transform.SetParent(root.transform, false);
        var textRt = (RectTransform)textGO.transform;
        textRt.anchorMin = new Vector2(0, 0);
        textRt.anchorMax = new Vector2(1, 1);
        textRt.offsetMin = new Vector2(40, 20);
        textRt.offsetMax = new Vector2(-120, -20);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "Step text";
        tmp.fontSize = 42;
        tmp.color = new Color(0.15f, 0.12f, 0.08f);
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.enableWordWrapping = true;
        tmp.raycastTarget = false;
        tmp.font = DefaultFont();

        // Checkmark
        var check = new GameObject("Checkmark", typeof(RectTransform));
        check.transform.SetParent(root.transform, false);
        var checkRt = (RectTransform)check.transform;
        checkRt.anchorMin = new Vector2(1, 0.5f);
        checkRt.anchorMax = new Vector2(1, 0.5f);
        checkRt.pivot = new Vector2(1, 0.5f);
        checkRt.anchoredPosition = new Vector2(-30, 0);
        checkRt.sizeDelta = new Vector2(64, 64);
        var checkImg = check.AddComponent<Image>();
        checkImg.color = CheckColor;
        checkImg.raycastTarget = false;
        checkImg.enabled = false;

        var checkText = new GameObject("CheckText", typeof(RectTransform));
        checkText.transform.SetParent(check.transform, false);
        var ctRt = (RectTransform)checkText.transform;
        ctRt.anchorMin = Vector2.zero;
        ctRt.anchorMax = Vector2.one;
        ctRt.offsetMin = Vector2.zero;
        ctRt.offsetMax = Vector2.zero;
        var ctTmp = checkText.AddComponent<TextMeshProUGUI>();
        ctTmp.text = "✓";
        ctTmp.fontSize = 56;
        ctTmp.color = Color.white;
        ctTmp.alignment = TextAlignmentOptions.Center;
        ctTmp.raycastTarget = false;
        ctTmp.font = DefaultFont();

        // StepCardUI component + wire
        var stepCardUI = root.AddComponent<StepCardUI>();
        stepCardUI.hitArea = rt;
        stepCardUI.displayText = tmp;
        stepCardUI.background = bg;
        stepCardUI.checkmark = checkImg;
        stepCardUI.glowBorder = glowImg;

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    // ================================================================
    // Panels
    // ================================================================

    private static GameObject CreateTitlePanel(Transform parent, UIManager uiManager)
    {
        var panel = CreatePanel("TitlePanel", parent);

        CreateTMP(panel.transform, "Title", "Recipe Reader",
            new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), new Vector2(1400, 240),
            120, TextAlignmentOptions.Center, new Color(0.35f, 0.18f, 0.08f));

        CreateTMP(panel.transform, "Subtitle", "A Cooking Adventure for Two!",
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(1400, 120),
            54, TextAlignmentOptions.Center, new Color(0.4f, 0.25f, 0.15f));

        var playBtn = CreateButton(panel.transform, "PlayButton", "Play",
            new Vector2(0.5f, 0.28f), new Vector2(460, 130), 54);
        uiManager.titlePlayButton = playBtn;

        return panel;
    }

    private static GameObject CreateDifficultyPanel(Transform parent, UIManager uiManager)
    {
        var panel = CreatePanel("DifficultyPanel", parent);

        CreateTMP(panel.transform, "Header", "Choose Your Level",
            new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), new Vector2(1400, 160),
            88, TextAlignmentOptions.Center, new Color(0.35f, 0.18f, 0.08f));

        float y = 0.68f;
        uiManager.easyButton = CreateDifficultyButton(panel.transform, "EasyButton",
            "Junior Chef", "Ages 6-8 • Simple steps with hints", y);
        y -= 0.22f;
        uiManager.mediumButton = CreateDifficultyButton(panel.transform, "MediumButton",
            "Sous Chef", "Ages 8-10 • Richer vocabulary", y);
        y -= 0.22f;
        uiManager.hardButton = CreateDifficultyButton(panel.transform, "HardButton",
            "Head Chef", "Ages 10-12 • Fill in the blank", y);

        var backBtn = CreateButton(panel.transform, "BackButton", "Back",
            new Vector2(0.12f, 0.08f), new Vector2(240, 90), 40);
        uiManager.difficultyBackButton = backBtn;

        return panel;
    }

    private static Button CreateDifficultyButton(Transform parent, string name, string title, string desc, float yAnchor)
    {
        var btn = CreateButton(parent, name, title,
            new Vector2(0.5f, yAnchor), new Vector2(1000, 160), 64);

        var descGO = new GameObject("Description", typeof(RectTransform));
        descGO.transform.SetParent(btn.transform, false);
        var rt = (RectTransform)descGO.transform;
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 16);
        rt.sizeDelta = new Vector2(0, 40);
        var tmp = descGO.AddComponent<TextMeshProUGUI>();
        tmp.text = desc;
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.25f, 0.15f, 0.05f, 0.85f);
        tmp.raycastTarget = false;
        tmp.font = DefaultFont();

        return btn;
    }

    private static GameObject CreateGameplayPanel(Transform parent, UIManager uiManager)
    {
        var panel = CreatePanel("GameplayPanel", parent);

        // Top banner
        var banner = new GameObject("RecipeBanner", typeof(RectTransform));
        banner.transform.SetParent(panel.transform, false);
        var bannerRt = (RectTransform)banner.transform;
        bannerRt.anchorMin = new Vector2(0, 1);
        bannerRt.anchorMax = new Vector2(1, 1);
        bannerRt.pivot = new Vector2(0.5f, 1);
        bannerRt.anchoredPosition = new Vector2(0, 0);
        bannerRt.sizeDelta = new Vector2(0, 120);
        var bannerImg = banner.AddComponent<Image>();
        bannerImg.color = new Color(0.95f, 0.72f, 0.35f);

        uiManager.recipeNameText = CreateTMP(banner.transform, "RecipeName", "Recipe Name",
            new Vector2(0, 0), new Vector2(1, 1), Vector2.zero,
            56, TextAlignmentOptions.Center, Color.white, stretch: true);

        uiManager.difficultyBadgeText = CreateTMP(banner.transform, "DifficultyBadge", "EASY",
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(260, 60),
            32, TextAlignmentOptions.Center, new Color(0.35f, 0.18f, 0.08f),
            anchoredPos: new Vector2(-150, 0));

        uiManager.scoreText = CreateTMP(panel.transform, "ScoreText", "Correct: 0   Oops: 0",
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(500, 60),
            32, TextAlignmentOptions.Right, new Color(0.25f, 0.15f, 0.05f),
            anchoredPos: new Vector2(-40, 40));

        uiManager.littleChefPromptText = CreateTMP(panel.transform, "LittleChefPrompt",
            "Place the Little Chef on a step to begin!",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1400, 120),
            44, TextAlignmentOptions.Center, new Color(0.75f, 0.3f, 0.1f));

        // Left: step card container with VerticalLayoutGroup
        var stepContainer = new GameObject("StepCardContainer", typeof(RectTransform));
        stepContainer.transform.SetParent(panel.transform, false);
        var scRt = (RectTransform)stepContainer.transform;
        scRt.anchorMin = new Vector2(0, 0);
        scRt.anchorMax = new Vector2(0.5f, 1);
        scRt.offsetMin = new Vector2(40, 120);
        scRt.offsetMax = new Vector2(-20, -140);
        var vlg = stepContainer.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.padding = new RectOffset(20, 20, 20, 20);
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperCenter;
        stepContainer.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        uiManager.stepCardContainer = scRt;

        // Right: cooking scene placeholder
        var scene = new GameObject("CookingScene", typeof(RectTransform));
        scene.transform.SetParent(panel.transform, false);
        var sceneRt = (RectTransform)scene.transform;
        sceneRt.anchorMin = new Vector2(0.5f, 0);
        sceneRt.anchorMax = new Vector2(1, 1);
        sceneRt.offsetMin = new Vector2(20, 200);
        sceneRt.offsetMax = new Vector2(-40, -140);
        var sceneImg = scene.AddComponent<Image>();
        sceneImg.color = new Color(1f, 0.95f, 0.85f, 0.6f);
        CreateTMP(scene.transform, "SceneLabel", "Kitchen Counter",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(800, 100),
            44, TextAlignmentOptions.Center, new Color(0.6f, 0.45f, 0.25f));

        // Bottom: piece reminder bar
        var reminderBar = new GameObject("PieceReminder", typeof(RectTransform));
        reminderBar.transform.SetParent(panel.transform, false);
        var rbRt = (RectTransform)reminderBar.transform;
        rbRt.anchorMin = new Vector2(0.5f, 0);
        rbRt.anchorMax = new Vector2(1, 0);
        rbRt.pivot = new Vector2(0.5f, 0);
        rbRt.offsetMin = new Vector2(20, 40);
        rbRt.offsetMax = new Vector2(-40, 160);
        var rbImg = reminderBar.AddComponent<Image>();
        rbImg.color = new Color(1f, 0.88f, 0.6f, 0.7f);
        var rbLayout = reminderBar.AddComponent<HorizontalLayoutGroup>();
        rbLayout.spacing = 16;
        rbLayout.padding = new RectOffset(16, 16, 16, 16);
        rbLayout.childAlignment = TextAnchor.MiddleCenter;
        rbLayout.childForceExpandWidth = false;
        rbLayout.childForceExpandHeight = false;
        string[] pieces = { "Spoon", "Sponge", "Spice Mill", "Knife", "Little Chef" };
        foreach (var p in pieces)
        {
            var slot = new GameObject(p + "Slot", typeof(RectTransform));
            slot.transform.SetParent(reminderBar.transform, false);
            var slotRt = (RectTransform)slot.transform;
            slotRt.sizeDelta = new Vector2(140, 90);
            var slotImg = slot.AddComponent<Image>();
            slotImg.color = new Color(1f, 1f, 1f, 0.7f);
            CreateTMP(slot.transform, "Label", p,
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero,
                26, TextAlignmentOptions.Center, new Color(0.3f, 0.2f, 0.1f), stretch: true);
        }

        return panel;
    }

    private static GameObject CreateResultsPanel(Transform parent, UIManager uiManager)
    {
        var panel = CreatePanel("ResultsPanel", parent);

        uiManager.resultsHeaderText = CreateTMP(panel.transform, "Header", "Recipe Complete!",
            new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.82f), new Vector2(1600, 200),
            110, TextAlignmentOptions.Center, new Color(0.35f, 0.18f, 0.08f));

        // Stars
        var stars = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var star = new GameObject($"Star{i}", typeof(RectTransform));
            star.transform.SetParent(panel.transform, false);
            var rt = (RectTransform)star.transform;
            rt.anchorMin = new Vector2(0.5f, 0.6f);
            rt.anchorMax = new Vector2(0.5f, 0.6f);
            rt.sizeDelta = new Vector2(140, 140);
            rt.anchoredPosition = new Vector2((i - 1) * 180, 0);
            var img = star.AddComponent<Image>();
            img.color = new Color(1f, 0.82f, 0.2f);
            CreateTMP(star.transform, "StarGlyph", "★",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero,
                110, TextAlignmentOptions.Center, Color.white, stretch: true);
            stars[i] = img;
        }
        uiManager.starImages = stars;

        uiManager.resultsStatsText = CreateTMP(panel.transform, "Stats",
            "Steps: 0/0\nAccuracy: 0%\nTime: 00:00",
            new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), new Vector2(1200, 240),
            44, TextAlignmentOptions.Center, new Color(0.25f, 0.15f, 0.05f));

        uiManager.playAnotherButton = CreateButton(panel.transform, "PlayAnotherButton",
            "Cook Another!", new Vector2(0.28f, 0.14f), new Vector2(460, 120), 40);
        uiManager.changeDifficultyButton = CreateButton(panel.transform, "ChangeDifficultyButton",
            "Change Difficulty", new Vector2(0.5f, 0.14f), new Vector2(460, 120), 40);
        uiManager.quitButton = CreateButton(panel.transform, "QuitButton",
            "Quit", new Vector2(0.72f, 0.14f), new Vector2(460, 120), 40);

        return panel;
    }

    // ================================================================
    // Primitives
    // ================================================================

    private static GameObject CreatePanel(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = PanelColor;
        return go;
    }

    private static TextMeshProUGUI CreateTMP(
        Transform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 size,
        float fontSize, TextAlignmentOptions align, Color color,
        bool stretch = false, Vector2? anchoredPos = null)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        if (stretch)
        {
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
        else
        {
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos ?? Vector2.zero;
        }
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = color;
        tmp.enableWordWrapping = true;
        tmp.raycastTarget = false;
        tmp.font = DefaultFont();
        return tmp;
    }

    private static Button CreateButton(Transform parent, string name, string label,
        Vector2 anchor, Vector2 size, float fontSize)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = ButtonColor;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var labelGO = new GameObject("Label", typeof(RectTransform));
        labelGO.transform.SetParent(go.transform, false);
        var lrt = (RectTransform)labelGO.transform;
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.2f, 0.1f, 0.02f);
        tmp.raycastTarget = false;
        tmp.font = DefaultFont();

        return btn;
    }

    // ================================================================
    // Helpers
    // ================================================================

    private static TMP_FontAsset _cachedFont;

    private static TMP_FontAsset DefaultFont()
    {
        if (_cachedFont != null) return _cachedFont;

        string[] candidates =
        {
            "Assets/TextMesh Pro/Resources/Fonts & Materials/Inter SDF.asset",
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset",
        };
        foreach (var path in candidates)
        {
            var f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (f != null) { _cachedFont = f; return f; }
        }

        var guids = AssetDatabase.FindAssets("Inter SDF t:TMP_FontAsset");
        foreach (var g in guids)
        {
            var f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(g));
            if (f != null) { _cachedFont = f; return f; }
        }
        guids = AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset");
        foreach (var g in guids)
        {
            var f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(g));
            if (f != null) { _cachedFont = f; return f; }
        }

        if (TMP_Settings.defaultFontAsset != null)
        {
            _cachedFont = TMP_Settings.defaultFontAsset;
        }
        return _cachedFont;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parent = Path.GetDirectoryName(path).Replace('\\', '/');
        var leaf = Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, leaf);
    }
}
