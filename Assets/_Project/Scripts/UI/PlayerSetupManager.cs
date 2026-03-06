using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using StoryGame.Core;

namespace StoryGame.UI
{
    public class PlayerSetupManager : MonoBehaviour
    {
        [Header("Karakter Seçimi")]
        [SerializeField] private Image[] characterCards;
        [SerializeField] private Sprite[] characterSprites;

        [Header("Ýsim Giriþi")]
        [SerializeField] private TMP_InputField nameInputField;

        [Header("UI")]
        [SerializeField] private Button confirmButton;

        private const string PLAYER_NAME_KEY = "PlayerName";
        private const string PLAYER_CHAR_KEY = "PlayerCharIndex";
        private const string DEFAULT_NAME = "Alex";

        private int _selectedCharIndex = 0;
        private Color _normalColor = new Color(1f, 1f, 1f, 0.6f);
        private Color _selectedColor = new Color(1f, 1f, 1f, 1f);

        private void Start()
        {
            // Kayýtlý deðerleri yükle
            string savedName = PlayerPrefs.GetString(PLAYER_NAME_KEY, DEFAULT_NAME);
            int savedCharIndex = PlayerPrefs.GetInt(PLAYER_CHAR_KEY, 0);

            nameInputField.text = savedName;
            _selectedCharIndex = savedCharIndex;

            // Karakter kartlarýný ayarla
            for (int i = 0; i < characterCards.Length; i++)
            {
                if (i < characterSprites.Length)
                    characterCards[i].sprite = characterSprites[i];

                int index = i;
                var button = characterCards[i].GetComponent<Button>();
                if (button == null)
                    button = characterCards[i].gameObject.AddComponent<Button>();
                button.onClick.AddListener(() => SelectCharacter(index));
            }

            UpdateCardVisuals();

            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        private void SelectCharacter(int index)
        {
            _selectedCharIndex = index;
            UpdateCardVisuals();
            ServiceLocator.Get<IAudioService>()?.PlaySFX("button_click");
        }

        private void UpdateCardVisuals()
        {
            for (int i = 0; i < characterCards.Length; i++)
            {
                if (i == _selectedCharIndex)
                {
                    characterCards[i].color = _selectedColor;
                    characterCards[i].transform.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack);
                }
                else
                {
                    characterCards[i].color = _normalColor;
                    characterCards[i].transform.DOScale(1f, 0.2f);
                }
            }
        }

        private void OnConfirmClicked()
        {
            string playerName = nameInputField.text.Trim();
            if (string.IsNullOrEmpty(playerName))
                playerName = DEFAULT_NAME;

            PlayerPrefs.SetString(PLAYER_NAME_KEY, playerName);
            PlayerPrefs.SetInt(PLAYER_CHAR_KEY, _selectedCharIndex);
            PlayerPrefs.Save();

            ServiceLocator.Get<IAudioService>()?.PlaySFX("button_click");
            Debug.Log($"[PlayerSetup] Ýsim: {playerName}, Karakter: {_selectedCharIndex}");
            SceneTransition.LoadScene("CharacterSelect");
        }
    }
}