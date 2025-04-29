using UnityEngine;

namespace Resources.Scripts.Fairy
{
    /// <summary>
    /// Controls fairy movement within a defined radius and handles sprite orientation.
    /// </summary>
    [DisallowMultipleComponent]
    public class FairyController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField, Range(1, 10), Tooltip("Maximum multiplier for movement radius from origin position.")]
        private int maxMoveRadius = 2;

        [SerializeField, Range(1, 20), Tooltip("Movement speed affecting interpolation.")]
        private int speed = 5;

        [SerializeField, Tooltip("Smoothing factor for movement interpolation.")]
        private float moveSmoothing = 0.1f;

        [Header("Randomization Settings")]
        [SerializeField, Tooltip("Minimum multiplier for random offset distance.")]
        private float minOffsetMultiplier = 1f;

        [SerializeField, Tooltip("Maximum multiplier for random offset distance.")]
        private float maxOffsetMultiplier = 3f;

        [Header("Debug Settings")]
        [SerializeField, Tooltip("Enable debug logging for movement.")]
        private bool debugLog;

        private Vector3 originPosition;
        private Vector3 targetPosition;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning("Missing SpriteRenderer component on Fairy.", this);
            }
        }

        /// <summary>
        /// Initializes the fairy's starting position.
        /// </summary>
        /// <param name="initialPosition">The starting world position for movement origin.</param>
        public void Init(Vector3 initialPosition)
        {
            originPosition = initialPosition;
            targetPosition = initialPosition;
        }

        /// <summary>
        /// Calculates a new random target within the defined radius.
        /// </summary>
        private void UpdateTargetPosition()
        {
            // Get a random direction.
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            // Compute total distance multiplier.
            float distanceFactor = Random.Range(minOffsetMultiplier, maxOffsetMultiplier) * maxMoveRadius;
            Vector3 randomOffset = new Vector3(randomDirection.x, randomDirection.y, 0f) * distanceFactor;
            targetPosition = originPosition + randomOffset;

            if (debugLog)
            {
                Debug.Log($"New target position: {targetPosition}", this);
            }
        }

        private void Update()
        {
            // If close to target, choose a new one.
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                UpdateTargetPosition();
            }

            // Smoothly interpolate towards the target.
            float t = moveSmoothing * speed * Time.deltaTime;
            Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, t);
            Vector3 moveDir = newPosition - transform.position;
            transform.position = newPosition;

            // Flip sprite based on horizontal movement direction.
            if (spriteRenderer != null && moveDir.x != 0f)
            {
                spriteRenderer.flipX = moveDir.x > 0f;
            }
        }

        /// <summary>
        /// Removes the fairy from the scene.
        /// </summary>
        public void DestroyFairy()
        {
            Destroy(gameObject);
        }
    }
}
