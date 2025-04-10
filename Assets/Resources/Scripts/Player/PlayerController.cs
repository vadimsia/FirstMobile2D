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
        #region Constants

        private const string IdleAnimationName = "Idle";
        private const string RunAnimationName = "Run";

        #endregion

        #region Inspector Fields

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

        [Header("DarkSkull / Troll Damage Settings")]
        [Tooltip("Максимальное число ударов от DarkSkull, после которых игрок умирает.")]
        [SerializeField]
        private int maxDarkSkullHits = 2;

        #endregion

        #region Private Fields

        private PlayerStatsHandler playerStats;
        private SpriteRenderer spriteRenderer;
        private float currentSlowMultiplier = 1f;
        private Coroutine slowCoroutine;
        private bool bonusActive;
        private float initialDistance = -1f; // Изначальное расстояние от игрока до финиша.
        private int darkSkullHitCount = 0;

        #endregion

        #region Unity Methods

        private void Start()
        {
            playerStats = GetComponent<PlayerStatsHandler>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (finishPoint != null)
            {
                initialDistance = Vector2.Distance(transform.position, finishPoint.position);
            }
            else
            {
                StartCoroutine(WaitForFinishMarker());
            }
        }

        private void Update()
        {
            UpdateMovement();
            UpdateLightOuterRange();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // При столкновении с феей – уничтожаем её и восстанавливаем ману.
            if (other.CompareTag(ETag.Fairy.ToString()))
            {
                FairyController fairy = other.GetComponent<FairyController>();
                fairy?.DestroyFairy();
                playerStats.RestoreMana(20f);
            }
        }

        #endregion

        #region Movement Methods

        /// <summary>
        /// Обновляет перемещение игрока на основе входных данных.
        /// Движение не выполняется, если активна карта лабиринта или зажата клавиша Space.
        /// Также обновляются анимация и переворот спрайта.
        /// </summary>
        private void UpdateMovement()
        {
            if ((LabyrinthMapController.Instance != null && LabyrinthMapController.Instance.IsMapActive) || Input.GetKey(KeyCode.Space))
                return;

            float horizontal = (joystick != null) ? joystick.Horizontal : Input.GetAxis("Horizontal");
            float vertical   = (joystick != null) ? joystick.Vertical   : Input.GetAxis("Vertical");
            float currentSpeed = ((joystick != null) ? joystickSpeed : keyboardSpeed) * currentSlowMultiplier;
            Vector2 movement = new Vector2(horizontal, vertical) * currentSpeed;

            // Перемещаем игрока.
            transform.Translate(movement * Time.deltaTime, Space.World);

            // Обновляем состояние анимации и переворот спрайта.
            if (Mathf.Approximately(horizontal, 0f) && Mathf.Approximately(vertical, 0f))
            {
                animator.Play(IdleAnimationName);
            }
            else
            {
                animator.Play(RunAnimationName);
                spriteRenderer.flipX = horizontal > 0f;
            }
        }

        #endregion

        #region Light Methods

        /// <summary>
        /// Обновляет внешний радиус освещения игрока в зависимости от расстояния до финиша.
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

        #endregion

        #region Utility Methods

        /// <summary>
        /// Ожидает появления финишного маркера в сцене.
        /// </summary>
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

        /// <summary>
        /// Обрабатывает смерть игрока, отключая все UI и уничтожая объект игрока.
        /// </summary>
        private void Die()
        {
            foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                canvas.gameObject.SetActive(false);
            }
            Destroy(gameObject);
        }

        #endregion

        #region Damage and Effects

        /// <summary>
        /// Получает урон от врага и выполняет эффект отталкивания (dash), если активирован.
        /// </summary>
        /// <param name="enemy">Контроллер врага, нанесшего урон.</param>
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
                // Отталкиваем игрока от врага.
                EntityUtils.MakeDash(transform, transform.position - enemy.transform.position);
            }
        }

        /// <summary>
        /// Применяет эффект замедления движения игрока на заданное время.
        /// </summary>
        /// <param name="slowFactor">Множитель замедления.</param>
        /// <param name="duration">Длительность эффекта.</param>
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

        /// <summary>
        /// Применяет эффект связывания (binding), который блокирует движение игрока на заданное время.
        /// Используется, например, при попадании снаряда гоблина.
        /// </summary>
        /// <param name="duration">Длительность связывания в секундах.</param>
        public void ApplyBinding(float duration)
        {
            StartCoroutine(BindingCoroutine(duration));
        }

        private IEnumerator BindingCoroutine(float duration)
        {
            float originalMultiplier = currentSlowMultiplier;
            currentSlowMultiplier = 0f;
            Debug.Log("Player bound for " + duration + " seconds.");
            yield return new WaitForSeconds(duration);
            currentSlowMultiplier = originalMultiplier;
            Debug.Log("Player unbound.");
        }

        /// <summary>
        /// Оглушает игрока на заданное время, полностью блокируя его движение.
        /// Например, при попадании в ловушку.
        /// </summary>
        /// <param name="duration">Длительность оглушения в секундах.</param>
        public void Stun(float duration)
        {
            StartCoroutine(StunCoroutine(duration));
        }

        private IEnumerator StunCoroutine(float duration)
        {
            float originalMultiplier = currentSlowMultiplier;
            currentSlowMultiplier = 0f;
            Debug.Log("Player stunned for " + duration + " seconds.");
            yield return new WaitForSeconds(duration);
            currentSlowMultiplier = originalMultiplier;
            Debug.Log("Player stun ended.");
        }

        /// <summary>
        /// Увеличивает скорость игрока на заданное время.
        /// </summary>
        /// <param name="multiplier">Множитель скорости.</param>
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
        /// Регистрирует попадание от врагов типа DarkSkull.
        /// После достижения предельного количества ударов игрок умирает.
        /// </summary>
        public void ReceiveDarkSkullHit()
        {
            darkSkullHitCount++;
            Debug.Log("Received DarkSkull hit. Count: " + darkSkullHitCount);
            if (darkSkullHitCount >= maxDarkSkullHits)
            {
                Die();
            }
        }

        /// <summary>
        /// Мгновенно убивает игрока при попадании от врагов типа Troll.
        /// </summary>
        public void ReceiveTrollHit()
        {
            Debug.Log("Received Troll hit. Player dies immediately.");
            Die();
        }

        #endregion
    }
}
