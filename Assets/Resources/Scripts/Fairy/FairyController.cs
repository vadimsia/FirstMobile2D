using UnityEngine;

namespace Resources.Scripts.Fairy
{
    /// <summary>
    /// Controls fairy behavior including movement and disappearance when collected.
    /// </summary>
    public class FairyController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField, Range(1, 10), Tooltip("Maximum radius multiplier for fairy movement from its start position.")]
        private int maxMoveRadius = 2;
        [SerializeField, Range(1, 20), Tooltip("Speed at which the fairy moves.")]
        private int speed = 5;
        [SerializeField, Tooltip("Smoothing factor for movement interpolation.")]
        private float moveSmoothing = 0.1f;

        [Header("Randomization Settings")]
        [SerializeField, Tooltip("Minimum multiplier for the random offset distance.")]
        private float minOffsetMultiplier = 1f;
        [SerializeField, Tooltip("Maximum multiplier for the random offset distance.")]
        private float maxOffsetMultiplier = 3f;

        [Header("Debug Settings")]
        [SerializeField, Tooltip("Enable debug logging for fairy movement.")]
        private bool debugLog = false;

        private Vector3 startPosition;
        private Vector3 targetPosition;

        /// <summary>
        /// Initializes the fairy with a starting position.
        /// </summary>
        /// <param name="startPosition">The initial position of the fairy.</param>
        public void Init(Vector3 startPosition)
        {
            this.startPosition = startPosition;
            targetPosition = startPosition;
        }

        /// <summary>
        /// Updates the target position for the fairy's movement.
        /// </summary>
        private void UpdateTargetPosition()
        {
            // Calculate a random offset within a unit circle and scale it with random multipliers and maxMoveRadius.
            Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);
            float multiplier = Random.Range(minOffsetMultiplier, maxOffsetMultiplier);
            targetPosition = startPosition + randomOffset * multiplier * maxMoveRadius;

            if (debugLog)
            {
                Debug.Log("New target position: " + targetPosition);
            }
        }

        private void Update()
        {
            // If the fairy is close enough to the target, update the target position.
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                UpdateTargetPosition();
            }

            // Smoothly move the fairy towards the target position.
            transform.position = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
        }

        /// <summary>
        /// Destroys the fairy from the scene when collected.
        /// </summary>
        public void DestroyFairy()
        {
            Destroy(gameObject);
        }
    }
}
