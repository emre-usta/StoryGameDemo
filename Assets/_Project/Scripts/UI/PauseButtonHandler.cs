using UnityEngine;
using UnityEngine.EventSystems;

namespace StoryGame.UI
{
    public class PauseButtonHandler : MonoBehaviour, IPointerDownHandler
    {
        public System.Action OnPressed;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnPressed?.Invoke();
        }
    }
}