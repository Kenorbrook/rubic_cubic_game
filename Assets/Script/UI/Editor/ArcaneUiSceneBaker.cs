using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ArcaneUiSceneBaker
{
    private const string MenuScenePath = "Assets/Scenes/Menu.unity";
    private const string GameScenePath = "Assets/Scenes/Game.unity";
    private const string BackgroundPath = "Assets/Resources/Generated/arcane_observatory_bg.png";
    private const string WarriorPath = "Assets/Resources/Generated/Characters/warrior.png";
    private const string MagePath = "Assets/Resources/Generated/Characters/mage.png";
    private const string GoblinPath = "Assets/Resources/Generated/Characters/goblin.png";
    private const string FontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
    private const string MiniGamePrefabPath = "Assets/Prefab/MiniGame.prefab";
    private const string BarPrefabPath = "Assets/Prefab/Bar.prefab";

    private static readonly Color Ink = new(0.025f, 0.035f, 0.075f, 0.96f);
    private static readonly Color Panel = new(0.018f, 0.028f, 0.07f, 0.97f);
    private static readonly Color PanelHover = new(0.105f, 0.145f, 0.24f, 1f);
    private static readonly Color Amber = new(0.91f, 0.67f, 0.29f, 1f);
    private static readonly Color Text = new(0.96f, 0.93f, 0.84f, 1f);
    private static readonly Color MutedText = new(0.73f, 0.75f, 0.82f, 1f);

    private static Sprite _background;
    private static Sprite _warrior;
    private static Sprite _mage;
    private static Sprite _goblin;
    private static TMP_FontAsset _font;

    [MenuItem("Tools/Arcane UI/Bake Menu and Game Scenes")]
    public static void BakeAllScenes()
    {
        var activeScene = SceneManager.GetActiveScene();
        if (activeScene.isDirty)
            throw new InvalidOperationException($"Scene '{activeScene.name}' has unsaved changes. Save or discard them before baking UI.");

        LoadAssets();
        var originalPath = activeScene.path;
        BakeTimingPrefabs();
        BakeMenuScene();
        BakeGameScene();

        if (!string.IsNullOrEmpty(originalPath) && originalPath != GameScenePath)
            EditorSceneManager.OpenScene(originalPath, OpenSceneMode.Single);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Arcane UI baked into Menu and Game scenes.");
    }

    private static void BakeTimingPrefabs()
    {
        var miniGame = PrefabUtility.LoadPrefabContents(MiniGamePrefabPath);
        try
        {
            var miniGameRect = miniGame.transform as RectTransform;
            if (miniGameRect != null)
                SetRect(miniGameRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(560f, 82f), Vector2.zero);

            var marker = miniGame.transform.Cast<Transform>()
                .FirstOrDefault(t => t.GetComponent<Image>() != null && t.GetComponent<RectTransform>() != null && t.name != "Image");
            RectTransform targetZone = null;
            Image targetZoneImage = null;
            if (marker != null)
            {
                marker.name = "TargetZone";
                targetZone = (RectTransform)marker;
                SetRect(targetZone, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(180f, 66f), Vector2.zero);
                targetZoneImage = marker.GetComponent<Image>();
                targetZoneImage.sprite = null;
                targetZoneImage.type = Image.Type.Simple;
                targetZoneImage.color = new Color(Amber.r, Amber.g, Amber.b, 0.18f);
                targetZoneImage.raycastTarget = false;
            }

            var timing = miniGame.GetComponent<WarriorTimingMiniGame>();
            if (timing != null)
            {
                var serialized = new SerializedObject(timing);
                serialized.FindProperty("_baseSpeed").floatValue = 95f;
                serialized.FindProperty("_speedIncreasePerLevel").floatValue = 22.77778f;
                serialized.FindProperty("_maxSpeed").floatValue = 300f;
                serialized.FindProperty("_firstLevelSpawnIntervalRange").vector2Value = new Vector2(4.5f, 6f);
                serialized.FindProperty("_spawnIntervalRange").vector2Value = new Vector2(1.5f, 2.4f);
                serialized.FindProperty("_maxDifficultyLevel").intValue = 10;
                serialized.FindProperty("_targetZoneWidth").floatValue = 180f;
                serialized.FindProperty("_rightSwipeStartOffsetFromCenter").floatValue = 90f;
                serialized.FindProperty("_leftSwipeEndOffsetFromCenter").floatValue = -90f;
                serialized.FindProperty("_targetZone").objectReferenceValue = targetZone;
                serialized.FindProperty("_targetZoneImage").objectReferenceValue = targetZoneImage;
                var lane = serialized.FindProperty("_laneRect").objectReferenceValue as RectTransform;
                var bars = serialized.FindProperty("_barsContainer").objectReferenceValue as RectTransform;
                if (lane != null)
                {
                    Stretch(lane);
                    var laneImage = lane.GetComponent<Image>();
                    if (laneImage != null)
                    {
                        laneImage.sprite = null;
                        laneImage.type = Image.Type.Simple;
                        laneImage.color = new Color(Panel.r, Panel.g, Panel.b, 0.96f);
                    }
                }
                if (bars != null)
                    Stretch(bars);
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(miniGame, MiniGamePrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(miniGame);
        }

        var bar = PrefabUtility.LoadPrefabContents(BarPrefabPath);
        try
        {
            var barRect = bar.transform as RectTransform;
            if (barRect != null)
                SetRect(barRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(58f, 58f), Vector2.zero);
            foreach (var image in bar.GetComponentsInChildren<Image>(true))
            {
                var imageRect = image.transform as RectTransform;
                if (imageRect != null && image.transform != bar.transform)
                    Stretch(imageRect);
            }
            PrefabUtility.SaveAsPrefabAsset(bar, BarPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(bar);
        }
    }

    private static void LoadAssets()
    {
        _background = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
        _warrior = AssetDatabase.LoadAssetAtPath<Sprite>(WarriorPath);
        _mage = AssetDatabase.LoadAssetAtPath<Sprite>(MagePath);
        _goblin = AssetDatabase.LoadAssetAtPath<Sprite>(GoblinPath);
        _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (_background == null || _warrior == null || _mage == null || _goblin == null || _font == null)
            throw new InvalidOperationException("Arcane UI assets are missing or not imported as sprites/font assets.");
    }

    private static void BakeMenuScene()
    {
        var scene = EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
        var view = GameObject.Find("View");
        if (view == null)
            throw new InvalidOperationException("Menu scene is missing View canvas.");

        ConfigureCanvas(view.GetComponent<Canvas>(), 0);
        var panel = view.transform.Find("Panel") as RectTransform;
        var choose = view.transform.Find("ChooseCharacter") as RectTransform;
        if (panel == null || choose == null)
            throw new InvalidOperationException("Menu scene is missing Panel or ChooseCharacter.");

        ConfigureBackground(panel);
        ConfigureBackground(choose);
        ClearBaked(panel);
        ClearBaked(choose);
        var oldSettings = view.transform.Find("ArcaneBaked_Settings");
        if (oldSettings != null)
            UnityEngine.Object.DestroyImmediate(oldSettings.gameObject);

        var tint = CreateImage(panel, "ArcaneBaked_MenuTint", new Color(0.01f, 0.018f, 0.045f, 0.32f));
        Stretch(tint.rectTransform);
        tint.transform.SetAsFirstSibling();
        var card = CreateCard(panel, "ArcaneBaked_MainCard", new Vector2(760f, 820f), new Vector2(0f, 10f));
        card.SetSiblingIndex(1);
        CreateLabel(panel, "ArcaneBaked_Title", "КУБ АРКАНУМА", 74f, Text, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1f), new Vector2(840f, 120f), new Vector2(0f, -250f));
        CreateLabel(panel, "ArcaneBaked_Subtitle", "Собирай грани. Побеждай врагов.", 30f, MutedText, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1f), new Vector2(760f, 64f), new Vector2(0f, -340f));

        StyleButton(FindButton(panel, "start game"), "Новая игра", new Vector2(0.5f, 0.5f), new Vector2(0f, 145f), new Vector2(600f, 112f), 34f);
        StyleButton(FindButton(panel, "Continue"), "Продолжить", new Vector2(0.5f, 0.5f), new Vector2(0f, -5f), new Vector2(600f, 112f), 34f);
        var settingsButton = FindButton(panel, "grades");
        StyleButton(settingsButton, "Настройки", new Vector2(0.5f, 0.5f), new Vector2(0f, -155f), new Vector2(600f, 112f), 32f);
        var desktopHint = CreateLabel(panel, "ArcaneBaked_Hint", "Свайп — повернуть слой  •  ПКМ — осмотреть 3D-куб", 28f, MutedText, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0f), new Vector2(900f, 58f), new Vector2(0f, 74f));
        desktopHint.gameObject.AddComponent<HideOnMobile>();

        var settingsBackground = CreateImage(view.transform, "ArcaneBaked_Settings", Color.white);
        settingsBackground.sprite = _background;
        settingsBackground.preserveAspect = false;
        var settingsRoot = settingsBackground.rectTransform;
        Stretch(settingsRoot);
        var settingsTint = CreateImage(settingsRoot, "ArcaneBaked_SettingsTint", new Color(0.01f, 0.018f, 0.045f, 0.42f));
        Stretch(settingsTint.rectTransform);
        var settingsCard = CreateCard(settingsRoot, "ArcaneBaked_SettingsCard", new Vector2(760f, 780f), Vector2.zero);
        CreateLabel(settingsCard, "Title", "НАСТРОЙКИ", 58f, Text, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(650f, 90f), new Vector2(0f, 270f));
        CreateLabel(settingsCard, "Subtitle", "Отображение и управление кубом", 28f, MutedText, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(650f, 60f), new Vector2(0f, 195f));
        var cubeModeButton = CreateButton(settingsCard, "CubeMode", "Куб: 2D");
        StyleButton(cubeModeButton, "Куб: 2D", new Vector2(0.5f, 0.5f), new Vector2(0f, 75f), new Vector2(590f, 105f), 32f);
        var invertVerticalButton = CreateButton(settingsCard, "InvertVertical", "Инверсия по вертикали: выкл");
        StyleButton(invertVerticalButton, "Инверсия по вертикали: выкл", new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(590f, 105f), 30f);
        var settingsBackButton = CreateButton(settingsCard, "Back", "Назад");
        StyleButton(settingsBackButton, "Назад", new Vector2(0.5f, 0.5f), new Vector2(0f, -245f), new Vector2(420f, 96f), 30f);

        var settingsController = view.GetComponent<MainMenuSettingsPanel>();
        if (settingsController == null)
            settingsController = view.AddComponent<MainMenuSettingsPanel>();
        var settingsSerialized = new SerializedObject(settingsController);
        settingsSerialized.FindProperty("_openButton").objectReferenceValue = settingsButton;
        settingsSerialized.FindProperty("_backButton").objectReferenceValue = settingsBackButton;
        settingsSerialized.FindProperty("_cubeModeButton").objectReferenceValue = cubeModeButton;
        settingsSerialized.FindProperty("_invertVerticalButton").objectReferenceValue = invertVerticalButton;
        settingsSerialized.FindProperty("_cubeModeLabel").objectReferenceValue = cubeModeButton.GetComponentInChildren<TMP_Text>();
        settingsSerialized.FindProperty("_invertVerticalLabel").objectReferenceValue = invertVerticalButton.GetComponentInChildren<TMP_Text>();
        settingsSerialized.FindProperty("_mainPanel").objectReferenceValue = panel.gameObject;
        settingsSerialized.FindProperty("_settingsPanel").objectReferenceValue = settingsRoot.gameObject;
        settingsSerialized.ApplyModifiedPropertiesWithoutUndo();
        settingsRoot.gameObject.SetActive(false);

        var chooseTint = CreateImage(choose, "ArcaneBaked_ChooseTint", new Color(0.01f, 0.018f, 0.045f, 0.42f));
        Stretch(chooseTint.rectTransform);
        chooseTint.transform.SetAsFirstSibling();
        CreateLabel(choose, "ArcaneBaked_ChooseTitle", "ВЫБЕРИТЕ ГЕРОЯ", 60f, Text, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1f), new Vector2(820f, 110f), new Vector2(0f, -190f));
        CreateLabel(choose, "ArcaneBaked_ChooseSubtitle", "Стиль боя можно сменить, начав новую игру", 29f, MutedText, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1f), new Vector2(850f, 60f), new Vector2(0f, -275f));
        StyleButton(FindButton(choose, "back"), "Назад", new Vector2(0f, 1f), new Vector2(145f, -100f), new Vector2(240f, 82f), 28f);
        StyleHeroButton(FindButton(choose, "warrior"), "Воин", _warrior, new Vector2(-225f, -35f));
        StyleHeroButton(FindButton(choose, "Mage"), "Маг", _mage, new Vector2(225f, -35f));

        var menuView = view.GetComponent<MenuView>();
        if (menuView != null)
        {
            var serialized = new SerializedObject(menuView);
            serialized.FindProperty("_mainPanel").objectReferenceValue = panel.gameObject;
            serialized.FindProperty("_chooseCharacterPanel").objectReferenceValue = choose.gameObject;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void BakeGameScene()
    {
        var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        var view = GameObject.Find("View");
        var cubeCanvasObject = GameObject.Find("Cube");
        var gameView = view != null ? view.GetComponent<GameView>() : null;
        if (view == null || cubeCanvasObject == null || gameView == null)
            throw new InvalidOperationException("Game scene is missing View, Cube, or GameView.");

        ConfigureCanvas(view.GetComponent<Canvas>(), 0);
        ConfigureCanvas(cubeCanvasObject.GetComponent<Canvas>(), 10);
        ConfigureBackground(view.transform.Find("Background") as RectTransform);
        BakeCharacters();
        BakeBattleHud(view.transform as RectTransform, gameView);
        BakeCube(cubeCanvasObject);
        BakePauseMenu();
        BakeLoseMenu();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void BakeCharacters()
    {
        var player = GameObject.Find("GameCircle/Player") ?? GameObject.Find("Player");
        var enemy = GameObject.Find("GameCircle/Enemy") ?? GameObject.Find("Enemy");
        ConfigureCharacter(player, _warrior, true);
        ConfigureCharacter(enemy, _goblin, false);
    }

    private static void ConfigureCharacter(GameObject character, Sprite sprite, bool facesRight)
    {
        if (character == null)
            return;

        var renderer = character.GetComponent<SpriteRenderer>();
        if (renderer == null)
            return;

        renderer.sprite = sprite;
        renderer.color = Color.white;
        var height = Mathf.Max(0.001f, sprite.bounds.size.y);
        var scale = 3.25f / height;
        character.transform.localScale = new Vector3(facesRight ? scale : -scale, scale, 1f);
        var position = character.transform.localPosition;
        position.y = facesRight ? 1.35f : 1.30f;
        character.transform.localPosition = position;
        foreach (var legacyText in character.GetComponentsInChildren<TMP_Text>(true))
            UnityEngine.Object.DestroyImmediate(legacyText.gameObject);
        EditorUtility.SetDirty(renderer);
    }

    private static void BakeBattleHud(RectTransform view, GameView gameView)
    {
        var previous = view.Find("ArcaneBaked_BattleHud");
        var timing = previous != null ? previous.Find("minigame") : view.Find("minigame");
        var pause = previous != null
            ? previous.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.gameObject.name == "Button")
            : view.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.gameObject.name == "Button");
        if (timing != null && previous != null)
            timing.SetParent(view, false);
        if (pause != null && previous != null)
            pause.transform.SetParent(view, false);
        if (previous != null)
            UnityEngine.Object.DestroyImmediate(previous.gameObject);

        var oldPlayerHp = view.Find("hp");
        var oldShield = view.Find("hp (1)");
        var hud = CreateRect(view, "ArcaneBaked_BattleHud");
        Stretch(hud);
        hud.gameObject.AddComponent<SafeAreaFitter>();
        hud.SetAsLastSibling();

        var playerCard = CreateCard(hud, "PlayerCard", new Vector2(410f, 152f), new Vector2(24f, -24f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        var playerHp = CreateHudLabel(playerCard, "PlayerHp", "125 / 125", 44f, new Vector2(0f, 0.43f), new Vector2(1f, 1f));
        var playerShield = CreateHudLabel(playerCard, "PlayerShield", "", 29f, new Vector2(0f, 0.19f), new Vector2(1f, 0.48f));
        var playerFill = CreateHealthBar(playerCard, new Color(0.24f, 0.72f, 0.48f, 1f));

        var enemyCard = CreateCard(hud, "EnemyCard", new Vector2(410f, 152f), new Vector2(-24f, -24f), new Vector2(1f, 1f), new Vector2(1f, 1f));
        var enemyHp = CreateHudLabel(enemyCard, "EnemyHp", "УР. 1     100 / 100", 34f, new Vector2(0f, 0.39f), new Vector2(1f, 1f));
        var enemyDamage = CreateHudLabel(enemyCard, "EnemyDamage", "", 28f, new Vector2(0f, 0.18f), new Vector2(1f, 0.43f));
        var enemyFill = CreateHealthBar(enemyCard, new Color(0.86f, 0.28f, 0.30f, 1f));

        if (pause == null)
        {
            pause = CreateButton(view, "Button", "Пауза");
            pause.gameObject.layer = view.gameObject.layer;
        }
        if (pause != null)
        {
            pause.transform.SetParent(hud, false);
            StyleButton(pause, "Пауза", new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(190f, 70f), 24f);
            var pauseRect = (RectTransform)pause.transform;
            pauseRect.pivot = new Vector2(0.5f, 1f);
            pauseRect.anchoredPosition = new Vector2(0f, -24f);
        }

        if (timing == null)
            timing = CreateRect(view, "minigame");
        if (timing != null)
        {
            timing.SetParent(hud, false);
            var timingRect = (RectTransform)timing;
            SetRect(timingRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(560f, 82f), new Vector2(0f, -172f));
            var gameCircle = UnityEngine.Object.FindFirstObjectByType<GameCircle>();
            if (gameCircle != null)
            {
                var circleSerialized = new SerializedObject(gameCircle);
                circleSerialized.FindProperty("_warriorMiniGameParent").objectReferenceValue = timingRect;
                circleSerialized.ApplyModifiedPropertiesWithoutUndo();
            }
            foreach (var image in timing.GetComponentsInChildren<Image>(true))
            {
                image.type = Image.Type.Simple;
                image.color = image.gameObject.name.Contains("1") ? Amber : new Color(Panel.r, Panel.g, Panel.b, 0.96f);
            }
        }

        var serialized = new SerializedObject(gameView);
        serialized.FindProperty("_playerHpText").objectReferenceValue = playerHp;
        serialized.FindProperty("_playerShield").objectReferenceValue = playerShield;
        serialized.FindProperty("_enemyHpText").objectReferenceValue = enemyHp;
        serialized.FindProperty("_enemyDamageText").objectReferenceValue = enemyDamage;
        serialized.FindProperty("_playerHpFill").objectReferenceValue = playerFill;
        serialized.FindProperty("_enemyHpFill").objectReferenceValue = enemyFill;
        if (pause != null)
            serialized.FindProperty("_pause").objectReferenceValue = pause;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        if (oldPlayerHp != null)
            UnityEngine.Object.DestroyImmediate(oldPlayerHp.gameObject);
        if (oldShield != null)
            UnityEngine.Object.DestroyImmediate(oldShield.gameObject);
    }

    private static void BakeCube(GameObject cubeCanvasObject)
    {
        var parent = cubeCanvasObject.transform.Find("ParentCube") as RectTransform;
        if (parent != null)
        {
            var cubeView = parent.GetComponent<RubikCubeView>();
            if (cubeView != null)
            {
                var serialized = new SerializedObject(cubeView);
                serialized.FindProperty("_shuffleSpeed").floatValue = 12f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            SetRect(parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(0f, -300f), new Vector2(0f, -130f));
            SetFace(parent.Find("FrontFace") as RectTransform, new Vector2(0f, -130f), 0.52f);
            SetFace(parent.Find("LeftFace") as RectTransform, new Vector2(-405f, -130f), 0.18f);
            SetFace(parent.Find("RightFace") as RectTransform, new Vector2(405f, -130f), 0.18f);
            SetFace(parent.Find("TopFace") as RectTransform, new Vector2(0f, 250f), 0.18f);
            SetFace(parent.Find("BottomFace") as RectTransform, new Vector2(0f, -530f), 0.18f);
        }

        StyleButton(FindButton(cubeCanvasObject.transform, "Shaffle"), "Перемешать", new Vector2(0.5f, 0f), new Vector2(-230f, 68f), new Vector2(380f, 100f), 30f);
        StyleButton(FindButton(cubeCanvasObject.transform, "Reset"), "Сбросить", new Vector2(0.5f, 0f), new Vector2(230f, 68f), new Vector2(380f, 100f), 30f);
    }

    private static void BakePauseMenu()
    {
        var root = FindSceneRoot("Menu");
        if (root == null)
            return;
        ConfigureCanvas(root.GetComponent<Canvas>(), 100);
        var overlay = root.transform.Cast<Transform>().FirstOrDefault(t => t.GetComponent<Image>() != null) as RectTransform;
        if (overlay == null)
            return;
        overlay.GetComponent<Image>().color = new Color(0.005f, 0.008f, 0.02f, 0.88f);
        Stretch(overlay);

        var menuPanel = root.GetComponent("MenuPanel");
        var menuSerialized = menuPanel != null ? new SerializedObject(menuPanel) : null;
        var continueButton = menuSerialized?.FindProperty("_continue").objectReferenceValue as Button;
        var menuButton = menuSerialized?.FindProperty("_menu").objectReferenceValue as Button;
        if (continueButton == null)
            continueButton = CreateButton(overlay, "Continue", "Продолжить");
        if (menuButton == null)
            menuButton = CreateButton(overlay, "Menu", "В главное меню");
        if (menuSerialized != null)
        {
            menuSerialized.FindProperty("_continue").objectReferenceValue = continueButton;
            menuSerialized.FindProperty("_menu").objectReferenceValue = menuButton;
            menuSerialized.ApplyModifiedPropertiesWithoutUndo();
        }
        continueButton.gameObject.SetActive(true);
        menuButton.gameObject.SetActive(true);
        var oldCard = overlay.Find("ArcaneBaked_DialogCard");
        var title = oldCard != null
            ? oldCard.Cast<Transform>().Select(t => t.GetComponent<TMP_Text>()).FirstOrDefault(t => t != null)
            : overlay.Cast<Transform>().Select(t => t.GetComponent<TMP_Text>()).FirstOrDefault(t => t != null);
        if (continueButton != null)
            continueButton.transform.SetParent(overlay, false);
        if (menuButton != null)
            menuButton.transform.SetParent(overlay, false);
        if (title != null)
            title.transform.SetParent(overlay, false);
        if (oldCard != null)
            UnityEngine.Object.DestroyImmediate(oldCard.gameObject);
        var card = CreateCard(overlay, "ArcaneBaked_DialogCard", new Vector2(650f, 700f), Vector2.zero);
        if (title != null)
        {
            title.transform.SetParent(card, false);
            StyleLabel(title, "ПАУЗА", 48f, Text, TextAlignmentOptions.Center);
            SetRect((RectTransform)title.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560f, 90f), new Vector2(0f, 245f));
        }
        if (continueButton != null)
        {
            continueButton.transform.SetParent(card, false);
            StyleButton(continueButton, "Продолжить", new Vector2(0.5f, 0.5f), new Vector2(0f, 100f), new Vector2(500f, 96f), 30f);
        }
        if (menuButton != null)
        {
            menuButton.transform.SetParent(card, false);
            StyleButton(menuButton, "В главное меню", new Vector2(0.5f, 0.5f), new Vector2(0f, -25f), new Vector2(500f, 96f), 30f);
        }

        var modeButton = CreateButton(card, "ArcaneBaked_CubeMode", "Куб: 2D");
        StyleButton(modeButton, "Куб: 2D", new Vector2(0.5f, 0.5f), new Vector2(0f, -150f), new Vector2(500f, 96f), 30f);
        var toggle = modeButton.gameObject.AddComponent<CubeModeToggleButton>();
        var toggleSerialized = new SerializedObject(toggle);
        toggleSerialized.FindProperty("_button").objectReferenceValue = modeButton;
        toggleSerialized.FindProperty("_label").objectReferenceValue = modeButton.GetComponentInChildren<TMP_Text>();
        toggleSerialized.FindProperty("_cube").objectReferenceValue = GameObject.Find("Cube/ParentCube")?.GetComponent<RubikCubeView>();
        toggleSerialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void BakeLoseMenu()
    {
        var root = FindSceneRoot("Lose");
        if (root == null)
            return;
        ConfigureCanvas(root.GetComponent<Canvas>(), 110);
        var overlay = root.transform.Cast<Transform>().FirstOrDefault(t => t.GetComponent<Image>() != null) as RectTransform;
        if (overlay == null)
            return;
        overlay.GetComponent<Image>().color = new Color(0.005f, 0.008f, 0.02f, 0.90f);
        Stretch(overlay);

        var menuPanel = root.GetComponent("MenuPanel");
        var menuSerialized = menuPanel != null ? new SerializedObject(menuPanel) : null;
        var continueButton = menuSerialized?.FindProperty("_continue").objectReferenceValue as Button;
        var menuButton = menuSerialized?.FindProperty("_menu").objectReferenceValue as Button;
        if (continueButton == null)
            continueButton = CreateButton(overlay, "Continue", "Сыграть снова");
        if (menuButton == null)
            menuButton = CreateButton(overlay, "Menu", "В главное меню");
        if (menuPanel != null)
            UnityEngine.Object.DestroyImmediate(menuPanel);
        var resultPanel = root.GetComponent<BattleResultPanel>();
        if (resultPanel == null)
            resultPanel = root.AddComponent<BattleResultPanel>();
        var resultSerialized = new SerializedObject(resultPanel);
        resultSerialized.FindProperty("_playAgain").objectReferenceValue = continueButton;
        resultSerialized.FindProperty("_menu").objectReferenceValue = menuButton;
        resultSerialized.ApplyModifiedPropertiesWithoutUndo();
        continueButton.gameObject.SetActive(true);
        menuButton.gameObject.SetActive(true);
        var gameView = UnityEngine.Object.FindFirstObjectByType<GameView>();
        var gameViewSerialized = gameView != null ? new SerializedObject(gameView) : null;
        var title = gameViewSerialized?.FindProperty("_battleEndTitleText").objectReferenceValue as TMP_Text;
        var statsCandidate = gameViewSerialized?.FindProperty("_battleEndStatsText").objectReferenceValue as TMP_Text;
        var oldCard = overlay.Find("ArcaneBaked_ResultCard");
        if (continueButton != null)
            continueButton.transform.SetParent(overlay, false);
        if (menuButton != null)
            menuButton.transform.SetParent(overlay, false);
        if (title != null)
            title.transform.SetParent(overlay, false);
        if (statsCandidate != null)
            statsCandidate.transform.SetParent(overlay, false);
        if (oldCard != null)
            UnityEngine.Object.DestroyImmediate(oldCard.gameObject);
        var card = CreateCard(overlay, "ArcaneBaked_ResultCard", new Vector2(760f, 880f), Vector2.zero);
        if (title != null)
        {
            title.transform.SetParent(card, false);
            StyleLabel(title, "БИТВА ОКОНЧЕНА", 46f, Text, TextAlignmentOptions.Center);
            SetRect((RectTransform)title.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(660f, 110f), new Vector2(0f, 315f));
        }
        if (statsCandidate != null)
        {
            statsCandidate.transform.SetParent(card, false);
            StyleLabel(statsCandidate, statsCandidate.text, 30f, MutedText, TextAlignmentOptions.Center);
            SetRect((RectTransform)statsCandidate.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(650f, 310f), new Vector2(0f, 90f));
        }
        if (continueButton != null)
        {
            continueButton.transform.SetParent(card, false);
            StyleButton(continueButton, "Сыграть снова", new Vector2(0.5f, 0.5f), new Vector2(0f, -180f), new Vector2(540f, 100f), 30f);
        }
        if (menuButton != null)
        {
            menuButton.transform.SetParent(card, false);
            StyleButton(menuButton, "В главное меню", new Vector2(0.5f, 0.5f), new Vector2(0f, -305f), new Vector2(540f, 100f), 30f);
        }
    }

    private static void ConfigureCanvas(Canvas canvas, int sortingOrder)
    {
        if (canvas == null)
            return;
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvas.sortingOrder = sortingOrder;
        EditorUtility.SetDirty(canvas);
        EditorUtility.SetDirty(scaler);
    }

    private static void ConfigureBackground(RectTransform rect)
    {
        if (rect == null)
            return;
        Stretch(rect);
        var image = rect.GetComponent<Image>();
        if (image == null)
            image = rect.gameObject.AddComponent<Image>();
        image.sprite = _background;
        image.color = Color.white;
        image.preserveAspect = false;
        image.raycastTarget = false;
        rect.SetAsFirstSibling();
        EditorUtility.SetDirty(image);
    }

    private static void ClearBaked(RectTransform root)
    {
        var baked = root.Cast<Transform>().Where(t => t.name.StartsWith("ArcaneBaked_", StringComparison.Ordinal)).ToArray();
        foreach (var item in baked)
            UnityEngine.Object.DestroyImmediate(item.gameObject);
    }

    private static RectTransform CreateRect(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static Image CreateImage(Transform parent, string name, Color color)
    {
        var rect = CreateRect(parent, name);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static RectTransform CreateCard(Transform parent, string name, Vector2 size, Vector2 position, Vector2? anchor = null, Vector2? pivot = null)
    {
        var image = CreateImage(parent, name, Panel);
        var rect = image.rectTransform;
        var usedAnchor = anchor ?? new Vector2(0.5f, 0.5f);
        SetRect(rect, usedAnchor, usedAnchor, pivot ?? new Vector2(0.5f, 0.5f), size, position);
        var outline = rect.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(Amber.r, Amber.g, Amber.b, 0.42f);
        outline.effectDistance = new Vector2(2f, -2f);
        var shadow = rect.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.62f);
        shadow.effectDistance = new Vector2(0f, -8f);
        return rect;
    }

    private static TextMeshProUGUI CreateLabel(Transform parent, string name, string value, float size, Color color,
        TextAlignmentOptions alignment, Vector2 anchor, Vector2 dimensions, Vector2 position)
    {
        var rect = CreateRect(parent, name);
        SetRect(rect, anchor, anchor, new Vector2(0.5f, 0.5f), dimensions, position);
        var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        StyleLabel(label, value, size, color, alignment);
        return label;
    }

    private static TextMeshProUGUI CreateHudLabel(RectTransform parent, string name, string value, float size, Vector2 anchorMin, Vector2 anchorMax)
    {
        var rect = CreateRect(parent, name);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = new Vector2(16f, 0f);
        rect.offsetMax = new Vector2(-16f, 0f);
        var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        StyleLabel(label, value, size, Text, TextAlignmentOptions.Left);
        label.textWrappingMode = TextWrappingModes.NoWrap;
        return label;
    }

    private static void StyleLabel(TMP_Text label, string value, float size, Color color, TextAlignmentOptions alignment)
    {
        label.text = value;
        label.font = _font;
        label.fontSize = size;
        label.color = color;
        label.alignment = alignment;
        label.fontStyle = FontStyles.Bold;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.overflowMode = TextOverflowModes.Truncate;
        label.raycastTarget = false;
    }

    private static Button CreateButton(Transform parent, string name, string labelValue)
    {
        var rect = CreateRect(parent, name);
        rect.gameObject.AddComponent<Image>();
        var button = rect.gameObject.AddComponent<Button>();
        CreateLabel(rect, "Label", labelValue, 30f, Text, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        Stretch((RectTransform)button.transform.GetChild(0));
        return button;
    }

    private static void StyleButton(Button button, string labelValue, Vector2 anchor, Vector2 position, Vector2 size, float fontSize)
    {
        if (button == null)
            return;
        var rect = (RectTransform)button.transform;
        SetRect(rect, anchor, anchor, new Vector2(0.5f, 0.5f), size, position);
        var image = button.GetComponent<Image>();
        image.sprite = null;
        image.type = Image.Type.Simple;
        image.color = Panel;
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.35f, 1.35f, 1.35f, 1f);
        colors.pressedColor = new Color(0.72f, 0.76f, 0.84f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.45f, 0.46f, 0.50f, 0.65f);
        colors.colorMultiplier = 1f;
        button.colors = colors;
        var outline = button.GetComponent<Outline>() ?? button.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(Amber.r, Amber.g, Amber.b, 0.55f);
        outline.effectDistance = new Vector2(2f, -2f);
        var shadow = button.GetComponent<Shadow>() ?? button.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.58f);
        shadow.effectDistance = new Vector2(0f, -6f);
        var label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            StyleLabel(label, labelValue, fontSize, Text, TextAlignmentOptions.Center);
            label.enableAutoSizing = true;
            label.fontSizeMax = fontSize;
            label.fontSizeMin = Mathf.Max(20f, fontSize - 8f);
            Stretch((RectTransform)label.transform);
            ((RectTransform)label.transform).offsetMin = new Vector2(18f, 8f);
            ((RectTransform)label.transform).offsetMax = new Vector2(-18f, -8f);
        }
    }

    private static void StyleHeroButton(Button button, string title, Sprite portrait, Vector2 position)
    {
        if (button == null)
            return;
        StyleButton(button, title, new Vector2(0.5f, 0.5f), position, new Vector2(410f, 690f), 34f);
        var label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            var labelRect = (RectTransform)label.transform;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 0.19f);
            labelRect.offsetMin = new Vector2(18f, 8f);
            labelRect.offsetMax = new Vector2(-18f, -8f);
        }
        var oldPortrait = button.transform.Find("ArcaneBaked_Portrait");
        if (oldPortrait != null)
            UnityEngine.Object.DestroyImmediate(oldPortrait.gameObject);
        var image = CreateImage(button.transform, "ArcaneBaked_Portrait", Color.white);
        image.sprite = portrait;
        image.preserveAspect = true;
        var rect = image.rectTransform;
        rect.anchorMin = new Vector2(0.05f, 0.20f);
        rect.anchorMax = new Vector2(0.95f, 0.95f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.SetAsFirstSibling();
    }

    private static Image CreateHealthBar(RectTransform parent, Color color)
    {
        var track = CreateImage(parent, "HealthTrack", new Color(0f, 0f, 0f, 0.64f));
        var trackRect = track.rectTransform;
        trackRect.anchorMin = new Vector2(0f, 0f);
        trackRect.anchorMax = new Vector2(1f, 0f);
        trackRect.offsetMin = new Vector2(16f, 12f);
        trackRect.offsetMax = new Vector2(-16f, 24f);
        var fill = CreateImage(trackRect, "Fill", color);
        Stretch(fill.rectTransform);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 1f;
        return fill;
    }

    private static Button FindButton(Transform root, string name)
    {
        var child = root.Find(name);
        return child != null ? child.GetComponent<Button>() : null;
    }

    private static GameObject FindSceneRoot(string name)
    {
        return SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(root => root.name == name);
    }

    private static void SetFace(RectTransform face, Vector2 position, float scale)
    {
        if (face == null)
            return;
        face.anchorMin = new Vector2(0.5f, 0.5f);
        face.anchorMax = new Vector2(0.5f, 0.5f);
        face.pivot = new Vector2(0.5f, 0.5f);
        face.anchoredPosition = position;
        face.localScale = Vector3.one * scale;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 position)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }
}
