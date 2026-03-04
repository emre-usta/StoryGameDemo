using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StoryGame.Core;

namespace StoryGame.UI
{
    public class SettingsManager : MonoBehaviour
    {
        [Header("UI Elemanlarý")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private TextMeshProUGUI musicValueText;
        [SerializeField] private TextMeshProUGUI sfxValueText;
        [SerializeField] private Button backButton;

        private IAudioService _audioService;

        private const string MUSIC_VOLUME_KEY = "MusicVolume";
        private const string SFX_VOLUME_KEY = "SFXVolume";

        private void Start()
        {
            _audioService = ServiceLocator.Get<IAudioService>();

            float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
            float sfxVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

            if (musicSlider != null)
            {
                musicSlider.value = musicVol;
                musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = sfxVol;
                sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            UpdateVolumeTexts(musicVol, sfxVol);

            _audioService?.SetMusicVolume(musicVol);
            _audioService?.SetSFXVolume(sfxVol);

            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
        }

        private void OnMusicVolumeChanged(float value)
        {
            _audioService?.SetMusicVolume(value);
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
            if (musicValueText != null)
                musicValueText.text = Mathf.RoundToInt(value * 100) + "%";
        }

        private void OnSFXVolumeChanged(float value)
        {
            _audioService?.SetSFXVolume(value);
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
            if (sfxValueText != null)
                sfxValueText.text = Mathf.RoundToInt(value * 100) + "%";
        }

        private void UpdateVolumeTexts(float music, float sfx)
        {
            if (musicValueText != null)
                musicValueText.text = Mathf.RoundToInt(music * 100) + "%";
            if (sfxValueText != null)
                sfxValueText.text = Mathf.RoundToInt(sfx * 100) + "%";
        }

        private void OnBackClicked()
        {
            _audioService?.PlaySFX("button_click");
            SceneTransition.LoadScene("MainMenu");
        }
    }
}