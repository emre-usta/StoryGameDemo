using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace StoryGame.Core
{
    public class SceneTransition : MonoBehaviour
    {
        private static SceneTransition _instance;

        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeDuration = 0.4f;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static void LoadScene(string sceneName)
        {
            if (_instance != null)
                _instance.StartCoroutine(_instance.FadeAndLoad(sceneName));
            else
                SceneManager.LoadScene(sceneName);
        }

        private IEnumerator FadeAndLoad(string sceneName)
        {
            yield return StartCoroutine(Fade(1f));
            Resources.UnloadUnusedAssets();
            SceneManager.LoadScene(sceneName);
            yield return StartCoroutine(Fade(0f));
        }

        private IEnumerator Fade(float targetAlpha)
        {
            if (fadeImage == null) yield break;

            float startAlpha = fadeImage.color.a;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
                fadeImage.color = new Color(0f, 0f, 0f, alpha);
                yield return null;
            }

            if (fadeImage != null)
                fadeImage.color = new Color(0f, 0f, 0f, targetAlpha);
        }
    }
}