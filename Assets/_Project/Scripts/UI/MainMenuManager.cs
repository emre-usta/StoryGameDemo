using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using StoryGame.Core;
using TMPro;

namespace StoryGame.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("UI Elemanlarý")]
        [SerializeField] private TextMeshProUGUI diamondText;
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;

        private IDiamondService _diamondService;

        private void Start()
        {
            var audio = ServiceLocator.Get<IAudioService>();
            audio?.PlayMusic("mainmenu");

            _diamondService = ServiceLocator.Get<IDiamondService>();
            UpdateDiamondUI();
            SetupButtons();
        }

        private void UpdateDiamondUI()
        {
            if (diamondText != null)
                diamondText.text = _diamondService.GetAmount().ToString();
        }

        private void SetupButtons()
        {
            if (playButton != null)
                playButton.onClick.AddListener(() =>
                {
                    ServiceLocator.Get<IAudioService>()?.PlaySFX("button_click");
                    OnPlayClicked();
                });

            if (settingsButton != null)
                settingsButton.onClick.AddListener(() =>
                {
                    ServiceLocator.Get<IAudioService>()?.PlaySFX("button_click");
                    OnSettingsClicked();
                });
        }

        private void OnPlayClicked()
        {
            Debug.Log("[MainMenuManager] Karakter seçim ekranýna geçiliyor...");
            SceneTransition.LoadScene("CharacterSelect");
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[MainMenuManager] Ayarlar açýlýyor...");
            ServiceLocator.Get<IAudioService>()?.PlaySFX("button_click");
            SceneTransition.LoadScene("Settings");
        }

        private void OnDestroy()
        {
            if (playButton != null)
                playButton.onClick.RemoveAllListeners();
            if (settingsButton != null)
                settingsButton.onClick.RemoveAllListeners();
        }
    }
}