using DG.Tweening;
using StoryGame.Characters;
using StoryGame.Core;
using StoryGame.Dialogue;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StoryGame.UI
{
    public class GameplayManager : MonoBehaviour
    {
        [Header("Diyalog UI")]
        [SerializeField] private GameObject narrationPanel;
        [SerializeField] private TextMeshProUGUI narrationText;
        [SerializeField] private TypewriterEffect typewriter;

        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;

        [SerializeField] private GameObject choicePanel;
        [SerializeField] private Button[] choiceButtons;

        [SerializeField] private Color normalButtonColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        [SerializeField] private Color diamondButtonColor = new Color(0.6f, 0.4f, 0.0f, 0.9f);
        [SerializeField] private Color diamondTextColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private Color normalTextColor = Color.white;

        [Header("Servisler")]
        [SerializeField] private BackgroundService backgroundService;

        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI diamondText;
        [SerializeField] private AffectionBar affectionBar;

        [Header("Diyalog Verisi")]
        [SerializeField] private DialogueData dialogueData;

        [Header("Oyuncu")]
        [SerializeField] private Image playerCharacterImage;
        [SerializeField] private Sprite[] playerSprites;

        [Header("Pause Menü")]
        [SerializeField] private Button menuButton;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;

        private DialogueEngine _dialogueEngine;
        private CharacterState _characterState;
        private IDiamondService _diamondService;
        private string _currentNodeText;
        private string _currentBackgroundId = "";

        private void Start()
        {
            var audio = ServiceLocator.Get<IAudioService>();
            audio?.PlayMusic("gameplay");
            _diamondService = ServiceLocator.Get<IDiamondService>();
            UpdateDiamondUI();
            UpdateAffectionBar();

            if (menuButton != null)
                menuButton.onClick.AddListener(OpenPauseMenu);
            if (resumeButton != null)
                resumeButton.onClick.AddListener(ClosePauseMenu);
            if (settingsButton != null)
                settingsButton.onClick.AddListener(() => {
                    ServiceLocator.Get<IAudioService>()?.PlaySFX("button_click");
                    Time.timeScale = 1f;
                    pauseMenuPanel.SetActive(false);
                    PlayerPrefs.SetString("PreviousScene", "Gameplay");
                    SceneTransition.LoadScene("Settings");
                });
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(() => {
                    ServiceLocator.Get<IAudioService>()?.PlaySFX("button_click");
                    Time.timeScale = 1f;
                    pauseMenuPanel.SetActive(false);
                    SceneTransition.LoadScene("MainMenu");
                });

            int charIndex = PlayerPrefs.GetInt("PlayerCharIndex", 0);
            if (playerCharacterImage != null && playerSprites.Length > charIndex)
                playerCharacterImage.sprite = playerSprites[charIndex];

            string characterId = PlayerPrefs.GetString("SelectedCharacter", "jenniffer");

            _characterState = new CharacterState();
            _characterState.characterId = characterId;

            _dialogueEngine = gameObject.AddComponent<DialogueEngine>();
            _dialogueEngine.OnNarrationNode += ShowNarration;
            _dialogueEngine.OnDialogueNode += ShowDialogue;
            _dialogueEngine.OnChoiceNode += ShowChoices;
            _dialogueEngine.OnEpisodeEnded += OnEpisodeEnded;

            HideAllPanels();

            bool continueGame = PlayerPrefs.GetString("ContinueGame", "false") == "true";
            var saveService = ServiceLocator.Get<ISaveService>();

            if (continueGame && saveService != null)
            {
                string savedNodeId = saveService.GetSavedNodeId(characterId);
                if (!string.IsNullOrEmpty(savedNodeId))
                {
                    var progress = saveService.LoadProgress(characterId);
                    _characterState.affectionPoints = progress.affection;
                    foreach (var flag in progress.flags)
                        _characterState.SetFlag(flag, 0);

                    // Arka planı yükle
                    string savedBackgroundId = saveService.GetSavedBackgroundId(characterId);
                    if (!string.IsNullOrEmpty(savedBackgroundId))
                    {
                        _currentBackgroundId = savedBackgroundId;
                        backgroundService?.ChangeBackground(savedBackgroundId);
                    }

                    _dialogueEngine.StartEpisodeFromNode(dialogueData, _characterState, savedNodeId);
                    Debug.Log($"[GameplayManager] Kaldığı yerden devam: {savedNodeId}");
                    return;
                }
            }

            if (dialogueData != null)
                _dialogueEngine.StartEpisode(dialogueData, _characterState);
            else
                Debug.LogWarning("[GameplayManager] DialogueData atanmadı!");
        }

        private void Update()
        {
            if (pauseMenuPanel != null && pauseMenuPanel.activeSelf) return;

            if (Input.GetMouseButtonDown(0))
            {
                if (typewriter != null && typewriter.IsTyping)
                {
                    if (narrationPanel.activeSelf)
                        typewriter.Skip(narrationText, _currentNodeText);
                    else if (dialoguePanel.activeSelf)
                        typewriter.Skip(dialogueText, _currentNodeText);
                }
                else if (narrationPanel != null && narrationPanel.activeSelf ||
                         dialoguePanel != null && dialoguePanel.activeSelf)
                {
                    ServiceLocator.Get<IAudioService>()?.PlaySFX("dialogue_advance");
                    _dialogueEngine.Advance();
                }
            }
        }

        private string ProcessText(string text)
        {
            string playerName = PlayerPrefs.GetString("PlayerName", "Alex");
            return text.Replace("{playerName}", playerName);
        }

        private void SaveCurrentProgress(string nodeId)
        {
            var saveService = ServiceLocator.Get<ISaveService>();
            if (saveService != null)
            {
                var flags = new List<string>();
                if (_characterState.trustEstablished) flags.Add("trustEstablished");
                if (_characterState.secretDiscovered) flags.Add("secretDiscovered");
                if (_characterState.recklessPath) flags.Add("recklessPath");
                saveService.SaveProgress(
                    _characterState.characterId,
                    nodeId,
                    _characterState.affectionPoints,
                    flags,
                    _currentBackgroundId
                );
            }
        }

        private void ShowNarration(DialogueNode node)
        {
            HideAllPanels();
            narrationPanel.SetActive(true);
            float originalYN = narrationPanel.transform.localPosition.y;
            narrationPanel.transform.localPosition = new Vector3(0, originalYN - 100f, 0);
            narrationPanel.transform.DOLocalMoveY(originalYN, 0.6f).SetEase(Ease.OutCubic);
            _currentNodeText = ProcessText(node.text);
            typewriter.Play(narrationText, _currentNodeText);
            UpdateAffectionBar();
            if (!string.IsNullOrEmpty(node.backgroundId))
            {
                _currentBackgroundId = node.backgroundId;
                backgroundService?.ChangeBackground(node.backgroundId);
            }
            if (playerCharacterImage != null)
                playerCharacterImage.gameObject.SetActive(false);
            SaveCurrentProgress(node.id);
        }

        private void ShowDialogue(DialogueNode node)
        {
            HideAllPanels();
            dialoguePanel.SetActive(true);
            float originalYD = dialoguePanel.transform.localPosition.y;
            dialoguePanel.transform.localPosition = new Vector3(0, originalYD - 100f, 0);
            dialoguePanel.transform.DOLocalMoveY(originalYD, 0.6f).SetEase(Ease.OutCubic);
            speakerNameText.text = node.speaker;
            _currentNodeText = ProcessText(node.text);
            typewriter.Play(dialogueText, _currentNodeText);
            UpdateAffectionBar();
            if (!string.IsNullOrEmpty(node.backgroundId))
            {
                _currentBackgroundId = node.backgroundId;
                backgroundService?.ChangeBackground(node.backgroundId);
            }
            bool isPlayerSpeaking = node.speaker == "Player" ||
                                     node.speaker == PlayerPrefs.GetString("PlayerName", "Alex");
            if (playerCharacterImage != null)
                playerCharacterImage.gameObject.SetActive(isPlayerSpeaking);
            SaveCurrentProgress(node.id);
        }

        private void ShowChoices(DialogueNode node)
        {
            HideAllPanels();
            choicePanel.SetActive(true);
            if (playerCharacterImage != null)
                playerCharacterImage.gameObject.SetActive(true);
            SaveCurrentProgress(node.id);
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (i < node.choices.Count)
                {
                    choiceButtons[i].gameObject.SetActive(true);
                    var choice = node.choices[i];
                    var buttonText = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    var buttonImage = choiceButtons[i].GetComponent<Image>();
                    if (choice.isDiamond)
                    {
                        if (buttonText != null)
                        {
                            buttonText.text = $"{choice.text} ({choice.diamondCost})";
                            buttonText.color = diamondTextColor;
                        }
                        if (buttonImage != null)
                            buttonImage.color = diamondButtonColor;
                    }
                    else
                    {
                        if (buttonText != null)
                        {
                            buttonText.text = choice.text;
                            buttonText.color = normalTextColor;
                        }
                        if (buttonImage != null)
                            buttonImage.color = normalButtonColor;
                    }
                    int index = i;
                    choiceButtons[i].onClick.RemoveAllListeners();
                    choiceButtons[i].onClick.AddListener(() => {
                        if (pauseMenuPanel != null && pauseMenuPanel.activeSelf) return;
                        _dialogueEngine.SelectChoice(index);
                        UpdateDiamondUI();
                    });
                    var canvasGroup = choiceButtons[i].GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                        canvasGroup = choiceButtons[i].gameObject.AddComponent<CanvasGroup>();
                    canvasGroup.alpha = 0f;
                    canvasGroup.DOFade(1f, 0.2f).SetDelay(i * 0.1f);
                }
                else
                {
                    choiceButtons[i].gameObject.SetActive(false);
                }
            }
            UpdateAffectionBar();
        }

        private void OnEpisodeEnded(EndingType ending)
        {
            Debug.Log($"[GameplayManager] Bölüm bitti! Ending: {ending}");
            ServiceLocator.Get<IAudioService>()?.PlaySFX("episode_complete");
            PlayerPrefs.SetString("LastEnding", ending.ToString());
            PlayerPrefs.SetInt("LastAffection", _characterState.affectionPoints);
            HideAllPanels();
            SceneTransition.LoadScene("EndingScreen");
        }

        private void HideAllPanels()
        {
            if (narrationPanel != null) narrationPanel.SetActive(false);
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (choicePanel != null) choicePanel.SetActive(false);
        }

        private void UpdateDiamondUI()
        {
            if (diamondText != null)
            {
                int targetAmount = (int)_diamondService.GetAmount();
                diamondText.transform.DOKill();
                diamondText.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 0.5f);
                diamondText.text = $"{targetAmount}";
            }
        }

        private void OnDestroy()
        {
            DOTween.KillAll();
            if (_dialogueEngine != null)
            {
                _dialogueEngine.OnNarrationNode -= ShowNarration;
                _dialogueEngine.OnDialogueNode -= ShowDialogue;
                _dialogueEngine.OnChoiceNode -= ShowChoices;
                _dialogueEngine.OnEpisodeEnded -= OnEpisodeEnded;
            }
        }

        private void UpdateAffectionBar()
        {
            if (affectionBar != null)
                affectionBar.UpdateBar(_characterState);
        }

        private void OpenPauseMenu()
        {
            ServiceLocator.Get<IAudioService>()?.PlaySFX("button_click");
            Time.timeScale = 0f;
            pauseMenuPanel.SetActive(true);
            pauseMenuPanel.transform.localScale = Vector3.zero;
            pauseMenuPanel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        private void ClosePauseMenu()
        {
            ServiceLocator.Get<IAudioService>()?.PlaySFX("button_click");
            pauseMenuPanel.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack).SetUpdate(true)
                .OnComplete(() => {
                    pauseMenuPanel.SetActive(false);
                    Time.timeScale = 1f;
                });
        }
    }
}