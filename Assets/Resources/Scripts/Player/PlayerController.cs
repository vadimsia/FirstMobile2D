using UnityEngine;
using System;
using System.Collections;
using Resources.Scripts.Enemy;
using Resources.Scripts.Misc;
using UnityEngine.Rendering.Universal;
using Resources.Scripts.Labyrinth;
using Spine;
using Spine.Unity;

namespace Resources.Scripts.Player
{
    /// <summary>
    /// Controls player movement, light, animation, dodge roll, evasion and traps.
    /// Uses Spine animation instead of Unity Animator / SpriteRenderer.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        #region Constants
        private const string SlowAnimationName   = "Goes_01_001";
        private const string RunAnimationName    = "Run_02_001";
        private const string JumpAnimationName   = "Jamp_03_004";
        private static readonly string[] IdleAnimations = {
            "Idle_01_001", "Idle_01_002", "Idle_02_001", "Idle_02_002"
        };
        private const float SlowThreshold        = 0.5f;
        private const float IdleThreshold        = 0.1f;
        #endregion

        #region Inspector Fields

        [Header("Movement Settings")]
        [SerializeField, Tooltip("Текущая скорость (для отладки)")]
        private float currentSpeed;
        [SerializeField] private PlayerJoystick joystick;
        [SerializeField] private GameObject trapPrefab;

        [Header("Light Settings")]
        [SerializeField] private Light2D playerLight;
        [SerializeField] private Transform finishPoint;
        [SerializeField, Range(0.1f, 5f)] private float baseLightRange = 1f;
        [SerializeField, Range(1f, 2f)] private float maxLightRange  = 2f;

        [Header("Player Settings")]
        public bool isImmortal;

        [Header("Animation Settings")]
        [Tooltip("Ссылка на Spine SkeletonAnimation (дочерний объект)")]
        [SerializeField] private SkeletonAnimation skeletonAnimation;

        [Header("DarkSkull / Troll Damage Settings")]
        [SerializeField] private int maxDarkSkullHits = 2;

        [Header("Dodge Roll Settings")]
        [SerializeField, Tooltip("Скорость кувырка (можно править вручную)")]
        private float rollSpeed = 6f;
        [SerializeField, Tooltip("Кулдаун между кувырками (сек)")]
        private float rollCooldown = 2f;
        private float rollDuration;

        #endregion

        #region Public Events & Properties

        public event Action<float> OnRollCooldownChanged;
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
        private float rollCooldownRemaining;

        private bool idleCycling;
        private int idleIndex;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (skeletonAnimation == null)
                skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();

            skeletonAnimation.state.Complete += HandleAnimationComplete;

