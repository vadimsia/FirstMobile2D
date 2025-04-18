// Resources/Scripts/GameManagers/ArenaManager.cs
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
        [SerializeField] private RectTransform clockHand;

        [Header("Параметры спавна")]
        [Tooltip("Половина размера области спавна (например, 50 означает, что объекты генерируются в диапазоне от -50 до 50)")]
        [SerializeField] private float spawnArea = 50f;

        private ArenaSettings currentSettings;
        private float timer;
        private bool playerSurvived;
        private List<Vector3> obstacleSpawnPositions = new List<Vector3>();

        private void Start()
        {
            // подгружаем настройки
            if (GameStageManager.currentStageData != null && GameStageManager.currentStageData.arenaSettings != null)
                currentSettings = GameStageManager.currentStageData.arenaSettings;
            else
                currentSettings = defaultArenaSettings;

            timer = currentSettings.survivalTime;
            if (clockHand != null)
                clockHand.localRotation = Quaternion.Euler(0f, 0f, -90f);

            InitializeArena();

            if (currentSettings.plantTreesAtEdges)
                PlantEdgeTrees();
        }

        private void InitializeArena()
        {
            Debug.Log("Arena инициализирована.");
            SpawnEnemies();
            SpawnFairies();
            SpawnObstacles();
        }

        private void SpawnEnemies()
        {
            for (int i = 0; i < currentSettings.enemyCount; i++)
            {
                if (currentSettings.enemyPrefabs.Length == 0)
                {
                    Debug.LogWarning("Enemy Prefabs не заданы.");
                    break;
                }
                var prefab = currentSettings.enemyPrefabs[Random.Range(0, currentSettings.enemyPrefabs.Length)];
                Instantiate(prefab, GetRandomPosition(), Quaternion.identity);
            }
        }

        private void SpawnFairies()
        {
            for (int i = 0; i < currentSettings.fairyCount; i++)
            {
                if (currentSettings.fairyPrefabs.Length == 0)
                {
                    Debug.LogWarning("Fairy Prefabs не заданы.");
                    break;
                }
                var prefab = currentSettings.fairyPrefabs[Random.Range(0, currentSettings.fairyPrefabs.Length)];
                Instantiate(prefab, GetRandomPosition(), Quaternion.identity);
            }
        }

        private void SpawnObstacles()
        {
            var types = currentSettings.obstacleTypes;
            if (types == null || types.Length == 0)
            {
                Debug.LogWarning("Препятствия не заданы.");
                return;
            }

            float region = spawnArea * 2f;
            obstacleSpawnPositions = GeneratePoissonPoints(currentSettings.obstacleMinDistance, region, 30);

            int required = 0;
            foreach (var os in types)
                required += Random.Range(os.minCount, os.maxCount + 1);

            if (obstacleSpawnPositions.Count < required)
                Debug.LogWarning($"Позиций {obstacleSpawnPositions.Count}, требуется {required}.");

            foreach (var os in types)
            {
                int cnt = Random.Range(os.minCount, os.maxCount + 1);
                for (int i = 0; i < cnt; i++)
                {
                    if (Random.value > os.spawnProbability) continue;
                    if (obstacleSpawnPositions.Count == 0) return;

                    int idx = Random.Range(0, obstacleSpawnPositions.Count);
                    var pos = obstacleSpawnPositions[idx];
                    obstacleSpawnPositions.RemoveAt(idx);

                    var inst = Instantiate(os.obstaclePrefab, pos, Quaternion.identity);
                    float s = Random.Range(os.minScale, os.maxScale);
                    inst.transform.localScale = new Vector3(s, s, s);
                }
            }
        }

        private Vector3 GetRandomPosition()
            => new Vector3(
                Random.Range(-spawnArea, spawnArea),
                Random.Range(-spawnArea, spawnArea),
                0f
            );

        private List<Vector3> GeneratePoissonPoints(float minDist, float size, int attempts)
        {
            var pts = new List<Vector3>();
            var spawn = new List<Vector2>();
            var first = new Vector2(
                Random.Range(-size/2f, size/2f),
                Random.Range(-size/2f, size/2f)
            );
            pts.Add(new Vector3(first.x, first.y, 0f));
            spawn.Add(first);

            while (spawn.Count > 0)
            {
                int i = Random.Range(0, spawn.Count);
                var center = spawn[i];
                bool accepted = false;
                for (int t = 0; t < attempts; t++)
                {
                    float ang = Random.value * Mathf.PI * 2f;
                    var dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                    var cand = center + dir * Random.Range(minDist, 2*minDist);

                    if (cand.x >= -size/2f && cand.x <= size/2f &&
                        cand.y >= -size/2f && cand.y <= size/2f)
                    {
                        bool ok = true;
                        foreach (var p in pts)
                            if (Vector2.Distance(cand, new Vector2(p.x, p.y)) < minDist)
                            { ok = false; break; }

                        if (ok)
                        {
                            pts.Add(new Vector3(cand.x, cand.y, 0f));
                            spawn.Add(cand);
                            accepted = true;
                        }
                    }
                }
                if (!accepted) spawn.RemoveAt(i);
            }
            return pts;
        }

        private void PlantEdgeTrees()
        {
            var types = currentSettings.obstacleTypes;
            if (types == null || types.Length < 2)
            {
                Debug.LogWarning("Нужно хотя бы 2 типа препятствий для деревьев.");
                return;
            }

            var prefabs = new GameObject[] {
                types[0].obstaclePrefab,
                types[1].obstaclePrefab
            };

            float half = spawnArea;
            var posList = new List<Vector3>();

            // создаём случайные позиции в 4 полосах толщины edgeForestThickness
            for (int i = 0; i < currentSettings.edgeTreesPerSide; i++)
            {
                // Top
                posList.Add(new Vector3(
                    Random.Range(-half, half),
                    Random.Range(half - currentSettings.edgeForestThickness, half),
                    0f));

                // Bottom
                posList.Add(new Vector3(
                    Random.Range(-half, half),
                    Random.Range(-half, -half + currentSettings.edgeForestThickness),
                    0f));

                // Left
                posList.Add(new Vector3(
                    Random.Range(-half, -half + currentSettings.edgeForestThickness),
                    Random.Range(-half, half),
                    0f));

                // Right
                posList.Add(new Vector3(
                    Random.Range(half - currentSettings.edgeForestThickness, half),
                    Random.Range(-half, half),
                    0f));
            }

            foreach (var basePos in posList)
            {
                // джиттер
                float jx = Random.Range(-currentSettings.edgeTreeJitterRange.x, currentSettings.edgeTreeJitterRange.x);
                float jy = Random.Range(-currentSettings.edgeTreeJitterRange.y, currentSettings.edgeTreeJitterRange.y);
                Vector3 p = new Vector3(basePos.x + jx, basePos.y + jy, 0f);

                var prefab = prefabs[Random.Range(0, prefabs.Length)];
                var tree = Instantiate(prefab, p, Quaternion.identity);

                float scale = Random.Range(
                    currentSettings.edgeTreeScaleRange.x,
                    currentSettings.edgeTreeScaleRange.y);
                tree.transform.localScale = new Vector3(scale, scale, scale);

                if (currentSettings.disableEdgeTreeColliders)
                    foreach (var col in tree.GetComponentsInChildren<Collider>())
                        col.enabled = false;
            }

            Debug.Log($"Посажено деревьев по краям: {posList.Count}");
        }

        private void Update()
        {
            if (playerSurvived) return;

            timer -= Time.deltaTime;
            UpdateTimerUI();

            if (timer <= 0f)
            {
                playerSurvived = true;
                LoadLabyrinthScene();
            }
        }

        private void UpdateTimerUI()
        {
            if (timerText != null)
                timerText.text = $"{timer:F1}";
            if (clockHand != null)
            {
                float norm = Mathf.Clamp01(timer / currentSettings.survivalTime);
                float ang = -90f - (1f - norm) * 360f;
                clockHand.localRotation = Quaternion.Euler(0f, 0f, ang);
            }
        }

        public void OnPlayerDeath()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void LoadLabyrinthScene()
        {
            if (GameStageManager.currentStageData != null)
                SceneManager.LoadScene(GameStageManager.currentStageData.labyrinthSceneName);
        }
    }
}
