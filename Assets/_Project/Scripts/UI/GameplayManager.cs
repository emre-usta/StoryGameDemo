using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using StoryGame.Core;
using StoryGame.Characters;
using StoryGame.Dialogue;
using TMPro;

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
                    if (buttonText != null)
                        buttonText.text = choice.isDiamond ? $"💎 {choice.text} ({choice.diamondCost})" : choice.text;

                    int index = i;
                    choiceButtons[i].onClick.RemoveAllListeners();
                    choiceButtons[i].onClick.AddListener(() => _dialogueEngine.SelectChoice(index));
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
            HideAllPanels();
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
                diamondText.text = _diamondService.GetAmount().ToString();
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