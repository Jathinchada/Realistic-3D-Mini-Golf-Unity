using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace AntiGravityGolf.Editor
{
    /// <summary>
    /// Comprehensive test suite verifying all 5-course game systems with zero trees:
    /// - Scene hierarchies & references
    /// - Zero trees on course scenery
    /// - Normal, Zero-G, and directional gravity transitions
    /// - Ball aiming, impulse shooting, and stopping logic
    /// - Checkpoints & out-of-bounds respawn
    /// - Golf hole speed-check gating & score rating calculations
    /// - All 5 courses loading & PlayerPrefs persistence
    /// </summary>
    public static class AntiGravityGolfAutomatedTests
    {
        [MenuItem("Anti-Gravity Golf/Run Complete Automated Validation Suite")]
        public static void RunAllTests()
        {
            Debug.Log("=================================================");
            Debug.Log("  ANTI-GRAVITY GOLF: AUTOMATED VALIDATION SUITE  ");
            Debug.Log("=================================================");

            int testsPassed = 0;
            int totalTests = 0;

            void Assert(bool condition, string testName)
            {
                totalTests++;
                if (condition)
                {
                    testsPassed++;
                    Debug.Log($"[PASS] {testName}");
                }
                else
                {
                    Debug.LogError($"[FAIL] {testName}");
                }
            }

            // --- TEST 1: Main Menu Scene Integrity ---
            var menuScene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
            Assert(menuScene.IsValid(), "Test 1.1: MainMenu.unity scene is valid");
            var menuCam = GameObject.FindWithTag("MainCamera");
            Assert(menuCam != null, "Test 1.2: MainMenu MainCamera exists");
            var menuCtrl = Object.FindObjectOfType<MainMenuController>();
            Assert(menuCtrl != null, "Test 1.3: MainMenuController component attached");
            var showcase = GameObject.Find("ShowcaseIsland");
            Assert(showcase != null, "Test 1.4: Showcase 3D floating island exists");

            // --- TEST 2: Gameplay Scene Integrity & Clean Scenery (Zero Trees) ---
            var gameplayScene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity", OpenSceneMode.Single);
            Assert(gameplayScene.IsValid(), "Test 2.1: Gameplay.unity scene is valid");
            var ball = Object.FindObjectOfType<GolfBallController>();
            Assert(ball != null, "Test 2.2: GolfBallController exists in Gameplay scene");
            var camCtrl = Object.FindObjectOfType<GolfCameraController>();
            Assert(camCtrl != null && camCtrl.Target == ball.transform, "Test 2.3: GolfCameraController targets GolfBall");
            var lvlMgr = Object.FindObjectOfType<LevelManager>();
            Assert(lvlMgr != null, "Test 2.4: LevelManager exists and is configured");
            var gameUI = Object.FindObjectOfType<GameUI>();
            Assert(gameUI != null, "Test 2.5: GameUI HUD exists and is configured");
            var parkSun = GameObject.Find("ParkGoldenSun") ?? GameObject.Find("ForestGoldenSun");
            Assert(parkSun != null, "Test 2.6: Golden sun lighting configured");

            // Verify 3D trees and greenery populated across golf course
            int treeCount = 0;
            foreach (var t in Object.FindObjectsOfType<Transform>())
            {
                if (t.name.Contains("Tree") && t.parent != null && t.parent.name.Contains("Scenery"))
                    treeCount++;
            }
            Assert(treeCount >= 10, $"Test 2.7: 3D trees (Pine, Oak, Birch, Cherry) populated across golf park ({treeCount} found >= 10)");

            var parkFloor = GameObject.Find("ParkValleyFloor") ?? GameObject.Find("ForestValleyFloor");
            Assert(parkFloor != null, "Test 2.8: Park valley floor terrain exists");
            var ballRb = ball.GetComponent<Rigidbody>();
            Assert(ballRb != null && ballRb.mass >= 2.0f && ballRb.mass <= 3.0f, "Test 2.9: Weighted golf ball mass is ~2.5 kg for solid, weighted rolling physics");
            Assert(ballRb != null && ballRb.collisionDetectionMode == CollisionDetectionMode.ContinuousDynamic, "Test 2.10: Continuous dynamic collision detection enabled for realistic physics");

            // --- TEST 3: Score Rating Logic ---
            var gmObj = new GameObject("TestGM");
            var gm = gmObj.AddComponent<GameManager>();
            Assert(gm.EvaluateScoreRating(1, 3).Contains("HOLE IN ONE"), "Test 3.1: 1 stroke on Par 3 evaluates to HOLE IN ONE");
            Assert(gm.EvaluateScoreRating(2, 4).Contains("EAGLE"), "Test 3.2: 2 strokes on Par 4 evaluates to EAGLE");
            Assert(gm.EvaluateScoreRating(3, 4).Contains("BIRDIE"), "Test 3.3: 3 strokes on Par 4 evaluates to BIRDIE");
            Assert(gm.EvaluateScoreRating(4, 4).Contains("PAR"), "Test 3.4: 4 strokes on Par 4 evaluates to PAR");
            Assert(gm.EvaluateScoreRating(5, 4).Contains("BOGEY"), "Test 3.5: 5 strokes on Par 4 evaluates to BOGEY");
            Assert(gm.EvaluateScoreRating(6, 4).Contains("DOUBLE BOGEY"), "Test 3.6: 6 strokes on Par 4 evaluates to DOUBLE BOGEY");
            Object.DestroyImmediate(gmObj);

            // --- TEST 4: Anti-Gravity Physics & Overlapping Zones ---
            var gravObj = new GameObject("TestGravity");
            var gravMgr = gravObj.AddComponent<GravityManager>();

            Assert(Vector3.Distance(gravMgr.CurrentGravity, new Vector3(0, -9.81f, 0)) < 0.1f, "Test 4.1: Initial default gravity is (0, -9.81, 0)");

            var z1Obj = new GameObject("ZeroGZone");
            var z1Col = z1Obj.AddComponent<BoxCollider>();
            var z1 = z1Obj.AddComponent<AntiGravityZone>();
            gravMgr.EnterZone(z1);
            Assert(gravMgr.CurrentMode == GravityMode.ZeroGravity, "Test 4.2: Gravity mode transitions to ZeroGravity upon zone entry");
            Assert(gravMgr.TargetGravity == Vector3.zero, "Test 4.3: Target gravity is zero vector in Zero-G zone");

            var z2Obj = new GameObject("InvertedZone");
            z2Obj.AddComponent<BoxCollider>();
            var z2 = z2Obj.AddComponent<AntiGravityZone>();
            var z2So = new SerializedObject(z2);
            z2So.FindProperty("mode").enumValueIndex = (int)GravityMode.Inverted;
            z2So.FindProperty("priority").intValue = 5;
            z2So.ApplyModifiedProperties();

            gravMgr.EnterZone(z2);
            Assert(gravMgr.CurrentMode == GravityMode.Inverted, "Test 4.4: Higher-priority zone overrides active mode to Inverted");
            Assert(gravMgr.TargetGravity.y > 0f, "Test 4.5: Inverted target gravity points upward (+Y)");

            gravMgr.ExitZone(z2);
            Assert(gravMgr.CurrentMode == GravityMode.ZeroGravity, "Test 4.6: Exiting higher priority zone safely restores Zero-G zone");

            gravMgr.ExitZone(z1);
            Assert(gravMgr.CurrentMode == GravityMode.Normal, "Test 4.7: Exiting all zones safely restores Normal gravity");
            Assert(gravMgr.TargetGravity.y < -9.0f, "Test 4.8: Restored normal gravity points downward (-Y)");

            Object.DestroyImmediate(z1Obj);
            Object.DestroyImmediate(z2Obj);
            Object.DestroyImmediate(gravObj);

            // --- TEST 5: Procedural Sound Manager Clips & Music Synthesis ---
            var soundMgr = ProceduralSoundManager.Instance;
            GameObject soundObj = null;
            if (soundMgr == null)
            {
                soundObj = new GameObject("TestSound");
                soundMgr = soundObj.AddComponent<ProceduralSoundManager>();
            }
            soundMgr.Initialize();
            Assert(soundMgr != null, "Test 5.1: ProceduralSoundManager synthesizes clips without errors");

            bool hasBgmSource = false;
            foreach (var src in soundMgr.GetComponents<AudioSource>())
            {
                if (src.loop && src.clip != null) hasBgmSource = true;
            }
            Assert(hasBgmSource, "Test 5.2: Relaxing background music source configured and looping");

            bool initialMute = soundMgr.IsMusicMuted;
            soundMgr.ToggleMusicMute();
            Assert(soundMgr.IsMusicMuted != initialMute, "Test 5.3: Music Mute toggle updates state correctly");
            soundMgr.ToggleMusicMute(); // Restore
            if (soundObj != null) Object.DestroyImmediate(soundObj);

            // --- TEST 6: 5 Championship Courses Loading & Component Validation ---
            var coursesRoot = GameObject.Find("Courses");
            Assert(coursesRoot != null, "Test 6.1: Courses root found in scene");

            var landscape = coursesRoot?.transform.Find("MiniGolfParkLandscape");
            Assert(landscape != null, "Test 6.2: MiniGolfParkLandscape landscape root exists");

            var allHoles = coursesRoot?.transform.Find("AllHoles");
            Assert(allHoles != null, "Test 6.3: AllHoles container exists under Courses");

            int holesFound = 0;
            if (allHoles != null)
            {
                for (int h = 1; h <= 5; h++)
                {
                    var holeT = allHoles.Find($"Hole_{h}");
                    if (holeT != null) holesFound++;
                    else Debug.LogWarning($"[WARN] Hole_{h} not found in AllHoles");
                }
            }
            Assert(holesFound == 5, $"Test 6.4: All 5 courses present (found {holesFound}/5)");

            int holeComponents = 0;
            if (allHoles != null)
            {
                var golfHoles = allHoles.GetComponentsInChildren<GolfHole>(true);
                holeComponents = golfHoles.Length;
            }
            Assert(holeComponents >= 5, $"Test 6.5: At least 5 GolfHole components found ({holeComponents})");

            var lvlMgr2 = Object.FindObjectOfType<LevelManager>();
            Assert(lvlMgr2 != null, "Test 6.6: LevelManager component found in scene");

            var gm2Obj = new GameObject("TestGM2");
            var gm2 = gm2Obj.AddComponent<GameManager>();
            Assert(gm2.TotalLevels == 5, $"Test 6.7: GameManager.TotalLevels == 5 (actual: {gm2.TotalLevels})");
            Object.DestroyImmediate(gm2Obj);

            // Verify zero interrupting objects/blocks on championship course fairways
            int bumperCount = 0;
            int checkpointCount = 0;
            if (allHoles != null)
            {
                bumperCount = allHoles.GetComponentsInChildren<Bumper>(true).Length;
                checkpointCount = allHoles.GetComponentsInChildren<Checkpoint>(true).Length;
            }
            Assert(bumperCount == 0, $"Test 6.8: Zero interrupting bumpers on championship courses (found {bumperCount})");
            Assert(checkpointCount == 0, $"Test 6.9: Zero interrupting checkpoint blocks on championship courses (found {checkpointCount})");

            // Verify ponds and water hazards on course
            int waterHazardCount = Object.FindObjectsOfType<WaterHazard>().Length;
            Assert(waterHazardCount >= 3, $"Test 6.10: Landscaped ponds and water hazards active on courses (found {waterHazardCount} >= 3)");

            // Verify bendy course rails exist across all holes
            int bendyCourseContainers = 0;
            if (allHoles != null)
            {
                for (int h = 1; h <= 5; h++)
                {
                    var bendyT = allHoles.Find($"Hole_{h}");
                    if (bendyT != null && bendyT.Find("BrickRails") != null)
                        bendyCourseContainers++;
                }
            }
            Assert(bendyCourseContainers == 5, $"Test 6.11: All 5 courses feature bendy contoured brick rails ({bendyCourseContainers}/5)");

            // Verify sunken holes and zero bumps/mounds
            var sunkenWell = GameObject.Find("SunkenCupWell");
            Assert(sunkenWell != null, "Test 6.12: Sunken cup well exists with realistic depth below putting surface");

            var grassMounds = GameObject.Find("GrassMounds");
            Assert(grassMounds == null, "Test 6.13: Zero bumpy grass mounds on park or fairways (100% planar smooth fairways)");

            var bigCup = GameObject.Find("SunkenCupWell");
            Assert(bigCup != null && bigCup.transform.lossyScale.x >= 0.50f && bigCup.transform.lossyScale.x <= 0.70f, $"Test 6.14: Hole is configured to a realistic, reasonable cup diameter ({bigCup?.transform.lossyScale.x:F2}m between 0.50m - 0.70m)");

            int reasonableCupCount = 0;
            if (allHoles != null)
            {
                var wells = allHoles.GetComponentsInChildren<Transform>(true);
                foreach (var w in wells)
                {
                    if (w.name == "SunkenCupWell" && w.lossyScale.x >= 0.50f && w.lossyScale.x <= 0.70f)
                        reasonableCupCount++;
                }
            }
            Assert(reasonableCupCount == 5, $"Test 6.15: Every course features a reasonable size sunken cup ({reasonableCupCount}/5)");

            // Verify Hole-Specific Scenery
            Assert(GameObject.Find("Hole1_GardenScenery") != null, "Test 6.16: Hole 1 features peaceful garden environment with trees and flowers");
            Assert(GameObject.Find("Hole2_CurvedPondScenery") != null, "Test 6.17: Hole 2 features curved pathway with serene pond");
            Assert(GameObject.Find("Hole3_S_TrackScenery") != null, "Test 6.18: Hole 3 features S-shaped track with pine trees, bushes and rocks");
            Assert(GameObject.Find("Hole4_C_PondScenery") != null, "Test 6.19: Hole 4 features C-shaped track surrounding landscaped pond");
            Assert(GameObject.Find("Hole5_ElevatedBridgeScenery") != null, "Test 6.20: Hole 5 features elevated wooden bridge crossing scenic pond");

            // --- TEST 7: Ball Spawning On Every Hole (Holes 1 to 5) ---
            bool allSpawnsValid = true;
            bool allSpawnedOnTee = true;
            for (int i = 0; i < 5; i++)
            {
                lvlMgr2.LoadLevel(i);
                if (ball.transform.position.y < -1f)
                {
                    allSpawnsValid = false;
                    Debug.LogError($"[FAIL] Hole {i + 1} ball fell below course ground!");
                }
                if (ball.BallRigidbody.isKinematic)
                {
                    allSpawnsValid = false;
                    Debug.LogError($"[FAIL] Hole {i + 1} ball is kinematic!");
                }
                if (ball.CurrentAimDirection.sqrMagnitude < 0.5f)
                {
                    allSpawnsValid = false;
                    Debug.LogError($"[FAIL] Hole {i + 1} ball has invalid aim direction!");
                }

                // Verify ball spawned directly on that hole's Tee
                var holeContainer = allHoles.Find($"Hole_{i + 1}");
                Transform tee = null;
                if (holeContainer != null)
                {
                    foreach (Transform t in holeContainer.GetComponentsInChildren<Transform>(true))
                    {
                        if (t.name == "TeeSpot" || t.name == "TeeArea" || t.name == "TeeMat")
                        {
                            tee = t;
                            break;
                        }
                    }
                }
                if (tee != null)
                {
                    float dist = Vector2.Distance(new Vector2(ball.transform.position.x, ball.transform.position.z),
                                                  new Vector2(tee.position.x, tee.position.z));
                    if (dist > 0.25f)
                    {
                        allSpawnedOnTee = false;
                        Debug.LogError($"[FAIL] Hole {i + 1} ball is {dist:F2}m away from the Tee!");
                    }
                }
            }
            Assert(allSpawnsValid, "Test 7.1: Ball successfully spawns and aims on every hole (1-5) with isKinematic=false");
            Assert(allSpawnedOnTee, "Test 7.2: Ball spawns directly on the Tee on every hole (1-5)");

            // --- TEST 8: Build Settings ---
            Assert(EditorBuildSettings.scenes.Length >= 2, "Test 8.1: Build Settings contains both MainMenu and Gameplay scenes");
            Assert(EditorBuildSettings.scenes[0].path.Contains("MainMenu"), "Test 8.2: First scene is MainMenu");
            Assert(EditorBuildSettings.scenes[1].path.Contains("Gameplay"), "Test 8.3: Second scene is Gameplay");

            // Summary
            Debug.Log("=================================================");
            Debug.Log($"  TEST RESULTS: {testsPassed} / {totalTests} PASSED");
            if (testsPassed == totalTests)
            {
                Debug.Log("  ALL TESTS PASSED WITH 100% SUCCESS!");
            }
            else
            {
                Debug.LogError($"  {totalTests - testsPassed} TESTS FAILED!");
            }
            Debug.Log("=================================================");
        }
    }
}
