using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StoryGame.Characters;

namespace StoryGame.UI
{
    public class AffectionBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI tierText;
        [SerializeField] private Image fillImage;

        private readonly Color _coldColor = new Color(0.4f, 0.6f, 0.8f);
        private readonly Color _neutralColor = new Color(0.6f, 0.6f, 0.6f);
        private readonly Color _friendlyColor = new Color(0.4f, 0.8f, 0.4f);
        private readonly Color _closeColor = new Color(0.9f, 0.7f, 0.2f);
        private readonly Color _devotedColor = new Color(0.9f, 0.3f, 0.4f);

        public void UpdateBar(CharacterState state)
        {
            if (state == null) return;

            if (slider != null)
            {
                slider.minValue = 0;
                slider.maxValue = 100;
                slider.value = state.affectionPoints;
            }

            var tier = state.GetTier();

            if (tierText != null)
                tierText.text = GetTierLabel(tier);

            if (fillImage != null)
                fillImage.color = GetTierColor(tier);
        }

        private string GetTierLabel(AffectionTier tier)
        {
            return tier switch
            {
                AffectionTier.Cold => "Soðuk",
                AffectionTier.Neutral => "Nötr",
                AffectionTier.Friendly => "Dostane",
                AffectionTier.Close => "Yakýn",
                AffectionTier.Devoted => "Adanmýþ",
                _ => ""
            };
        }

        private Color GetTierColor(AffectionTier tier)
        {
            return tier switch
            {
                AffectionTier.Cold => _coldColor,
                AffectionTier.Neutral => _neutralColor,
                AffectionTier.Friendly => _friendlyColor,
                AffectionTier.Close => _closeColor,
                AffectionTier.Devoted => _devotedColor,
                _ => _neutralColor
            };
        }
    }
}