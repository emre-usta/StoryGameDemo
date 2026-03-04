using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using StoryGame.Core;
using StoryGame.Characters;
using TMPro;

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

        private int _currentIndex = 0;
        private IDiamondService _diamondService;
        private ISaveService _saveService;

        private void Start()
        {
            _diamondService = ServiceLocator.Get<IDiamondService>();
            _saveService = ServiceLocator.Get<ISaveService>();

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

            // Kilit kontrolü
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
                    _saveService.UnlockCharacter(character.characterId);
                    Debug.Log($"[CharacterSelect] {character.characterName} açıldı!");
                    UpdateUI();
                }
                return;
            }

            Debug.Log($"[CharacterSelect] {character.characterName} seçildi, oyun başlıyor...");
            PlayerPrefs.SetString("SelectedCharacter", character.characterId);
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