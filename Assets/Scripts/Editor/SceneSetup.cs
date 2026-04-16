using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using JamTemplate.UI;
using JamTemplate.Save;
using JamTemplate.Scene;
using JamTemplate.GameState;
using JamTemplate.Audio;
using JamTemplate.Demo;

namespace JamTemplate.Editor
{
    /// <summary>
    /// Creates all template scenes with proper UI hierarchies and component wiring.
    /// Run via menu: JamTemplate > Create All Scenes, or batch mode: -executeMethod JamTemplate.Editor.SceneSetup.CreateAllScenes
    /// </summary>
    public static class SceneSetup
    {
        private const string ScenesPath = "Assets/Scenes";
        private static Font defaultFont;

        [MenuItem("JamTemplate/Create All Scenes")]
        public static void CreateAllScenes()
        {
            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Create ScriptableObject assets if they don't exist
            CreateSaveConfigAsset();
            CreateCreditsDataAsset();
            CreateAudioConfigAsset();

            // Ensure scenes directory exists
            if (!AssetDatabase.IsValidFolder(ScenesPath))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            CreateSplashScene();
            CreateTitleScene();
            CreateSettingsScene();
            CreateCreditsScene();
            CreatePauseScene();
            CreateWinScene();
            CreateLoseScene();
            CreateDemoScene();

            UpdateBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[SceneSetup] All scenes created and build settings updated.");
        }

        // ─────────────────────────────────────────────
        // SPLASH SCENE
        // ─────────────────────────────────────────────
        private static void CreateSplashScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var cam = CreateCamera();

            // EventSystem
            CreateEventSystem();

            // Main Canvas with video display
            var canvas = CreateCanvas("SplashCanvas", 0);
            var canvasGroup = canvas.AddComponent<CanvasGroup>();

            // Background (black)
            var bg = CreateImage(canvas.transform, "Background", Color.black);
            StretchRectTransform(bg.GetComponent<RectTransform>());

            // Raw Image for video
            var rawImage = CreateRawImage(canvas.transform, "VideoDisplay");
            StretchRectTransform(rawImage.GetComponent<RectTransform>());

            // "Skip" hint text
            var skipText = CreateText(canvas.transform, "SkipText", "Press any key to skip", 16, TextAnchor.LowerCenter);
            var skipRect = skipText.GetComponent<RectTransform>();
            skipRect.anchorMin = new Vector2(0f, 0f);
            skipRect.anchorMax = new Vector2(1f, 0.1f);
            skipRect.offsetMin = Vector2.zero;
            skipRect.offsetMax = Vector2.zero;

            // Fade overlay Canvas (sort order 999)
            var fadeCanvas = CreateCanvas("FadeOverlay", 999);
            var fadeGroup = fadeCanvas.AddComponent<CanvasGroup>();
            fadeGroup.alpha = 1f;
            var fadeImage = CreateImage(fadeCanvas.transform, "FadeImage", Color.black);
            StretchRectTransform(fadeImage.GetComponent<RectTransform>());

            // VideoPlayer component on separate GO
            var videoGo = new GameObject("VideoPlayer");
            var videoPlayer = videoGo.AddComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.APIOnly;
            videoPlayer.isLooping = false;

            // SplashController
            var controllerGo = new GameObject("SplashController");
            var controller = controllerGo.AddComponent<SplashController>();
            SetPrivateField(controller, "videoPlayer", videoPlayer);
            SetPrivateField(controller, "videoImage", rawImage.GetComponent<RawImage>());
            SetPrivateField(controller, "fadeCanvasGroup", fadeGroup);
            SetPrivateField(controller, "nextSceneName", "Title");

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Splash.unity");
        }

        // ─────────────────────────────────────────────
        // TITLE SCENE
        // ─────────────────────────────────────────────
        private static void CreateTitleScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var cam = CreateCamera();

            // EventSystem
            CreateEventSystem();

            // ── Singletons ──

            // ScreenFade
            var fadeCanvas = CreateCanvas("ScreenFade", 999);
            var fadeGroup = fadeCanvas.AddComponent<CanvasGroup>();
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
            var fadeImage = CreateImage(fadeCanvas.transform, "FadeImage", Color.black);
            StretchRectTransform(fadeImage.GetComponent<RectTransform>());
            fadeCanvas.AddComponent<ScreenFade>();

