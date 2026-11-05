using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

#if UNITY_6000_0_OR_NEWER
using BallPhysicMaterial = UnityEngine.PhysicsMaterial;
using BallPhysicCombine = UnityEngine.PhysicsMaterialCombine;
#else
using BallPhysicMaterial = UnityEngine.PhysicMaterial;
using BallPhysicCombine = UnityEngine.PhysicMaterialCombine;
#endif

namespace AntiGravityGolf.Editor
{
    /// <summary>
    /// Automated setup tool for Realistic 3D Mini Golf (5 Championship Courses, Zero Trees).
    /// Builds and configures all scenes, 5-course blueprint park geometry, physics, UI, and build settings.
    /// Can be executed from the Unity menu (Anti-Gravity Golf -> Build Complete Game)
    /// or from command line batchmode.
    /// </summary>
    public static class AntiGravityGolfSetup
    {
        [MenuItem("Anti-Gravity Golf/Build Complete Game (All Scenes & Levels)")]
        public static void BuildCompleteGame()
        {
            Debug.Log("[AntiGravityGolf] Starting automated game build (5 Courses, No Trees)...");

            EnsureFolderStructure();
            BuildGameplayScene();
            BuildMainMenuScene();
            ConfigureBuildSettings();

            Debug.Log("[AntiGravityGolf] Complete Game Build successful! Ready to play.");
        }

        private static void EnsureFolderStructure()
        {
            if (!Directory.Exists("Assets/Scenes")) Directory.CreateDirectory("Assets/Scenes");
            if (!Directory.Exists("Assets/Materials")) Directory.CreateDirectory("Assets/Materials");
            if (!Directory.Exists("Assets/Prefabs")) Directory.CreateDirectory("Assets/Prefabs");
            if (!Directory.Exists("Assets/Textures")) Directory.CreateDirectory("Assets/Textures");
            AssetDatabase.Refresh();
        }

        #region Gameplay Scene Construction

        [MenuItem("Anti-Gravity Golf/Build Gameplay Scene Only")]
        public static void BuildGameplayScene()
        {
            Debug.Log("[AntiGravityGolf] Building Gameplay Scene (5 Courses, No Trees)...");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Core Systems GameObject
            GameObject coreSystems = new GameObject("CoreSystems");
            GameManager gameManager = coreSystems.AddComponent<GameManager>();
            GravityManager gravityManager = coreSystems.AddComponent<GravityManager>();
            ProceduralSoundManager soundManager = coreSystems.AddComponent<ProceduralSoundManager>();
            SettingsManager settingsManager = coreSystems.AddComponent<SettingsManager>();

            // 2. Open Natural Lighting & Environment (Zero Trees)
            GameObject envRoot = new GameObject("Environment");
            CourseVisuals.SetupForestEnvironment(envRoot.transform);

            // 3. Golf Ball GameObject
            GameObject ballObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ballObj.name = "GolfBall";
            ballObj.transform.position = new Vector3(27f, 0.4f, -17.5f);
            ballObj.transform.localScale = Vector3.one * 0.38f;

            Shader stdShader = Shader.Find("Standard") ?? Shader.Find("Mobile/Diffuse");
            Material ballMat = new Material(stdShader) { name = "Mat_GolfBall_Inst" };
            ballMat.color = new Color(0.99f, 0.99f, 0.99f);
            if (ballMat.HasProperty("_Glossiness")) ballMat.SetFloat("_Glossiness", 0.95f);
            if (ballMat.HasProperty("_Metallic")) ballMat.SetFloat("_Metallic", 0.05f);
            ballObj.GetComponent<MeshRenderer>().sharedMaterial = ballMat;

            Rigidbody rb = ballObj.AddComponent<Rigidbody>();
            rb.mass = 2.5f;
            rb.linearDamping = 0.55f;
            rb.angularDamping = 0.45f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            SphereCollider sc = ballObj.GetComponent<SphereCollider>();
            BallPhysicMaterial ballPhysMat = new BallPhysicMaterial("BallPhysMat")
            {
                bounciness = 0.08f,
                dynamicFriction = 0.22f,
                staticFriction = 0.28f,
                frictionCombine = BallPhysicCombine.Average,
                bounceCombine = BallPhysicCombine.Minimum
            };
            sc.material = ballPhysMat;

            // Trail Renderer
            TrailRenderer trail = ballObj.AddComponent<TrailRenderer>();
            trail.time = 0.45f;
            trail.startWidth = 0.15f;
            trail.endWidth = 0.02f;
            Material trailMat = new Material(Shader.Find("Sprites/Default"));
            trail.material = trailMat;
            trail.startColor = new Color(0.2f, 0.95f, 0.45f, 0.75f);
            trail.endColor = new Color(1.0f, 0.85f, 0.25f, 0f);
            trail.emitting = false;

            GameObject ballLight = new GameObject("BallGlowLight");
            ballLight.transform.SetParent(ballObj.transform, false);
            Light bl = ballLight.AddComponent<Light>();
            bl.type = LightType.Point;
            bl.range = 3.0f;
            bl.intensity = 0.85f;
            bl.color = new Color(1.0f, 0.95f, 0.82f);

            // Aim Indicator
            GameObject aimObj = new GameObject("AimIndicator");
            aimObj.transform.SetParent(ballObj.transform, false);
            AimIndicator aimInd = aimObj.AddComponent<AimIndicator>();

            // Ball Controller
            GolfBallController ballCtrl = ballObj.AddComponent<GolfBallController>();
            SerializedObject ballSo = new SerializedObject(ballCtrl);
            ballSo.FindProperty("aimIndicator").objectReferenceValue = aimInd;
            ballSo.FindProperty("ballTrail").objectReferenceValue = trail;
            ballSo.ApplyModifiedProperties();

            // 4. Camera with open sky
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.72f, 0.85f);
            camObj.AddComponent<AudioListener>();
            GolfCameraController camCtrl = camObj.AddComponent<GolfCameraController>();
            camCtrl.Target = ballObj.transform;

