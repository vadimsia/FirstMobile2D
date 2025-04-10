using Resources.Scripts.Player;
using UnityEngine;
using System.Collections;

namespace Resources.Scripts.Enemy
{
    public enum EnemyType
    {
        Standard,
        Goblin,
        DarkSkull,
        Troll
    }

    public class EnemyController : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Common Settings")]
        [Tooltip("Имя врага для отладки и логирования.")]
        public string enemyName = "Enemy";
        public EnemyType enemyType = EnemyType.Standard;

        [Header("Movement Settings")]
        [Tooltip("Базовая скорость передвижения врага.")]
        public float speed = 1f;
        [Tooltip("Множитель замедления (при наложении эффекта).")]
        public float slowMultiplier = 1f;
        [Tooltip("Дальность обнаружения игрока.")]
        public float detectionRange = 5f;
        [Tooltip("Дальность атаки.")]
        public float attackRange = 1f;

        [Header("Patrol Settings")]
        [Tooltip("Радиус патрулирования относительно точки спавна.")]
        public float patrolRadius = 3f;
        [Tooltip("Множитель скорости патрулирования.")]
        public float patrolSpeedMultiplier = 0.5f;

        [Header("Standard Attack Settings")]
        [Tooltip("Время между ударами стандартного врага.")]
        public float attackCooldown = 1f;
        [Tooltip("Нужно ли отталкивать игрока при атаке.")]
        public bool pushPlayer = true;
        [Tooltip("Сила отталкивания игрока.")]
        public float pushForceMultiplier = 1f;

        [Header("Goblin Attack Settings")]
        [Tooltip("Время между выстрелами гоблина.")]
        public float goblinAttackCooldownTime = 2f;
        [Tooltip("Длительность эффекта связывания при попадании снаряда.")]
        public float bindingDuration = 0.5f;
        [Tooltip("Скорость полёта снаряда.")]
        public float projectileSpeed = 5f;
        [Tooltip("Время жизни снаряда.")]
        public float goblinProjectileLifeTime = 5f;
        [Tooltip("Угол разброса снаряда.")]
        public float goblinProjectileSpreadAngle = 0f;
        [Tooltip("Масштаб снаряда.")]
        public Vector3 goblinProjectileScale = Vector3.one;
        [Tooltip("Урон снаряда гоблина.")]
        public float goblinProjectileDamage = 0f;
        [Tooltip("Префаб снаряда гоблина.")]
        public GameObject projectilePrefab;
        [Tooltip("Точка, из которой происходит атака (выстрел).")]
        public Transform attackPoint;

        [Header("DarkSkull Attack Settings")]
        [Tooltip("Время между ударами DarkSkull.")]
        public float darkSkullAttackCooldownTime = 1f;
        [Tooltip("Сила отталкивания игрока при атаке DarkSkull.")]
        public float darkSkullPushForce = 5f;
        [Tooltip("Длительность анимации атаки DarkSkull.")]
        public float darkSkullAttackAnimationDuration = 1f;

        [Header("Troll Attack Settings")]
        [Tooltip("Время между ударами Troll.")]
        public float trollAttackCooldownTime = 1f;
        [Tooltip("Сила отталкивания игрока при атаке Troll.")]
        public float trollPushForce = 5f;
        [Tooltip("Длительность анимации атаки Troll.")]
        public float trollAttackAnimationDuration = 1f;

        [Header("Animation Settings")]
        public Animator animator;
        [Tooltip("Имя анимации состояния ожидания.")]
        public string idleAnimationName = "Idle";
        [Tooltip("Имя анимации бега (идентично при патрулировании или преследовании).")]
        public string walkAnimationName = "Walk";
        [Tooltip("Имя анимации атаки.")]
        public string attackAnimationName = "Attack";
        public SpriteRenderer spriteRenderer;

        [Header("Animation Durations")]
        [Tooltip("Длительность анимации атаки гоблина.")]
        public float goblinAttackAnimationDuration = 1f;

        [Header("Debug Settings")]
        public bool debugLog;
        #endregion

        #region Private Fields
        private float currentSpeed;
        private Rigidbody2D rb;
        private PlayerController player;
        private bool isAttacking;
        private float lastAttackTime;
        private Vector3 spawnPoint;
        private Vector3 patrolTarget;
        #endregion

        #region Unity Methods
        private void Start()
        {
            spawnPoint = transform.position;
    
            
    
            player = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
            currentSpeed = speed;
            rb = GetComponent<Rigidbody2D>();

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (animator != null)
                animator.Play(idleAnimationName);

            patrolTarget = GetRandomPatrolPoint();
        }


