using UnityEngine;
using TMPro;

namespace Resources.Scripts.Player
{
    /// <summary>
    /// Хранит и обновляет состояние здоровья, маны и собранных фей.
    /// Также даёт возможность уклоняться и отображать UI-текст уклонения.
    /// </summary>
    public class PlayerStatsHandler : MonoBehaviour
    {
        [Header("Fairy Collection")]
        [SerializeField, Tooltip("Количество собранных фей")]       private int fairyCount;
        public int FairyCount { get => fairyCount; set => fairyCount = Mathf.Max(0, value); }

        [Header("Health Settings")]
        [SerializeField, Range(5, 50), Tooltip("Текущее здоровье игрока")]        private int health = 20;
        [SerializeField, Tooltip("Максимальное здоровье игрока")]                 private int maxHealth = 50;
        public int Health { get => health; set => health = Mathf.Clamp(value, 0, maxHealth); }

        [Header("Mana Settings")]
        [SerializeField, Tooltip("Максимальная мана")]                            private float maxMana = 100f;
        [SerializeField, Tooltip("Текущая мана")]                                 private float currentMana;
        [SerializeField, Tooltip("Скорость восстановления маны в секунду")]        private float manaRegenRate = 10f;

        [Header("Mana Regen Rate Bonus")]
        private float manaRegenRateBonus;
        private float defaultManaRegenRate;

        [Header("Mana Regen Delay")]
        [SerializeField, Tooltip("Задержка после использования перед регеном маны")] private float manaRegenDelayAfterSpell = 2f;
        private float manaRegenDelayTimer;
        
        [Header("Movement Settings")]
        [SerializeField, Tooltip("Базовая скорость движения")]                     private float baseMoveSpeed = 5f;
        [SerializeField, Tooltip("Текущая итоговая скорость (с учётом перков)")]   private float currentMoveSpeed;
        private float moveSpeedPercentBonus;

        [Header("Fairy Pull Settings")]
        [SerializeField, Tooltip("Базовый радиус притягивания фей")]               private float basePullRange = 3f;
        private float pullRangePercentBonus;

        [Header("Debug (Runtime)")]
        [SerializeField, Tooltip("Текущий радиус притягивания фей")]              private float debugPullRange;

        [Header("Evasion Settings")]
        [SerializeField, Range(0f, 100f), Tooltip("Текущий шанс уклонения (%)")]  private float baseEvasionChance = 10f;
        [SerializeField, Tooltip("Кулдаун между уклонениями (сек)")]               private float evasionCooldown = 1f;
        [SerializeField, Tooltip("Префаб UI-текста для отображения уклонения")]    private GameObject evasionTextPrefab;
        private float evasionCooldownTimer;

        private float defaultMaxMana, defaultManaRegenDelay, defaultBaseMoveSpeed,
                      defaultEvasionChance, defaultBasePullRange;

        private void Awake()
        {
            // Сохраняем исходные значения
            defaultMaxMana            = maxMana;
            defaultManaRegenDelay     = manaRegenDelayAfterSpell;
            defaultManaRegenRate      = manaRegenRate;
            defaultBaseMoveSpeed      = baseMoveSpeed;
            defaultEvasionChance      = baseEvasionChance;
            defaultBasePullRange      = basePullRange;

            currentMana = maxMana;
            UpdateCurrentMoveSpeed();
        }

        private void Update()
        {
            if (manaRegenDelayTimer > 0f) manaRegenDelayTimer -= Time.deltaTime;
            else RegenerateMana();

            if (evasionCooldownTimer > 0f) evasionCooldownTimer -= Time.deltaTime;
            debugPullRange = PullRange;
        }

        private void RegenerateMana()
        {
            currentMana = Mathf.Min(currentMana + manaRegenRate * Time.deltaTime, maxMana);
        }

        public bool UseMana(float amount)
        {
            if (currentMana >= amount)
            {
                currentMana -= amount;
                manaRegenDelayTimer = manaRegenDelayAfterSpell;
                return true;
            }
            return false;
        }

        public void RestoreMana(float amount)
        {
            currentMana = Mathf.Min(currentMana + amount, maxMana);
        }

        public void ResetStats()
        {
            maxMana                   = defaultMaxMana;
            manaRegenDelayAfterSpell  = defaultManaRegenDelay;
            manaRegenRate             = defaultManaRegenRate;
            manaRegenRateBonus        = 0f;
            baseMoveSpeed             = defaultBaseMoveSpeed;
            moveSpeedPercentBonus     = 0f;
            baseEvasionChance         = defaultEvasionChance;
            pullRangePercentBonus     = 0f;
            evasionCooldownTimer      = 0f;
            currentMana               = Mathf.Min(currentMana, maxMana);
            UpdateCurrentMoveSpeed();
        }

        // Новое: пассивное увеличение регена маны
        public void ModifyManaRegenRate(float bonus)
        {
            manaRegenRateBonus += bonus;
            manaRegenRate = defaultManaRegenRate + manaRegenRateBonus;
        }

        public void ModifyMaxMana(float extra)               => maxMana = defaultMaxMana + extra;
        public void ModifyMoveSpeedPercent(float bonus)      { moveSpeedPercentBonus += bonus; UpdateCurrentMoveSpeed(); }
        public void ModifyPullRangePercent(float bonus)      => pullRangePercentBonus += bonus;
        public void ModifyEvasion(float bonus)               => baseEvasionChance += bonus;

        private void UpdateCurrentMoveSpeed()                => currentMoveSpeed = baseMoveSpeed * (1f + moveSpeedPercentBonus / 100f);

        public float PullRange => basePullRange * (1f + pullRangePercentBonus / 100f);
        public float CurrentMana => currentMana;
        public float MaxMana => maxMana;

        /// <summary>
        /// Для PlayerController: возвращает итоговую скорость.
        /// </summary>
        public float GetTotalMoveSpeed() => currentMoveSpeed;

        public bool TryEvade(Vector3 worldPosition)
        {
            if (evasionCooldownTimer > 0f) return false;
            bool evaded = Random.value <= baseEvasionChance / 100f;
            if (evaded)
            {
                ShowEvasionText(worldPosition);
                evasionCooldownTimer = evasionCooldown;
            }
            return evaded;
        }

        private void ShowEvasionText(Vector3 worldPosition)
        {
            if (evasionTextPrefab == null) return;

            var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("EvasionText: Canvas не найден.");
                return;
            }

            var go = Instantiate(evasionTextPrefab, canvas.transform, false);
            var rt = go.GetComponent<RectTransform>();
            var tmp = go.GetComponent<TMP_Text>();
            if (rt == null || tmp == null)
            {
                Debug.LogError("EvasionTextPrefab должен иметь RectTransform и TMP_Text");
                Destroy(go);
                return;
            }

            var screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPoint,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 localPoint
            );
            rt.anchoredPosition = localPoint;
        }
    }
}
