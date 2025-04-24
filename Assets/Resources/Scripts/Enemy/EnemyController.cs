// Resources/Scripts/Enemy/EnemyController.cs
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

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
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

        [Header("Patrol Settings")]
        [Tooltip("Радиус патрулирования относительно точки спавна.")]
        public float patrolRadius = 3f;

        [Tooltip("Множитель скорости патрулирования.")]
        public float patrolSpeedMultiplier = 0.5f;

        [Header("Standard Attack Settings")]
        [Tooltip("Дальность атаки стандартного врага.")]
        public float attackRange = 1f;

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
        public float goblinProjectileSpreadAngle;

        [Tooltip("Масштаб снаряда.")]
        public Vector3 goblinProjectileScale = Vector3.one;

        [Tooltip("Урон снаряда гоблина.")]
        public float goblinProjectileDamage;

        [Tooltip("Префаб снаряда гоблина.")]
        public GameObject projectilePrefab;

        [Tooltip("Точка, из которой происходит атака (выстрел).")]
        public Transform attackPoint;

        [Tooltip("Длительность анимации атаки гоблина.")]
        public float goblinAttackAnimationDuration = 1f;

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

        [Tooltip("Имя анимации бега.")]
        public string walkAnimationName = "Walk";

        [Tooltip("Имя анимации атаки.")]
        public string attackAnimationName = "Attack";

        public SpriteRenderer spriteRenderer;

        [Header("Debug Settings")]
        public bool debugLog;

        #endregion

        #region Private Fields

        
        private Rigidbody2D rb;
        private PlayerController player;
        private bool isAttacking;
        private float lastAttackTime;
        private Vector3 spawnPoint;
        private Vector3 patrolTarget;
        private bool isPlayerInTrigger;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            var bc = GetComponent<BoxCollider2D>();
            bc.isTrigger = true;
        }

        private void Start()
        {
            spawnPoint = transform.position;
            
            player = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (animator != null)
                animator.Play(idleAnimationName);

            patrolTarget = GetRandomPatrolPoint();
        }

        private void Update()
        {
            if (player == null)
            {
                Patrol();
                return;
            }

            float dist = Vector3.Distance(transform.position, player.transform.position);

            if (enemyType == EnemyType.DarkSkull || enemyType == EnemyType.Troll)
            {
                if (isPlayerInTrigger)
                    AttackPlayer();
                else if (dist <= detectionRange)
                    ChasePlayer();
                else
                    Patrol();
            }
            else
            {
                if (dist <= attackRange)
                    AttackPlayer();
                else if (dist <= detectionRange)
                    ChasePlayer();
                else
                    Patrol();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((enemyType == EnemyType.DarkSkull || enemyType == EnemyType.Troll)
                && other.CompareTag("Player"))
            {
                isPlayerInTrigger = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if ((enemyType == EnemyType.DarkSkull || enemyType == EnemyType.Troll)
                && other.CompareTag("Player"))
            {
                isPlayerInTrigger = false;
            }
        }

        #endregion

        #region Movement Methods

        private void ChasePlayer()
        {
            if (!isAttacking && animator != null
                             && !animator.GetCurrentAnimatorStateInfo(0).IsName(walkAnimationName))
                animator.Play(walkAnimationName);

            TurnToTarget(player.transform.position);
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.transform.position,
                speed * Time.deltaTime);
        }

        private void Patrol()
        {
            if (!isAttacking && animator != null
                             && !animator.GetCurrentAnimatorStateInfo(0).IsName(walkAnimationName))
                animator.Play(walkAnimationName);

            TurnToTarget(patrolTarget);
            transform.position = Vector3.MoveTowards(
                transform.position,
                patrolTarget,
                speed * patrolSpeedMultiplier * Time.deltaTime);

            if (Vector3.Distance(transform.position, patrolTarget) < 0.1f)
                patrolTarget = GetRandomPatrolPoint();
        }

        private void TurnToTarget(Vector3 targetPosition)
        {
            float dx = targetPosition.x - transform.position.x;
            transform.eulerAngles = dx < 0
                ? new Vector3(0, 0, 0)
                : new Vector3(0, 180, 0);
        }

        private Vector3 GetRandomPatrolPoint()
        {
            Vector2 rnd = Random.insideUnitCircle * patrolRadius;
            return spawnPoint + new Vector3(rnd.x, rnd.y, 0);
        }

        #endregion

        #region Attack Methods

        private void AttackPlayer()
        {
            if (isAttacking) return;
            float sinceLast = Time.time - lastAttackTime;

            switch (enemyType)
            {
                case EnemyType.Goblin:
                    if (sinceLast >= goblinAttackCooldownTime)
                        StartCoroutine(PerformGoblinAttack());
                    break;
                case EnemyType.DarkSkull:
                    if (sinceLast >= darkSkullAttackCooldownTime)
                        StartCoroutine(PerformDarkSkullAttack());
                    break;
                case EnemyType.Troll:
                    if (sinceLast >= trollAttackCooldownTime)
                        StartCoroutine(PerformTrollAttack());
                    break;
                case EnemyType.Standard:
                default:
                    if (sinceLast >= attackCooldown)
                        StartCoroutine(PerformStandardAttack());
                    break;
            }
        }

        private IEnumerator PerformStandardAttack()
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            float orig = speed;
            speed = 0f;
            animator?.Play(attackAnimationName);

            yield return new WaitForSeconds(attackCooldown);

            // Наносим урон игроку
            player.TakeDamage(this);
            if (debugLog) Debug.Log($"{enemyName} стандартный удар.");

            // Пуш вручную через Transform
            if (pushPlayer)
            {
                Vector3 dir = (player.transform.position - transform.position).normalized;
                player.transform.position += dir * pushForceMultiplier;
            }

            speed = orig;
            isAttacking = false;
            UpdateAnimationState();
        }

        private IEnumerator PerformGoblinAttack()
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            float orig = speed;
            speed = 0f;
            animator?.Play(attackAnimationName);

            yield return new WaitForSeconds(goblinAttackAnimationDuration);

            SpawnProjectileEvent();

            speed = orig;
            isAttacking = false;
            UpdateAnimationState();
        }

        private IEnumerator PerformDarkSkullAttack()
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            float orig = speed;
            speed = 0f;
            animator?.Play(attackAnimationName);

            yield return new WaitForSeconds(darkSkullAttackAnimationDuration);

            RegisterDarkSkullHitEvent();

            speed = orig;
            isAttacking = false;
            UpdateAnimationState();
        }

        private IEnumerator PerformTrollAttack()
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            float orig = speed;
            speed = 0f;
            animator?.Play(attackAnimationName);

            yield return new WaitForSeconds(trollAttackAnimationDuration);

            RegisterTrollHitEvent();

            speed = orig;
            isAttacking = false;
            UpdateAnimationState();
        }

        #endregion

        #region Animation Events

        public void SpawnProjectileEvent()
        {
            Vector3 spawnPos = attackPoint != null
                ? attackPoint.position
                : transform.position;
            Vector2 dir = (player.transform.position - spawnPos).normalized;

            if (goblinProjectileSpreadAngle > 0f)
            {
                float a = Random.Range(-goblinProjectileSpreadAngle, goblinProjectileSpreadAngle);
                dir = Quaternion.Euler(0, 0, a) * dir;
            }

            if (projectilePrefab != null)
            {
                var proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
                proj.transform.localScale = goblinProjectileScale;
                if (proj.TryGetComponent<GoblinProjectile>(out var gp))
                {
                    gp.SetParameters(dir, projectileSpeed,
                        bindingDuration,
                        goblinProjectileLifeTime,
                        goblinProjectileDamage);
                }
                else if (proj.TryGetComponent<Rigidbody2D>(out var prb))
                {
                    prb.AddForce(dir * projectileSpeed, ForceMode2D.Impulse);
                }
            }
            else Debug.LogWarning($"[{enemyName}] Нет префаба снаряда");
        }

        public void RegisterDarkSkullHitEvent()
        {
            if (!isPlayerInTrigger) return;

            // Прямой вызов урона
            player.ReceiveDarkSkullHit();
            if (pushPlayer)
            {
                Vector3 dir = (player.transform.position - transform.position).normalized;
                player.transform.position += dir * darkSkullPushForce;
            }
        }

        public void RegisterTrollHitEvent()
        {
            if (!isPlayerInTrigger) return;

            player.ReceiveTrollHit();
            if (pushPlayer)
            {
                Vector3 dir = (player.transform.position - transform.position).normalized;
                player.transform.position += dir * trollPushForce;
            }
        }

        #endregion

        #region Effects & Animation State

        public void ApplySlow(float slowFactor, float duration)
        {
            StartCoroutine(SlowEffect(slowFactor, duration));
        }

        private IEnumerator SlowEffect(float slowFactor, float duration)
        {
            float orig = speed;
            speed = orig * slowFactor;
            yield return new WaitForSeconds(duration);
            speed = orig;
        }

        public void ApplyPush(Vector2 force)
        {
            rb.AddForce(force, ForceMode2D.Impulse);
        }

        private void UpdateAnimationState()
        {
            if (player == null)
            {
                animator?.Play(walkAnimationName);
                return;
            }

            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist <= detectionRange)
            {
                animator?.Play(dist <= attackRange ? idleAnimationName : walkAnimationName);
            }
            else animator?.Play(idleAnimationName);
        }

        #endregion
    }
}