        private void Update()
        {
            // Если игрок не найден, враг патрулирует
            if (player == null)
            {
                Patrol();
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            // Если враг может атаковать (находится в зоне атаки) – не приближается далее
            if (distanceToPlayer <= attackRange)
            {
                AttackPlayer();
            }
            else if (distanceToPlayer <= detectionRange)
            {
                ChasePlayer();
            }
            else
            {
                Patrol();
            }
        }
        #endregion

        #region Movement Methods
        /// <summary>
        /// Выполняет плавное преследование игрока.
        /// </summary>
        private void ChasePlayer()
        {
            if (!isAttacking)
            {
                if (animator != null && !animator.GetCurrentAnimatorStateInfo(0).IsName(walkAnimationName))
                    animator.Play(walkAnimationName);
            }
            
            TurnToTarget(player.transform.position);
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
        }

        /// <summary>
        /// Выполняет патрулирование вокруг точки спавна.
        /// </summary>
        private void Patrol()
        {
            if (!isAttacking)
            {
                if (animator != null && !animator.GetCurrentAnimatorStateInfo(0).IsName(walkAnimationName))
                    animator.Play(walkAnimationName);
            }
            
            TurnToTarget(patrolTarget);
            float patrolSpeed = speed * patrolSpeedMultiplier;
            transform.position = Vector3.MoveTowards(transform.position, patrolTarget, patrolSpeed * Time.deltaTime);

            // Если достигнута цель патрулирования – выбираем новую
            if (Vector3.Distance(transform.position, patrolTarget) < 0.1f)
            {
                patrolTarget = GetRandomPatrolPoint();
            }
        }

        /// <summary>
        /// Поворачивает врага в сторону указанной цели.
        /// </summary>
        private void TurnToTarget(Vector3 targetPosition)
        {
            float deltaX = targetPosition.x - transform.position.x;
            transform.eulerAngles = (deltaX < 0) ? new Vector3(0, 0, 0) : new Vector3(0, 180, 0);
        }

        /// <summary>
        /// Возвращает случайную точку для патрулирования вокруг точки спавна.
        /// </summary>
        private Vector3 GetRandomPatrolPoint()
        {
            Vector2 randomPoint = Random.insideUnitCircle * patrolRadius;
            return spawnPoint + new Vector3(randomPoint.x, randomPoint.y, 0);
        }
        #endregion

        #region Attack Methods
        /// <summary>
        /// Выбирает тип атаки врага в зависимости от его типа.
        /// </summary>
        private void AttackPlayer()
        {
            if (isAttacking) return;
            float timeSinceLastAttack = Time.time - lastAttackTime;

            switch (enemyType)
            {
                case EnemyType.Goblin:
                    if (timeSinceLastAttack >= goblinAttackCooldownTime)
                        StartCoroutine(PerformGoblinAttack());
                    break;
                case EnemyType.DarkSkull:
                    if (timeSinceLastAttack >= darkSkullAttackCooldownTime)
                        StartCoroutine(PerformDarkSkullAttack());
                    break;
                case EnemyType.Troll:
                    if (timeSinceLastAttack >= trollAttackCooldownTime)
                        StartCoroutine(PerformTrollAttack());
                    break;
                case EnemyType.Standard:
                default:
                    if (timeSinceLastAttack >= attackCooldown)
                        StartCoroutine(PerformStandardAttack());
                    break;
            }
        }

        /// <summary>
        /// Выполняет стандартную атаку (ближний бой).
        /// </summary>
        private IEnumerator PerformStandardAttack()
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            float originalSpeed = speed;
            speed = 0f;

            if (animator != null)
                animator.Play(attackAnimationName);

            yield return new WaitForSeconds(attackCooldown);

            // Наносим урон игроку
            player.TakeDamage(this);
            if (debugLog)
                Debug.Log($"{enemyName} атаковал игрока стандартным ударом.");

            if (pushPlayer && player.TryGetComponent<Rigidbody2D>(out Rigidbody2D playerRb))
            {
                Vector2 pushDirection = (player.transform.position - transform.position).normalized;
                playerRb.AddForce(pushDirection * pushForceMultiplier, ForceMode2D.Impulse);
            }

            speed = originalSpeed;
            isAttacking = false;
            UpdateAnimationState(); // Обновляем анимацию после атаки
        }

