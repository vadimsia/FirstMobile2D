using UnityEngine;
using TMPro;

namespace Resources.Scripts.Player
{
    /// <summary>
    /// Хранит и обновляет состояние здоровья, маны и собранных фей.
    /// Также даёт возможность уклоняться и отображать UI-текст уклонения на Canvas.
    /// Поддерживает процентные бонусы к скорости передвижения, радиусу притягивания фей и позволяет в инспекторе видеть текущие значения.
    /// </summary>
    public class PlayerStatsHandler : MonoBehaviour
    {
        [Header("Fairy Collection")]
        [SerializeField, Tooltip("Количество собранных фей")]
        private int fairyCount;
        public int FairyCount
        {
            get => fairyCount;
            set => fairyCount = Mathf.Max(0, value);
        }

        [Header("Health Settings")]
        [SerializeField, Range(5, 50), Tooltip("Текущее здоровье игрока")]
        private int health = 20;
        [SerializeField, Tooltip("Максимальное здоровье игрока")]
        private int maxHealth = 50;
        public int Health
        {
            get => health;
            set => health = Mathf.Clamp(value, 0, maxHealth);
        }

        [Header("Mana Settings")]
        [SerializeField, Tooltip("Максимальная мана")]
        private float maxMana = 100f;
        [SerializeField, Tooltip("Текущая мана")]
        private float currentMana;
        [SerializeField, Tooltip("Скорость восстановления маны в секунду")]
        private float manaRegenRate = 10f;

        [Header("Mana Regen Delay")]
        [SerializeField, Tooltip("Задержка после использования перед регеном маны")]
        private float manaRegenDelayAfterSpell = 2f;
        private float manaRegenDelayTimer;

        [Header("Movement Settings")]
        [SerializeField, Tooltip("Базовая скорость движения")]
        private float baseMoveSpeed = 5f;
        [SerializeField, Tooltip("Текущая итоговая скорость (с учётом перков)")]
        private float currentMoveSpeed;
        private float moveSpeedPercentBonus;  // бонус в процентах

        [Header("Fairy Pull Settings")]
        [SerializeField, Tooltip("Базовый радиус притягивания фей")]
        private float basePullRange = 3f;
        private float pullRangePercentBonus;  // бонус в процентах

        [Header("Debug (Runtime)")]
        [SerializeField, Tooltip("Текущий радиус притягивания фей с учётом перков")]
        private float debugPullRange;

        [Header("Evasion Settings")]
        [SerializeField, Range(0f, 100f), Tooltip("Текущий шанс уклонения (%)")]
        private float baseEvasionChance = 10f;
        [SerializeField, Tooltip("Кулдаун между уклонениями (сек)")]
        private float evasionCooldown = 1f;
        [SerializeField, Tooltip("Префаб UI-текста для отображения уклонения (TextMeshProUGUI)")]
        private GameObject evasionTextPrefab;
        private float evasionCooldownTimer;

        // Сохранённые дефолты для сброса
        private float defaultMaxMana;
        private float defaultManaRegenDelay;
        private float defaultBaseMoveSpeed;
        private float defaultEvasionChance;
        private float defaultBasePullRange;

        private void Awake()
        {
            defaultMaxMana = maxMana;
            defaultManaRegenDelay = manaRegenDelayAfterSpell;
            defaultBaseMoveSpeed = baseMoveSpeed;
            defaultEvasionChance = baseEvasionChance;
            defaultBasePullRange = basePullRange;
            currentMana = maxMana;
            UpdateCurrentMoveSpeed();
        }

        private void Update()
        {
            // Реген маны
            if (manaRegenDelayTimer > 0f)
                manaRegenDelayTimer -= Time.deltaTime;
            else
                RegenerateMana();

            // Кулдаун уклонения
            if (evasionCooldownTimer > 0f)
                evasionCooldownTimer -= Time.deltaTime;

            // Обновляем значение для инспектора
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

        /// <summary>
        /// Сбрасывает все модификаторы к дефолту.
        /// </summary>
        public void ResetStats()
        {
            maxMana = defaultMaxMana;
            manaRegenDelayAfterSpell = defaultManaRegenDelay;
            baseMoveSpeed = defaultBaseMoveSpeed;
            moveSpeedPercentBonus = 0f;
            baseEvasionChance = defaultEvasionChance;
            pullRangePercentBonus = 0f;
            evasionCooldownTimer = 0f;
            currentMana = Mathf.Min(currentMana, maxMana);
            UpdateCurrentMoveSpeed();
        }

        public void ModifyManaRegenDelay(float reduction)
        {
            manaRegenDelayAfterSpell = Mathf.Max(0f, manaRegenDelayAfterSpell - reduction);
        }

        public void ModifyMaxMana(float extra)
        {
            maxMana = defaultMaxMana + extra;
            currentMana = Mathf.Min(currentMana, maxMana);
        }

        /// <summary>
        /// Добавляет процентный бонус к скорости передвижения.
        /// </summary>
        public void ModifyMoveSpeedPercent(float percentBonus)
        {
            moveSpeedPercentBonus += percentBonus;
            UpdateCurrentMoveSpeed();
        }

        /// <summary>
        /// Пересчитывает и сохраняет текущую итоговую скорость.
        /// </summary>
        private void UpdateCurrentMoveSpeed()
        {
            currentMoveSpeed = baseMoveSpeed * (1f + moveSpeedPercentBonus / 100f);
        }

        /// <summary>
        /// Добавляет процентный бонус к радиусу притягивания фей.
        /// </summary>
        public void ModifyPullRangePercent(float percentBonus)
        {
            pullRangePercentBonus += percentBonus;
        }

        /// <summary>
        /// Возвращает радиус притягивания фей с учётом бонусов.
        /// </summary>
        public float PullRange => basePullRange * (1f + pullRangePercentBonus / 100f);

        public void ModifyEvasion(float bonus)
        {
            baseEvasionChance += bonus;
        }
        public float GetTotalMoveSpeed()
        {
            return currentMoveSpeed;
        }

        public float CurrentMana => currentMana;
        public float MaxMana => maxMana;

        /// <summary>
        /// Пытается уклониться от удара в точке worldPosition.
        /// </summary>
        public bool TryEvade(Vector3 worldPosition)
        {
            if (evasionCooldownTimer > 0f)
                return false;

            bool evaded = Random.value <= baseEvasionChance / 100f;
            if (evaded)
            {
                ShowEvasionText(worldPosition);
                evasionCooldownTimer = evasionCooldown;
            }
            return evaded;
        }

        /// <summary>
        /// Инстанцирует UI-текст в Canvas и позиционирует его над игроком.
        /// </summary>
        private void ShowEvasionText(Vector3 worldPosition)
        {
            if (evasionTextPrefab == null)
                return;

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("EvasionText: Canvas не найден в сцене");
                return;
            }

            GameObject go = Instantiate(evasionTextPrefab, canvas.transform, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            TMP_Text tmp = go.GetComponent<TMP_Text>();
            if (rt == null || tmp == null)
            {
                Debug.LogError("EvasionTextPrefab должен иметь RectTransform и TMP_Text");
                Destroy(go);
                return;
            }

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPoint,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 localPoint);

            rt.anchoredPosition = localPoint;
        }
    }
}
