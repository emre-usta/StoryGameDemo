using DG.Tweening;
using StoryGame.Core;
using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StoryGame.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("UI Elemanları")]
        [SerializeField] private TextMeshProUGUI diamondText;
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;

        [SerializeField] private GameObject dailyRewardPopup;
        [SerializeField] private TextMeshProUGUI popupText;

        private IDiamondService _diamondService;

        private void Start()
        {
            _diamondService = ServiceLocator.Get<IDiamondService>();

            var audio = ServiceLocator.Get<IAudioService>();
            audio?.PlayMusic("mainmenu");

            var dailyReward = ServiceLocator.Get<DailyRewardService>();
            if (dailyReward != null && dailyReward.IsDailyRewardAvailable())
            {
                int amount = dailyReward.ClaimDailyReward();
                if (amount > 0)
                {
                    Debug.Log($"[MainMenu] Günlük ödül alındı: {amount} elmas!");
                    UpdateDiamondUI();
                    ShowDailyRewardPopup(amount);
                }
            }

            UpdateDiamondUI();
            SetupButtons();
        }

        private void UpdateDiamondUI()
        {
            if (diamondText != null)
                diamondText.text = $"{_diamondService.GetAmount()}";
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
            Debug.Log("[MainMenuManager] Karakter seçim ekranına geçiliyor...");
            SceneTransition.LoadScene("CharacterSelect");
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[MainMenuManager] Ayarlar açılıyor...");
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

        private void ShowDailyRewardPopup(int amount)
        {
            if (dailyRewardPopup == null) return;
            if (popupText != null)
                popupText.text = $"Günlük Ödül\n+{amount} 💎";
            dailyRewardPopup.transform.localScale = Vector3.zero;
            dailyRewardPopup.SetActive(true);
            StartCoroutine(PopupRoutine());
        }

        private IEnumerator PopupRoutine()
        {
            dailyRewardPopup.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(2.3f);
            dailyRewardPopup.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack);
            yield return new WaitForSeconds(0.3f);
            dailyRewardPopup.SetActive(false);
        }
    }
}