        /// <summary>
        /// Выполняет атаку гоблина с выстрелом снаряда.
        /// </summary>
        private IEnumerator PerformGoblinAttack()
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            float originalSpeed = speed;
            speed = 0f;

            if (animator != null)
                animator.Play(attackAnimationName);

            yield return new WaitForSeconds(goblinAttackAnimationDuration);

            SpawnProjectileEvent();

            speed = originalSpeed;
            isAttacking = false;
            UpdateAnimationState(); // Обновляем анимацию
        }

        /// <summary>
        /// Выполняет атаку DarkSkull (удар с отталкиванием).
        /// </summary>
        private IEnumerator PerformDarkSkullAttack()
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            float originalSpeed = speed;
            speed = 0f;

            if (animator != null)
                animator.Play(attackAnimationName);

            yield return new WaitForSeconds(darkSkullAttackAnimationDuration);

            RegisterDarkSkullHitEvent();

            speed = originalSpeed;
            isAttacking = false;
            UpdateAnimationState();
        }

        /// <summary>
        /// Выполняет атаку Troll (удар с отталкиванием).
        /// </summary>
        private IEnumerator PerformTrollAttack()
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            float originalSpeed = speed;
            speed = 0f;

            if (animator != null)
                animator.Play(attackAnimationName);

            yield return new WaitForSeconds(trollAttackAnimationDuration);

            RegisterTrollHitEvent();

            speed = originalSpeed;
            isAttacking = false;
            UpdateAnimationState();
        }
        #endregion

        #region Animation Event Methods
        /// <summary>
        /// Метод вызывается для создания снаряда у гоблина.
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
                Debug.LogWarning($"Префаб снаряда не назначен для {enemyName}");
            }
        }

        /// <summary>
        /// Метод для регистрации попадания DarkSkull.
        /// </summary>
        public void RegisterDarkSkullHitEvent()
        {
            if (player != null)
            {
                player.ReceiveDarkSkullHit();
                if (pushPlayer && player.TryGetComponent<Rigidbody2D>(out Rigidbody2D playerRb))
                {
                    Vector2 pushDirection = (player.transform.position - transform.position).normalized;
                    playerRb.AddForce(pushDirection * darkSkullPushForce, ForceMode2D.Impulse);
                }
            }
        }

        /// <summary>
        /// Метод для регистрации попадания Troll.
        /// </summary>
        public void RegisterTrollHitEvent()
        {
            if (player != null)
            {
                player.ReceiveTrollHit();
                if (pushPlayer && player.TryGetComponent<Rigidbody2D>(out Rigidbody2D playerRb))
                {
                    Vector2 pushDirection = (player.transform.position - transform.position).normalized;
                    playerRb.AddForce(pushDirection * trollPushForce, ForceMode2D.Impulse);
                }
            }
        }
        #endregion

        #region Effects
        /// <summary>
        /// Внешний вызов для применения эффекта замедления.
        /// </summary>
        public void ApplySlow(float slowFactor, float duration)
        {
            StartCoroutine(SlowEffect(slowFactor, duration));
        }

        /// <summary>
        /// Корутина, реализующая эффект замедления.
        /// </summary>
        private IEnumerator SlowEffect(float slowFactor, float duration)
        {
            float originalSpeed = speed;
            speed = originalSpeed * slowFactor;
            yield return new WaitForSeconds(duration);
            speed = originalSpeed;
        }

        /// <summary>
        /// Внешний вызов для применения отталкивания.
        /// </summary>
        public void ApplyPush(Vector2 force)
        {
            if (rb != null)
                rb.AddForce(force, ForceMode2D.Impulse);
        }
        #endregion

        #region Animation State Update
        /// <summary>
        /// Обновляет состояние анимации в зависимости от текущего поведения.
        /// Если враг не атакует, выбирается анимация Walk, если движется, или Idle, если стоит.
        /// </summary>
        private void UpdateAnimationState()
        {
            if (player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
                // Если игрок в зоне обнаружения – проверяем, движемся ли мы (Chase) или уже в зоне атаки
                if (distanceToPlayer <= detectionRange)
                {
                    // Если враг находится в зоне атаки, можно переключить в Idle (например, если атака завершилась)
                    if (distanceToPlayer <= attackRange)
                        animator.Play(idleAnimationName);
                    else
                        animator.Play(walkAnimationName);
                }
                else
                {
                    animator.Play(idleAnimationName);
                }
            }
            else
            {
                // Если игрок не найден – используем анимацию патрулирования (Walk)
                animator.Play(walkAnimationName);
            }
        }
        #endregion
    }
}
