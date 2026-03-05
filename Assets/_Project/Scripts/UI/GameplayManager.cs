using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using StoryGame.Core;
using StoryGame.Characters;
using StoryGame.Dialogue;
using TMPro;
using DG.Tweening;

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

        private DialogueEngine _dialogueEngine;
        private CharacterState _characterState;
        private IDiamondService _diamondService;
        private string _currentNodeText;

        private void Start()
        {
            var audio = ServiceLocator.Get<IAudioService>();
            audio?.PlayMusic("gameplay");

            _diamondService = ServiceLocator.Get<IDiamondService>();
            UpdateDiamondUI();
            UpdateAffectionBar();

            // Seçili karakteri al
            string characterId = PlayerPrefs.GetString("SelectedCharacter", "jenniffer");

            // CharacterState oluştur
            _characterState = new CharacterState();
            _characterState.characterId = characterId;

            // DialogueEngine kur
            _dialogueEngine = gameObject.AddComponent<DialogueEngine>();
            _dialogueEngine.OnNarrationNode += ShowNarration;
            _dialogueEngine.OnDialogueNode += ShowDialogue;
            _dialogueEngine.OnChoiceNode += ShowChoices;
            _dialogueEngine.OnEpisodeEnded += OnEpisodeEnded;

            // Panelleri kapat
            HideAllPanels();

            // Diyalogu başlat
            if (dialogueData != null)
                _dialogueEngine.StartEpisode(dialogueData, _characterState);
            else
                Debug.LogWarning("[GameplayManager] DialogueData atanmadı!");
        }

        private void Update()
        {
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

        private void ShowNarration(DialogueNode node)
        {
            HideAllPanels();
            narrationPanel.SetActive(true);
            float originalYN = narrationPanel.transform.localPosition.y;
            narrationPanel.transform.localPosition = new Vector3(0, originalYN - 100f, 0);
            narrationPanel.transform.DOLocalMoveY(originalYN, 0.6f).SetEase(Ease.OutCubic);
            _currentNodeText = node.text;
            typewriter.Play(narrationText, node.text);
            UpdateAffectionBar();
            if (!string.IsNullOrEmpty(node.backgroundId))
                backgroundService?.ChangeBackground(node.backgroundId);
        }

        private void ShowDialogue(DialogueNode node)
        {
            HideAllPanels();
            dialoguePanel.SetActive(true);
            float originalYD = dialoguePanel.transform.localPosition.y;
            dialoguePanel.transform.localPosition = new Vector3(0, originalYD - 100f, 0);
            dialoguePanel.transform.DOLocalMoveY(originalYD, 0.6f).SetEase(Ease.OutCubic);
            speakerNameText.text = node.speaker;
            _currentNodeText = node.text;
            typewriter.Play(dialogueText, node.text);
            UpdateAffectionBar();
            if (!string.IsNullOrEmpty(node.backgroundId))
                backgroundService?.ChangeBackground(node.backgroundId);
        }

        private void ShowChoices(DialogueNode node)
        {
            HideAllPanels();
            choicePanel.SetActive(true);

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
                        _dialogueEngine.SelectChoice(index);
                        UpdateDiamondUI();
                    });

                    // Staggered fade in
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
    }
}