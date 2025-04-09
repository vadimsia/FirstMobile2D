using Resources.Scripts.Player;
using UnityEngine;
using System.Collections;

namespace Resources.Scripts.Enemy
{
    public enum EnemyType
    {
        Standard,   // базовая атака (например, удар при контакте)
        Goblin,     // атака снарядом – попадание определяется столкновением снаряда
        DarkSkull,  // удар, попадание регистрируется через Animation Event
        Troll       // удар, попадание регистрируется через Animation Event
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
        [Tooltip("Длительность привязки снаряда (сек).")]
        public float bindingDuration = 0.5f;
        [Tooltip("Скорость полёта снаряда гоблина.")]
        public float projectileSpeed = 5f;
        [Tooltip("Время жизни снаряда (сек).")]
        public float goblinProjectileLifeTime = 5f;
        [Tooltip("Угол случайного разброса направления снаряда (в градусах).")]
        public float goblinProjectileSpreadAngle = 0f;
        [Tooltip("Масштаб (размер) снаряда.")]
        public Vector3 goblinProjectileScale = Vector3.one;
        [Tooltip("Урон снаряда (если применяется, 0 – не наносит дополнительного урона).")]
        public float goblinProjectileDamage = 0f;
        [Tooltip("Префаб снаряда для гоблина (должен содержать компонент GoblinProjectile, SpriteRenderer и Collider2D с isTrigger = true).")]
        public GameObject projectilePrefab;
        [Tooltip("Точка, из которой появляется снаряд гоблина.")]
        public Transform attackPoint;

        [Header("DarkSkull Attack Settings")]
        [Tooltip("Длительность кулдауна между ударами DarkSkull.")]
        public float darkSkullAttackCooldownTime = 1f;
        [Tooltip("Сила отталкивания игрока ударом DarkSkull.")]
        public float darkSkullPushForce = 5f;

        [Header("Troll Attack Settings")]
        [Tooltip("Длительность кулдауна между ударами Troll.")]
        public float trollAttackCooldownTime = 1f;
        [Tooltip("Сила отталкивания игрока ударом Troll.")]
        public float trollPushForce = 5f;

        [Header("Animation Settings")]
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

        [Header("Animation Durations")]
        [Tooltip("Длительность анимации атаки гоблина (сек).")]
        public float goblinAttackAnimationDuration = 1f;
        [Tooltip("Длительность анимации удара DarkSkull (сек).")]
        public float darkSkullAttackAnimationDuration = 1f;
        [Tooltip("Длительность анимации удара Troll (сек).")]
        public float trollAttackAnimationDuration = 1f;

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
            player = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
            currentSpeed = speed;
            rb = GetComponent<Rigidbody2D>();

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            // По умолчанию враг может находиться в состоянии ожидания (Idle)
            if (animator != null)
                animator.Play(idleAnimationName);
        }

