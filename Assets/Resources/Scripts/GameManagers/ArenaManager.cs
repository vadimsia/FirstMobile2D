// Resources/Scripts/GameManagers/ArenaManager.cs
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private RectTransform clockHand;

        [Header("Параметры спавна")]
        [Tooltip("Половина размера области спавна (например, 50 означает диапазон от -50 до 50)")]
        [SerializeField] private float spawnArea = 50f;

        private ArenaSettings currentSettings;
        private float timer;
        private bool playerSurvived;
        private List<Vector3> obstacleSpawnPositions = new List<Vector3>();
        private Transform edgeTreesParent;

        private void Start()
        {
            currentSettings = (GameStageManager.currentStageData?.arenaSettings) ?? defaultArenaSettings;
            timer = currentSettings.survivalTime;
            if (clockHand != null)
                clockHand.localRotation = Quaternion.Euler(0f, 0f, -90f);

            InitializeArena();

            if (currentSettings.plantTreesAtEdges)
            {
                edgeTreesParent = new GameObject("EdgeTrees").transform;
                edgeTreesParent.SetParent(transform, false);
                PlantEdgeTrees();
            }
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
            if (currentSettings.enemyPrefabs.Length == 0)
            {
                Debug.LogWarning("Enemy Prefabs не заданы.");
                return;
            }
            for (int i = 0; i < currentSettings.enemyCount; i++)
            {
                var prefab = currentSettings.enemyPrefabs.RandomElement();
                Instantiate(prefab, GetRandomPosition(), Quaternion.identity);
            }
        }

        private void SpawnFairies()
        {
            if (currentSettings.fairyPrefabs.Length == 0)
            {
                Debug.LogWarning("Fairy Prefabs не заданы.");
                return;
            }
            for (int i = 0; i < currentSettings.fairyCount; i++)
            {
                var prefab = currentSettings.fairyPrefabs.RandomElement();
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

            int required = types.Sum(os => Random.Range(os.minCount, os.maxCount + 1));
            if (obstacleSpawnPositions.Count < required)
                Debug.LogWarning($"Позиций {obstacleSpawnPositions.Count}, требуется {required}.");

            foreach (var os in types)
            {
                int cnt = Random.Range(os.minCount, os.maxCount + 1);
                for (int i = 0; i < cnt; i++)
                {
                    if (Random.value > os.spawnProbability || obstacleSpawnPositions.Count == 0)
                        continue;

                    int idx = Random.Range(0, obstacleSpawnPositions.Count);
                    var pos = obstacleSpawnPositions[idx];
                    obstacleSpawnPositions.RemoveAt(idx);

                    var inst = Instantiate(os.obstaclePrefab, pos, Quaternion.identity);
                    float s = Random.Range(os.minScale, os.maxScale);
                    inst.transform.localScale = new Vector3(s, s, s);
                }
            }
        }

        private Vector3 GetRandomPosition() =>
            new Vector3(
                Random.Range(-spawnArea, spawnArea),
                Random.Range(-spawnArea, spawnArea),
                0f
            );

        private List<Vector3> GeneratePoissonPoints(float minDist, float size, int attempts)
        {
            var pts = new List<Vector3>();
            var spawn = new List<Vector2>();
            var first = new Vector2(
                Random.Range(-size / 2f, size / 2f),
                Random.Range(-size / 2f, size / 2f)
            );
            pts.Add(first.ToVector3());
            spawn.Add(first);

            while (spawn.Count > 0)
            {
                int i = Random.Range(0, spawn.Count);
                var center = spawn[i];
                bool accepted = false;

                for (int t = 0; t < attempts; t++)
                {
                    float ang = Random.value * Mathf.PI * 2f;
                    var cand = center + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * Random.Range(minDist, 2 * minDist);

                    if (Mathf.Abs(cand.x) > size / 2f || Mathf.Abs(cand.y) > size / 2f)
                        continue;

                    if (pts.Any(p => Vector2.Distance(new Vector2(p.x, p.y), cand) < minDist))
                        continue;

                    pts.Add(cand.ToVector3());
                    spawn.Add(cand);
                    accepted = true;
                    break;
                }

                if (!accepted) spawn.RemoveAt(i);
            }

            return pts;
        }

        private void PlantEdgeTrees()
        {
            var types = currentSettings.obstacleTypes;
            if (types == null || types.Length == 0)
            {
                Debug.LogWarning("Нужно хотя бы 1 тип препятствия для деревьев.");
                return;
            }

            var prefabs = types.Select(t => t.obstaclePrefab).ToArray();
            float half = spawnArea;
            int perSide = currentSettings.edgeTreesPerSide;
            int maxAttempts = perSide * 20;
            int totalPlaced = 0;

            for (int side = 0; side < 4; side++)
            {
                int placed = 0, attempts = 0;

                while (placed < perSide && attempts < maxAttempts)
                {
                    attempts++;

                    // выбираем позицию с джиттером по стороне
                    Vector3 pos = side switch
                    {
                        0 => new Vector3(Random.Range(-half, half),
                                         Random.Range(half - currentSettings.edgeForestThickness, half),
                                         0f),
                        1 => new Vector3(Random.Range(-half, half),
                                         Random.Range(-half, -half + currentSettings.edgeForestThickness),
                                         0f),
                        2 => new Vector3(Random.Range(-half, -half + currentSettings.edgeForestThickness),
                                         Random.Range(-half, half),
                                         0f),
                        _ => new Vector3(Random.Range(half - currentSettings.edgeForestThickness, half),
                                        Random.Range(-half, half),
                                        0f),
                    };
                    pos.x += Random.Range(-currentSettings.edgeTreeJitterRange.x, currentSettings.edgeTreeJitterRange.x);
                    pos.y += Random.Range(-currentSettings.edgeTreeJitterRange.y, currentSettings.edgeTreeJitterRange.y);

                    // инстанциируем дерево
                    var prefab = prefabs.RandomElement();
                    var tree = Instantiate(prefab, pos, Quaternion.identity, edgeTreesParent);
                    float scale = Random.Range(currentSettings.edgeTreeScaleRange.x, currentSettings.edgeTreeScaleRange.y);
                    tree.transform.localScale = Vector3.one * scale;

                    // проверяем CircleCollider2D-триггер
                    var trigger = tree.GetComponentsInChildren<CircleCollider2D>()
                                      .FirstOrDefault(c => c.isTrigger);
                    if (trigger == null)
                    {
                        Debug.LogWarning("На префабе дерева не найден CircleCollider2D с IsTrigger!");
                        FinalizeTree(tree);
                        placed++; totalPlaced++;
                        continue;
                    }

                    float worldRadius = trigger.radius * Mathf.Max(tree.transform.lossyScale.x, tree.transform.lossyScale.y);
                    var hits = Physics2D.OverlapCircleAll(pos, worldRadius)
                                       .Where(c => c.isTrigger).ToArray();

                    if (hits.Length <= 1)
                    {
                        FinalizeTree(tree);
                        placed++; totalPlaced++;
                    }
                    else
                    {
                        Destroy(tree);
                    }
                }

                Debug.Log($"Сторона {side}: попыток={attempts}, посажено={placed}/{perSide}");
            }

            Debug.Log($"Итого деревьев по краям посажено: {totalPlaced}");
        }

        private void FinalizeTree(GameObject tree)
        {
            // 1) отключаем обычные коллайдеры
            if (currentSettings.disableEdgeTreeColliders)
                foreach (var col in tree.GetComponentsInChildren<Collider>())
                    col.enabled = false;

            // 2) ставим sortingOrder: y=spawnArea → 1, y=-spawnArea → 300
            var sr = tree.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                float y = tree.transform.position.y;
                float t = Mathf.InverseLerp(spawnArea, -spawnArea, y);
                sr.sortingOrder = Mathf.RoundToInt(Mathf.Lerp(1, 300, t));
            }
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

        public void OnPlayerDeath() =>
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        private void LoadLabyrinthScene()
        {
            if (GameStageManager.currentStageData != null)
                SceneManager.LoadScene(GameStageManager.currentStageData.labyrinthSceneName);
        }
    }

    public static class Extensions
    {
        public static T RandomElement<T>(this T[] arr) => arr[Random.Range(0, arr.Length)];
        public static Vector3 ToVector3(this Vector2 v) => new Vector3(v.x, v.y, 0f);
    }
}
