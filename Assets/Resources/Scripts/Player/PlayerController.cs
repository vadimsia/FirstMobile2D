using UnityEngine;
using System.Collections;
using Resources.Scripts.Enemy;
using Resources.Scripts.Fairy;
using Resources.Scripts.Misc;
using UnityEngine.Rendering.Universal; // For Light2D
using Resources.Scripts.Labyrinth;       // For accessing LabyrinthMapController

namespace Resources.Scripts.Player
{
    /// <summary>
    /// Controls player movement, interactions, dynamic light range based on proximity to the finish point,
    /// and handles animation switching.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField, Range(1, 10), Tooltip("Movement speed when using keyboard input.")]
        private float keyboardSpeed = 3f;
        [SerializeField, Range(1, 10), Tooltip("Movement speed when using joystick input.")]
        private float joystickSpeed = 3f;

        [SerializeField, Tooltip("Reference to the custom joystick component (optional).")]
        private PlayerJoystick joystick;

        [SerializeField, Tooltip("Prefab for trap objects (if needed).")]
        private GameObject trapPrefab;

        [Header("Light Settings")]
        [SerializeField, Tooltip("Reference to the Light2D component attached to the player.")]
        private Light2D playerLight;
        [SerializeField, Tooltip("Reference to the finish marker transform (assigned from labyrinth).")]
        private Transform finishPoint;
        [SerializeField, Range(0.1f, 5f), Tooltip("Base light range (fixed value).")]
        private float baseLightRange = 1f;
        [SerializeField, Range(1f, 2f), Tooltip("Maximum light range when near the finish.")]
        private float maxLightRange = 2f;

        [Header("Player Settings")]
        [Tooltip("If enabled, the player will be immune to damage.")]
        public bool isImmortal;

        [Header("Animation Settings")]
        [SerializeField, Tooltip("Animator component for controlling player animations.")]
        private Animator animator;

        // Private variables for managing player state.
        private PlayerStatsHandler playerStats;
        private SpriteRenderer spriteRenderer;
        private float currentSlowMultiplier = 1f;
        private Coroutine slowCoroutine;
        private bool bonusActive;
        private float initialDistance = -1f; // Initial distance from player to finish.

        private void Start()
        {
            playerStats = GetComponent<PlayerStatsHandler>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            // If finishPoint is not assigned, wait for the finish marker to appear.
            if (finishPoint == null)
            {
                StartCoroutine(WaitForFinishMarker());
            }
            else
            {
                initialDistance = Vector2.Distance(transform.position, finishPoint.position);
            }
        }

        /// <summary>
        /// Coroutine that waits until the finish marker appears in the scene.
        /// </summary>
        private IEnumerator WaitForFinishMarker()
        {
            while (finishPoint == null)
            {
                GameObject finishObj = GameObject.FindGameObjectWithTag(ETag.Fairy.ToString()); // If finish marker uses a specific tag, update accordingly.
                if (finishObj != null)
                {
                    finishPoint = finishObj.transform;
                    Debug.Log("Finish marker found: " + finishPoint.name);
                    initialDistance = Vector2.Distance(transform.position, finishPoint.position);
                }
                yield return null;
            }
        }

        private void Update()
        {
            UpdateMovement();
            UpdateLightOuterRange();
        }

        /// <summary>
        /// Updates player movement based on input.
        /// Movement is disabled if the map is active.
        /// Also updates animation and sprite flipping based on direction.
        /// </summary>
        private void UpdateMovement()
        {
            // Do not move if the minimap is active or if Space is held down.
            if ((LabyrinthMapController.Instance != null && LabyrinthMapController.Instance.IsMapActive) || Input.GetKey(KeyCode.Space))
                return;

            float horizontal = (joystick != null) ? joystick.Horizontal : Input.GetAxis("Horizontal");
            float vertical   = (joystick != null) ? joystick.Vertical   : Input.GetAxis("Vertical");
            float currentSpeed = ((joystick != null) ? joystickSpeed : keyboardSpeed) * currentSlowMultiplier;
            Vector2 movement = new Vector2(horizontal, vertical) * currentSpeed;

            // Move the player in world space.
            transform.Translate(movement * Time.deltaTime, Space.World);

            // Update animation state and sprite flipping.
            if (Mathf.Approximately(horizontal, 0f) && Mathf.Approximately(vertical, 0f))
            {
                animator.Play("Idle");
            }
            else
            {
                animator.Play("Run");
                // Flip sprite based on horizontal movement.
                spriteRenderer.flipX = horizontal > 0f;
            }
        }

