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
            if (timerText != null)
            {
                timerText.text = $"Время: {timer:F1}";
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
