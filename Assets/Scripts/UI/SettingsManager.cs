using UnityEngine;
using UnityEngine.UI;

namespace AntiGravityGolf
{
    /// <summary>
    /// Manages audio volume and control sensitivity settings, saving them to PlayerPrefs.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        [Header("UI Sliders (Optional)")]
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider mouseSensSlider;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            LoadSettings();
        }

        private void Start()
        {
            if (sfxSlider != null)
            {
                sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
                sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            }

            if (musicSlider != null)
            {
                musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.65f);
                musicSlider.onValueChanged.AddListener(SetMusicVolume);
            }

            if (mouseSensSlider != null)
            {
                mouseSensSlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 2.5f);
                mouseSensSlider.onValueChanged.AddListener(SetMouseSensitivity);
            }
        }

        public void LoadSettings()
        {
            float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
            float music = PlayerPrefs.GetFloat("MusicVolume", 0.65f);

            if (ProceduralSoundManager.Instance != null)
            {
                ProceduralSoundManager.Instance.SetSFXVolume(sfx);
                ProceduralSoundManager.Instance.SetMusicVolume(music);
            }
        }

        public void SetSFXVolume(float val)
        {
            if (ProceduralSoundManager.Instance != null)
            {
                ProceduralSoundManager.Instance.SetSFXVolume(val);
            }
        }

        public void SetMusicVolume(float val)
        {
            if (ProceduralSoundManager.Instance != null)
            {
                ProceduralSoundManager.Instance.SetMusicVolume(val);
            }
        }

        public void SetMouseSensitivity(float val)
        {
            PlayerPrefs.SetFloat("MouseSensitivity", val);
        }
    }
}