            // SceneManagerWrapper
            var sceneManagerGo = new GameObject("SceneManagerWrapper");
            sceneManagerGo.AddComponent<SceneManagerWrapper>();

            // SaveManager
            var saveManagerGo = new GameObject("SaveManager");
            var saveManager = saveManagerGo.AddComponent<SaveManager>();
            var saveConfig = AssetDatabase.LoadAssetAtPath<SaveConfig>("Assets/Data/SaveConfig.asset");
            if (saveConfig != null)
                SetPrivateField(saveManager, "config", saveConfig);

            // GameStateManager
            var gameStateGo = new GameObject("GameStateManager");
            gameStateGo.AddComponent<GameStateManager>();

            // AudioManager
            var audioManagerGo = new GameObject("AudioManager");
            var audioManager = audioManagerGo.AddComponent<AudioManager>();
            var audioConfig = AssetDatabase.LoadAssetAtPath<AudioConfig>("Assets/Data/AudioConfig.asset");
            if (audioConfig != null)
                SetPrivateField(audioManager, "config", audioConfig);

            // ── UI ──
            var canvas = CreateCanvas("TitleCanvas", 0);

            // Background
            var bg = CreateImage(canvas.transform, "Background", new Color(0.1f, 0.1f, 0.15f, 1f));
            StretchRectTransform(bg.GetComponent<RectTransform>());

