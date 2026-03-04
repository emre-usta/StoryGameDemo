using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace StoryGame.Core
{
    public class BackgroundService : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private BackgroundEntry[] backgrounds;

        [System.Serializable]
        public class BackgroundEntry
        {
            public string id;
            public Sprite sprite;
        }

        private Dictionary<string, Sprite> _backgroundDict;

        private void Awake()
        {
            _backgroundDict = new Dictionary<string, Sprite>();
            foreach (var entry in backgrounds)
            {
                if (!string.IsNullOrEmpty(entry.id) && entry.sprite != null)
                    _backgroundDict[entry.id] = entry.sprite;
            }
        }

        public void ChangeBackground(string backgroundId)
        {
            if (string.IsNullOrEmpty(backgroundId)) return;

            if (_backgroundDict.TryGetValue(backgroundId, out var sprite))
                StartCoroutine(FadeBackground(sprite));
            else
                Debug.LogWarning($"[BackgroundService] Background bulunamadý: {backgroundId}");
        }

        private IEnumerator FadeBackground(Sprite newSprite)
        {
            yield return StartCoroutine(FadeAlpha(0f));
            backgroundImage.sprite = newSprite;
            yield return StartCoroutine(FadeAlpha(1f));
        }

        private IEnumerator FadeAlpha(float target)
        {
            float start = backgroundImage.color.a;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(start, target, elapsed / fadeDuration);
                backgroundImage.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }

            backgroundImage.color = new Color(1f, 1f, 1f, target);
        }
    }
}