using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StoryGame.Characters;
using StoryGame.Core;

namespace StoryGame.UI
{
    public class EndingScreenManager : MonoBehaviour
    {
        [Header("UI Elemanlarý")]
        [SerializeField] private TextMeshProUGUI endingTitleText;
        [SerializeField] private TextMeshProUGUI endingDescriptionText;
        [SerializeField] private TextMeshProUGUI affectionText;
        [SerializeField] private Image endingImage;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Ending Görselleri")]
        [SerializeField] private Sprite deepBondSprite;
        [SerializeField] private Sprite passionateChaosSprite;
        [SerializeField] private Sprite casualFriendSprite;
        [SerializeField] private Sprite coldGoodbyeSprite;
        [SerializeField] private Sprite secretFaceoffSprite;

        private void Start()
        {
            string endingStr = PlayerPrefs.GetString("LastEnding", "ColdGoodbye");
            int affection = PlayerPrefs.GetInt("LastAffection", 0);

            if (System.Enum.TryParse<EndingType>(endingStr, out var ending))
                ShowEnding(ending, affection);

            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void ShowEnding(EndingType ending, int affection)
        {
            if (affectionText != null)
                affectionText.text = $"Ýliþki Puaný: {affection}";

            switch (ending)
            {
                case EndingType.DeepBond:
                    SetEnding("Derin Bað", "Aranýzdaki bað çok güçlendi. Bu iliþki özel bir þeye dönüþüyor...", deepBondSprite);
                    break;
                case EndingType.PassionateChaos:
                    SetEnding("Tutkulu Kaos", "Her þey çok yoðun ve karmaþýk. Ama bu heyecaný seviyorsun.", passionateChaosSprite);
                    break;
                case EndingType.CasualFriend:
                    SetEnding("Sýradan Arkadaþ", "Ýyi bir arkadaþlýk kuruldu. Belki zamanla daha fazlasý olur.", casualFriendSprite);
                    break;
                case EndingType.ColdGoodbye:
                    SetEnding("Soðuk Veda", "Aranýzdaki mesafe kapanmadý. Belki bir dahaki sefere...", coldGoodbyeSprite);
                    break;
                case EndingType.SecretFaceoff:
                    SetEnding("Gizli Yüzleþme", "Saklanan bir sýr ortaya çýktý. Her þey deðiþti.", secretFaceoffSprite);
                    break;
            }
        }

        private void SetEnding(string title, string description, Sprite sprite)
        {
            if (endingTitleText != null)
                endingTitleText.text = title;
            if (endingDescriptionText != null)
                endingDescriptionText.text = description;
            if (endingImage != null && sprite != null)
                endingImage.sprite = sprite;
        }

        private void OnContinueClicked()
        {
            ServiceLocator.Get<IAudioService>()?.PlaySFX("button_click");
            SceneTransition.LoadScene("CharacterSelect");
        }

        private void OnMainMenuClicked()
        {
            ServiceLocator.Get<IAudioService>()?.PlaySFX("button_click");
            SceneTransition.LoadScene("MainMenu");
        }
    }
}