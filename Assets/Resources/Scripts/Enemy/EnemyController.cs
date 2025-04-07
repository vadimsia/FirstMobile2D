using Resources.Scripts.Player;
using UnityEngine;
using System.Collections;

namespace Resources.Scripts.Enemy
{
    public enum EnemyType
    {
        Standard,   // базовая атака (например, удар при контакте)
        Goblin,     // связывающая атака снарядом
        DarkSkull,  // можно добавить свою логику
        Troll       // можно добавить свою логику
    }

    public class EnemyController : MonoBehaviour
    {
        [Header("Common Settings")]
        [Tooltip("Имя врага (для отладки).")]
        public string enemyName = "Enemy";
        [Tooltip("Тип врага.")]
        public EnemyType enemyType = EnemyType.Standard;

        [Header("Movement Settings")]
        [Tooltip("Базовая скорость перемещения врага.")]
        public int speed = 1;
        [Tooltip("Множитель для замедления врага.")]
        public float slowMultiplier = 1f;
        [Tooltip("Максимальное расстояние для обнаружения игрока.")]
        public float detectionRange = 5f;
        [Tooltip("Расстояние, на котором враг начинает атаку.")]
        public float attackRange = 1f;

        [Header("Standard Attack Settings")]
        [Tooltip("Интервал между атаками (сек) для стандартного типа.")]
        public float attackCooldown = 1f;
        [Tooltip("Включить отталкивание игрока при контакте.")]
        public bool pushPlayer = true;
        [Tooltip("Сила отталкивания игрока.")]
        public float pushForceMultiplier = 1f;

        [Header("Goblin Attack Settings")]
        [Tooltip("Длительность кулдауна между атаками гоблина.")]
        public float goblinAttackCooldownTime = 2f;
        [Tooltip("Время связывания игрока при попадании снаряда.")]
        public float bindingDuration = 2f;
        [Tooltip("Скорость полёта снаряда гоблина.")]
        public float projectileSpeed = 5f;
        [Tooltip("Префаб снаряда для гоблина (должен содержать GoblinProjectile, SpriteRenderer и Collider2D с isTrigger = true).")]
        public GameObject projectilePrefab;
        [Tooltip("Точка, из которой появляется снаряд гоблина.")]
        public Transform attackPoint;

        // Если анимации не используются, переменные ниже можно удалить или оставить для будущего расширения.
        [Header("Animation Settings (не используется)")]
        [Tooltip("Компонент Animator, отвечающий за анимацию врага.")]
        public Animator animator;
        [Tooltip("Название анимации для состояния Idle.")]
        public string idleAnimationName = "Idle";
        [Tooltip("Название анимации для состояния Walk.")]
        public string walkAnimationName = "Walk";
        [Tooltip("Название анимации для атаки.")]
        public string attackAnimationName = "Attack";
        [Tooltip("Компонент SpriteRenderer для управления направлением спрайта.")]
        public SpriteRenderer spriteRenderer;

        [Header("Debug Settings")]
        [Tooltip("Включить вывод отладочной информации в консоль.")]
        public bool debugLog;

        private float currentSpeed;
        private Rigidbody2D rb;
        private PlayerController player;
        private bool isAttacking;
        private float lastAttackTime;

        private void Start()
        {
            // Поиск игрока по тегу "Player"
            player = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
            currentSpeed = speed;
            rb = GetComponent<Rigidbody2D>();

            // Если ссылки на компоненты не установлены через инспектор, пробуем получить их автоматически
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (player == null)
            {
                Destroy(gameObject);
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            // Если игрок вне зоны обнаружения – враг движется к игроку
            if (distanceToPlayer > detectionRange)
            {
                isAttacking = false;
                MoveTowardsPlayer();
                return;
            }

            // Если игрок в зоне атаки – выбираем тип атаки
            if (enemyType == EnemyType.Goblin)
            {
                // Гоблин останавливается и атакует, если игрок в пределах attackRange
                if (distanceToPlayer <= attackRange)
                {
                    if (!isAttacking && Time.time - lastAttackTime >= goblinAttackCooldownTime)
                    {
                        StartCoroutine(PerformGoblinAttack());
                    }
                }
                else
                {
                    isAttacking = false;
                    MoveTowardsPlayer();
                }
            }
            else
            {
                // Стандартная атака (ближний бой)
                if (distanceToPlayer <= attackRange)
                {
                    if (Time.time - lastAttackTime >= attackCooldown)
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
                else
                {
                    MoveTowardsPlayer();
                }
            }
        }

        /// <summary>
        /// Перемещает врага к игроку с использованием плавного движения.
        /// </summary>
        private void MoveTowardsPlayer()
        {
            transform.position = Vector3.Lerp(transform.position, player.transform.position, Time.deltaTime * currentSpeed);
            // Обновляем направление спрайта (если используется)
            if (spriteRenderer != null)
            {
                float direction = player.transform.position.x - transform.position.x;
                spriteRenderer.flipX = direction > 0;
            }
        }

        /// <summary>
        /// Выполняет атаку гоблина:
        /// враг останавливается, создаёт снаряд, который летит в направлении, рассчитанном по положению игрока в момент атаки.
        /// </summary>
        private IEnumerator PerformGoblinAttack()
        {
            isAttacking = true;
            lastAttackTime = Time.time;

            // Остановка движения на время атаки
            float originalSpeed = currentSpeed;
            currentSpeed = 0;

            // Задаём позицию для спауна снаряда: если attackPoint не назначен, используем позицию врага
            Vector3 spawnPosition = (attackPoint != null) ? attackPoint.position : transform.position;
            // Направление рассчитывается по позиции игрока в момент атаки
            Vector2 attackDirection = (player.transform.position - spawnPosition).normalized;

            // Создаём снаряд
            if (projectilePrefab != null)
            {
                GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
                GoblinProjectile goblinProjectile = projectile.GetComponent<GoblinProjectile>();
                if (goblinProjectile != null)
                {
                    goblinProjectile.SetParameters(attackDirection, projectileSpeed, bindingDuration);
                }
                else if (projectile.TryGetComponent<Rigidbody2D>(out Rigidbody2D projRb))
                {
                    projRb.AddForce(attackDirection * projectileSpeed, ForceMode2D.Impulse);
                }
            }
            else
            {
                Debug.LogWarning("Префаб снаряда не назначен для " + enemyName);
            }

            // Ждём время кулдауна атаки
            yield return new WaitForSeconds(goblinAttackCooldownTime);

            // Возобновление движения
            currentSpeed = originalSpeed;
            isAttacking = false;
        }

        /// <summary>
        /// Применяет эффект замедления к врагу на заданное время.
        /// </summary>
        /// <param name="slowFactor">Коэффициент замедления (например, 0.5 — половина скорости).</param>
        /// <param name="duration">Длительность замедления в секундах.</param>
        public void ApplySlow(float slowFactor, float duration)
        {
            StartCoroutine(SlowEffect(slowFactor, duration));
        }

        private IEnumerator SlowEffect(float slowFactor, float duration)
        {
            float originalSpeed = currentSpeed;
            currentSpeed = speed * slowFactor;
            yield return new WaitForSeconds(duration);
            currentSpeed = originalSpeed;
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