        private void Update()
        {
            if (player == null)
            {
                Destroy(gameObject);
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            // Если игрок вне зоны обнаружения – враг преследует игрока.
            if (distanceToPlayer > detectionRange)
            {
                isAttacking = false;
                MoveTowardsPlayer();
                return;
            }

            // Выбор логики атаки в зависимости от типа врага.
            switch (enemyType)
            {
                case EnemyType.Goblin:
                    if (distanceToPlayer <= attackRange)
                    {
                        if (!isAttacking && Time.time - lastAttackTime >= goblinAttackCooldownTime)
                            StartCoroutine(PerformGoblinAttack());
                    }
                    else
                    {
                        isAttacking = false;
                        MoveTowardsPlayer();
                    }
                    break;

                case EnemyType.DarkSkull:
                    if (distanceToPlayer <= attackRange)
                    {
                        if (!isAttacking && Time.time - lastAttackTime >= darkSkullAttackCooldownTime)
                            StartCoroutine(PerformDarkSkullAttack());
                    }
                    else
                    {
                        isAttacking = false;
                        MoveTowardsPlayer();
                    }
                    break;

                case EnemyType.Troll:
                    if (distanceToPlayer <= attackRange)
                    {
                        if (!isAttacking && Time.time - lastAttackTime >= trollAttackCooldownTime)
                            StartCoroutine(PerformTrollAttack());
                    }
                    else
                    {
                        isAttacking = false;
                        MoveTowardsPlayer();
                    }
                    break;

                case EnemyType.Standard:
                default:
                    if (distanceToPlayer <= attackRange)
                    {
                        if (Time.time - lastAttackTime >= attackCooldown)
                        {
                            lastAttackTime = Time.time;
                            player.TakeDamage(this);
                            if (debugLog)
                                Debug.Log(enemyName + " атаковал игрока.");
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
                    break;
            }
        }

        /// <summary>
        /// Перемещает врага к игроку с плавным движением и проигрыванием анимации ходьбы.
        /// </summary>
        private void MoveTowardsPlayer()
        {
            // При движении проигрываем анимацию ходьбы
            if (animator != null)
                animator.Play(walkAnimationName);

            transform.position = Vector3.Lerp(transform.position, player.transform.position, Time.deltaTime * currentSpeed);

            if (spriteRenderer != null)
            {
                float direction = player.transform.position.x - transform.position.x;
                spriteRenderer.flipX = direction > 0;
            }
        }

        /// <summary>
        /// Выполняет атаку гоблина: проигрывается анимация атаки (Attack),
        /// после чего через Animation Event вызывается SpawnProjectileEvent, который создаст снаряд.
        /// После окончания анимации движение врага восстанавливается.
        /// </summary>
        private IEnumerator PerformGoblinAttack()
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            float originalSpeed = currentSpeed;
            currentSpeed = 0;

            if (animator != null)
                animator.Play(attackAnimationName);

            // Ожидаем время анимации атаки гоблина, в этот момент должен сработать Animation Event,
            // который вызовет метод SpawnProjectileEvent.
            yield return new WaitForSeconds(goblinAttackAnimationDuration);

            currentSpeed = originalSpeed;
            isAttacking = false;
        }

        /// <summary>
        /// Выполняет атаку DarkSkull: проигрывается анимация атаки,
        /// а в нужный момент (через Animation Event) вызывается RegisterDarkSkullHitEvent.
        /// После окончания анимации движение врага восстанавливается.
        /// </summary>
        private IEnumerator PerformDarkSkullAttack()
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            float originalSpeed = currentSpeed;
            currentSpeed = 0;

            if (animator != null)
                animator.Play(attackAnimationName);

            yield return new WaitForSeconds(darkSkullAttackAnimationDuration);

            currentSpeed = originalSpeed;
            isAttacking = false;
        }

        /// <summary>
        /// Выполняет атаку Troll: проигрывается анимация атаки,
        /// а в нужный момент (через Animation Event) вызывается RegisterTrollHitEvent.
        /// После окончания анимации движение врага восстанавливается.
        /// </summary>
        private IEnumerator PerformTrollAttack()
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            float originalSpeed = currentSpeed;
            currentSpeed = 0;

            if (animator != null)
                animator.Play(attackAnimationName);

            yield return new WaitForSeconds(trollAttackAnimationDuration);

            currentSpeed = originalSpeed;
            isAttacking = false;
        }

        #region Animation Event Methods

        /// <summary>
        /// Метод для создания снаряда гоблина, вызываемый через Animation Event в клипе атаки.
        /// Снаряд создаётся и получает все необходимые параметры.
        /// </summary>
        public void SpawnProjectileEvent()
        {
            Vector3 spawnPosition = (attackPoint != null) ? attackPoint.position : transform.position;
            Vector2 attackDirection = (player.transform.position - spawnPosition).normalized;

            if (goblinProjectileSpreadAngle > 0f)
            {
                float randomAngle = Random.Range(-goblinProjectileSpreadAngle, goblinProjectileSpreadAngle);
                attackDirection = Quaternion.Euler(0, 0, randomAngle) * attackDirection;
            }

            if (projectilePrefab != null)
            {
                GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
                projectile.transform.localScale = goblinProjectileScale;
                GoblinProjectile goblinProjectile = projectile.GetComponent<GoblinProjectile>();
                if (goblinProjectile != null)
                {
                    goblinProjectile.SetParameters(attackDirection, projectileSpeed, bindingDuration, goblinProjectileLifeTime, goblinProjectileDamage);
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
        }

        /// <summary>
        /// Метод, вызываемый через Animation Event для атаки DarkSkull.
        /// Регистрирует попадание и отталкивание игрока.
        /// </summary>
        public void RegisterDarkSkullHitEvent()
        {
            player.ReceiveDarkSkullHit();
            if (pushPlayer && player.TryGetComponent<Rigidbody2D>(out Rigidbody2D playerRb))
            {
                Vector2 pushDirection = (player.transform.position - transform.position).normalized;
                playerRb.AddForce(pushDirection * darkSkullPushForce, ForceMode2D.Impulse);
            }
        }

        /// <summary>
        /// Метод, вызываемый через Animation Event для атаки Troll.
        /// Регистрирует попадание и отталкивание игрока.
        /// </summary>
        public void RegisterTrollHitEvent()
        {
            player.ReceiveTrollHit();
            if (pushPlayer && player.TryGetComponent<Rigidbody2D>(out Rigidbody2D playerRb))
            {
                Vector2 pushDirection = (player.transform.position - transform.position).normalized;
                playerRb.AddForce(pushDirection * trollPushForce, ForceMode2D.Impulse);
            }
        }

        #endregion

        /// <summary>
        /// Применяет эффект замедления к врагу на заданное время.
        /// </summary>
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
        public void ApplyPush(Vector2 force)
        {
            if (rb != null)
                rb.AddForce(force, ForceMode2D.Impulse);
        }
    }
}