            // 5. Courses Root & 5 Championship Courses Geometry
            GameObject coursesRoot = new GameObject("Courses");
            System.Collections.Generic.List<GameObject> holeContainers;
            System.Collections.Generic.List<UnityEngine.Transform> spawnTransforms;
            UnityEngine.Transform practiceSpawn;
            CourseBuilder.Build18HolePark(coursesRoot.transform, out holeContainers, out spawnTransforms, out practiceSpawn);

            if (spawnTransforms.Count > 0)
            {
                ballObj.transform.position = spawnTransforms[0].position;
            }

            // Level Manager — wire 5 courses and practice green
            GameObject levelMgrObj = new GameObject("LevelManager");
            LevelManager levelMgr = levelMgrObj.AddComponent<LevelManager>();
            SerializedObject lvlSo = new SerializedObject(levelMgr);
            lvlSo.FindProperty("ball").objectReferenceValue = ballCtrl;
            lvlSo.FindProperty("camController").objectReferenceValue = camCtrl;
            lvlSo.FindProperty("practiceSpawnPoint").objectReferenceValue = practiceSpawn;

            SerializedProperty containersProp = lvlSo.FindProperty("levelContainers");
            containersProp.arraySize = holeContainers.Count;
            for (int i = 0; i < holeContainers.Count; i++)
                containersProp.GetArrayElementAtIndex(i).objectReferenceValue = holeContainers[i];

            SerializedProperty spawnsProp = lvlSo.FindProperty("spawnPoints");
            spawnsProp.arraySize = spawnTransforms.Count;
            for (int i = 0; i < spawnTransforms.Count; i++)
                spawnsProp.GetArrayElementAtIndex(i).objectReferenceValue = spawnTransforms[i];

            lvlSo.ApplyModifiedProperties();

            // 6. Build UI Canvas
            BuildGameplayUI(ballCtrl);

            // Save Scene
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Gameplay.unity");
            Debug.Log("[AntiGravityGolf] Saved Assets/Scenes/Gameplay.unity successfully.");
        }

        #endregion

        #region Main Menu Scene Construction

        [MenuItem("Anti-Gravity Golf/Build Main Menu Scene Only")]
        public static void BuildMainMenuScene()
        {
            Debug.Log("[AntiGravityGolf] Building Main Menu Scene...");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject coreSystems = new GameObject("CoreSystems");
            coreSystems.AddComponent<GameManager>();
            coreSystems.AddComponent<ProceduralSoundManager>();
            coreSystems.AddComponent<SettingsManager>();

            GameObject envRoot = new GameObject("Environment");
            CourseVisuals.SetupForestEnvironment(envRoot.transform);

            // Showcase Island
            GameObject islandRoot = new GameObject("ShowcaseIsland");
            CourseBuilder.CreatePlatform(islandRoot.transform, Vector3.zero, new Vector3(8f, 0.5f, 8f), true);
            CourseBuilder.CreateBorders(islandRoot.transform, Vector3.zero, new Vector3(8f, 0.5f, 8f), 0.5f);
            CourseBuilder.CreateHole(islandRoot.transform, new Vector3(0f, 0.05f, 1.5f));
            CourseBuilder.CreateSandBunker(islandRoot.transform, new Vector3(-2f, 0.05f, -2f), new Vector3(2f, 0.1f, 2f));
            CourseBuilder.CreateBumper(islandRoot.transform, new Vector3(2f, 0.4f, -2f));

            GameObject demoBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            demoBall.name = "DemoBall";
            demoBall.transform.SetParent(islandRoot.transform, false);
            demoBall.transform.localPosition = new Vector3(0f, 0.28f, -1.8f);
            demoBall.transform.localScale = Vector3.one * 0.38f;
            Object.DestroyImmediate(demoBall.GetComponent<Collider>());

            islandRoot.AddComponent<RotatingObstacle>();
            SerializedObject rotSo = new SerializedObject(islandRoot.GetComponent<RotatingObstacle>());
            rotSo.FindProperty("rotationSpeed").floatValue = 10f;
            rotSo.ApplyModifiedProperties();

            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.72f, 0.85f);
            camObj.transform.position = new Vector3(0f, 6.5f, -11f);
            camObj.transform.rotation = Quaternion.Euler(28f, 0f, 0f);
            camObj.AddComponent<AudioListener>();

