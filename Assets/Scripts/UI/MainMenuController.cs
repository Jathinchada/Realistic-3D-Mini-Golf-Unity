using UnityEngine;
using UnityEngine.UI;

namespace AntiGravityGolf
{
    /// <summary>
    /// Controls Main Menu interactions: Play 18 Holes, Select Hole (1-18), Practice Green,
    /// How to Play, Settings, and Quit.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Main Navigation Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private Button practiceGreenButton;
        [SerializeField] private Button howToPlayButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Modals")]
        [SerializeField] private GameObject levelSelectModal;
        [SerializeField] private GameObject howToPlayModal;
        [SerializeField] private GameObject settingsModal;

        [Header("Level Select Container")]
        [SerializeField] private Transform levelButtonsContainer;

        [Header("Close Buttons")]
        [SerializeField] private Button closeLevelSelectBtn;
        [SerializeField] private Button closeHowToPlayBtn;
        [SerializeField] private Button closeSettingsBtn;

        private void Start()
        {
            Time.timeScale = 1f;

            if (playButton != null) playButton.onClick.AddListener(() => OnPlayClicked());
            if (levelSelectButton != null) levelSelectButton.onClick.AddListener(() => OpenModal(levelSelectModal));
            if (practiceGreenButton != null) practiceGreenButton.onClick.AddListener(() => OnPracticeGreenClicked());
            if (howToPlayButton != null) howToPlayButton.onClick.AddListener(() => OpenModal(howToPlayModal));
            if (settingsButton != null) settingsButton.onClick.AddListener(() => OpenModal(settingsModal));
            if (quitButton != null) quitButton.onClick.AddListener(() => OnQuitClicked());

            if (closeLevelSelectBtn != null) closeLevelSelectBtn.onClick.AddListener(() => CloseModal(levelSelectModal));
            if (closeHowToPlayBtn != null) closeHowToPlayBtn.onClick.AddListener(() => CloseModal(howToPlayModal));
            if (closeSettingsBtn != null) closeSettingsBtn.onClick.AddListener(() => CloseModal(settingsModal));

            SetupLevelSelectButtons();

            if (levelSelectModal != null) levelSelectModal.SetActive(false);
            if (howToPlayModal != null) howToPlayModal.SetActive(false);
            if (settingsModal != null) settingsModal.SetActive(false);
        }

        private void SetupLevelSelectButtons()
        {
            if (levelButtonsContainer == null) return;

            Button[] btns = levelButtonsContainer.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < btns.Length; i++)
            {
                int holeIdx = i;
                btns[i].onClick.RemoveAllListeners();
                btns[i].onClick.AddListener(() => LaunchLevel(holeIdx));
            }
        }

        public void OnPlayClicked()
        {
            PlaySound();
            LaunchLevel(0);
        }

        public void OnPracticeGreenClicked()
        {
            PlaySound();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadGameplayScene(0, practice: true);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Gameplay");
            }
        }

        public void LaunchLevel(int levelIndex)
        {
            PlaySound();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadGameplayScene(levelIndex, practice: false);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Gameplay");
            }
        }

        public void OpenModal(GameObject modal)
        {
            PlaySound();
            if (modal != null) modal.SetActive(true);
        }

        public void CloseModal(GameObject modal)
        {
            PlaySound();
            if (modal != null) modal.SetActive(false);
        }

        public void OnQuitClicked()
        {
            PlaySound();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void PlaySound()
        {
            if (ProceduralSoundManager.Instance != null)
            {
                ProceduralSoundManager.Instance.PlayUIClick();
            }
        }
    }
}
