using UnityEngine;
using UnityEngine.EventSystems;
using Resources.Scripts.Labyrinth; // For accessing LabyrinthMapController

namespace Resources.Scripts.Player
{
    /// <summary>
    /// Implements a virtual joystick for mobile or touch input.
    /// Disables input when the minimap is active.
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

        /// <summary>
        /// Called when the user presses the joystick.
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (LabyrinthMapController.Instance != null && LabyrinthMapController.Instance.IsMapActive)
                return;

            OnDrag(eventData);
        }

        /// <summary>
        /// Called when the user drags the joystick.
        /// Calculates and updates the input vector.
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            if (LabyrinthMapController.Instance != null && LabyrinthMapController.Instance.IsMapActive)
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position,
                    eventData.pressEventCamera, out Vector2 pos))
            {
                // Normalize the input vector relative to the background size.
                inputVector = new Vector2(pos.x / background.sizeDelta.x * 2, pos.y / background.sizeDelta.y * 2);
                if (inputVector.sqrMagnitude > 1f)
                    inputVector.Normalize();

                // Set the handle position using the configurable handleRange.
                handle.anchoredPosition = inputVector * handleRange;
            }
        }

        /// <summary>
        /// Resets the joystick when the user releases the input.
        /// </summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            inputVector = Vector2.zero;
            handle.anchoredPosition = Vector2.zero;
        }

        /// <summary>
        /// Gets the horizontal component of the joystick input.
        /// </summary>
        public float Horizontal => inputVector.x;

        /// <summary>
        /// Gets the vertical component of the joystick input.
        /// </summary>
        public float Vertical => inputVector.y;
    }
}