            BuildMainMenuUI();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
            Debug.Log("[AntiGravityGolf] Saved Assets/Scenes/MainMenu.unity successfully.");
        }

        #endregion

        #region UI Generators

        private static Font GetSystemFont()
        {
            Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (f == null) f = Font.CreateDynamicFontFromOSFont("Arial", 18);
            if (f == null) f = Font.CreateDynamicFontFromOSFont("Helvetica", 18);
            return f;
        }

        private static void BuildGameplayUI(GolfBallController ballCtrl)
        {
            Font defaultFont = GetSystemFont();

            GameObject canvasObj = new GameObject("HUDCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            GameUI gameUI = canvasObj.AddComponent<GameUI>();
            SerializedObject uiSo = new SerializedObject(gameUI);

            // 1. Top HUD Bar
            GameObject topBar = CreateUIPanel(canvasObj.transform, "TopBar", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform topRt = topBar.GetComponent<RectTransform>();
            topRt.pivot = new Vector2(0.5f, 1f);
            topRt.sizeDelta = new Vector2(0f, 75f);
            topRt.anchoredPosition = Vector2.zero;
            SetPanelBackground(topBar, new Color(0.05f, 0.08f, 0.12f, 0.85f), false);

            Text lvlText = CreateUIText(topBar.transform, "LevelNameText", "COURSE 1: THE WINDING S-BEND", defaultFont, 21, TextAnchor.MiddleLeft, Color.white);
            SetRectTransform(lvlText.gameObject, new Vector2(0.012f, 0f), new Vector2(0.26f, 1f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("levelNameText").objectReferenceValue = lvlText;

            Text strokeText = CreateUIText(topBar.transform, "StrokeText", "STROKES: 0", defaultFont, 22, TextAnchor.MiddleCenter, new Color(0.2f, 1f, 0.8f));
            SetRectTransform(strokeText.gameObject, new Vector2(0.265f, 0.35f), new Vector2(0.39f, 0.95f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("strokeText").objectReferenceValue = strokeText;

            Text parText = CreateUIText(topBar.transform, "ParText", "PAR: 2", defaultFont, 16, TextAnchor.MiddleCenter, new Color(0.7f, 0.85f, 1f));
            SetRectTransform(parText.gameObject, new Vector2(0.265f, 0.05f), new Vector2(0.39f, 0.45f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("parText").objectReferenceValue = parText;

            Text overallText = CreateUIText(topBar.transform, "OverallScoreText", "SCORE: E (0 Total)", defaultFont, 20, TextAnchor.MiddleCenter, new Color(1.0f, 0.88f, 0.3f));
            SetRectTransform(overallText.gameObject, new Vector2(0.395f, 0f), new Vector2(0.575f, 1f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("overallScoreText").objectReferenceValue = overallText;

            // HUD Action Buttons (0.585 to 0.99)
            Button muteBtn = CreateUIButton(topBar.transform, "MusicMuteBtn", "🔊 MUSIC", defaultFont, 14);
            SetRectTransform(muteBtn.gameObject, new Vector2(0.585f, 0.15f), new Vector2(0.665f, 0.85f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("musicMuteBtn").objectReferenceValue = muteBtn;
            uiSo.FindProperty("musicMuteText").objectReferenceValue = muteBtn.GetComponentInChildren<Text>();

            Button mapBtn = CreateUIButton(topBar.transform, "MapOverviewBtn", "MAP [M]", defaultFont, 15);
            SetRectTransform(mapBtn.gameObject, new Vector2(0.675f, 0.15f), new Vector2(0.755f, 0.85f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("mapOverviewBtn").objectReferenceValue = mapBtn;

            Button cardBtn = CreateUIButton(topBar.transform, "ScorecardBtn", "CARD [TAB]", defaultFont, 15);
            SetRectTransform(cardBtn.gameObject, new Vector2(0.765f, 0.15f), new Vector2(0.845f, 0.85f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("scorecardToggleBtn").objectReferenceValue = cardBtn;

            Button resetBtn = CreateUIButton(topBar.transform, "ResetHoleBtn", "RESET [R]", defaultFont, 15);
            SetRectTransform(resetBtn.gameObject, new Vector2(0.855f, 0.15f), new Vector2(0.93f, 0.85f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("resetHoleBtn").objectReferenceValue = resetBtn;

            Button pauseBtn = CreateUIButton(topBar.transform, "PauseBtn", "||", defaultFont, 20);
            SetRectTransform(pauseBtn.gameObject, new Vector2(0.94f, 0.15f), new Vector2(0.988f, 0.85f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("pauseBtn").objectReferenceValue = pauseBtn;

            // 2. Water Penalty Banner
            Text penaltyText = CreateUIText(canvasObj.transform, "PenaltyBannerText", "WATER HAZARD! +1 PENALTY STROKE", defaultFont, 28, TextAnchor.MiddleCenter, new Color(1.0f, 0.25f, 0.25f));
            SetRectTransform(penaltyText.gameObject, new Vector2(0.2f, 0.65f), new Vector2(0.8f, 0.73f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("penaltyBannerText").objectReferenceValue = penaltyText;

            // 3. Instruction Hint
            Text hintText = CreateUIText(canvasObj.transform, "HintText", "Drag mouse backward or Hold Spacebar to charge putt | M for Map | Tab for Scorecard | Esc to Pause", defaultFont, 17, TextAnchor.MiddleCenter, new Color(0.85f, 0.92f, 1f, 0.85f));
            SetRectTransform(hintText.gameObject, new Vector2(0.15f, 0.03f), new Vector2(0.85f, 0.07f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("contextualHintText").objectReferenceValue = hintText;

            // 4. Power Meter
            GameObject powerContainer = CreateUIPanel(canvasObj.transform, "PowerGauge", new Vector2(0.35f, 0.09f), new Vector2(0.65f, 0.13f), Vector2.zero, Vector2.zero);
            SetPanelBackground(powerContainer, new Color(0.08f, 0.1f, 0.15f, 0.85f));
            uiSo.FindProperty("powerBarContainer").objectReferenceValue = powerContainer;

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(powerContainer.transform, false);
            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 0f;
            fillImg.color = Color.cyan;
            SetRectTransform(fillObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            uiSo.FindProperty("powerBarFill").objectReferenceValue = fillImg;

            Text powerLabel = CreateUIText(powerContainer.transform, "PowerLabel", "PUTTING POWER", defaultFont, 14, TextAnchor.MiddleCenter, Color.white);
            SetRectTransform(powerLabel.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // 5. 5-Course Scorecard Modal (Tab)
            GameObject scorecardPanel = CreateUIPanel(canvasObj.transform, "ScorecardPanel", new Vector2(0.18f, 0.14f), new Vector2(0.82f, 0.86f), Vector2.zero, Vector2.zero);
            SetPanelBackground(scorecardPanel, new Color(0.04f, 0.07f, 0.12f, 0.98f), true);
            uiSo.FindProperty("scorecardPanel").objectReferenceValue = scorecardPanel;

            Text cardTitle = CreateUIText(scorecardPanel.transform, "CardTitle", "CHAMPIONSHIP SCORECARD (5 COURSES)", defaultFont, 32, TextAnchor.MiddleCenter, new Color(0.2f, 0.95f, 0.45f));
            SetRectTransform(cardTitle.gameObject, new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.98f), Vector2.zero, Vector2.zero);

            Text cardOutText = CreateUIText(scorecardPanel.transform, "CardOutSummary", "5 CHAMPIONSHIP COURSES", defaultFont, 18, TextAnchor.UpperLeft, Color.white);
            SetRectTransform(cardOutText.gameObject, new Vector2(0.08f, 0.22f), new Vector2(0.58f, 0.85f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("scorecardOutSummaryText").objectReferenceValue = cardOutText;

            Text cardInText = CreateUIText(scorecardPanel.transform, "CardInSummary", "Course Information", defaultFont, 18, TextAnchor.UpperLeft, new Color(0.75f, 0.85f, 1f));
            SetRectTransform(cardInText.gameObject, new Vector2(0.62f, 0.22f), new Vector2(0.92f, 0.85f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("scorecardInSummaryText").objectReferenceValue = cardInText;

            Text cardTotalText = CreateUIText(scorecardPanel.transform, "CardTotalText", "TOTAL PAR: 14 | TOTAL STROKES: 0 (E)", defaultFont, 22, TextAnchor.MiddleCenter, new Color(1.0f, 0.88f, 0.3f));
            SetRectTransform(cardTotalText.gameObject, new Vector2(0.05f, 0.12f), new Vector2(0.95f, 0.20f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("scorecardTotalText").objectReferenceValue = cardTotalText;

            Button closeCardBtn = CreateUIButton(scorecardPanel.transform, "CloseScorecardBtn", "CLOSE [TAB]", defaultFont, 18);
            SetRectTransform(closeCardBtn.gameObject, new Vector2(0.40f, 0.03f), new Vector2(0.60f, 0.10f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("closeScorecardBtn").objectReferenceValue = closeCardBtn;

            // 6. Level Complete Modal
            GameObject completePanel = CreateUIPanel(canvasObj.transform, "LevelCompleteModal", new Vector2(0.30f, 0.20f), new Vector2(0.70f, 0.80f), Vector2.zero, Vector2.zero);
            SetPanelBackground(completePanel, new Color(0.04f, 0.07f, 0.14f, 0.98f), true);
            uiSo.FindProperty("levelCompletePanel").objectReferenceValue = completePanel;

            Text compTitle = CreateUIText(completePanel.transform, "CompTitle", "COURSE COMPLETE!", defaultFont, 40, TextAnchor.MiddleCenter, Color.white);
            SetRectTransform(compTitle.gameObject, new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.96f), Vector2.zero, Vector2.zero);

            Text ratingText = CreateUIText(completePanel.transform, "RatingText", "PAR (E)", defaultFont, 36, TextAnchor.MiddleCenter, new Color(0.2f, 1f, 0.4f));
            SetRectTransform(ratingText.gameObject, new Vector2(0.1f, 0.68f), new Vector2(0.9f, 0.82f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("scoreRatingText").objectReferenceValue = ratingText;

            Text compStrokes = CreateUIText(completePanel.transform, "CompStrokes", "Strokes: 2", defaultFont, 24, TextAnchor.MiddleCenter, Color.white);
            SetRectTransform(compStrokes.gameObject, new Vector2(0.1f, 0.54f), new Vector2(0.5f, 0.66f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("completeStrokesText").objectReferenceValue = compStrokes;

            Text compPar = CreateUIText(completePanel.transform, "CompPar", "Par: 2", defaultFont, 24, TextAnchor.MiddleCenter, Color.white);
            SetRectTransform(compPar.gameObject, new Vector2(0.5f, 0.54f), new Vector2(0.9f, 0.66f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("completeParText").objectReferenceValue = compPar;

            Text bestText = CreateUIText(completePanel.transform, "BestScoreText", "Course Record: 2 strokes", defaultFont, 20, TextAnchor.MiddleCenter, new Color(0.7f, 0.9f, 1f));
            SetRectTransform(bestText.gameObject, new Vector2(0.1f, 0.42f), new Vector2(0.9f, 0.52f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("bestScoreText").objectReferenceValue = bestText;

            Button nextBtn = CreateUIButton(completePanel.transform, "NextLevelBtn", "NEXT COURSE [N]", defaultFont, 24);
            SetRectTransform(nextBtn.gameObject, new Vector2(0.2f, 0.28f), new Vector2(0.8f, 0.40f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("nextLevelButton").objectReferenceValue = nextBtn;

            Button retryBtn = CreateUIButton(completePanel.transform, "RetryBtn", "RETRY COURSE [R]", defaultFont, 20);
            SetRectTransform(retryBtn.gameObject, new Vector2(0.2f, 0.16f), new Vector2(0.8f, 0.26f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("retryButton").objectReferenceValue = retryBtn;

            Button compMenuBtn = CreateUIButton(completePanel.transform, "MenuBtn", "MAIN MENU", defaultFont, 20);
            SetRectTransform(compMenuBtn.gameObject, new Vector2(0.2f, 0.04f), new Vector2(0.8f, 0.14f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("completeMenuButton").objectReferenceValue = compMenuBtn;

            // 7. Course Complete Modal (After Course 5)
            GameObject courseCompPanel = CreateUIPanel(canvasObj.transform, "CourseCompleteModal", new Vector2(0.25f, 0.20f), new Vector2(0.75f, 0.80f), Vector2.zero, Vector2.zero);
            SetPanelBackground(courseCompPanel, new Color(0.04f, 0.08f, 0.15f, 0.98f), true);
            uiSo.FindProperty("courseCompletePanel").objectReferenceValue = courseCompPanel;

            Text ccTitle = CreateUIText(courseCompPanel.transform, "CCTitle", "TOURNAMENT COMPLETED!", defaultFont, 40, TextAnchor.MiddleCenter, new Color(1.0f, 0.85f, 0.25f));
            SetRectTransform(ccTitle.gameObject, new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.96f), Vector2.zero, Vector2.zero);

            Text finalScore = CreateUIText(courseCompPanel.transform, "FinalScore", "FINAL SCORE: 14 (E)", defaultFont, 36, TextAnchor.MiddleCenter, Color.white);
            SetRectTransform(finalScore.gameObject, new Vector2(0.05f, 0.65f), new Vector2(0.95f, 0.80f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("finalScoreText").objectReferenceValue = finalScore;

            Text finalSumm = CreateUIText(courseCompPanel.transform, "FinalSummary", "You completed all 5 miniature golf courses surrounded by scenic trees, ponds, and peaceful nature!", defaultFont, 20, TextAnchor.MiddleCenter, new Color(0.8f, 0.9f, 1f));
            SetRectTransform(finalSumm.gameObject, new Vector2(0.05f, 0.45f), new Vector2(0.95f, 0.62f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("finalSummaryText").objectReferenceValue = finalSumm;

            Button replayCourseBtn = CreateUIButton(courseCompPanel.transform, "ReplayCourseBtn", "REPLAY 5 COURSES", defaultFont, 22);
            SetRectTransform(replayCourseBtn.gameObject, new Vector2(0.2f, 0.25f), new Vector2(0.8f, 0.38f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("replayCourseBtn").objectReferenceValue = replayCourseBtn;

            Button returnMenuBtn = CreateUIButton(courseCompPanel.transform, "ReturnMenuBtn", "MAIN MENU", defaultFont, 22);
            SetRectTransform(returnMenuBtn.gameObject, new Vector2(0.2f, 0.08f), new Vector2(0.8f, 0.21f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("returnMenuBtn").objectReferenceValue = returnMenuBtn;

            // 8. Pause Modal Panel
            GameObject pausePanel = CreateUIPanel(canvasObj.transform, "PauseModal", new Vector2(0.32f, 0.20f), new Vector2(0.68f, 0.80f), Vector2.zero, Vector2.zero);
            SetPanelBackground(pausePanel, new Color(0.06f, 0.08f, 0.15f, 0.95f), true);
            uiSo.FindProperty("pausePanel").objectReferenceValue = pausePanel;

            Text pauseTitle = CreateUIText(pausePanel.transform, "PauseTitle", "GAME PAUSED", defaultFont, 36, TextAnchor.MiddleCenter, Color.cyan);
            SetRectTransform(pauseTitle.gameObject, new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.95f), Vector2.zero, Vector2.zero);

            Button resumeBtn = CreateUIButton(pausePanel.transform, "ResumeBtn", "RESUME", defaultFont, 22);
            SetRectTransform(resumeBtn.gameObject, new Vector2(0.2f, 0.64f), new Vector2(0.8f, 0.77f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("resumeButton").objectReferenceValue = resumeBtn;

            Button pauseRestartBtn = CreateUIButton(pausePanel.transform, "RestartHoleBtn", "RESTART COURSE (R)", defaultFont, 20);
            SetRectTransform(pauseRestartBtn.gameObject, new Vector2(0.2f, 0.49f), new Vector2(0.8f, 0.62f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("pauseRestartButton").objectReferenceValue = pauseRestartBtn;

            Button pauseRestartCourseBtn = CreateUIButton(pausePanel.transform, "RestartCourseBtn", "RESTART 5 COURSES", defaultFont, 20);
            SetRectTransform(pauseRestartCourseBtn.gameObject, new Vector2(0.2f, 0.34f), new Vector2(0.8f, 0.47f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("pauseRestartCourseButton").objectReferenceValue = pauseRestartCourseBtn;

            Button pauseSettingsBtn = CreateUIButton(pausePanel.transform, "SettingsBtn", "SETTINGS", defaultFont, 20);
            SetRectTransform(pauseSettingsBtn.gameObject, new Vector2(0.2f, 0.19f), new Vector2(0.8f, 0.32f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("pauseSettingsButton").objectReferenceValue = pauseSettingsBtn;

            Button pauseMenuBtn = CreateUIButton(pausePanel.transform, "MenuBtn", "MAIN MENU", defaultFont, 20);
            SetRectTransform(pauseMenuBtn.gameObject, new Vector2(0.2f, 0.04f), new Vector2(0.8f, 0.17f), Vector2.zero, Vector2.zero);
            uiSo.FindProperty("pauseMenuButton").objectReferenceValue = pauseMenuBtn;

            // 9. Settings Panel
            GameObject settingsPanel = CreateUIPanel(canvasObj.transform, "SettingsPanel", new Vector2(0.30f, 0.25f), new Vector2(0.70f, 0.75f), Vector2.zero, Vector2.zero);
            SetPanelBackground(settingsPanel, new Color(0.06f, 0.08f, 0.16f, 0.98f), true);
            uiSo.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;

            Text setHeader = CreateUIText(settingsPanel.transform, "Header", "AUDIO & CONTROLS", defaultFont, 32, TextAnchor.MiddleCenter, Color.cyan);
            SetRectTransform(setHeader.gameObject, new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.96f), Vector2.zero, Vector2.zero);

            Text sfxLabel = CreateUIText(settingsPanel.transform, "SFXLabel", "SFX Volume: 85%", defaultFont, 20, TextAnchor.MiddleLeft, Color.white);
            SetRectTransform(sfxLabel.gameObject, new Vector2(0.15f, 0.64f), new Vector2(0.85f, 0.74f), Vector2.zero, Vector2.zero);

            Text musicLabel = CreateUIText(settingsPanel.transform, "MusicLabel", "Ambient Volume: 45%", defaultFont, 20, TextAnchor.MiddleLeft, Color.white);
            SetRectTransform(musicLabel.gameObject, new Vector2(0.15f, 0.46f), new Vector2(0.85f, 0.56f), Vector2.zero, Vector2.zero);

            Text sensLabel = CreateUIText(settingsPanel.transform, "SensLabel", "Mouse Sensitivity: 2.5", defaultFont, 20, TextAnchor.MiddleLeft, Color.white);
            SetRectTransform(sensLabel.gameObject, new Vector2(0.15f, 0.28f), new Vector2(0.85f, 0.38f), Vector2.zero, Vector2.zero);

            Button closeSettings = CreateUIButton(settingsPanel.transform, "CloseBtn", "BACK", defaultFont, 20);
            SetRectTransform(closeSettings.gameObject, new Vector2(0.3f, 0.06f), new Vector2(0.7f, 0.18f), Vector2.zero, Vector2.zero);
            closeSettings.onClick.AddListener(() => settingsPanel.SetActive(false));

            uiSo.ApplyModifiedProperties();
        }

        private static void BuildMainMenuUI()
        {
            Font defaultFont = GetSystemFont();

            GameObject canvasObj = new GameObject("MainMenuCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            MainMenuController menuCtrl = canvasObj.AddComponent<MainMenuController>();
            SerializedObject menuSo = new SerializedObject(menuCtrl);

            GameObject sidePanel = CreateUIPanel(canvasObj.transform, "SideMenuPanel", new Vector2(0.06f, 0.08f), new Vector2(0.44f, 0.92f), Vector2.zero, Vector2.zero);
            SetPanelBackground(sidePanel, new Color(0.04f, 0.07f, 0.14f, 0.90f));

            Text title = CreateUIText(sidePanel.transform, "GameTitle", "REALISTIC 3D\nMINI GOLF\n5 COURSES", defaultFont, 36, TextAnchor.MiddleLeft, new Color(0.2f, 0.95f, 0.45f));
            SetRectTransform(title.gameObject, new Vector2(0.08f, 0.77f), new Vector2(0.92f, 0.95f), Vector2.zero, Vector2.zero);

            Text tagline = CreateUIText(sidePanel.transform, "Tagline", "Championship Park Blueprint (Zero Trees, Clean View)", defaultFont, 16, TextAnchor.MiddleLeft, new Color(0.95f, 0.95f, 0.85f, 0.9f));
            SetRectTransform(tagline.gameObject, new Vector2(0.08f, 0.70f), new Vector2(0.92f, 0.76f), Vector2.zero, Vector2.zero);

            Button playBtn = CreateUIButton(sidePanel.transform, "PlayBtn", "▶  PLAY 5 COURSES", defaultFont, 24);
            SetRectTransform(playBtn.gameObject, new Vector2(0.08f, 0.57f), new Vector2(0.92f, 0.67f), Vector2.zero, Vector2.zero);
            menuSo.FindProperty("playButton").objectReferenceValue = playBtn;

            Button levelSelectBtn = CreateUIButton(sidePanel.transform, "LevelSelectBtn", "COURSE SELECT (1 - 5)", defaultFont, 20);
            SetRectTransform(levelSelectBtn.gameObject, new Vector2(0.08f, 0.45f), new Vector2(0.92f, 0.55f), Vector2.zero, Vector2.zero);
            menuSo.FindProperty("levelSelectButton").objectReferenceValue = levelSelectBtn;

            Button practiceBtn = CreateUIButton(sidePanel.transform, "PracticeBtn", "PRACTICE GREEN", defaultFont, 20);
            SetRectTransform(practiceBtn.gameObject, new Vector2(0.08f, 0.33f), new Vector2(0.92f, 0.43f), Vector2.zero, Vector2.zero);
            menuSo.FindProperty("practiceGreenButton").objectReferenceValue = practiceBtn;

            Button howToPlayBtn = CreateUIButton(sidePanel.transform, "HowToPlayBtn", "HOW TO PLAY", defaultFont, 20);
            SetRectTransform(howToPlayBtn.gameObject, new Vector2(0.08f, 0.21f), new Vector2(0.92f, 0.31f), Vector2.zero, Vector2.zero);
            menuSo.FindProperty("howToPlayButton").objectReferenceValue = howToPlayBtn;

            Button settingsBtn = CreateUIButton(sidePanel.transform, "SettingsBtn", "SETTINGS", defaultFont, 20);
            SetRectTransform(settingsBtn.gameObject, new Vector2(0.08f, 0.10f), new Vector2(0.50f, 0.19f), Vector2.zero, Vector2.zero);
            menuSo.FindProperty("settingsButton").objectReferenceValue = settingsBtn;

            Button quitBtn = CreateUIButton(sidePanel.transform, "QuitBtn", "QUIT", defaultFont, 20);
            SetRectTransform(quitBtn.gameObject, new Vector2(0.54f, 0.10f), new Vector2(0.92f, 0.19f), Vector2.zero, Vector2.zero);
            menuSo.FindProperty("quitButton").objectReferenceValue = quitBtn;

            // Course Select Modal (5 Courses)
            GameObject lvlModal = CreateUIPanel(canvasObj.transform, "LevelSelectModal", new Vector2(0.46f, 0.12f), new Vector2(0.95f, 0.88f), Vector2.zero, Vector2.zero);
            SetPanelBackground(lvlModal, new Color(0.04f, 0.08f, 0.15f, 0.98f));
            menuSo.FindProperty("levelSelectModal").objectReferenceValue = lvlModal;

            Text lvlHeader = CreateUIText(lvlModal.transform, "Header", "SELECT MINI GOLF COURSE", defaultFont, 30, TextAnchor.MiddleCenter, new Color(0.2f, 0.95f, 0.45f));
            SetRectTransform(lvlHeader.gameObject, new Vector2(0.05f, 0.89f), new Vector2(0.95f, 0.97f), Vector2.zero, Vector2.zero);

            GameObject gridObj = new GameObject("CourseButtonsContainer");
            gridObj.transform.SetParent(lvlModal.transform, false);
            RectTransform gridRt = gridObj.AddComponent<RectTransform>();
            gridRt.anchorMin = new Vector2(0.05f, 0.12f);
            gridRt.anchorMax = new Vector2(0.95f, 0.88f);
            gridRt.offsetMin = Vector2.zero;
            gridRt.offsetMax = Vector2.zero;
            menuSo.FindProperty("levelButtonsContainer").objectReferenceValue = gridRt;

            string[] courseLabels = new string[] {
                "COURSE 1: THE GRAND ENTRANCE (PAR 2)\nCurving Fairway & Right-Side Sand Bunker",
                "COURSE 2: RIVER FAIRWAY & SAND TRAP (PAR 3)\nLong North River Fairway with Central Bunker",
                "COURSE 3: WATERFALL GROTTO DROP (PAR 3)\nMulti-Level Terrace Stepped Stone Ramp",
                "COURSE 4: ISLAND BRIDGE CROSSING (PAR 3)\nWooden River Footbridge Crossing to Island Green",
                "COURSE 5: GRAND FINALE POND WRAP (PAR 3)\nChampionship S-Curve Surrounding Central Pond"
            };

            for (int i = 0; i < courseLabels.Length; i++)
            {
                float yMax = 1.0f - (i * 0.19f) - 0.015f;
                float yMin = yMax - 0.155f;

                Button cBtn = CreateUIButton(gridRt.transform, $"Course_{i + 1}_Btn", courseLabels[i], defaultFont, 17);
                SetRectTransform(cBtn.gameObject, new Vector2(0.02f, yMin), new Vector2(0.98f, yMax), Vector2.zero, Vector2.zero);
            }

            Button closeLvlBtn = CreateUIButton(lvlModal.transform, "CloseLvlBtn", "CLOSE", defaultFont, 18);
            SetRectTransform(closeLvlBtn.gameObject, new Vector2(0.40f, 0.02f), new Vector2(0.60f, 0.10f), Vector2.zero, Vector2.zero);
            menuSo.FindProperty("closeLevelSelectBtn").objectReferenceValue = closeLvlBtn;

            // How To Play Modal
            GameObject howModal = CreateUIPanel(canvasObj.transform, "HowToPlayModal", new Vector2(0.46f, 0.12f), new Vector2(0.95f, 0.88f), Vector2.zero, Vector2.zero);
            SetPanelBackground(howModal, new Color(0.04f, 0.08f, 0.15f, 0.98f));
            menuSo.FindProperty("howToPlayModal").objectReferenceValue = howModal;

            Text howHeader = CreateUIText(howModal.transform, "Header", "HOW TO PLAY & CONTROLS", defaultFont, 32, TextAnchor.MiddleCenter, new Color(0.2f, 0.95f, 0.45f));
            SetRectTransform(howHeader.gameObject, new Vector2(0.05f, 0.90f), new Vector2(0.95f, 0.98f), Vector2.zero, Vector2.zero);

            string instructions = 
                "• REALISTIC PUTTING CONTROLS:\n" +
                "  Mouse: Drag backward from ball to set aim & power, then release to putt!\n" +
                "  Keyboard: Aim with Mouse/A/D/Arrows, hold [SPACEBAR] to charge power, release to putt!\n" +
                "  Right Click: Cancel current shot while aiming/charging.\n\n" +
                "• CAMERA & NAVIGATION:\n" +
                "  Hold Right Mouse Button to orbit camera around ball.\n" +
                "  Scroll Wheel to zoom in / out.\n" +
                "  Press [M] to toggle bird's-eye Map Overview of the entire 5-course park!\n" +
                "  Press [TAB] to inspect the 5-course Championship Scorecard.\n" +
                "  Press [R] to reset ball to tee | [Esc] or [P] to Pause.\n\n" +
                "• SCENIC TREE-FREE LAYOUT:\n" +
                "  Clean, unobstructed sightlines across all 5 courses, ponds, and waterfalls!";

            Text instructText = CreateUIText(howModal.transform, "Instructions", instructions, defaultFont, 18, TextAnchor.UpperLeft, Color.white);
            SetRectTransform(instructText.gameObject, new Vector2(0.06f, 0.14f), new Vector2(0.94f, 0.88f), Vector2.zero, Vector2.zero);

            Button closeHowBtn = CreateUIButton(howModal.transform, "CloseHowBtn", "CLOSE", defaultFont, 18);
            SetRectTransform(closeHowBtn.gameObject, new Vector2(0.40f, 0.03f), new Vector2(0.60f, 0.11f), Vector2.zero, Vector2.zero);
            menuSo.FindProperty("closeHowToPlayBtn").objectReferenceValue = closeHowBtn;

            // Settings Modal
            GameObject setModal = CreateUIPanel(canvasObj.transform, "SettingsModal", new Vector2(0.46f, 0.20f), new Vector2(0.90f, 0.80f), Vector2.zero, Vector2.zero);
            SetPanelBackground(setModal, new Color(0.04f, 0.08f, 0.15f, 0.98f));
            menuSo.FindProperty("settingsModal").objectReferenceValue = setModal;

            Text setHeader = CreateUIText(setModal.transform, "Header", "SETTINGS", defaultFont, 32, TextAnchor.MiddleCenter, Color.cyan);
            SetRectTransform(setHeader.gameObject, new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.96f), Vector2.zero, Vector2.zero);

            Text sfxText = CreateUIText(setModal.transform, "SFX", "Sound Effects: 85% (Procedural Synthesis)", defaultFont, 20, TextAnchor.MiddleLeft, Color.white);
            SetRectTransform(sfxText.gameObject, new Vector2(0.12f, 0.60f), new Vector2(0.88f, 0.72f), Vector2.zero, Vector2.zero);

            Text ambText = CreateUIText(setModal.transform, "Ambient", "Atmosphere & Water: 45%", defaultFont, 20, TextAnchor.MiddleLeft, Color.white);
            SetRectTransform(ambText.gameObject, new Vector2(0.12f, 0.42f), new Vector2(0.88f, 0.54f), Vector2.zero, Vector2.zero);

            Text inputInfo = CreateUIText(setModal.transform, "InputInfo", "Controls: Mouse Drag / Spacebar Charge / Arrow Keys", defaultFont, 18, TextAnchor.MiddleLeft, new Color(0.7f, 0.85f, 1f));
            SetRectTransform(inputInfo.gameObject, new Vector2(0.12f, 0.24f), new Vector2(0.88f, 0.36f), Vector2.zero, Vector2.zero);

            Button closeSetBtn = CreateUIButton(setModal.transform, "CloseSetBtn", "CLOSE", defaultFont, 18);
            SetRectTransform(closeSetBtn.gameObject, new Vector2(0.35f, 0.05f), new Vector2(0.65f, 0.16f), Vector2.zero, Vector2.zero);
            menuSo.FindProperty("closeSettingsBtn").objectReferenceValue = closeSetBtn;

            menuSo.ApplyModifiedProperties();
        }

        #endregion

        #region UI Helpers

        private static GameObject CreateUIPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return obj;
        }

        private static void SetPanelBackground(GameObject obj, Color color, bool raycastTarget = false)
        {
            Image img = obj.GetComponent<Image>();
            if (img == null) img = obj.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = raycastTarget;
        }

        private static Text CreateUIText(Transform parent, string name, string content, Font font, int fontSize, TextAnchor alignment, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            Text txt = obj.AddComponent<Text>();
            txt.text = content;
            txt.font = font;
            txt.fontSize = fontSize;
            txt.alignment = alignment;
            txt.color = color;
            txt.raycastTarget = false;
            return txt;
        }

        private static Button CreateUIButton(Transform parent, string name, string label, Font font, int fontSize)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.12f, 0.18f, 0.3f, 0.95f);

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(0.12f, 0.18f, 0.3f, 0.95f);
            cb.highlightedColor = new Color(0.18f, 0.4f, 0.65f, 1f);
            cb.pressedColor = new Color(0.1f, 0.6f, 0.9f, 1f);
            cb.selectedColor = cb.highlightedColor;
            cb.disabledColor = new Color(0.1f, 0.1f, 0.15f, 0.5f);
            btn.colors = cb;

            Text txt = CreateUIText(btnObj.transform, "Label", label, font, fontSize, TextAnchor.MiddleCenter, Color.white);
            SetRectTransform(txt.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            return btn;
        }

        private static void SetRectTransform(GameObject obj, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt == null) rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        #endregion

        #region Build Settings

        public static void ConfigureBuildSettings()
        {
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Gameplay.unity", true)
            };
            EditorBuildSettings.scenes = scenes;
            Debug.Log("[AntiGravityGolf] Configured EditorBuildSettings with MainMenu (0) and Gameplay (1).");
        }

        #endregion
    }
}