            // Подгоняем длительность кувырка под Spine‑анимацию
            var anim = skeletonAnimation.Skeleton.Data.FindAnimation(JumpAnimationName);
            rollDuration = anim != null ? anim.Duration : 0.3f;
        }

        private void OnDestroy()
        {
            skeletonAnimation.state.Complete -= HandleAnimationComplete;
        }

        private void Start()
        {
            playerStats = GetComponent<PlayerStatsHandler>();
            PlayIdleSequence();
            UpdateCurrentSpeedDisplay();

            if (finishPoint != null)
                initialDistance = Vector2.Distance(transform.position, finishPoint.position);
            else
                StartCoroutine(WaitForFinishMarker());
        }

        private void Update()
        {
            if (!isRolling) UpdateMovement();
            UpdateLightOuterRange();
            TickRollCooldown();

            if (Input.GetKeyDown(KeyCode.LeftShift)) TryRoll();
            if (!isRolling && Input.GetKeyDown(KeyCode.Space)) StartRollAnimation();
        }

        #endregion

        #region Movement Methods

        private void UpdateMovement()
        {
            if ((LabyrinthMapController.Instance?.IsMapActive ?? false) || Input.GetKey(KeyCode.Space))
                return;

            float h = joystick != null ? joystick.Horizontal : Input.GetAxis("Horizontal");
            float v = joystick != null ? joystick.Vertical : Input.GetAxis("Vertical");
            Vector2 dir = new Vector2(h, v);

            if (dir.magnitude > IdleThreshold)
            {
                idleCycling = false;
                lastMoveDirection = dir.normalized;

                if (dir.magnitude < SlowThreshold)
                    PlayAnimation(SlowAnimationName, true);
                else
                    PlayAnimation(RunAnimationName, true);

                // Флип через Spine API
                skeletonAnimation.skeleton.FlipX = lastMoveDirection.x > 0f;
            }
            else
            {
                if (!idleCycling)
                    PlayIdleSequence();
            }

            float spd = playerStats.GetTotalMoveSpeed() * currentSlowMultiplier;
            UpdateCurrentSpeedDisplay(spd);
            transform.Translate(dir * spd * Time.deltaTime, Space.World);
        }

        private void UpdateCurrentSpeedDisplay(float speed = -1f)
        {
            currentSpeed = speed < 0f ? playerStats.GetTotalMoveSpeed() : speed;
        }

        #endregion

        #region Dodge Roll

        public void TryRoll()
        {
            if (!canRoll || isRolling) return;
            StartCoroutine(RollCoroutine());
        }

        private void StartRollAnimation()
        {
            PlayAnimation(JumpAnimationName, false);
        }

        private IEnumerator RollCoroutine()
        {
            isRolling = true;
            canRoll = false;
            rollCooldownRemaining = rollCooldown;
            OnRollCooldownChanged?.Invoke(1f);

            // Запускаем кувырок
            PlayAnimation(JumpAnimationName, false);

            Vector2 dir = lastMoveDirection.normalized;
            float t = 0f;
            while (t < rollDuration)
            {
                // Только перемещение — без ручного вращения
                transform.Translate(dir * rollSpeed * Time.deltaTime, Space.World);
                t += Time.deltaTime;
                yield return null;
            }

            transform.rotation = Quaternion.identity;
            isRolling = false;
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

        #region Damage and Evasion

        public void TakeDamage(EnemyController enemy)
        {
            if (isImmortal || isRolling) return;
            if (playerStats.TryEvade(transform.position)) return;

            playerStats.Health -= enemy.GetComponent<EnemyStatsHandler>().Damage;
            if (playerStats.Health <= 0) { Die(); return; }
            if (enemy.pushPlayer)
                EntityUtils.MakeDash(transform, transform.position - enemy.transform.position);
        }

        #endregion

        #region Other Effects

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

        public void ApplyBinding(float duration)
        {
            StartCoroutine(BindingCoroutine(duration));
        }

        private IEnumerator BindingCoroutine(float duration)
        {
            float orig = currentSlowMultiplier;
            currentSlowMultiplier = 0f;
            yield return new WaitForSeconds(duration);
            currentSlowMultiplier = orig;
        }

        public void Stun(float duration)
        {
            StartCoroutine(StunCoroutine(duration));
        }

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
            playerStats.ModifyMoveSpeedPercent((mult - 1f) * 100f);
            UpdateCurrentSpeedDisplay();
            yield return new WaitForSeconds(duration);
            playerStats.ResetStats();
            UpdateCurrentSpeedDisplay();
            bonusActive = false;
        }

        public void ReceiveDarkSkullHit()
        {
            darkSkullHitCount++;
            if (darkSkullHitCount >= maxDarkSkullHits) Die();
        }

        public void ReceiveTrollHit()
        {
            Die();
        }

        private void Die()
        {
            var allCanvases = UnityEngine.Object.FindObjectsByType<Canvas>(
                UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None);
            foreach (var canvas in allCanvases)
                canvas.gameObject.SetActive(false);
            Destroy(gameObject);
        }

        #endregion

        #region Spine Helper

        private void PlayAnimation(string animName, bool loop)
        {
            if (Array.IndexOf(IdleAnimations, animName) < 0)
                idleCycling = false;

            var current = skeletonAnimation.state.GetCurrent(0);
            if (current != null && current.Animation.Name == animName) return;
            skeletonAnimation.state.SetAnimation(0, animName, loop);
        }

        private void HandleAnimationComplete(TrackEntry entry)
        {
            if (idleCycling && entry.Animation.Name == IdleAnimations[idleIndex])
            {
                idleIndex = (idleIndex + 1) % IdleAnimations.Length;
                skeletonAnimation.state.SetAnimation(0, IdleAnimations[idleIndex], false);
                return;
            }

            if (entry.Animation.Name == JumpAnimationName)
                PlayIdleSequence();
        }

        private void PlayIdleSequence()
        {
            idleCycling = true;
            idleIndex = 0;
            skeletonAnimation.state.SetAnimation(0, IdleAnimations[idleIndex], false);
        }

        #endregion
    }
}
