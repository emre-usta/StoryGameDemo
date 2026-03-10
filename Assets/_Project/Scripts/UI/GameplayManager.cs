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
        [SerializeField] private CharacterData[] characterDataList;

        [Header("Oyuncu")]
        [SerializeField] private Image playerCharacterImage;
        [SerializeField] private Sprite[] playerSprites;

        [Header("Pause Menü")]
        [SerializeField] private PauseButtonHandler pauseButtonHandler;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Bölüm Geçiş Ekranı")]
        [SerializeField] private GameObject episodeTransitionPanel;
        [SerializeField] private TextMeshProUGUI episodeTitleText;
        [SerializeField] private TextMeshProUGUI episodeCommentText;
        [SerializeField] private Button episodeContinueButton;

        private bool _blockInput = false;
        private DialogueEngine _dialogueEngine;
        private CharacterState _characterState;
        private IDiamondService _diamondService;
        private string _currentNodeText;
        private string _currentBackgroundId = "";
        private int _nextEpisodeIndex;

        private void Start()
        {
            var audio = ServiceLocator.Get<IAudioService>();
            audio?.PlayMusic("gameplay");
            _diamondService = ServiceLocator.Get<IDiamondService>();
            UpdateDiamondUI();
            UpdateAffectionBar();

            if (pauseButtonHandler != null)
                pauseButtonHandler.OnPressed += OpenPauseMenu;
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

            // Doğru CharacterData'yı bul
            CharacterData characterData = null;
            foreach (var cd in characterDataList)
            {
                if (cd.characterId == characterId)
                {
                    characterData = cd;
                    break;
                }
            }

            if (characterData == null)
            {
                Debug.LogError($"[GameplayManager] CharacterData bulunamadı: {characterId}");
                return;
            }

            var saveService = ServiceLocator.Get<ISaveService>();

            // Kaldığı yerden devam
            bool continueGame = PlayerPrefs.GetString("ContinueGame", "false") == "true";
            if (continueGame && saveService != null)
            {
                string savedNodeId = saveService.GetSavedNodeId(characterId);
                if (!string.IsNullOrEmpty(savedNodeId))
                {
                    var progress = saveService.LoadProgress(characterId);
                    _characterState.affectionPoints = progress.affection;
                    foreach (var flag in progress.flags)
                        _characterState.SetFlag(flag, 0);

                    string savedBackgroundId = saveService.GetSavedBackgroundId(characterId);
                    if (!string.IsNullOrEmpty(savedBackgroundId))
                    {
                        _currentBackgroundId = savedBackgroundId;
                        backgroundService?.SetBackgroundImmediate(savedBackgroundId);
                    }

                    // Kaydedilen episode'u bul
                    int savedEpisodeIndex = saveService.GetLastPlayedEpisode(characterId);
                    DialogueData savedDialogueData = null;
                    if (characterData.episodes != null && characterData.episodes.Length > savedEpisodeIndex)
                        savedDialogueData = characterData.episodes[savedEpisodeIndex];

                    if (savedDialogueData != null)
                    {
                        _dialogueEngine.StartEpisodeFromNode(savedDialogueData, _characterState, savedNodeId);
                        Debug.Log($"[GameplayManager] Kaldığı yerden devam: {savedNodeId}, Episode: {savedEpisodeIndex}");
                        return;
                    }
                }
            }

            // Yeni oyun — ilk episode'u başlat
            int episodeIndex = saveService?.GetLastPlayedEpisode(characterId) ?? 0;
            DialogueData dialogueData = null;
            if (characterData.episodes != null && characterData.episodes.Length > episodeIndex)
                dialogueData = characterData.episodes[episodeIndex];

            if (dialogueData != null)
                _dialogueEngine.StartEpisode(dialogueData, _characterState);
            else
            {
                Debug.Log($"[GameplayManager] Tüm bölümler tamamlandı. Episode: {episodeIndex}");
                SceneTransition.LoadScene("EndingScreen");
            }
        }

        private void Update()
        {
            if (_blockInput)
            {
                _blockInput = false;
                return;
            }

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
                // Tüm 7 flag kaydediliyor — yeni eklenenler: firstCrack, sharedSilence
                var flags = new List<string>();
                if (_characterState.trustEstablished) flags.Add("trustEstablished");
                if (_characterState.secretDiscovered) flags.Add("secretDiscovered");
                if (_characterState.recklessPath) flags.Add("recklessPath");
                if (_characterState.smoothTalker) flags.Add("smoothTalker");
                if (_characterState.deepConnection) flags.Add("deepConnection");
                if (_characterState.firstCrack) flags.Add("firstCrack");
                if (_characterState.sharedSilence) flags.Add("sharedSilence");
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
                            buttonText.text = ProcessText($"{choice.text} ({choice.diamondCost})");
                            buttonText.color = diamondTextColor;
                        }
                        if (buttonImage != null)
                            buttonImage.color = diamondButtonColor;
                    }
                    else
                    {
                        if (buttonText != null)
                        {
                            buttonText.text = ProcessText(choice.text);
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

            string characterId = _characterState.characterId;
            var saveService = ServiceLocator.Get<ISaveService>();
            int currentEpisode = saveService?.GetLastPlayedEpisode(characterId) ?? 0;
            int nextEpisode = currentEpisode + 1;

            // Doğru CharacterData'yı bul
            CharacterData characterData = null;
            foreach (var cd in characterDataList)
            {
                if (cd.characterId == characterId)
                {
                    characterData = cd;
                    break;
                }
            }

            // Son bölüm mü?
            bool isLastEpisode = characterData == null ||
                                 characterData.episodes == null ||
                                 nextEpisode >= characterData.episodes.Length;

            if (isLastEpisode)
            {
                // Gerçek oyun sonu
                saveService?.SetLastPlayedEpisode(characterId, nextEpisode);
                saveService?.DeleteProgress(characterId);
                PlayerPrefs.SetString("LastEnding", ending.ToString());
                PlayerPrefs.SetInt("LastAffection", _characterState.affectionPoints);
                HideAllPanels();
                SceneTransition.LoadScene("EndingScreen");
            }
            else
            {
                // Ara bölüm sonu — sonraki bölümü başlat
                saveService?.SetLastPlayedEpisode(characterId, nextEpisode);
                saveService?.DeleteProgress(characterId);
                PlayerPrefs.SetString("LastEnding", ending.ToString());
                PlayerPrefs.SetInt("LastAffection", _characterState.affectionPoints);
                HideAllPanels();

                // Bölüm arası ekran göster
                ShowEpisodeTransition(nextEpisode, ending);
            }
        }

        private void ShowEpisodeTransition(int nextEpisodeIndex, EndingType ending)
        {
            _nextEpisodeIndex = nextEpisodeIndex;

            if (episodeTransitionPanel == null)
            {
                StartNextEpisode(nextEpisodeIndex);
                return;
            }

            episodeTransitionPanel.SetActive(true);

            if (episodeTitleText != null)
                episodeTitleText.text = $"Bölüm {nextEpisodeIndex} Tamamlandı";

            if (episodeCommentText != null)
                episodeCommentText.text = GetEpisodeComment(ending, _characterState.affectionPoints);

            if (episodeContinueButton != null)
            {
                episodeContinueButton.onClick.RemoveAllListeners();
                episodeContinueButton.onClick.AddListener(() => {
                    episodeTransitionPanel.SetActive(false);
                    StartNextEpisode(nextEpisodeIndex);
                });
            }
        }

        private string GetEpisodeComment(EndingType ending, int affection)
        {
            if (affection >= 60)
                return "Harika gidiyorsun! Jenniffer seni fark etti. Bir sonraki bölümde her şey değişebilir.";
            else if (affection >= 40)
                return "İyi bir başlangıç. Ama Jenniffer hâlâ mesafeli. Daha fazlasını yapabilirsin.";
            else if (affection >= 20)
                return "Aranızda bir şey var ama henüz yeterli değil. Bir sonraki şansını iyi kullan.";
            else
                return "Jenniffer ikna olmadı. Ama her şey bitmedi — devam et ve tekrar dene.";
        }

        private void StartNextEpisode(int episodeIndex)
        {
            string characterId = _characterState.characterId;

            CharacterData characterData = null;
            foreach (var cd in characterDataList)
            {
                if (cd.characterId == characterId)
                {
                    characterData = cd;
                    break;
                }
            }

            if (characterData == null || characterData.episodes == null ||
                episodeIndex >= characterData.episodes.Length)
            {
                Debug.LogWarning("[GameplayManager] Sonraki bölüm bulunamadı.");
                return;
            }

            var nextDialogueData = characterData.episodes[episodeIndex];

            // CharacterState affection'ı koru ama süresi dolan flagleri temizle
            _characterState.ExpireFlags(episodeIndex);

            _dialogueEngine.StartEpisode(nextDialogueData, _characterState);
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
            _blockInput = true;
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