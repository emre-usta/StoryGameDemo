using DG.Tweening;
using StoryGame.Characters;
using StoryGame.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StoryGame.UI
{
    public class CharacterSelectManager : MonoBehaviour
    {
        [Header("Karakter Verileri")]
        [SerializeField] private CharacterData[] characters;

        [Header("UI Elemanları")]
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI characterDescriptionText;
        [SerializeField] private TextMeshProUGUI diamondText;
        [SerializeField] private Image characterPortrait;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Button backButton;

        [SerializeField] private Button profileButton;
        [SerializeField] private Image profileButtonImage;
        [SerializeField] private Sprite[] playerSprites;

        [Header("Continue Popup")]
        [SerializeField] private GameObject continuePopup;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button newGameButton;

        private int _currentIndex = 0;
        private IDiamondService _diamondService;
        private ISaveService _saveService;

        private void Start()
        {
            int charIndex = PlayerPrefs.GetInt("PlayerCharIndex", 0);
            if (profileButtonImage != null && playerSprites.Length > charIndex)
                profileButtonImage.sprite = playerSprites[charIndex];

            if (profileButton != null)
                profileButton.onClick.AddListener(() => {
                    ServiceLocator.Get<IAudioService>()?.PlaySFX("button_click");
                    SceneTransition.LoadScene("PlayerSetup");
                });

            _diamondService = ServiceLocator.Get<IDiamondService>();
            _saveService = ServiceLocator.Get<ISaveService>();

            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);
            if (newGameButton != null)
                newGameButton.onClick.AddListener(OnNewGameClicked);

            SetupButtons();
            UpdateUI();
        }

        private void SetupButtons()
        {
            if (nextButton != null)
                nextButton.onClick.AddListener(() => { ServiceLocator.Get<IAudioService>()?.PlaySFX("button_click"); OnNextClicked(); });
            if (prevButton != null)
                prevButton.onClick.AddListener(() => { ServiceLocator.Get<IAudioService>()?.PlaySFX("button_click"); OnPrevClicked(); });
            if (playButton != null)
                playButton.onClick.AddListener(() => { ServiceLocator.Get<IAudioService>()?.PlaySFX("button_click"); OnPlayClicked(); });
            if (backButton != null)
                backButton.onClick.AddListener(() => { ServiceLocator.Get<IAudioService>()?.PlaySFX("button_click"); OnBackClicked(); });
        }

        private void UpdateUI()
        {
            if (characters == null || characters.Length == 0) return;

            var character = characters[_currentIndex];

            if (characterNameText != null)
                characterNameText.text = character.characterName;

            if (characterDescriptionText != null)
                characterDescriptionText.text = character.description;

            if (characterPortrait != null && character.portrait != null)
                characterPortrait.sprite = character.portrait;

            if (diamondText != null)
                diamondText.text = _diamondService.GetAmount().ToString();

            bool isUnlocked = character.isUnlockedByDefault ||
                              _saveService.IsCharacterUnlocked(character.characterId);

            if (playButton != null)
            {
                var buttonText = playButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.text = isUnlocked ? "Oyna" : $"Aç ({character.unlockDiamondCost} 💎)";
            }
        }

        private void OnNextClicked()
        {
            if (characters == null || characters.Length == 0) return;
            _currentIndex = (_currentIndex + 1) % characters.Length;
            UpdateUI();
        }

        private void OnPrevClicked()
        {
            if (characters == null || characters.Length == 0) return;
            _currentIndex = (_currentIndex - 1 + characters.Length) % characters.Length;
            UpdateUI();
        }

        private void OnPlayClicked()
        {
            if (characters == null || characters.Length == 0) return;

            var character = characters[_currentIndex];
            bool isUnlocked = character.isUnlockedByDefault ||
                              _saveService.IsCharacterUnlocked(character.characterId);

            if (!isUnlocked)
            {
                if (_diamondService.TrySpend(character.unlockDiamondCost))
                {
                    ServiceLocator.Get<IAudioService>()?.PlaySFX("diamond_spend");
                    _saveService.UnlockCharacter(character.characterId);
                    Debug.Log($"[CharacterSelect] {character.characterName} açıldı!");
                    UpdateUI();
                }
                return;
            }

            // Kayıt var mı kontrol et
            string savedNodeId = _saveService.GetSavedNodeId(character.characterId);
            if (!string.IsNullOrEmpty(savedNodeId))
            {
                // Popup göster
                ShowContinuePopup();
            }
            else
            {
                StartNewGame(character.characterId);
            }
        }

        private void ShowContinuePopup()
        {
            if (continuePopup == null) return;
            continuePopup.SetActive(true);
            continuePopup.transform.localScale = Vector3.zero;
            continuePopup.transform.DOScale(1f, 0.3f).SetEase(DG.Tweening.Ease.OutBack);
        }

        private void OnContinueClicked()
        {
            ServiceLocator.Get<IAudioService>()?.PlaySFX("button_click");
            var character = characters[_currentIndex];
            PlayerPrefs.SetString("SelectedCharacter", character.characterId);
            PlayerPrefs.SetString("ContinueGame", "true");
            continuePopup.SetActive(false);
            SceneTransition.LoadScene("Gameplay");
        }

        private void OnNewGameClicked()
        {
            ServiceLocator.Get<IAudioService>()?.PlaySFX("button_click");
            var character = characters[_currentIndex];
            _saveService.DeleteProgress(character.characterId);
            continuePopup.SetActive(false);
            StartNewGame(character.characterId);
        }

        private void StartNewGame(string characterId)
        {
            PlayerPrefs.SetString("SelectedCharacter", characterId);
            PlayerPrefs.SetString("ContinueGame", "false");
            SceneTransition.LoadScene("Gameplay");
        }

        private void OnBackClicked()
        {
            SceneTransition.LoadScene("MainMenu");
        }

        private void OnDestroy()
        {
            if (nextButton != null) nextButton.onClick.RemoveAllListeners();
            if (prevButton != null) prevButton.onClick.RemoveAllListeners();
            if (playButton != null) playButton.onClick.RemoveAllListeners();
            if (backButton != null) backButton.onClick.RemoveAllListeners();
        }
    }
}