using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Resources.Scripts.Data;
using System.Collections.Generic;

namespace Resources.Scripts.GameManagers
{
    public class ArenaManager : MonoBehaviour
    {
        [Header("Настройки по умолчанию (на случай отсутствия выбранного этапа)")]
        [SerializeField] private ArenaSettings defaultArenaSettings;

        [Header("UI Таймер")]
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("UI Стрелка таймера (Clock Hand)")]
        [SerializeField] private RectTransform clockHand;

        // Область спавна определяется как квадрат с координатами от -spawnArea до +spawnArea по осям X и Y.
        [Header("Параметры спавна")]
        [Tooltip("Половина размера области спавна (например, 50 означает, что объекты будут генерироваться в диапазоне от -50 до 50)")]
        [SerializeField] private float spawnArea = 50f;

        private ArenaSettings currentSettings;
        private float timer;
        private bool playerSurvived;

        // Позиции для спавна препятствий, сгенерированные алгоритмом Poisson Disk Sampling.
        private List<Vector3> obstacleSpawnPositions = new List<Vector3>();

        private void Start()
        {
            // Берём настройки арены из выбранного этапа или используем настройки по умолчанию.
            if (GameStageManager.currentStageData != null && GameStageManager.currentStageData.arenaSettings != null)
            {
                currentSettings = GameStageManager.currentStageData.arenaSettings;
            }
            else
            {
                currentSettings = defaultArenaSettings;
            }

            timer = currentSettings.survivalTime;
            if (clockHand != null)
            {
                clockHand.localRotation = Quaternion.Euler(0f, 0f, -90f);
            }
            InitializeArena();
        }

        /// <summary>
        /// Инициализация арены: спавн врагов, фей и препятствий.
        /// </summary>
        private void InitializeArena()
        {
            Debug.Log("Arena инициализирована.");
            SpawnEnemies();
            SpawnFairies();
            SpawnObstacles();
        }

        /// <summary>
        /// Спавн врагов на арене.
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
        /// Спавн фей на арене.
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
        /// Спавн препятствий по всей области.
        /// Используется Poisson Disk Sampling для генерации позиций, удовлетворяющих условию минимального расстояния.
        /// Каждому типу препятствия назначается случайное количество экземпляров, и для каждого выбирается случайная позиция из списка.
        /// </summary>
        private void SpawnObstacles()
        {
            if (currentSettings.obstacleTypes == null || currentSettings.obstacleTypes.Length == 0)
            {
                Debug.LogWarning("Препятствия не заданы в настройках арены.");
                return;
            }
            
            // Генерируем позиции для спавна препятствий.
            // Размер региона – это 2 * spawnArea (квадрат от -spawnArea до +spawnArea).
            float regionSize = spawnArea * 2f;
            obstacleSpawnPositions = GeneratePoissonPoints(
                currentSettings.obstacleMinDistance,
                regionSize,
                30 // число попыток для каждого спавн-поинта
            );

            int totalRequired = 0;
            foreach (var obstacleSettings in currentSettings.obstacleTypes)
            {
                totalRequired += Random.Range(obstacleSettings.minCount, obstacleSettings.maxCount + 1);
            }

            if(obstacleSpawnPositions.Count < totalRequired)
            {
                Debug.LogWarning($"Доступно позиций: {obstacleSpawnPositions.Count}, а требуется минимум: {totalRequired}. " +
                                 "Возможно, уменьшите значение минимального расстояния между препятствиями или увеличьте область спавна.");
            }
            
            // Для каждого типа препятствия спавним нужное число экземпляров.
            foreach (var obstacleSettings in currentSettings.obstacleTypes)
            {
                int count = Random.Range(obstacleSettings.minCount, obstacleSettings.maxCount + 1);
                for (int i = 0; i < count; i++)
                {
                    // Пропуск спавна по вероятности.
                    if (Random.value > obstacleSettings.spawnProbability)
                        continue;

                    if (obstacleSpawnPositions.Count == 0)
                    {
                        Debug.LogWarning("Закончились доступные позиции для препятствий.");
                        return;
                    }

                    int index = Random.Range(0, obstacleSpawnPositions.Count);
                    Vector3 spawnPosition = obstacleSpawnPositions[index];
                    obstacleSpawnPositions.RemoveAt(index);

                    Quaternion rotation = Quaternion.identity;
                    GameObject obstacleInstance = Instantiate(obstacleSettings.obstaclePrefab, spawnPosition, rotation);
                    
                    float scale = Random.Range(obstacleSettings.minScale, obstacleSettings.maxScale);
                    obstacleInstance.transform.localScale = new Vector3(scale, scale, scale);
                }
            }
        }

        /// <summary>
        /// Возвращает случайную позицию в пределах области спавна по плоскости X-Y (Z фиксирована на 0).
        /// </summary>
        private Vector3 GetRandomPosition()
        {
            return new Vector3(
                Random.Range(-spawnArea, spawnArea),
                Random.Range(-spawnArea, spawnArea),
                0f
            );
        }

        /// <summary>
        /// Алгоритм Poisson Disk Sampling для генерации точек, разделённых минимальным расстоянием.
        /// Размер региона задаётся как квадрат со стороной regionSize.
        /// </summary>
        /// <param name="minDistance">Минимальное расстояние между точками</param>
        /// <param name="regionSize">Размер стороны квадратного региона</param>
        /// <param name="numSamplesBeforeRejection">Количество попыток для каждой точки</param>
        /// <returns>Список точек типа Vector3 (ось Z = 0)</returns>
        private List<Vector3> GeneratePoissonPoints(float minDistance, float regionSize, int numSamplesBeforeRejection)
        {
            List<Vector3> points = new List<Vector3>();
            List<Vector2> spawnPoints = new List<Vector2>();

            // Начальная точка выбирается случайно внутри региона
            Vector2 firstPoint = new Vector2(Random.Range(-regionSize/2f, regionSize/2f), Random.Range(-regionSize/2f, regionSize/2f));
            points.Add(new Vector3(firstPoint.x, firstPoint.y, 0f));
            spawnPoints.Add(firstPoint);

            while (spawnPoints.Count > 0)
            {
                int spawnIndex = Random.Range(0, spawnPoints.Count);
                Vector2 spawnCenter = spawnPoints[spawnIndex];
                bool candidateAccepted = false;
                
                for (int i = 0; i < numSamplesBeforeRejection; i++)
                {
                    float angle = Random.value * Mathf.PI * 2;
                    Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    Vector2 candidate = spawnCenter + dir * Random.Range(minDistance, 2 * minDistance);
                    
                    // Проверяем, находится ли точка внутри региона
                    if (candidate.x >= -regionSize/2f && candidate.x <= regionSize/2f &&
                        candidate.y >= -regionSize/2f && candidate.y <= regionSize/2f)
                    {
                        bool isValid = true;
                        foreach (var p in points)
                        {
                            if (Vector2.Distance(candidate, new Vector2(p.x, p.y)) < minDistance)
                            {
                                isValid = false;
                                break;
                            }
                        }
                        if (isValid)
                        {
                            points.Add(new Vector3(candidate.x, candidate.y, 0f));
                            spawnPoints.Add(candidate);
                            candidateAccepted = true;
                        }
                    }
                }
                if (!candidateAccepted)
                {
                    spawnPoints.RemoveAt(spawnIndex);
                }
            }
            return points;
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
                float normalizedTime = Mathf.Clamp01(timer / currentSettings.survivalTime);
                float angle = -90f - (1f - normalizedTime) * 360f;
                clockHand.localRotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        /// <summary>
        /// Перезагружает текущую сцену в случае смерти игрока.
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
