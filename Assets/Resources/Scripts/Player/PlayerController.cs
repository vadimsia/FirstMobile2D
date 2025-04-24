// Resources/Scripts/Player/PlayerController.cs
using UnityEngine;
using System;
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
    /// handles animation switching and dodge roll functionality with cooldown.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        #region Constants

        private const string IdleAnimationName = "Idle";
        private const string RunAnimationName = "Run";

        #endregion

        #region Inspector Fields

        [Header("Movement Settings")]
        [SerializeField, Range(1, 10)] private float keyboardSpeed = 3f;
        [SerializeField, Range(1, 10)] private float joystickSpeed = 3f;
        [SerializeField] private PlayerJoystick joystick;
        [SerializeField] private GameObject trapPrefab;

        [Header("Light Settings")]
        [SerializeField] private Light2D playerLight;
        [SerializeField] private Transform finishPoint;
        [SerializeField, Range(0.1f, 5f)] private float baseLightRange = 1f;
        [SerializeField, Range(1f, 2f)] private float maxLightRange = 2f;

        [Header("Player Settings")]
        public bool isImmortal;

        [Header("Animation Settings")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite rollSprite;

        [Header("DarkSkull / Troll Damage Settings")]
        [SerializeField] private int maxDarkSkullHits = 2;

        [Header("Dodge Roll Settings")]
        [SerializeField, Tooltip("Скорость кувырка")] private float rollSpeed = 6f;
        [SerializeField, Tooltip("Длительность кувырка (сек)")] private float rollDuration = 0.3f;
        [SerializeField, Tooltip("Кулдаун между кувырками (сек)")] private float rollCooldown = 2f;

        #endregion

        #region Public Events & Properties

        /// <summary>Нормализованное значение (0–1) оставшегося кулдауна кувырка</summary>
        public event Action<float> OnRollCooldownChanged;

        /// <summary>Длительность кулдауна для UI</summary>
        public float RollCooldownDuration => rollCooldown;

        #endregion

        #region Private Fields

        private PlayerStatsHandler playerStats;
        private float currentSlowMultiplier = 1f;
        private Coroutine slowCoroutine;
        private bool bonusActive;
        private float initialDistance = -1f;
        private int darkSkullHitCount;

        private Vector2 lastMoveDirection = Vector2.left;
        private bool isRolling;
        private bool canRoll = true;
        private Sprite originalSprite;

        private float rollCooldownRemaining;

        #endregion

        #region Unity Methods

        private void Start()
        {
            playerStats = GetComponent<PlayerStatsHandler>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

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
            TickRollCooldown();

            // Клавиша для теста кувырка
            if (Input.GetKeyDown(KeyCode.LeftShift))
                TryRoll();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(ETag.Fairy.ToString()))
            {
                var fairy = other.GetComponent<FairyController>();
                fairy?.DestroyFairy();
                playerStats.RestoreMana(20f);
            }
        }

        #endregion

        #region Movement Methods

        private void UpdateMovement()
        {
            if ((LabyrinthMapController.Instance != null && LabyrinthMapController.Instance.IsMapActive)
                || Input.GetKey(KeyCode.Space))
                return;

            float h = joystick != null ? joystick.Horizontal : Input.GetAxis("Horizontal");
            float v = joystick != null ? joystick.Vertical : Input.GetAxis("Vertical");

            Vector2 dir = new Vector2(h, v);
            if (dir.magnitude > 0.1f)
                lastMoveDirection = dir.normalized;

            float speed = (joystick != null ? joystickSpeed : keyboardSpeed) * currentSlowMultiplier;
            float delta = speed * Time.deltaTime;              // multiply floats first for efficiency
            Vector2 move = dir * delta;
            transform.Translate(move, Space.World);

            if (move == Vector2.zero)
                animator.Play(IdleAnimationName);
            else
            {
                animator.Play(RunAnimationName);
                spriteRenderer.flipX = h > 0f;
            }
        }

        #endregion

        #region Dodge Roll

        /// <summary>Публичный метод для вызова кувырка (из UI или кода)</summary>
        public void TryRoll()
        {
            if (!canRoll || isRolling) return;
            StartCoroutine(RollCoroutine());
        }

        private IEnumerator RollCoroutine()
        {
            isRolling = true;
            canRoll = false;

            // Инициализируем кулдаун
            rollCooldownRemaining = rollCooldown;
            OnRollCooldownChanged?.Invoke(1f);

            // Смена спрайта и блокировка аниматора
            animator.enabled = false;
            originalSprite = spriteRenderer.sprite;
            spriteRenderer.sprite = rollSprite;

            Vector2 dir = lastMoveDirection.normalized;
            float rotSign = dir.x >= 0 ? -1f : 1f;
            float rotSpeed = 360f / rollDuration * rotSign;

            float t = 0f;
            while (t < rollDuration)
            {
                float rollDelta = rollSpeed * Time.deltaTime;      // multiply floats first for efficiency
                transform.Translate(dir * rollDelta, Space.World);
                transform.Rotate(0f, 0f, rotSpeed * Time.deltaTime);
                t += Time.deltaTime;
                yield return null;
            }

            // Сброс состояния
            spriteRenderer.sprite = originalSprite;
            transform.rotation = Quaternion.identity;
            animator.enabled = true;
            isRolling = false;

            // Ждём окончания оставшегося кулдауна
            yield return new WaitForSeconds(rollCooldownRemaining);
            canRoll = true;
        }

        private void TickRollCooldown()
        {
            if (rollCooldownRemaining <= 0f) return;

            rollCooldownRemaining -= Time.deltaTime;
            if (rollCooldownRemaining < 0f) rollCooldownRemaining = 0f;
            OnRollCooldownChanged?.Invoke(rollCooldownRemaining / rollCooldown);
        }

        #endregion

        #region Light Methods

        private void UpdateLightOuterRange()
        {
            if (finishPoint == null || playerLight == null || initialDistance <= 0f) return;
            float dist = Vector2.Distance(transform.position, finishPoint.position);
            float t = 1f - Mathf.Clamp01(dist / initialDistance);
            playerLight.pointLightOuterRadius = Mathf.Lerp(baseLightRange, maxLightRange, t);
        }

        private IEnumerator WaitForFinishMarker()
        {
            while (finishPoint == null)
            {
                var obj = GameObject.FindGameObjectWithTag(ETag.Fairy.ToString());
                if (obj != null)
                {
                    finishPoint = obj.transform;
                    initialDistance = Vector2.Distance(transform.position, finishPoint.position);
                }
                yield return null;
            }
        }

        #endregion

        #region Damage and Effects

        public void TakeDamage(EnemyController enemy)
        {
            if (isImmortal || isRolling) return;
            var stats = enemy.GetComponent<EnemyStatsHandler>();
            playerStats.Health -= stats.Damage;
            if (playerStats.Health <= 0) { Die(); return; }
            if (enemy.pushPlayer)
                EntityUtils.MakeDash(transform, transform.position - enemy.transform.position);
        }

        public void ApplySlow(float factor, float duration)
        {
            if (slowCoroutine != null) StopCoroutine(slowCoroutine);
            slowCoroutine = StartCoroutine(SlowCoroutine(factor, duration));
        }

        private IEnumerator SlowCoroutine(float factor, float duration)
        {
            currentSlowMultiplier = factor;
            yield return new WaitForSeconds(duration);
            currentSlowMultiplier = 1f;
        }

        public void ApplyBinding(float duration) => StartCoroutine(BindingCoroutine(duration));
        private IEnumerator BindingCoroutine(float duration)
        {
            float orig = currentSlowMultiplier;
            currentSlowMultiplier = 0f;
            yield return new WaitForSeconds(duration);
            currentSlowMultiplier = orig;
        }

        public void Stun(float duration) => StartCoroutine(StunCoroutine(duration));
        private IEnumerator StunCoroutine(float duration)
        {
            float orig = currentSlowMultiplier;
            currentSlowMultiplier = 0f;
            yield return new WaitForSeconds(duration);
            currentSlowMultiplier = orig;
        }

        public void IncreaseSpeed(float mult)
        {
            if (bonusActive) return;
            StartCoroutine(SpeedBoostCoroutine(mult, 5f));
        }

        private IEnumerator SpeedBoostCoroutine(float mult, float duration)
        {
            bonusActive = true;
            float orig = joystickSpeed;
            joystickSpeed *= mult;
            yield return new WaitForSeconds(duration);
            joystickSpeed = orig;
            bonusActive = false;
        }

        public void ReceiveDarkSkullHit()
        {
            darkSkullHitCount++;
            if (darkSkullHitCount >= maxDarkSkullHits) Die();
        }

        public void ReceiveTrollHit() => Die();

        private void Die()
        {
            
            var allCanvases = UnityEngine.Object.FindObjectsByType<Canvas>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var canvas in allCanvases)
            {
                canvas.gameObject.SetActive(false);
            }
            Destroy(gameObject);
        }

        #endregion
    }
}
