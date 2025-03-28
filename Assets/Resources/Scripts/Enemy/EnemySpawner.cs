using UnityEngine;
using System.Collections;

namespace Resources.Scripts.Enemy
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawning Settings")]
        [SerializeField, Tooltip("Prefab of the enemy to spawn.")]
        private GameObject enemyPrefab;
        [SerializeField, Tooltip("Time interval between enemy spawns (in seconds).")]
        private float spawnInterval = 5f;
        [SerializeField, Tooltip("Maximum number of enemies allowed at once.")]
        private int maxEnemies = 10;
        [SerializeField, Tooltip("Radius of the spawn area.")]
        private float spawnRadius = 10f;
        [SerializeField, Tooltip("Enable random spawn positions within the spawn area.")]
        private bool randomizeSpawnPosition = true;

        [Header("Debug Settings")]
        [SerializeField, Tooltip("Enable debug logging for enemy spawning.")]
        private bool debugLog = false;

        private int currentEnemyCount = 0;

        private void Start()
        {
            // Start the enemy spawning loop
            StartCoroutine(SpawnEnemyLoop());
        }

        /// <summary>
        /// Continuously spawns enemies based on the spawn interval and current enemy count.
        /// </summary>
        private IEnumerator SpawnEnemyLoop()
        {
            while (true)
            {
                if (currentEnemyCount < maxEnemies && enemyPrefab != null)
                {
                    SpawnEnemy();
                }
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        /// <summary>
        /// Spawns an enemy at a random or fixed position within the spawn area.
        /// </summary>
        private void SpawnEnemy()
        {
            Vector3 spawnPosition = transform.position;
            if (randomizeSpawnPosition)
            {
                // Calculate a random position within the defined spawn radius
                Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
                spawnPosition += new Vector3(randomOffset.x, randomOffset.y, 0f);
            }

            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            currentEnemyCount++;

            if (debugLog)
            {
                Debug.Log("Spawned enemy. Total count: " + currentEnemyCount);
            }
        }

        /// <summary>
        /// Decreases the enemy count, intended to be called when an enemy is destroyed.
        /// </summary>
        public void OnEnemyDestroyed()
        {
            currentEnemyCount = Mathf.Max(0, currentEnemyCount - 1);
        }
    }
}
