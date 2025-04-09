using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Resources.Scripts.Data;

namespace Resources.Scripts.GameManagers
{
    public class ArenaManager : MonoBehaviour
    {
        [Header("Настройки по умолчанию (на случай отсутствия выбранного этапа)")]
        [SerializeField] private ArenaSettings defaultArenaSettings;

        [Header("UI Таймер")]
        [SerializeField] private TextMeshProUGUI timerText;

        // Ссылка на UI image (стрелка таймера)
        [Header("UI Стрелка таймера (Clock Hand)")]
        [SerializeField] private RectTransform clockHand;

        private ArenaSettings currentSettings;
        private float timer;
        private bool playerSurvived;

        private void Start()
        {
            // Берём настройки арены из выбранного этапа, если они заданы, иначе - настройки по умолчанию
            if (GameStageManager.currentStageData != null && GameStageManager.currentStageData.arenaSettings != null)
            {
                currentSettings = GameStageManager.currentStageData.arenaSettings;
            }
            else
            {
                currentSettings = defaultArenaSettings;
            }

            timer = currentSettings.survivalTime;
            // Устанавливаем начальный поворот стрелки (–90 градусов по Z)
            if (clockHand != null)
            {
                clockHand.localRotation = Quaternion.Euler(0f, 0f, -90f);
            }
            InitializeArena();
        }

        /// <summary>
        /// Инициализация арены с учетом настроек: спавн врагов и фей.
        /// </summary>
        private void InitializeArena()
        {
            Debug.Log("Arena инициализирована. Размер: " + currentSettings.arenaSize);
            SpawnEnemies();
            SpawnFairies();
        }

        /// <summary>
        /// Спавнит заданное количество врагов на арене.
        /// </summary>
        private void SpawnEnemies()
        {
            for (int i = 0; i < currentSettings.enemyCount; i++)
            {
                if (currentSettings.enemyPrefabs.Length == 0)
                {
                    Debug.LogWarning("Enemy Prefabs не заданы в ArenaSettings.");
                    break;
                }
                GameObject enemyPrefab = currentSettings.enemyPrefabs[Random.Range(0, currentSettings.enemyPrefabs.Length)];
                Instantiate(enemyPrefab, GetRandomPosition(), Quaternion.identity);
            }
        }

        /// <summary>
        /// Спавнит заданное количество фей на арене.
        /// </summary>
        private void SpawnFairies()
        {
            for (int i = 0; i < currentSettings.fairyCount; i++)
            {
                if (currentSettings.fairyPrefabs.Length == 0)
                {
                    Debug.LogWarning("Fairy Prefabs не заданы в ArenaSettings.");
                    break;
                }
                GameObject fairyPrefab = currentSettings.fairyPrefabs[Random.Range(0, currentSettings.fairyPrefabs.Length)];
                Instantiate(fairyPrefab, GetRandomPosition(), Quaternion.identity);
            }
        }

        /// <summary>
        /// Вычисляет случайную позицию внутри арены.
        /// </summary>
        /// <returns>Случайное положение в пределах размеров арены.</returns>
        private Vector3 GetRandomPosition()
        {
            float halfSize = currentSettings.arenaSize / 2f;
            return new Vector3(Random.Range(-halfSize, halfSize), 0f, Random.Range(-halfSize, halfSize));
        }

        private void Update()
        {
            if (!playerSurvived)
            {
                timer -= Time.deltaTime;
                UpdateTimerUI();

                if (timer <= 0f)
                {
                    playerSurvived = true;
                    LoadLabyrinthScene();
                }
            }
        }

        /// <summary>
        /// Обновляет UI таймера и поворот стрелки.
        /// </summary>
        private void UpdateTimerUI()
        {
            if (timerText != null)
            {
                timerText.text = $"{timer:F1}";
            }

            if (clockHand != null)
            {
                // При полном времени (timer == survivalTime) угол = -90, при нуле – -450.
                float normalizedTime = Mathf.Clamp01(timer / currentSettings.survivalTime);
                float angle = -90f - (1f - normalizedTime) * 360f;
                clockHand.localRotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        /// <summary>
        /// Вызывается при смерти игрока. Перезагружает текущую сцену.
        /// </summary>
        public void OnPlayerDeath()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// Загружает следующую сцену (например, лабиринт) после успешного выживания.
        /// </summary>
        private void LoadLabyrinthScene()
        {
            if (GameStageManager.currentStageData != null)
            {
                SceneManager.LoadScene(GameStageManager.currentStageData.labyrinthSceneName);
            }
        }
    }
}
