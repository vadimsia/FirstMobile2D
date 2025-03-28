using System.Collections.Generic;
using UnityEngine;

namespace Resources.Scripts.Fairy
{
    /// <summary>
    /// Spawns fairies in the scene at specified intervals.
    /// </summary>
    public class FairySpawner : MonoBehaviour
    {
        [Header("Spawning Settings")]
        [SerializeField, Tooltip("List of fairy prefabs to spawn.")]
        private List<GameObject> fairyPrefabs;
        [SerializeField, Range(1, 20), Tooltip("Maximum number of fairies allowed simultaneously.")]
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
        }

        /// <summary>
        /// Spawns a fairy prefab at the spawner's position (optionally randomized).
        /// </summary>
        /// <param name="prefab">The fairy prefab to spawn.</param>
        private void Spawn(GameObject prefab)
        {
            // Ensure there is at least one fairy prefab in the list.
            if (fairyPrefabs == null || fairyPrefabs.Count == 0)
                return;

            // Do not spawn if the maximum number of fairies is reached.
            if (transform.childCount >= maxFairies)
                return;

            Vector3 spawnPosition = transform.position;
            if (randomizeSpawnPosition && spawnAreaRadius > 0f)
            {
                // Calculate a random position within a circle of given radius.
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
                // Randomly select a fairy prefab from the list to spawn.
                if (fairyPrefabs != null && fairyPrefabs.Count > 0)
                {
                    Spawn(fairyPrefabs[Random.Range(0, fairyPrefabs.Count)]);
                }
                timer = spawnInterval;
            }
        }
    }
}
