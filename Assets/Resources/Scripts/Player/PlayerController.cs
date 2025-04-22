using UnityEngine;
using System.Collections;
using Resources.Scripts.Enemy;
using Resources.Scripts.Fairy;
using Resources.Scripts.Misc;
using UnityEngine.Rendering.Universal; // For Light2D
using Resources.Scripts.Labyrinth;

namespace Resources.Scripts.Player
{
    /// <summary>
    /// Controls player movement, interactions, dynamic light range based on proximity to the finish point,
    /// handles animation switching and dodge roll functionality.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        #region Constants

        private const string IdleAnimationName = "Idle";
        private const string RunAnimationName = "Run";
        private const string RollAnimationName = "Roll";

        #endregion

        #region Inspector Fields

        [Header("Movement Settings")]
        [SerializeField, Range(1, 10)]
        private float keyboardSpeed = 3f;
        [SerializeField, Range(1, 10)]
        private float joystickSpeed = 3f;
        [SerializeField]
        private PlayerJoystick joystick;
        [SerializeField]
        private GameObject trapPrefab;

        [Header("Light Settings")]
        [SerializeField]
        private Light2D playerLight;
        [SerializeField]
        private Transform finishPoint;
        [SerializeField, Range(0.1f, 5f)]
        private float baseLightRange = 1f;
        [SerializeField, Range(1f, 2f)]
        private float maxLightRange = 2f;

        [Header("Player Settings")]
        public bool isImmortal;

        [Header("Animation Settings")]
        [SerializeField]
        private Animator animator;

        [Header("DarkSkull / Troll Damage Settings")]
        [SerializeField]
        private int maxDarkSkullHits = 2;

        [Header("Dodge Roll Settings")]
        [SerializeField]
        private float rollSpeed = 6f;
        [SerializeField]
        private float rollDuration = 0.3f;
        [SerializeField]
        private float rollCooldown = 2f;

        #endregion

        #region Private Fields

        private PlayerStatsHandler playerStats;
        private SpriteRenderer spriteRenderer;
        private float currentSlowMultiplier = 1f;
        private Coroutine slowCoroutine;
        private bool bonusActive;
        private float initialDistance = -1f;
        private int darkSkullHitCount = 0;

        private Vector2 lastMoveDirection = Vector2.right;
        private bool isRolling = false;
        private bool canRoll = true;

        #endregion

        #region Unity Methods

        private void Start()
        {
            playerStats = GetComponent<PlayerStatsHandler>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (finishPoint != null)
                initialDistance = Vector2.Distance(transform.position, finishPoint.position);
            else
                StartCoroutine(WaitForFinishMarker());
        }

        private void Update()
        {
            if (!isRolling)
                UpdateMovement();

            UpdateLightOuterRange();

            if (Input.GetKeyDown(KeyCode.LeftShift) && canRoll && !isRolling)
                StartCoroutine(RollCoroutine());
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(ETag.Fairy.ToString()))
            {
                FairyController fairy = other.GetComponent<FairyController>();
                fairy?.DestroyFairy();
                playerStats.RestoreMana(20f);
            }
        }

        #endregion

        #region Movement Methods

        private void UpdateMovement()
        {
            if ((LabyrinthMapController.Instance != null && LabyrinthMapController.Instance.IsMapActive) || Input.GetKey(KeyCode.Space))
                return;

            float horizontal = joystick != null ? joystick.Horizontal : Input.GetAxis("Horizontal");
            float vertical = joystick != null ? joystick.Vertical : Input.GetAxis("Vertical");

            Vector2 inputDirection = new Vector2(horizontal, vertical);
            if (inputDirection.magnitude > 0.1f)
                lastMoveDirection = inputDirection.normalized;

            float currentSpeed = (joystick != null ? joystickSpeed : keyboardSpeed) * currentSlowMultiplier;
            Vector2 movement = inputDirection * currentSpeed;

            transform.Translate(movement * Time.deltaTime, Space.World);

            if (movement == Vector2.zero)
                animator.Play(IdleAnimationName);
            else
            {
                animator.Play(RunAnimationName);
                spriteRenderer.flipX = horizontal > 0f;
            }
        }

        #endregion

        #region Dodge Roll

        private IEnumerator RollCoroutine()
        {
            isRolling = true;
            canRoll = false;

            animator.Play(RollAnimationName);
            Vector2 rollDirection = lastMoveDirection.normalized;
            float timer = 0f;

            while (timer < rollDuration)
            {
                transform.Translate(rollDirection * rollSpeed * Time.deltaTime);
                timer += Time.deltaTime;
                yield return null;
            }

            isRolling = false;
            yield return new WaitForSeconds(rollCooldown);
            canRoll = true;
        }

        #endregion

        #region Light Methods

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

        #endregion

        #region Utility Methods

        private IEnumerator WaitForFinishMarker()
        {
            while (finishPoint == null)
            {
                GameObject finishObj = GameObject.FindGameObjectWithTag(ETag.Fairy.ToString());
                if (finishObj != null)
                {
                    finishPoint = finishObj.transform;
                    Debug.Log("Finish marker found: " + finishPoint.name);
                    initialDistance = Vector2.Distance(transform.position, finishPoint.position);
                }
                yield return null;
            }
        }

        private void Die()
        {
            foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                canvas.gameObject.SetActive(false);

            Destroy(gameObject);
        }

        #endregion

        #region Damage and Effects

        public void TakeDamage(EnemyController enemy)
        {
            if (isImmortal || isRolling)
                return;

            EnemyStatsHandler enemyStats = enemy.GetComponent<EnemyStatsHandler>();
            playerStats.Health -= enemyStats.Damage;

            if (playerStats.Health <= 0)
            {
                Die();
                return;
            }

            if (enemy.pushPlayer)
                EntityUtils.MakeDash(transform, transform.position - enemy.transform.position);
        }

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

        public void ApplyBinding(float duration)
        {
            StartCoroutine(BindingCoroutine(duration));
        }

        private IEnumerator BindingCoroutine(float duration)
        {
            float originalMultiplier = currentSlowMultiplier;
            currentSlowMultiplier = 0f;
            yield return new WaitForSeconds(duration);
            currentSlowMultiplier = originalMultiplier;
        }

        public void Stun(float duration)
        {
            StartCoroutine(StunCoroutine(duration));
        }

        private IEnumerator StunCoroutine(float duration)
        {
            float originalMultiplier = currentSlowMultiplier;
            currentSlowMultiplier = 0f;
            yield return new WaitForSeconds(duration);
            currentSlowMultiplier = originalMultiplier;
        }

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

        public void ReceiveDarkSkullHit()
        {
            darkSkullHitCount++;
            if (darkSkullHitCount >= maxDarkSkullHits)
                Die();
        }

        public void ReceiveTrollHit()
        {
            Die();
        }
        
        public void TryRoll()
        {
            if (canRoll && !isRolling)
                StartCoroutine(RollCoroutine());
        }


        #endregion
    }
}