            // Title text
            var title = CreateText(canvas.transform, "TitleText", "Game Title", 48, TextAnchor.MiddleCenter);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.2f, 0.65f);
            titleRect.anchorMax = new Vector2(0.8f, 0.85f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            // Buttons
            float btnY = 0.55f;
            float btnStep = 0.1f;
            var playBtn = CreateButton(canvas.transform, "PlayButton", "Play", 0.35f, btnY, 0.65f, btnY - 0.06f);
            btnY -= btnStep;
            var settingsBtn = CreateButton(canvas.transform, "SettingsButton", "Settings", 0.35f, btnY, 0.65f, btnY - 0.06f);
            btnY -= btnStep;
            var creditsBtn = CreateButton(canvas.transform, "CreditsButton", "Credits", 0.35f, btnY, 0.65f, btnY - 0.06f);
            btnY -= btnStep;
            var quitBtn = CreateButton(canvas.transform, "QuitButton", "Quit", 0.35f, btnY, 0.65f, btnY - 0.06f);

            // TitleController
            var controllerGo = new GameObject("TitleController");
            var controller = controllerGo.AddComponent<TitleController>();
            SetPrivateField(controller, "playButton", playBtn.GetComponent<Button>());
            SetPrivateField(controller, "settingsButton", settingsBtn.GetComponent<Button>());
            SetPrivateField(controller, "creditsButton", creditsBtn.GetComponent<Button>());
            SetPrivateField(controller, "quitButton", quitBtn.GetComponent<Button>());

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Title.unity");
        }

        // ─────────────────────────────────────────────
        // SETTINGS SCENE (Additive)
        // ─────────────────────────────────────────────
        private static void CreateSettingsScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var canvas = CreateCanvas("SettingsCanvas", 10);

            // Semi-transparent overlay
            var overlay = CreateImage(canvas.transform, "Overlay", new Color(0f, 0f, 0f, 0.7f));
            StretchRectTransform(overlay.GetComponent<RectTransform>());

            // Panel
            var panel = CreateImage(canvas.transform, "Panel", new Color(0.15f, 0.15f, 0.2f, 0.95f));
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.2f, 0.15f);
            panelRect.anchorMax = new Vector2(0.8f, 0.85f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Title
            var title = CreateText(panel.transform, "TitleText", "Settings", 36, TextAnchor.MiddleCenter);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.85f);
            titleRect.anchorMax = new Vector2(0.9f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            // Sliders
            var masterSlider = CreateLabeledSlider(panel.transform, "MasterVolume", "Master Volume", 0.7f);
            var musicSlider = CreateLabeledSlider(panel.transform, "MusicVolume", "Music Volume", 0.55f);
            var sfxSlider = CreateLabeledSlider(panel.transform, "SfxVolume", "SFX Volume", 0.4f);

            // Back button
            var backBtn = CreateButton(panel.transform, "BackButton", "Back", 0.3f, 0.15f, 0.7f, 0.07f);

            // SettingsController
            var controller = canvas.AddComponent<SettingsController>();
            SetPrivateField(controller, "masterVolumeSlider", masterSlider);
            SetPrivateField(controller, "musicVolumeSlider", musicSlider);
            SetPrivateField(controller, "sfxVolumeSlider", sfxSlider);
            SetPrivateField(controller, "backButton", backBtn.GetComponent<Button>());

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Settings.unity");
        }

        // ─────────────────────────────────────────────
        // CREDITS SCENE (Additive)
        // ─────────────────────────────────────────────
        private static void CreateCreditsScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var canvas = CreateCanvas("CreditsCanvas", 10);

            // Semi-transparent overlay
            var overlay = CreateImage(canvas.transform, "Overlay", new Color(0f, 0f, 0f, 0.7f));
            StretchRectTransform(overlay.GetComponent<RectTransform>());

            // Panel
            var panel = CreateImage(canvas.transform, "Panel", new Color(0.15f, 0.15f, 0.2f, 0.95f));
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.15f, 0.1f);
            panelRect.anchorMax = new Vector2(0.85f, 0.9f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Title
            var title = CreateText(panel.transform, "TitleText", "Credits", 36, TextAnchor.MiddleCenter);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.9f);
            titleRect.anchorMax = new Vector2(0.9f, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            // ScrollRect
            var scrollGo = new GameObject("ScrollView", typeof(RectTransform));
            scrollGo.transform.SetParent(panel.transform, false);
            var scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            var scrollRectTransform = scrollGo.GetComponent<RectTransform>();
            scrollRectTransform.anchorMin = new Vector2(0.05f, 0.15f);
            scrollRectTransform.anchorMax = new Vector2(0.95f, 0.88f);
            scrollRectTransform.offsetMin = Vector2.zero;
            scrollRectTransform.offsetMax = Vector2.zero;
            var scrollImage = scrollGo.AddComponent<Image>();
            scrollImage.color = new Color(0, 0, 0, 0.01f); // Nearly invisible, needed for scroll masking
            scrollGo.AddComponent<Mask>().showMaskGraphic = false;

            // Content
            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(scrollGo.transform, false);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0f, 600f);
            var contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.childAlignment = TextAnchor.UpperCenter;
            contentLayout.spacing = 5f;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            var contentFitter = content.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRect;

            // Back button
            var backBtn = CreateButton(panel.transform, "BackButton", "Back", 0.3f, 0.05f, 0.7f, 0f);

            // CreditsController
            var controller = canvas.AddComponent<CreditsController>();
            SetPrivateField(controller, "contentParent", content.transform);
            SetPrivateField(controller, "backButton", backBtn.GetComponent<Button>());
            SetPrivateField(controller, "scrollRect", scrollRect);

            var creditsData = AssetDatabase.LoadAssetAtPath<CreditsData>("Assets/Data/CreditsData.asset");
            if (creditsData != null)
                SetPrivateField(controller, "creditsData", creditsData);

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Credits.unity");
        }

        // ─────────────────────────────────────────────
        // PAUSE SCENE (Additive)
        // ─────────────────────────────────────────────
        private static void CreatePauseScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var canvas = CreateCanvas("PauseCanvas", 10);

            // Semi-transparent overlay
            var overlay = CreateImage(canvas.transform, "Overlay", new Color(0f, 0f, 0f, 0.5f));
            StretchRectTransform(overlay.GetComponent<RectTransform>());

            // Panel
            var panel = CreateImage(canvas.transform, "Panel", new Color(0.15f, 0.15f, 0.2f, 0.95f));
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.25f, 0.15f);
            panelRect.anchorMax = new Vector2(0.75f, 0.85f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Title
            var title = CreateText(panel.transform, "TitleText", "Paused", 36, TextAnchor.MiddleCenter);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.85f);
            titleRect.anchorMax = new Vector2(0.9f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            // Buttons
            float btnY = 0.72f;
            float btnStep = 0.12f;
            var resumeBtn = CreateButton(panel.transform, "ResumeButton", "Resume", 0.15f, btnY, 0.85f, btnY - 0.08f);
            btnY -= btnStep;
            var restartBtn = CreateButton(panel.transform, "RestartButton", "Restart", 0.15f, btnY, 0.85f, btnY - 0.08f);
            btnY -= btnStep;
            var settingsBtn = CreateButton(panel.transform, "SettingsButton", "Settings", 0.15f, btnY, 0.85f, btnY - 0.08f);
            btnY -= btnStep;
            var mainMenuBtn = CreateButton(panel.transform, "MainMenuButton", "Main Menu", 0.15f, btnY, 0.85f, btnY - 0.08f);
            btnY -= btnStep;
            var quitBtn = CreateButton(panel.transform, "QuitButton", "Quit", 0.15f, btnY, 0.85f, btnY - 0.08f);

            // PauseController
            var controller = canvas.AddComponent<PauseController>();
            SetPrivateField(controller, "resumeButton", resumeBtn.GetComponent<Button>());
            SetPrivateField(controller, "restartButton", restartBtn.GetComponent<Button>());
            SetPrivateField(controller, "settingsButton", settingsBtn.GetComponent<Button>());
            SetPrivateField(controller, "mainMenuButton", mainMenuBtn.GetComponent<Button>());
            SetPrivateField(controller, "quitButton", quitBtn.GetComponent<Button>());

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Pause.unity");
        }

        // ─────────────────────────────────────────────
        // WIN SCENE (Additive)
        // ─────────────────────────────────────────────
        private static void CreateWinScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var canvas = CreateCanvas("WinCanvas", 10);

            var overlay = CreateImage(canvas.transform, "Overlay", new Color(0f, 0f, 0f, 0.6f));
            StretchRectTransform(overlay.GetComponent<RectTransform>());

            var panel = CreateImage(canvas.transform, "Panel", new Color(0.1f, 0.2f, 0.1f, 0.95f));
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.2f, 0.2f);
            panelRect.anchorMax = new Vector2(0.8f, 0.8f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Header
            var header = CreateText(panel.transform, "HeaderText", "You Win!", 42, TextAnchor.MiddleCenter);
            var headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.1f, 0.75f);
            headerRect.anchorMax = new Vector2(0.9f, 0.95f);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;

            // Message
            var message = CreateText(panel.transform, "MessageText", "Congratulations!", 20, TextAnchor.MiddleCenter);
            var messageRect = message.GetComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0.1f, 0.5f);
            messageRect.anchorMax = new Vector2(0.9f, 0.7f);
            messageRect.offsetMin = Vector2.zero;
            messageRect.offsetMax = Vector2.zero;

            // Buttons
            var retryBtn = CreateButton(panel.transform, "RetryButton", "Play Again", 0.15f, 0.3f, 0.85f, 0.2f);
            var menuBtn = CreateButton(panel.transform, "MainMenuButton", "Main Menu", 0.15f, 0.15f, 0.85f, 0.05f);

            // EndScreenController
            var controller = canvas.AddComponent<EndScreenController>();
            SetPrivateField(controller, "headerText", header.GetComponent<Text>());
            SetPrivateField(controller, "messageText", message.GetComponent<Text>());
            SetPrivateField(controller, "retryButton", retryBtn.GetComponent<Button>());
            SetPrivateField(controller, "mainMenuButton", menuBtn.GetComponent<Button>());
            SetPrivateField(controller, "headerString", "You Win!");
            SetPrivateField(controller, "messageString", "Congratulations!");
            SetPrivateField(controller, "retryButtonLabel", "Play Again");

            // Get retry button text for label update
            var retryText = retryBtn.GetComponentInChildren<Text>();
            SetPrivateField(controller, "retryButtonText", retryText);

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Win.unity");
        }

        // ─────────────────────────────────────────────
        // LOSE SCENE (Additive)
        // ─────────────────────────────────────────────
        private static void CreateLoseScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var canvas = CreateCanvas("LoseCanvas", 10);

            var overlay = CreateImage(canvas.transform, "Overlay", new Color(0f, 0f, 0f, 0.6f));
            StretchRectTransform(overlay.GetComponent<RectTransform>());

            var panel = CreateImage(canvas.transform, "Panel", new Color(0.25f, 0.1f, 0.1f, 0.95f));
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.2f, 0.2f);
            panelRect.anchorMax = new Vector2(0.8f, 0.8f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Header
            var header = CreateText(panel.transform, "HeaderText", "Game Over", 42, TextAnchor.MiddleCenter);
            var headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.1f, 0.75f);
            headerRect.anchorMax = new Vector2(0.9f, 0.95f);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;

            // Message
            var message = CreateText(panel.transform, "MessageText", "Better luck next time!", 20, TextAnchor.MiddleCenter);
            var messageRect = message.GetComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0.1f, 0.5f);
            messageRect.anchorMax = new Vector2(0.9f, 0.7f);
            messageRect.offsetMin = Vector2.zero;
            messageRect.offsetMax = Vector2.zero;

            // Buttons
            var retryBtn = CreateButton(panel.transform, "RetryButton", "Try Again", 0.15f, 0.3f, 0.85f, 0.2f);
            var menuBtn = CreateButton(panel.transform, "MainMenuButton", "Main Menu", 0.15f, 0.15f, 0.85f, 0.05f);

            // EndScreenController
            var controller = canvas.AddComponent<EndScreenController>();
            SetPrivateField(controller, "headerText", header.GetComponent<Text>());
            SetPrivateField(controller, "messageText", message.GetComponent<Text>());
            SetPrivateField(controller, "retryButton", retryBtn.GetComponent<Button>());
            SetPrivateField(controller, "mainMenuButton", menuBtn.GetComponent<Button>());
            SetPrivateField(controller, "headerString", "Game Over");
            SetPrivateField(controller, "messageString", "Better luck next time!");
            SetPrivateField(controller, "retryButtonLabel", "Try Again");

            var retryText = retryBtn.GetComponentInChildren<Text>();
            SetPrivateField(controller, "retryButtonText", retryText);

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Lose.unity");
        }

        // ─────────────────────────────────────────────
        // DEMO SCENE (Gameplay placeholder)
        // ─────────────────────────────────────────────
        private static void CreateDemoScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cam = CreateCamera();
            CreateEventSystem();

            var canvas = CreateCanvas("DemoCanvas", 0);

            // Background
            var bg = CreateImage(canvas.transform, "Background", new Color(0.12f, 0.12f, 0.18f, 1f));
            StretchRectTransform(bg.GetComponent<RectTransform>());

            // Info text
            var info = CreateText(canvas.transform, "InfoText", "Demo Gameplay Scene\n\nPress ESC to Pause\nUse buttons below to test flows", 20, TextAnchor.MiddleCenter);
            var infoRect = info.GetComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0.1f, 0.5f);
            infoRect.anchorMax = new Vector2(0.9f, 0.9f);
            infoRect.offsetMin = Vector2.zero;
            infoRect.offsetMax = Vector2.zero;

            // Buttons
            var pauseBtn = CreateButton(canvas.transform, "PauseButton", "Pause", 0.1f, 0.35f, 0.35f, 0.25f);
            var winBtn = CreateButton(canvas.transform, "WinButton", "Trigger Win", 0.38f, 0.35f, 0.62f, 0.25f);
            var loseBtn = CreateButton(canvas.transform, "LoseButton", "Trigger Lose", 0.65f, 0.35f, 0.9f, 0.25f);

            // DemoController
            var controllerGo = new GameObject("DemoController");
            var controller = controllerGo.AddComponent<DemoController>();
            SetPrivateField(controller, "pauseButton", pauseBtn.GetComponent<Button>());
            SetPrivateField(controller, "winButton", winBtn.GetComponent<Button>());
            SetPrivateField(controller, "loseButton", loseBtn.GetComponent<Button>());

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Demo.unity");
        }

        // ─────────────────────────────────────────────
        // BUILD SETTINGS
        // ─────────────────────────────────────────────
        private static void UpdateBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene($"{ScenesPath}/Splash.unity", true),
                new EditorBuildSettingsScene($"{ScenesPath}/Title.unity", true),
                new EditorBuildSettingsScene($"{ScenesPath}/Settings.unity", true),
                new EditorBuildSettingsScene($"{ScenesPath}/Credits.unity", true),
                new EditorBuildSettingsScene($"{ScenesPath}/Demo.unity", true),
                new EditorBuildSettingsScene($"{ScenesPath}/Pause.unity", true),
                new EditorBuildSettingsScene($"{ScenesPath}/Win.unity", true),
                new EditorBuildSettingsScene($"{ScenesPath}/Lose.unity", true),
            };
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        // ─────────────────────────────────────────────
        // ASSET CREATION
        // ─────────────────────────────────────────────
        private static void CreateSaveConfigAsset()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");

            string path = "Assets/Data/SaveConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<SaveConfig>(path) != null) return;

            var config = ScriptableObject.CreateInstance<SaveConfig>();
            AssetDatabase.CreateAsset(config, path);
        }

        private static void CreateCreditsDataAsset()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");

            string path = "Assets/Data/CreditsData.asset";
            if (AssetDatabase.LoadAssetAtPath<CreditsData>(path) != null) return;

            var data = ScriptableObject.CreateInstance<CreditsData>();
            AssetDatabase.CreateAsset(data, path);
        }

        private static void CreateAudioConfigAsset()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");

            string path = "Assets/Data/AudioConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<AudioConfig>(path) != null) return;

            var config = ScriptableObject.CreateInstance<AudioConfig>();
            AssetDatabase.CreateAsset(config, path);
        }

        // ─────────────────────────────────────────────
        // UI HELPERS
        // ─────────────────────────────────────────────
        private static GameObject CreateCamera()
        {
            var go = new GameObject("Main Camera");
            var cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.orthographic = true;
            go.AddComponent<AudioListener>();
            go.tag = "MainCamera";
            return go;
        }

        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private static GameObject CreateCanvas(string name, int sortOrder)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            go.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        private static GameObject CreateImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = color;
            return go;
        }

        private static GameObject CreateRawImage(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.AddComponent<RawImage>();
            return go;
        }

        private static GameObject CreateText(Transform parent, string name, string text, int fontSize, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var textComp = go.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = fontSize;
            textComp.alignment = alignment;
            textComp.color = Color.white;
            textComp.font = defaultFont;
            return go;
        }

        private static GameObject CreateButton(Transform parent, string name, string label,
            float anchorMinX, float anchorMinY, float anchorMaxX, float anchorMaxY)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.25f, 0.25f, 0.35f, 1f);
            go.AddComponent<Button>();

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(anchorMinX, anchorMaxY); // Note: anchorMaxY is lower
            rect.anchorMax = new Vector2(anchorMaxX, anchorMinY);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Button label
            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.AddComponent<Text>();
            text.text = label;
            text.fontSize = 20;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = defaultFont;
            StretchRectTransform(textGo.GetComponent<RectTransform>());

            return go;
        }

        private static Slider CreateLabeledSlider(Transform parent, string name, string label, float yCenter)
        {
            float sliderHeight = 0.08f;

            // Label
            var labelGo = CreateText(parent, $"{name}Label", label, 18, TextAnchor.MiddleLeft);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.08f, yCenter + 0.01f);
            labelRect.anchorMax = new Vector2(0.4f, yCenter + sliderHeight);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            // Slider container
            var sliderGo = new GameObject(name, typeof(RectTransform));
            sliderGo.transform.SetParent(parent, false);
            var sliderRect = sliderGo.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.42f, yCenter + 0.01f);
            sliderRect.anchorMax = new Vector2(0.92f, yCenter + sliderHeight);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;

            // Background
            var bgGo = CreateImage(sliderGo.transform, "Background", new Color(0.2f, 0.2f, 0.2f, 1f));
            StretchRectTransform(bgGo.GetComponent<RectTransform>());

            // Fill Area
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGo.transform, false);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRect.offsetMin = new Vector2(5f, 0f);
            fillAreaRect.offsetMax = new Vector2(-5f, 0f);

            var fill = CreateImage(fillArea.transform, "Fill", new Color(0.3f, 0.6f, 0.9f, 1f));
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            // Handle area
            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderGo.transform, false);
            var handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(10f, 0f);
            handleAreaRect.offsetMax = new Vector2(-10f, 0f);

            var handle = CreateImage(handleArea.transform, "Handle", Color.white);
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20f, 0f);
            handleRect.anchorMin = new Vector2(0f, 0f);
            handleRect.anchorMax = new Vector2(0f, 1f);

            // Slider component
            var slider = sliderGo.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            slider.targetGraphic = handle.GetComponent<Image>();

            return slider;
        }

        private static void StretchRectTransform(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// Sets a private/serialized field on a component using SerializedObject.
        /// This ensures the value is properly saved in the scene file.
        /// </summary>
        private static void SetPrivateField(Component component, string fieldName, object value)
        {
            var so = new SerializedObject(component);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[SceneSetup] Field '{fieldName}' not found on {component.GetType().Name}");
                return;
            }

            switch (value)
            {
                case string s:
                    prop.stringValue = s;
                    break;
                case int i:
                    prop.intValue = i;
                    break;
                case float f:
                    prop.floatValue = f;
                    break;
                case bool b:
                    prop.boolValue = b;
                    break;
                case Object obj:
                    prop.objectReferenceValue = obj;
                    break;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
