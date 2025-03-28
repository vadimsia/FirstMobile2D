using Resources.Scripts.Player;
using UnityEngine;
using System.Collections;

namespace Resources.Scripts.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("Базовая скорость перемещения врага.")]
        public int speed = 1;
        [Tooltip("Множитель для замедления врага.")]
        public float slowMultiplier = 1f;

        [Header("Attack Settings")]
        [Tooltip("Максимальное расстояние для обнаружения игрока.")]
        public float detectionRange = 5f;
        [Tooltip("Расстояние, на котором враг начинает атаку.")]
        public float attackRange = 1f;
        [Tooltip("Интервал между атаками (сек).")]
        public float attackCooldown = 1f;
        private float lastAttackTime;

        [Header("Player Interaction Settings")]
        [Tooltip("Включить отталкивание игрока при контакте.")]
        public bool pushPlayer = true;
        [Tooltip("Сила отталкивания игрока.")]
        public float pushForceMultiplier = 1f;

        [Header("Animation Settings")]
        [Tooltip("Компонент Animator, отвечающий за анимацию врага.")]
        public Animator animator;
        [Tooltip("Название анимации для состояния Idle.")]
        public string idleAnimationName = "Idle";
        [Tooltip("Название анимации для состояния Walk.")]
        public string walkAnimationName = "Walk";
        [Tooltip("Компонент SpriteRenderer для управления направлением спрайта.")]
        public SpriteRenderer spriteRenderer;

        [Header("Enemy Customization Settings")]
        [Tooltip("Имя врага (для отладки и настройки).")]
        public string enemyName = "Enemy";

        [Header("Debug Settings")]
        [Tooltip("Включить вывод отладочной информации в консоль.")]
        public bool debugLog;

        private float currentSpeed;
        private Coroutine slowCoroutine;
        private Rigidbody2D rb;
        private PlayerController player;
        private bool isChasing;

        private void Start()
        {
            // Поиск игрока по тегу "Player"
            player = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
            currentSpeed = speed;
            rb = GetComponent<Rigidbody2D>();

            // Если ссылки на компоненты не установлены через инспектор, попробуем получить их автоматически
            if (animator == null)
                animator = GetComponent<Animator>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            FollowPlayer();
            AnimateMovement();
        }

        /// <summary>
        /// Перемещает врага к игроку, если игрок находится в пределах detectionRange.
        /// При приближении на расстояние attackRange враг атакует игрока.
        /// </summary>
        private void FollowPlayer()
        {
            if (player == null)
            {
                // Если игрок отсутствует, уничтожаем врага
                Destroy(gameObject);
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            // Если игрок за пределами обнаружения, не преследуем
            if (distanceToPlayer > detectionRange)
            {
                isChasing = false;
                return;
            }

            // Преследуем игрока
            isChasing = true;
            // Используем Lerp для плавного перемещения
            transform.position = Vector3.Lerp(transform.position, player.transform.position, Time.deltaTime * currentSpeed);

            // Атака, если игрок в пределах атаки и прошло время перезарядки
            if (distanceToPlayer <= attackRange && Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time;
                player.TakeDamage(this);

                if (debugLog)
                {
                    Debug.Log(enemyName + " атаковал игрока.");
                }

                // Отталкивание игрока
                if (pushPlayer && player.TryGetComponent<Rigidbody2D>(out Rigidbody2D playerRb))
                {
                    Vector2 pushDirection = (player.transform.position - transform.position).normalized;
                    playerRb.AddForce(pushDirection * pushForceMultiplier, ForceMode2D.Impulse);
                }
            }
        }

        /// <summary>
        /// Обновляет анимацию врага и его ориентацию в зависимости от направления движения.
        /// </summary>
        private void AnimateMovement()
        {
            if (animator != null)
            {
                if (isChasing)
                {
                    animator.Play(walkAnimationName);
                }
                else
                {
                    animator.Play(idleAnimationName);
                }
            }

            // Если игрок преследуется, определяем направление движения относительно игрока.
            if (spriteRenderer != null && player != null)
            {
                float direction = player.transform.position.x - transform.position.x;
                if (direction > 0) // Игрок справа — двигаемся вправо
                {
                    spriteRenderer.flipX = true;
                }
                else if (direction < 0) // Игрок слева — двигаемся влево
                {
                    spriteRenderer.flipX = false;
                }
            }
        }

        /// <summary>
        /// Применяет эффект замедления к врагу на заданный период времени.
        /// </summary>
        /// <param name="slowFactor">Коэффициент замедления (например, 0.5 — половина скорости).</param>
        /// <param name="duration">Длительность замедления в секундах.</param>
        public void ApplySlow(float slowFactor, float duration)
        {
            if (slowCoroutine != null)
                StopCoroutine(slowCoroutine);
            slowCoroutine = StartCoroutine(SlowEffect(slowFactor, duration));
        }

        /// <summary>
        /// Корутина для обработки эффекта замедления.
        /// </summary>
        private IEnumerator SlowEffect(float slowFactor, float duration)
        {
            currentSpeed = speed * slowFactor;
            yield return new WaitForSeconds(duration);
            currentSpeed = speed;
        }

        /// <summary>
        /// Применяет импульс к врагу.
        /// </summary>
        /// <param name="force">Вектор силы импульса.</param>
        public void ApplyPush(Vector2 force)
        {
            if (rb != null)
                rb.AddForce(force, ForceMode2D.Impulse);
        }
    }
}
