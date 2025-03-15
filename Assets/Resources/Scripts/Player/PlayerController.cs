using UnityEngine;
using System.Collections;
using Resources.Scripts.Enemy;
using Resources.Scripts.Fairy;
using Resources.Scripts.Misc;
using UnityEngine.Rendering.Universal; // For Light2D
using Resources.Scripts.Labyrinth; // Для доступа к MapController

namespace Resources.Scripts.Player
{
    /// <summary>
    /// Controls player movement, interactions, and dynamic light outer range based on proximity to the finish.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [SerializeField, Range(1, 10)]
        private float keyboardSpeed = 3f;
        [SerializeField, Range(1, 10)]
        private float joystickSpeed = 3f;

        [SerializeField]
        private PlayerJoystick joystick;
        [SerializeField]
        private GameObject trapPrefab;

        private PlayerStatsHandler playerStats;
        private float currentSlowMultiplier = 1f;
        private Coroutine slowCoroutine;
        private bool bonusActive;

        [SerializeField]
        private Light2D playerLight; // Reference to the Light2D component
        [SerializeField]
        private Transform finishPoint; // Reference to the finish marker (assigned from labyrinth)

        // Base light outer range is fixed at 1 and should not change.
        [SerializeField, Range(0.1f, 5f)]
        private float baseLightRange = 1f;
        // Maximum outer range when the player is at the finish.
        [SerializeField, Range(1f, 2f)]
        private float maxLightRange = 2f;

        // Stores the initial distance from the player to the finish at the start.
        private float initialDistance = -1f;

        /// <summary>
        /// Initializes the player.
        /// </summary>
        private void Start()
        {
            playerStats = GetComponent<PlayerStatsHandler>();

            // If finishPoint is not assigned, wait for the finish marker.
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
                GameObject finishObj = GameObject.FindWithTag("Finish");
                if (finishObj != null)
                {
                    finishPoint = finishObj.transform;
                    Debug.Log("Finish marker found: " + finishPoint.name);
                    initialDistance = Vector2.Distance(transform.position, finishPoint.position);
                }
                yield return null;
            }
        }

        /// <summary>
        /// Updates player movement and light outer range each frame.
        /// </summary>
        private void Update()
        {
            UpdateMovement();
            UpdateLightOuterRange();
        }

        /// <summary>
        /// Handles player movement based on input.
        /// Movement is disabled while the map is active.
        /// </summary>
        private void UpdateMovement()
        {
            // Prevent movement if the map is visible.
            if (LabyrinthMapController.Instance != null && LabyrinthMapController.Instance.IsMapActive)
                return;

            if (Input.GetKey(KeyCode.Space))
                return;

            float horizontal = (joystick != null) ? joystick.Horizontal : Input.GetAxis("Horizontal");
            float vertical   = (joystick != null) ? joystick.Vertical   : Input.GetAxis("Vertical");
            float currentSpeed = ((joystick != null) ? joystickSpeed : keyboardSpeed) * currentSlowMultiplier;
            Vector2 movement = new Vector2(horizontal, vertical) * currentSpeed;
            transform.Translate(movement * Time.deltaTime);
        }

        /// <summary>
        /// Adjusts the player's light outer range based on the distance to the finish marker.
        /// </summary>
        private void UpdateLightOuterRange()
        {
            if (finishPoint != null && playerLight != null && initialDistance > 0)
            {
                float currentDistance = Vector2.Distance(transform.position, finishPoint.position);
                float t = 1f - Mathf.Clamp01(currentDistance / initialDistance);
                float outerRange = Mathf.Lerp(baseLightRange, maxLightRange, t);
                playerLight.pointLightOuterRadius = outerRange;
            }
        }

        /// <summary>
        /// Called when the player's health reaches zero.
        /// </summary>
        private void Die()
        {
            foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                canvas.gameObject.SetActive(false);
            }
            Destroy(gameObject);
        }

        /// <summary>
        /// Handles trigger collisions (e.g., with Fairy).
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Fairy"))
            {
                FairyController fairy = other.GetComponent<FairyController>();
                fairy.Destroy();
                playerStats.FairyCount++;
            }
        }

        /// <summary>
        /// Reduces the player's health when hit by an enemy and applies a dash effect.
        /// </summary>
        public void TakeDamage(EnemyController enemy)
        {
            EnemyStatsHandler enemyStats = enemy.GetComponent<EnemyStatsHandler>();
            playerStats.Health -= enemyStats.Damage;
            if (playerStats.Health <= 0)
            {
                Die();
                return;
            }
            EntityUtils.MakeDash(transform, transform.position - enemy.transform.position);
        }

        /// <summary>
        /// Temporarily increases the player's speed as a bonus.
        /// </summary>
        public void IncreaseSpeed(float multiplier)
        {
            if (bonusActive)
                return;
            StartCoroutine(IncreaseSpeedCoroutine(multiplier, 5f));
        }

        /// <summary>
        /// Coroutine for temporary speed increase.
        /// </summary>
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
        /// Stuns the player for a given duration.
        /// </summary>
        public void Stun(float duration)
        {
            StartCoroutine(StunCoroutine(duration));
        }

        /// <summary>
        /// Coroutine for stun effect.
        /// </summary>
        private IEnumerator StunCoroutine(float duration)
        {
            currentSlowMultiplier = 0f;
            yield return new WaitForSeconds(duration);
            currentSlowMultiplier = 1f;
        }

        /// <summary>
        /// Applies a slow effect to the player's movement for a specified duration.
        /// </summary>
        public void ApplySlow(float slowFactor, float duration)
        {
            if (slowCoroutine != null)
                StopCoroutine(slowCoroutine);
            slowCoroutine = StartCoroutine(SlowEffectCoroutine(slowFactor, duration));
        }

        /// <summary>
        /// Coroutine for slow effect.
        /// </summary>
        private IEnumerator SlowEffectCoroutine(float slowFactor, float duration)
        {
            currentSlowMultiplier = slowFactor;
            yield return new WaitForSeconds(duration);
            currentSlowMultiplier = 1f;
        }
    }
}
