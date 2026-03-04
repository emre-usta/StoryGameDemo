using StoryGame.Core;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StoryGame.UI
{
    public class TitleScreenManager : MonoBehaviour
    {
        [Header("UI Elemanlarý")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private Image backgroundImage;

        [Header("Ayarlar")]
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private float fadeDuration = 1f;

        private void Start()
        {
            StartCoroutine(TitleSequence());
        }

        private IEnumerator TitleSequence()
        {
            // Baþlangýçta þeffaf
            SetAlpha(0f);

            // Fade in
            yield return StartCoroutine(FadeAll(1f));

            // Bekle
            yield return new WaitForSeconds(displayDuration);

            // Fade out
            yield return StartCoroutine(FadeAll(0f));

            // MainMenu'ye geç
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        private IEnumerator FadeAll(float target)
        {
            float elapsed = 0f;
            float startTitle = titleText != null ? titleText.color.a : 0f;
            float startSubtitle = subtitleText != null ? subtitleText.color.a : 0f;
            float startBg = backgroundImage != null ? backgroundImage.color.a : 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;

                if (titleText != null)
                    titleText.color = new Color(titleText.color.r, titleText.color.g, titleText.color.b, Mathf.Lerp(startTitle, target, t));

                if (subtitleText != null)
                    subtitleText.color = new Color(subtitleText.color.r, subtitleText.color.g, subtitleText.color.b, Mathf.Lerp(startSubtitle, target, t));

                if (backgroundImage != null)
                    backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, Mathf.Lerp(startBg, target, t));

                yield return null;
            }

            SetAlpha(target);
        }

        private void SetAlpha(float alpha)
        {
            if (titleText != null)
                titleText.color = new Color(titleText.color.r, titleText.color.g, titleText.color.b, alpha);

            if (subtitleText != null)
                subtitleText.color = new Color(subtitleText.color.r, subtitleText.color.g, subtitleText.color.b, alpha);

            if (backgroundImage != null)
                backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, alpha);
        }
    }
}