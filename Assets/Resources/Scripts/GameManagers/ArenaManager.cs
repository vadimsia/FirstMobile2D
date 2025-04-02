using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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

        // Добавляем ссылку на UI image (стрелка)
        [Header("UI Стрелка таймера (Clock Hand)")]
        [SerializeField] private RectTransform clockHand;

        private ArenaSettings currentSettings;
        private float timer;
        private bool playerSurvived = false;

        private void Start()
        {
            // Берём настройки арены из выбранного этапа
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

        private void InitializeArena()
        {
            // Пример настройки размера арены
            Debug.Log("Arena инициализирована. Размер: " + currentSettings.arenaSize);
            // Пример спавна врагов:
            for (int i = 0; i < currentSettings.enemyCount; i++)
            {
                GameObject enemyPrefab = currentSettings.enemyPrefabs[Random.Range(0, currentSettings.enemyPrefabs.Length)];
                Instantiate(enemyPrefab, GetRandomPosition(), Quaternion.identity);
            }
        }

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

        private void UpdateTimerUI()
        {
            // Обновляем текст таймера
            if (timerText != null)
            {
                timerText.text = $"{timer:F1}";
            }

            // Обновляем поворот стрелки таймера
            if (clockHand != null)
            {
                // При полном времени (timer == survivalTime) угол = -90, при нуле — -450.
                float normalizedTime = Mathf.Clamp01(timer / currentSettings.survivalTime);
                float angle = -90f - (1f - normalizedTime) * 360f;
                clockHand.localRotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        // Метод, вызываемый, когда игрок погибает (например, из логики игрока или столкновений)
        public void OnPlayerDeath()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void LoadLabyrinthScene()
        {
            if (GameStageManager.currentStageData != null)
            {
                SceneManager.LoadScene(GameStageManager.currentStageData.labyrinthSceneName);
            }
        }
    }
}
