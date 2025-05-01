using UnityEngine;

namespace Resources.Scripts.CameraSystem
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField, Tooltip("Transform of the target to follow (e.g., the player).")]
        private Transform target;

        [Header("Movement Settings")]
        [SerializeField, Range(1, 20), Tooltip("Speed at which the camera follows the target.")]
        private int followSpeed = 5;
        [SerializeField, Tooltip("Fixed Z offset for the camera position.")]
        private float zOffset = -10f;

        [Header("Camera Projection Settings")]
        [SerializeField, Range(1, 20), Tooltip("Minimum orthographic size of the camera.")]
        private float projMin = 2f;
        [SerializeField, Range(1, 20), Tooltip("Maximum orthographic size of the camera.")]
        private float projMax = 10f;
        [SerializeField, Tooltip("Key used to toggle camera zoom.")]
        private KeyCode zoomKey = KeyCode.Space;
        [SerializeField, Tooltip("Speed at which the camera zooms.")]
        private float zoomSpeed = 5f;

        private Camera mainCamera;

        private void Start()
        {
            // Cache the main camera reference for performance
            mainCamera = UnityEngine.Camera.main;
        }

        private void Update()
        {
            UpdatePosition();
            UpdateProjectionSize();
        }

        /// <summary>
        /// Smoothly moves the camera towards the target position.
        /// </summary>
        private void UpdatePosition()
        {
            if (target == null)
            {
                // If the target is missing (e.g., destroyed), exit early.
                return;
            }
            Vector3 newPos = Vector3.Lerp(transform.position, target.position, Time.deltaTime * followSpeed);
            newPos.z = zOffset;
            transform.position = newPos;
        }

        /// <summary>
        /// Smoothly adjusts the camera's orthographic size based on input.
        /// </summary>
        private void UpdateProjectionSize()
        {
            if (mainCamera == null)
            {
                return;
            }
            float targetSize = Input.GetKey(zoomKey) ? projMax : projMin;
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
        }
    }
}
