using Resources.Scripts.Player;
using UnityEngine;
using System.Collections;

namespace Resources.Scripts.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField, Range(1, 15), Tooltip("Base movement speed of the enemy.")]
        private int speed = 1;
        [SerializeField, Tooltip("Multiplier for enemy movement speed when slowed.")]
        private float slowMultiplier = 1f;

        [Header("Attack Settings")]
        [SerializeField, Tooltip("Maximum distance at which the enemy will detect the player.")]
        private float detectionRange = 5f;
        [SerializeField, Tooltip("Distance at which the enemy starts attacking the player.")]
        private float attackRange = 1f;
        [SerializeField, Tooltip("Time interval between consecutive attacks (in seconds).")]
        private float attackCooldown = 1f;
        private float lastAttackTime;

        [Header("Player Interaction Settings")]
        [Tooltip("Enables or disables pushing the player on contact.")]
        public bool pushPlayer = true;
        [SerializeField, Tooltip("Force applied to the player when pushed.")]
        private float pushForceMultiplier = 1f;

        [Header("Debug Settings")]
        [SerializeField, Tooltip("Enable debug logging for enemy actions.")]
        private bool debugLog = false;

        private float currentSpeed;
        private Coroutine slowCoroutine;
        private Rigidbody2D rb;
        private PlayerController player;

        private void Start()
        {
            // Find and cache the player reference using tag "Player"
            player = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
            currentSpeed = speed;
            rb = GetComponent<Rigidbody2D>(); // For physics interactions such as push
        }

        private void Update()
        {
            FollowPlayer();
        }

        /// <summary>
        /// Moves the enemy toward the player if within detection range and attacks if in range.
        /// </summary>
        private void FollowPlayer()
        {
            if (player == null)
            {
                // Destroy the enemy if the player is missing (e.g., destroyed)
                Destroy(gameObject);
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            // Only pursue if within detection range
            if (distanceToPlayer > detectionRange)
                return;

            // Smoothly move towards the player's position
            transform.position = Vector3.Lerp(transform.position, player.transform.position, Time.deltaTime * currentSpeed);

            // Attack if close enough and cooldown has passed
            if (distanceToPlayer <= attackRange && Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time;
                player.TakeDamage(this);
                if (debugLog)
                {
                    Debug.Log("Enemy attacked the player.");
                }

                // Optionally, push the player if enabled
                if (pushPlayer && player.TryGetComponent<Rigidbody2D>(out Rigidbody2D playerRb))
                {
                    Vector2 pushDirection = (player.transform.position - transform.position).normalized;
                    playerRb.AddForce(pushDirection * pushForceMultiplier, ForceMode2D.Impulse);
                }
            }
        }

        /// <summary>
        /// Applies a slow effect to the enemy for a specified duration.
        /// </summary>
        /// <param name="slowFactor">Factor by which the enemy's speed is reduced.</param>
        /// <param name="duration">Duration of the slow effect in seconds.</param>
        public void ApplySlow(float slowFactor, float duration)
        {
            if (slowCoroutine != null)
                StopCoroutine(slowCoroutine);

            slowCoroutine = StartCoroutine(SlowEffect(slowFactor, duration));
        }

        /// <summary>
        /// Coroutine that handles the slow effect's duration.
        /// </summary>
        private IEnumerator SlowEffect(float slowFactor, float duration)
        {
            currentSpeed = speed * slowFactor;
            yield return new WaitForSeconds(duration);
            currentSpeed = speed;
        }

        /// <summary>
        /// Applies an impulse force to the enemy.
        /// </summary>
        /// <param name="force">The force vector to apply.</param>
        public void ApplyPush(Vector2 force)
        {
            if (rb != null)
                rb.AddForce(force, ForceMode2D.Impulse);
        }
    }
}
