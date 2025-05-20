using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Resources.Scripts.Labyrinth;

namespace Resources.Scripts.Player
{
    /// <summary>
    /// Implements a virtual joystick for mobile or touch input.
    /// Disables input when the minimap is active.
    /// Добавлена анимация возврата ручки.
    /// </summary>
    public class PlayerJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Joystick UI")]
        [SerializeField, Tooltip("Background RectTransform of the joystick.")]
        private RectTransform background;
        [SerializeField, Tooltip("Handle RectTransform of the joystick.")]
        private RectTransform handle;

        [Header("Joystick Handle Settings")]
        [SerializeField, Tooltip("Максимальное расстояние движения handle от центра джойстика.")]
        private float handleRange = 50f;

        // The current input vector (normalized) representing joystick movement.
        private Vector2 inputVector = Vector2.zero;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (LabyrinthMapController.Instance != null && LabyrinthMapController.Instance.IsMapActive)
                return;

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (LabyrinthMapController.Instance != null && LabyrinthMapController.Instance.IsMapActive)
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position,
                    eventData.pressEventCamera, out Vector2 pos))
            {
                inputVector = new Vector2(pos.x / background.sizeDelta.x * 2, pos.y / background.sizeDelta.y * 2);
                if (inputVector.sqrMagnitude > 1f)
                    inputVector.Normalize();

                // Set the handle position using the configurable handleRange.
                handle
                    .DOAnchorPos(inputVector * handleRange, 0.1f)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            inputVector = Vector2.zero;
            handle
                .DOAnchorPos(Vector2.zero, 0.2f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        public float Horizontal => inputVector.x;
        public float Vertical   => inputVector.y;
    }
}
