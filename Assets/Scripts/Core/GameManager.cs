using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AntiGravityGolf
{
    public enum GameState
    {
        MainMenu,
        Playing,
        HoleCompleted,
        CourseCompleted,
        Paused
    }

    /// <summary>
    /// Master GameManager orchestrating the 5-course golf round, strokes, par calculations,
    /// scorecard tracking, practice mode, water penalties, and course progression.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("5 Courses Blueprint Configuration")]
        [SerializeField] private LevelData[] levels = new LevelData[]
        {
            new LevelData { levelName = "Course 1: The Winding S-Bend", par = 2, description = "A classic double-curve putting lane that tests your touch and banking skills around smooth brick curbs." },
            new LevelData { levelName = "Course 2: The Double Dogleg", par = 3, description = "Strategic zig-zag fairway with dual 90-degree dogleg bends across the northern park lawn." },
            new LevelData { levelName = "Course 3: The Horseshoe Loop", par = 3, description = "An iconic 180-degree hairpin curve sweeping around the terrace green." },
            new LevelData { levelName = "Course 4: The Serpentine Chicane", par = 3, description = "A thrilling winding chicane path that bends back and forth to the island cup." },
            new LevelData { levelName = "Course 5: The Grand Finale Crescent", par = 3, description = "A magnificent sweeping crescent arc wrapping around the central park landscape." }
        };

        [Header("State")]
        [SerializeField] private GameState currentState = GameState.Playing;
        [SerializeField] private int currentLevelIndex = 0;
        [SerializeField] private int currentStrokes = 0;
        [SerializeField] private bool isPracticeMode = false;

        // 5-course scorecard tracking
        private int[] strokesPerHole = new int[5];
        private bool[] holeCompleted = new bool[5];

        public event Action<int> OnStrokesChanged;
        public event Action<int, LevelData> OnLevelLoaded;
        public event Action<string, int, int> OnHoleCompleted; // rating, strokes, par
        public event Action<int, int> OnCourseCompleted; // totalStrokes, totalPar
        public event Action<string> OnPenaltyApplied;
        public event Action<bool> OnPauseToggled;

        public GameState CurrentState => currentState;
        public int CurrentLevelIndex => currentLevelIndex;
        public int CurrentStrokes => currentStrokes;
        public LevelData CurrentLevelData => (levels != null && currentLevelIndex < levels.Length) ? levels[currentLevelIndex] : null;
        public LevelData[] AllLevels => levels;
        public bool IsPaused => currentState == GameState.Paused;
        public int TotalLevels => levels != null ? levels.Length : 5;
        public bool IsPracticeMode => isPracticeMode;
        public int[] StrokesPerHole => strokesPerHole;
        public bool[] HoleCompleted => holeCompleted;

        public int CourseTotalPar
        {
            get
            {
                int total = 0;
                if (levels != null)
                {
                    for (int i = 0; i < levels.Length; i++) total += levels[i].par;
                }
                return total > 0 ? total : 14;
            }
        }

        public int CompletedStrokesTotal
        {
            get
            {
                int total = 0;
                for (int i = 0; i < 5; i++)
                {
                    if (holeCompleted[i]) total += strokesPerHole[i];
                }
                if (!isPracticeMode && currentState == GameState.Playing)
                {
                    total += currentStrokes;
                }
                return total;
            }
        }

        public int ScoreRelativeToPar
        {
            get
            {
                int scoreDiff = 0;
                for (int i = 0; i < 5; i++)
                {
                    if (holeCompleted[i] && i < levels.Length)
                    {
                        scoreDiff += (strokesPerHole[i] - levels[i].par);
                    }
                }
                return scoreDiff;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ResetScorecard();
        }

        private void Start()
        {
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                currentState = GameState.MainMenu;
            }
            else
            {
                StartLevel(currentLevelIndex);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                if (currentState == GameState.Playing)
                {
                    TogglePause(true);
                }
                else if (currentState == GameState.Paused)
                {
                    TogglePause(false);
                }
            }

            if (Input.GetKeyDown(KeyCode.R) && currentState == GameState.Playing)
            {
                RestartCurrentLevel();
            }

            if (Input.GetKeyDown(KeyCode.N) && currentState == GameState.HoleCompleted)
            {
                NextLevel();
            }
        }

        public void ResetScorecard()
        {
            strokesPerHole = new int[5];
            holeCompleted = new bool[5];
        }

        public void StartLevel(int levelIndex)
        {
            isPracticeMode = false;
            currentLevelIndex = Mathf.Clamp(levelIndex, 0, levels.Length - 1);
            currentStrokes = 0;
            currentState = GameState.Playing;
            Time.timeScale = 1f;

            OnStrokesChanged?.Invoke(currentStrokes);
            OnLevelLoaded?.Invoke(currentLevelIndex, CurrentLevelData);

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadLevel(currentLevelIndex);
            }
        }

        public void StartPracticeGreenMode()
        {
            isPracticeMode = true;
            currentLevelIndex = -1;
            currentStrokes = 0;
            currentState = GameState.Playing;
            Time.timeScale = 1f;

            LevelData practiceData = new LevelData
            {
                levelName = "Practice Putting Green",
                par = 2,
                description = "Warm up your putting stroke on the practice green near the entrance!"
            };

            OnStrokesChanged?.Invoke(currentStrokes);
            OnLevelLoaded?.Invoke(-1, practiceData);

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadPracticeGreen();
            }
        }

        public void RegisterStroke()
        {
            currentStrokes++;
            OnStrokesChanged?.Invoke(currentStrokes);
        }

        public void RegisterPenalty(string reason = "WATER HAZARD! +1 PENALTY STROKE")
        {
            currentStrokes++;
            OnStrokesChanged?.Invoke(currentStrokes);
            OnPenaltyApplied?.Invoke(reason);
        }

        public void CompleteCurrentHole()
        {
            if (isPracticeMode)
            {
                ProceduralSoundManager.Instance?.PlayHoleComplete();
                currentStrokes = 0;
                OnStrokesChanged?.Invoke(0);
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.ResetPracticeBall();
                }
                return;
            }

            int par = CurrentLevelData != null ? CurrentLevelData.par : 3;
            string rating = EvaluateScoreRating(currentStrokes, par);

            strokesPerHole[currentLevelIndex] = currentStrokes;
            holeCompleted[currentLevelIndex] = true;

            string scoreKey = $"BestScore_Course_{currentLevelIndex}";
            int best = PlayerPrefs.GetInt(scoreKey, 999);
            if (currentStrokes < best)
            {
                PlayerPrefs.SetInt(scoreKey, currentStrokes);
            }

            int unlocked = PlayerPrefs.GetInt("UnlockedLevels", 1);
            if (currentLevelIndex + 2 > unlocked && currentLevelIndex + 1 < levels.Length)
            {
                PlayerPrefs.SetInt("UnlockedLevels", currentLevelIndex + 2);
            }
            PlayerPrefs.Save();

            if (currentLevelIndex >= levels.Length - 1)
            {
                currentState = GameState.CourseCompleted;
                OnHoleCompleted?.Invoke(rating, currentStrokes, par);
                OnCourseCompleted?.Invoke(CompletedStrokesTotal, CourseTotalPar);
            }
            else
            {
                currentState = GameState.HoleCompleted;
                OnHoleCompleted?.Invoke(rating, currentStrokes, par);
            }
        }

        public string EvaluateScoreRating(int strokes, int par)
        {
            if (strokes == 1) return "HOLE IN ONE!";
            int diff = strokes - par;

            if (diff <= -3) return "ALBATROSS (-3)";
            if (diff == -2) return "EAGLE (-2)";
            if (diff == -1) return "BIRDIE (-1)";
            if (diff == 0) return "PAR (E)";
            if (diff == 1) return "BOGEY (+1)";
            if (diff == 2) return "DOUBLE BOGEY (+2)";
            if (diff == 3) return "TRIPLE BOGEY (+3)";
            return $"SCORE (+{diff})";
        }

        public void NextLevel()
        {
            if (currentLevelIndex + 1 < levels.Length)
            {
                StartLevel(currentLevelIndex + 1);
            }
            else
            {
                RestartFromHole1();
            }
        }

        public void RestartCurrentLevel()
        {
            if (isPracticeMode)
            {
                StartPracticeGreenMode();
            }
            else
            {
                StartLevel(currentLevelIndex);
            }
        }

        public void RestartFromHole1()
        {
            ResetScorecard();
            currentLevelIndex = 0;
            currentStrokes = 0;
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            OnStrokesChanged?.Invoke(currentStrokes);
            OnLevelLoaded?.Invoke(currentLevelIndex, CurrentLevelData);
            if (LevelManager.Instance != null)
                LevelManager.Instance.RestartFullCourse();
        }

        public void TogglePause(bool pause)
        {
            currentState = pause ? GameState.Paused : GameState.Playing;
            Time.timeScale = pause ? 0f : 1f;
            OnPauseToggled?.Invoke(pause);
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            currentState = GameState.MainMenu;
            SceneManager.LoadScene("MainMenu");
        }

        public void LoadGameplayScene(int levelIndex = 0, bool practice = false)
        {
            currentLevelIndex = levelIndex;
            isPracticeMode = practice;
            if (levelIndex == 0) ResetScorecard();
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            SceneManager.LoadScene("Gameplay");
        }
    }
}
