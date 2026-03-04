using System.Collections;
using UnityEngine;
using TMPro;

namespace StoryGame.UI
{
    public class TypewriterEffect : MonoBehaviour
    {
        [SerializeField] private float charactersPerSecond = 30f;

        private Coroutine _typingCoroutine;
        private bool _isTyping;

        public bool IsTyping => _isTyping;

        public void Play(TextMeshProUGUI textComponent, string fullText, System.Action onComplete = null)
        {
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);

            _typingCoroutine = StartCoroutine(TypeRoutine(textComponent, fullText, onComplete));
        }

        public void Skip(TextMeshProUGUI textComponent, string fullText)
        {
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);

            _isTyping = false;
            textComponent.text = fullText;
        }

        private IEnumerator TypeRoutine(TextMeshProUGUI textComponent, string fullText, System.Action onComplete)
        {
            _isTyping = true;
            textComponent.text = "";

            float delay = 1f / charactersPerSecond;

            foreach (char c in fullText)
            {
                textComponent.text += c;
                yield return new WaitForSeconds(delay);
            }

            _isTyping = false;
            onComplete?.Invoke();
        }
    }
}