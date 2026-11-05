using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AntiGravityGolf
{
    /// <summary>
    /// Coordinates the in-game HUD, shot power meter, 5-course scorecard,
    /// water penalty notices, course overview toggle, hole completion modals,
    /// and final tournament completion screen.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance { get; private set; }

        [Header("HUD Elements")]
        [SerializeField] private Text levelNameText;
        [SerializeField] private Text strokeText;
        [SerializeField] private Text parText;
        [SerializeField] private Text overallScoreText;
        [SerializeField] private Image powerBarFill;
        [SerializeField] private GameObject powerBarContainer;
        [SerializeField] private Text contextualHintText;
        [SerializeField] private Text penaltyBannerText;

        [Header("HUD Action Buttons")]
        [SerializeField] private Button resetHoleBtn;
        [SerializeField] private Button mapOverviewBtn;
        [SerializeField] private Button scorecardToggleBtn;
        [SerializeField] private Button pauseBtn;
        [SerializeField] private Button musicMuteBtn;
        [SerializeField] private Text musicMuteText;

        [Header("Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject levelCompletePanel;
        [SerializeField] private GameObject scorecardPanel;
        [SerializeField] private GameObject courseCompletePanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Level Complete Modal")]
        [SerializeField] private Text scoreRatingText;
        [SerializeField] private Text completeStrokesText;
        [SerializeField] private Text completeParText;
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button completeMenuButton;

        [Header("Course Complete Modal (After Course 5)")]
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text finalSummaryText;
        [SerializeField] private Button replayCourseBtn;
        [SerializeField] private Button returnMenuBtn;

        [Header("Scorecard Table Text (5 Courses)")]
        [SerializeField] private Text scorecardOutSummaryText;
        [SerializeField] private Text scorecardInSummaryText;
        [SerializeField] private Text scorecardTotalText;
        [SerializeField] private Button closeScorecardBtn;

        [Header("Pause Modal Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button pauseRestartButton;
        [SerializeField] private Button pauseRestartCourseButton;
        [SerializeField] private Button pauseSettingsButton;
        [SerializeField] private Button pauseMenuButton;

        private GolfBallController ball;
        private Coroutine penaltyRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            EnforceHUDLayout();
        }

        private void Start()
        {
            EnforceHUDLayout();
            ball = FindObjectOfType<GolfBallController>();

            if (resumeButton != null) resumeButton.onClick.AddListener(() => OnResumeClicked());
            if (pauseRestartButton != null) pauseRestartButton.onClick.AddListener(() => OnRestartClicked());
            if (pauseRestartCourseButton != null) pauseRestartCourseButton.onClick.AddListener(() => OnRestartCourseClicked());
            if (pauseSettingsButton != null) pauseSettingsButton.onClick.AddListener(() => ToggleSettingsPanel(true));
            if (pauseMenuButton != null) pauseMenuButton.onClick.AddListener(() => OnMenuClicked());

            if (nextLevelButton != null) nextLevelButton.onClick.AddListener(() => OnNextLevelClicked());
            if (retryButton != null) retryButton.onClick.AddListener(() => OnRestartClicked());
            if (completeMenuButton != null) completeMenuButton.onClick.AddListener(() => OnMenuClicked());

            if (replayCourseBtn != null) replayCourseBtn.onClick.AddListener(() => OnRestartCourseClicked());
            if (returnMenuBtn != null) returnMenuBtn.onClick.AddListener(() => OnMenuClicked());

            if (resetHoleBtn != null) resetHoleBtn.onClick.AddListener(() => OnRestartClicked());
            if (mapOverviewBtn != null) mapOverviewBtn.onClick.AddListener(() => OnMapOverviewClicked());
            if (scorecardToggleBtn != null) scorecardToggleBtn.onClick.AddListener(() => ToggleScorecardPanel());
            if (closeScorecardBtn != null) closeScorecardBtn.onClick.AddListener(() => ToggleScorecardPanel(false));
            if (pauseBtn != null) pauseBtn.onClick.AddListener(() => GameManager.Instance?.TogglePause(true));
            if (musicMuteBtn != null)
            {
                musicMuteBtn.onClick.AddListener(() => OnMusicMuteClicked());
                bool isMuted = ProceduralSoundManager.Instance != null && ProceduralSoundManager.Instance.IsMusicMuted;
                UpdateMusicMuteUI(isMuted);
            }

            if (pausePanel != null) pausePanel.SetActive(false);
            if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
            if (scorecardPanel != null) scorecardPanel.SetActive(false);
            if (courseCompletePanel != null) courseCompletePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (penaltyBannerText != null) penaltyBannerText.gameObject.SetActive(false);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStrokesChanged += UpdateStrokeCount;
                GameManager.Instance.OnLevelLoaded += OnLevelLoaded;
                GameManager.Instance.OnHoleCompleted += ShowLevelCompleteModal;
                GameManager.Instance.OnCourseCompleted += ShowCourseCompleteModal;
                GameManager.Instance.OnPenaltyApplied += ShowPenaltyBanner;
                GameManager.Instance.OnPauseToggled += OnPauseStateChanged;

                UpdateStrokeCount(GameManager.Instance.CurrentStrokes);
                if (GameManager.Instance.CurrentLevelData != null)
                {
                    OnLevelLoaded(GameManager.Instance.CurrentLevelIndex, GameManager.Instance.CurrentLevelData);
                }
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStrokesChanged -= UpdateStrokeCount;
                GameManager.Instance.OnLevelLoaded -= OnLevelLoaded;
                GameManager.Instance.OnHoleCompleted -= ShowLevelCompleteModal;
                GameManager.Instance.OnCourseCompleted -= ShowCourseCompleteModal;
                GameManager.Instance.OnPenaltyApplied -= ShowPenaltyBanner;
                GameManager.Instance.OnPauseToggled -= OnPauseStateChanged;
            }
        }

        private void Update()
        {
            UpdatePowerGauge();

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleScorecardPanel();
            }
        }

        private void UpdatePowerGauge()
        {
            if (ball == null) ball = FindObjectOfType<GolfBallController>();
            if (ball == null) return;

            if (ball.IsAiming && powerBarFill != null)
            {
                if (powerBarContainer != null) powerBarContainer.SetActive(true);
                powerBarFill.fillAmount = ball.CurrentPowerRatio;

                float p = ball.CurrentPowerRatio;
                powerBarFill.color = p < 0.5f 
                    ? Color.Lerp(Color.cyan, Color.yellow, p * 2f) 
                    : Color.Lerp(Color.yellow, new Color(1f, 0.2f, 0.4f), (p - 0.5f) * 2f);
            }
            else
            {
                if (powerBarContainer != null) powerBarContainer.SetActive(false);
            }
        }

        public void UpdateStrokeCount(int strokes)
        {
            if (strokeText != null)
            {
                strokeText.text = $"STROKES: {strokes}";
            }

            if (overallScoreText != null && GameManager.Instance != null)
            {
                int diff = GameManager.Instance.ScoreRelativeToPar;
                string diffStr = diff == 0 ? "E (PAR)" : (diff > 0 ? $"+{diff}" : $"{diff}");
                overallScoreText.text = $"SCORE: {diffStr} ({GameManager.Instance.CompletedStrokesTotal} Total)";
            }
        }

        public void OnLevelLoaded(int index, LevelData data)
        {
            if (levelNameText != null)
            {
                levelNameText.text = index >= 0 ? data.levelName.ToUpper() : "PRACTICE GREEN";
            }
            if (parText != null)
            {
                parText.text = $"PAR: {data.par}";
            }
            if (contextualHintText != null)
            {
                contextualHintText.text = data.description;
            }

            if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
            if (courseCompletePanel != null) courseCompletePanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);

            UpdateStrokeCount(0);
        }

        public void OnMusicMuteClicked()
        {
            if (ProceduralSoundManager.Instance != null)
            {
                bool isMuted = ProceduralSoundManager.Instance.ToggleMusicMute();
                UpdateMusicMuteUI(isMuted);
                ProceduralSoundManager.Instance.PlayUIClick();
            }
        }

        public void UpdateMusicMuteUI(bool isMuted)
        {
            if (musicMuteText != null)
            {
                musicMuteText.text = isMuted ? "🔇 UNMUTE" : "🔊 MUSIC";
            }
        }

        public void ShowPenaltyBanner(string message)
        {
            if (penaltyBannerText == null) return;
            if (penaltyRoutine != null) StopCoroutine(penaltyRoutine);
            penaltyRoutine = StartCoroutine(PenaltyBannerFade(message));
        }

        private IEnumerator PenaltyBannerFade(string message)
        {
            penaltyBannerText.text = message;
            penaltyBannerText.gameObject.SetActive(true);
            yield return new WaitForSeconds(2.2f);
            penaltyBannerText.gameObject.SetActive(false);
        }

        public void ShowLevelCompleteModal(string rating, int strokes, int par)
        {
            if (levelCompletePanel == null) return;
            levelCompletePanel.SetActive(true);

            if (scoreRatingText != null)
            {
                scoreRatingText.text = rating;
                scoreRatingText.color = strokes <= par ? new Color(0.2f, 1f, 0.4f) : new Color(1f, 0.8f, 0.3f);
            }
            if (completeStrokesText != null) completeStrokesText.text = $"Strokes: {strokes}";
            if (completeParText != null) completeParText.text = $"Par: {par}";

            int currentLevel = GameManager.Instance != null ? GameManager.Instance.CurrentLevelIndex : 0;
            int best = PlayerPrefs.GetInt($"BestScore_Course_{currentLevel}", strokes);
            if (bestScoreText != null) bestScoreText.text = $"Course Record: {best} strokes";
        }

        public void ShowCourseCompleteModal(int totalStrokes, int totalPar)
        {
            if (courseCompletePanel == null) return;
            courseCompletePanel.SetActive(true);

            int diff = totalStrokes - totalPar;
            string rel = diff == 0 ? "EVEN (E)" : (diff > 0 ? $"+{diff}" : $"{diff}");

            if (finalScoreText != null)
            {
                finalScoreText.text = $"FINAL SCORE: {totalStrokes} ({rel})";
            }
            if (finalSummaryText != null)
            {
                finalSummaryText.text = $"Congratulations! You completed all 5 championship mini-golf courses!\nRegulation Par: {totalPar} | Final Strokes: {totalStrokes}";
            }
        }

        public void ToggleScorecardPanel(bool? explicitState = null)
        {
            if (scorecardPanel == null) return;
            bool newState = explicitState.HasValue ? explicitState.Value : !scorecardPanel.activeSelf;
            scorecardPanel.SetActive(newState);

            if (newState)
            {
                PopulateScorecard();
            }

            if (ProceduralSoundManager.Instance != null)
                ProceduralSoundManager.Instance.PlayUIClick();
        }

        private void PopulateScorecard()
        {
            if (GameManager.Instance == null) return;

            var gm = GameManager.Instance;
            var levels = gm.AllLevels;
            var scores = gm.StrokesPerHole;
            var done = gm.HoleCompleted;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("CHAMPIONSHIP COURSES:");
            sb.AppendLine("Course                       Par   Strokes   Score");
            sb.AppendLine("--------------------------------------------------");

            int totalPar = 0;
            int totalStrokes = 0;

            for (int i = 0; i < levels.Length && i < scores.Length; i++)
            {
                totalPar += levels[i].par;
                string s = done[i] ? scores[i].ToString() : "-";
                string net = done[i] 
                    ? (scores[i] - levels[i].par == 0 ? "E" : (scores[i] - levels[i].par > 0 ? $"+{scores[i] - levels[i].par}" : $"{scores[i] - levels[i].par}")) 
                    : "-";

                if (done[i]) totalStrokes += scores[i];
                string cName = levels[i].levelName;
                if (cName.Length > 26) cName = cName.Substring(0, 26);
                sb.AppendLine($"{cName,-28} {levels[i].par,-5} {s,-9} {net}");
            }

            if (scorecardOutSummaryText != null) scorecardOutSummaryText.text = sb.ToString();
            if (scorecardInSummaryText != null) scorecardInSummaryText.text = "Tree-Free Open Course View\nScenic Ponds, Waterfalls & Bridges\nEnjoy Championship Mini Golf!";

            int totalDiff = totalStrokes - totalPar;
            string totalDiffStr = totalDiff == 0 ? "E" : (totalDiff > 0 ? $"+{totalDiff}" : $"{totalDiff}");
            if (scorecardTotalText != null)
            {
                scorecardTotalText.text = $"TOTAL PAR: {totalPar} | TOTAL STROKES: {totalStrokes} ({totalDiffStr})";
            }
        }

        private void OnPauseStateChanged(bool paused)
        {
            if (pausePanel != null) pausePanel.SetActive(paused);
            if (!paused && settingsPanel != null) settingsPanel.SetActive(false);
        }

        public void OnMapOverviewClicked()
        {
            var camCtrl = FindObjectOfType<GolfCameraController>();
            if (camCtrl != null) camCtrl.ToggleMapOverview();
        }

        public void OnResumeClicked()
        {
            if (ProceduralSoundManager.Instance != null) ProceduralSoundManager.Instance.PlayUIClick();
            if (GameManager.Instance != null) GameManager.Instance.TogglePause(false);
        }

        public void OnRestartClicked()
        {
            if (ProceduralSoundManager.Instance != null) ProceduralSoundManager.Instance.PlayUIClick();
            if (GameManager.Instance != null) GameManager.Instance.RestartCurrentLevel();
        }

        public void OnRestartCourseClicked()
        {
            if (ProceduralSoundManager.Instance != null) ProceduralSoundManager.Instance.PlayUIClick();
            if (GameManager.Instance != null) GameManager.Instance.RestartFromHole1();
        }

        public void OnNextLevelClicked()
        {
            if (ProceduralSoundManager.Instance != null) ProceduralSoundManager.Instance.PlayUIClick();
            if (GameManager.Instance != null) GameManager.Instance.NextLevel();
        }

        public void OnMenuClicked()
        {
            if (ProceduralSoundManager.Instance != null) ProceduralSoundManager.Instance.PlayUIClick();
            if (GameManager.Instance != null) GameManager.Instance.LoadMainMenu();
        }

        public void ToggleSettingsPanel(bool open)
        {
            if (ProceduralSoundManager.Instance != null) ProceduralSoundManager.Instance.PlayUIClick();
            if (settingsPanel != null) settingsPanel.SetActive(open);
        }

        public void EnforceHUDLayout()
        {
            // 1. Position and pin TopBar cleanly to top edge of screen
            Transform topBarTransform = levelNameText != null ? levelNameText.transform.parent : transform.Find("TopBar");
            if (topBarTransform != null)
            {
                RectTransform rt = topBarTransform.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0f, 1f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(0.5f, 1f);
                    rt.anchoredPosition = Vector2.zero;
                    rt.sizeDelta = new Vector2(0f, 75f);
                }

                Image bg = topBarTransform.GetComponent<Image>();
                if (bg != null) bg.raycastTarget = false;
            }

            // 2. Format labels inside TopBar
            if (levelNameText != null)
            {
                RectTransform rt = levelNameText.rectTransform;
                rt.anchorMin = new Vector2(0.012f, 0f);
                rt.anchorMax = new Vector2(0.26f, 1f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                levelNameText.alignment = TextAnchor.MiddleLeft;
                levelNameText.fontSize = 20;
                levelNameText.raycastTarget = false;
            }

            if (strokeText != null)
            {
                RectTransform rt = strokeText.rectTransform;
                rt.anchorMin = new Vector2(0.265f, 0.40f);
                rt.anchorMax = new Vector2(0.39f, 0.95f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                strokeText.alignment = TextAnchor.MiddleCenter;
                strokeText.fontSize = 22;
                strokeText.raycastTarget = false;
            }

            if (parText != null)
            {
                RectTransform rt = parText.rectTransform;
                rt.anchorMin = new Vector2(0.265f, 0.05f);
                rt.anchorMax = new Vector2(0.39f, 0.42f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                parText.alignment = TextAnchor.MiddleCenter;
                parText.fontSize = 16;
                parText.raycastTarget = false;
            }

            if (overallScoreText != null)
            {
                RectTransform rt = overallScoreText.rectTransform;
                rt.anchorMin = new Vector2(0.395f, 0f);
                rt.anchorMax = new Vector2(0.575f, 1f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                overallScoreText.alignment = TextAnchor.MiddleCenter;
                overallScoreText.fontSize = 20;
                overallScoreText.raycastTarget = false;
            }

            // 3. Format action buttons inside TopBar with generous padding
            if (musicMuteBtn != null)
            {
                RectTransform rt = musicMuteBtn.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.585f, 0.15f);
                rt.anchorMax = new Vector2(0.665f, 0.85f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            if (mapOverviewBtn != null)
            {
                RectTransform rt = mapOverviewBtn.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.675f, 0.15f);
                rt.anchorMax = new Vector2(0.755f, 0.85f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            if (scorecardToggleBtn != null)
            {
                RectTransform rt = scorecardToggleBtn.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.765f, 0.15f);
                rt.anchorMax = new Vector2(0.845f, 0.85f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            if (resetHoleBtn != null)
            {
                RectTransform rt = resetHoleBtn.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.855f, 0.15f);
                rt.anchorMax = new Vector2(0.93f, 0.85f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            if (pauseBtn != null)
            {
                RectTransform rt = pauseBtn.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.94f, 0.15f);
                rt.anchorMax = new Vector2(0.988f, 0.85f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            if (contextualHintText != null) contextualHintText.raycastTarget = false;
            if (penaltyBannerText != null) penaltyBannerText.raycastTarget = false;
        }
    }
}
