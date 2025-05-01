using System.Collections.Generic;
using UnityEngine;
using Resources.Scripts.Data;

namespace Resources.Scripts.Fairy
{
    /// <summary>
    /// Spawns fairy prefabs at specified intervals within a defined area.
    /// Retrieves configuration from an ArenaSettings asset.
    /// </summary>
    [DisallowMultipleComponent]
    public class FairySpawner : MonoBehaviour
    {
        [Header("Spawning Settings")]
        [SerializeField, Tooltip("ArenaSettings asset supplying fairy count and prefabs.")]
        private ArenaSettings arenaSettings;

        [Header("Spawn Timing")]
        [SerializeField, Range(1f, 30f), Tooltip("Time between spawns in seconds.")]
        private float spawnInterval = 5f;

        [Header("Spawn Area")]
        [SerializeField, Tooltip("Radius around spawner for random spawn offset. Zero for fixed position.")]
        private float spawnAreaRadius;
        [SerializeField, Tooltip("Enable random position within spawn area.")]
        private bool randomizeSpawnPosition;

        [Header("Debug Settings")]
        [SerializeField, Tooltip("Enable debug logging.")]
        private bool debugLog;

        // Internal configuration loaded from ArenaSettings.
        private List<GameObject> fairyPrefabs = new List<GameObject>();
        private int maxFairies = 5;

        // Internal timer for scheduling spawns.
        private float spawnTimer;

        private void Awake()
        {
            // Initialize spawn timer.
            spawnTimer = spawnInterval;

            if (arenaSettings == null)
            {
                Debug.LogWarning($"{nameof(FairySpawner)} requires an ArenaSettings reference.", this);
                return;
            }

            maxFairies = Mathf.Max(1, arenaSettings.fairyCount);

            if (arenaSettings.fairyPrefabs != null && arenaSettings.fairyPrefabs.Length > 0)
            {
                fairyPrefabs = new List<GameObject>(arenaSettings.fairyPrefabs);
            }
            else
            {
                Debug.LogWarning("No fairy prefabs defined in ArenaSettings.", this);
            }
        }

        /// <summary>
        /// Spawns a single fairy prefab at the spawner's location, optionally randomized within the spawn area.
        /// </summary>
        /// <param name="prefab">Prefab to instantiate.</param>
        private void Spawn(GameObject prefab)
        {
            if (prefab == null || fairyPrefabs.Count == 0)
                return;

            if (transform.childCount >= maxFairies)
                return;

            Vector3 spawnPosition = transform.position;
            if (randomizeSpawnPosition && spawnAreaRadius > 0f)
            {
                Vector2 offset = Random.insideUnitCircle * spawnAreaRadius;
                spawnPosition += new Vector3(offset.x, offset.y, 0f);
            }

            GameObject instance = Instantiate(prefab, spawnPosition, Quaternion.identity, transform);
            if (instance.TryGetComponent(out FairyController fairy))
            {
                fairy.Init(spawnPosition);
            }

            if (debugLog)
            {
                Debug.Log($"Spawned fairy '{prefab.name}' at {spawnPosition}. Current: {transform.childCount}/{maxFairies}.", this);
            }
        }

        private void Update()
        {
            if (fairyPrefabs.Count == 0)
                return;

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                int index = Random.Range(0, fairyPrefabs.Count);
                Spawn(fairyPrefabs[index]);
                spawnTimer = spawnInterval;
            }
        }
    }
}
