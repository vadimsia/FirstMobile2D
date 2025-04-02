using System.Collections.Generic;
using UnityEngine;
using Resources.Scripts.Data; // чтобы получить доступ к ArenaSettings

namespace Resources.Scripts.Fairy
{
    /// <summary>
    /// Spawns fairies in the scene at specified intervals, using settings defined in ArenaSettings.
    /// </summary>
    public class FairySpawner : MonoBehaviour
    {
        [Header("Spawning Settings (from ArenaSettings)")]
        [SerializeField, Tooltip("Reference to the ArenaSettings asset to retrieve fairy configuration.")]
        private ArenaSettings arenaSettings;

        // Локальные переменные для настроек фей.
        private List<GameObject> fairyPrefabs = new List<GameObject>();
        private int maxFairies = 5;
        
        [SerializeField, Range(1f, 30f), Tooltip("Time interval between spawns in seconds.")]
        private float spawnInterval = 5f;
        [SerializeField, Tooltip("Radius of the spawn area offset. Set to 0 for fixed spawn position.")]
        private float spawnAreaRadius = 0f;
        [SerializeField, Tooltip("Enable random spawn positions within the defined spawn area.")]
        private bool randomizeSpawnPosition = false;

        [Header("Debug Settings")]
        [SerializeField, Tooltip("Enable debug logging for fairy spawning.")]
        private bool debugLog = false;

        private float timer;

        private void Start()
        {
            timer = spawnInterval;

            if (arenaSettings != null)
            {
                // Используем настройки из ArenaSettings.
                maxFairies = arenaSettings.fairyCount;
                if (arenaSettings.fairyPrefabs != null && arenaSettings.fairyPrefabs.Length > 0)
                {
                    fairyPrefabs.AddRange(arenaSettings.fairyPrefabs);
                }
                else
                {
                    Debug.LogWarning("В ArenaSettings не заданы префабы фей.");
                }
            }
            else
            {
                Debug.LogWarning("ArenaSettings не назначены в FairySpawner.");
            }
        }

        /// <summary>
        /// Spawns a fairy prefab at the spawner's position (optionally randomized).
        /// </summary>
        /// <param name="prefab">The fairy prefab to spawn.</param>
        private void Spawn(GameObject prefab)
        {
            // Проверяем, что список префабов заполнен.
            if (fairyPrefabs == null || fairyPrefabs.Count == 0)
                return;

            // Не спавним, если достигнуто максимальное количество фей.
            if (transform.childCount >= maxFairies)
                return;

            Vector3 spawnPosition = transform.position;
            if (randomizeSpawnPosition && spawnAreaRadius > 0f)
            {
                // Вычисляем случайное смещение в пределах заданного радиуса.
                Vector2 randomOffset = Random.insideUnitCircle * spawnAreaRadius;
                spawnPosition += new Vector3(randomOffset.x, randomOffset.y, 0f);
            }

            GameObject fairyInstance = Instantiate(prefab, spawnPosition, Quaternion.identity, transform);
            FairyController fairy = fairyInstance.GetComponent<FairyController>();
            if (fairy != null)
            {
                fairy.Init(spawnPosition);
            }

            if (debugLog)
            {
                Debug.Log("Spawned fairy at position: " + spawnPosition);
            }
        }

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                // Выбираем случайный префаб из списка и спавним его.
                if (fairyPrefabs != null && fairyPrefabs.Count > 0)
                {
                    Spawn(fairyPrefabs[Random.Range(0, fairyPrefabs.Count)]);
                }
                timer = spawnInterval;
            }
        }
    }
}
