using UnityEngine;
using UnityEngine.EventSystems;

namespace Resources.Scripts.Player
{
    public class PlayerJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;

        private Vector2 inputVector = Vector2.zero;

        // Called when the user presses the joystick.
        public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

        // Handles the dragging of the joystick.
        public void OnDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position,
                    eventData.pressEventCamera, out Vector2 pos))
            {
                inputVector = new Vector2(pos.x / background.sizeDelta.x * 2, pos.y / background.sizeDelta.y * 2);
                if (inputVector.sqrMagnitude > 1f)
                    inputVector.Normalize();

                handle.anchoredPosition = inputVector * (background.sizeDelta / 3);
            }
        }

        // Resets the joystick when the user releases the input.
        public void OnPointerUp(PointerEventData eventData)
        {
            inputVector = Vector2.zero;
            handle.anchoredPosition = Vector2.zero;
        }

        public float Horizontal => inputVector.x;
        public float Vertical => inputVector.y;
    }
}