        /// <summary>
        /// Adjusts the player's light outer radius based on distance to the finish marker.
        /// </summary>
        private void UpdateLightOuterRange()
        {
            if (finishPoint != null && playerLight != null && initialDistance > 0)
            {
                float currentDistance = Vector2.Distance(transform.position, finishPoint.position);
                // Calculate a t value from 0 to 1 based on distance.
                float t = 1f - Mathf.Clamp01(currentDistance / initialDistance);
                float outerRange = Mathf.Lerp(baseLightRange, maxLightRange, t);
                playerLight.pointLightOuterRadius = outerRange;
            }
        }

        /// <summary>
        /// Handles player death by disabling all UI canvases and destroying the player object.
        /// </summary>
        private void Die()
        {
            foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                canvas.gameObject.SetActive(false);
            }
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // When colliding with a fairy, destroy it and restore mana.
            if (other.CompareTag(ETag.Fairy.ToString()))
            {
                FairyController fairy = other.GetComponent<FairyController>();
                fairy.DestroyFairy();
                playerStats.RestoreMana(20f);
            }
        }

        /// <summary>
        /// Reduces the player's health based on enemy damage and applies a dash effect if enabled.
        /// </summary>
        /// <param name="enemy">Enemy controller that hit the player.</param>
        public void TakeDamage(EnemyController enemy)
        {
            if (isImmortal)
                return;

            EnemyStatsHandler enemyStats = enemy.GetComponent<EnemyStatsHandler>();
            playerStats.Health -= enemyStats.Damage;
            if (playerStats.Health <= 0)
            {
                Die();
                return;
            }
            if (enemy.pushPlayer)
            {
                // Dash away from the enemy.
                EntityUtils.MakeDash(transform, transform.position - enemy.transform.position);
            }
        }

        /// <summary>
        /// Temporarily increases the player's movement speed by a multiplier for a set duration.
        /// </summary>
        /// <param name="multiplier">Speed multiplier to apply.</param>
        public void IncreaseSpeed(float multiplier)
        {
            if (bonusActive)
                return;
            StartCoroutine(IncreaseSpeedCoroutine(multiplier, 5f));
        }

        private IEnumerator IncreaseSpeedCoroutine(float multiplier, float duration)
        {
            bonusActive = true;
            float originalJoystickSpeed = joystickSpeed;
            joystickSpeed *= multiplier;
            yield return new WaitForSeconds(duration);
            joystickSpeed = originalJoystickSpeed;
            bonusActive = false;
        }

        /// <summary>
        /// Stuns the player for the specified duration.
        /// </summary>
        /// <param name="duration">Duration of the stun effect.</param>
        public void Stun(float duration)
        {
            StartCoroutine(StunCoroutine(duration));
        }

        private IEnumerator StunCoroutine(float duration)
        {
            currentSlowMultiplier = 0f;
            yield return new WaitForSeconds(duration);
            currentSlowMultiplier = 1f;
        }

        /// <summary>
        /// Applies a slow effect to the player's movement for a specified duration.
        /// </summary>
        /// <param name="slowFactor">Factor by which movement speed is reduced.</param>
        /// <param name="duration">Duration of the slow effect.</param>
        public void ApplySlow(float slowFactor, float duration)
        {
            if (slowCoroutine != null)
                StopCoroutine(slowCoroutine);
            slowCoroutine = StartCoroutine(SlowEffectCoroutine(slowFactor, duration));
        }

        private IEnumerator SlowEffectCoroutine(float slowFactor, float duration)
        {
            currentSlowMultiplier = slowFactor;
            yield return new WaitForSeconds(duration);
            currentSlowMultiplier = 1f;
        }
    }
}
