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

        [Header("Spawn Constraints")]
        [SerializeField, Tooltip("Minimum distance from the player allowed for enemy spawn.")]
        private float minDistanceFromPlayer = 5f;
        [SerializeField, Tooltip("Minimum distance between spawned enemies.")]
        private float minDistanceBetweenEnemies = 2f;
        [SerializeField, Tooltip("Maximum number of attempts to generate a valid spawn position.")]
        private int maxSpawnAttempts = 10;
        [SerializeField, Tooltip("Layer mask for enemy objects (to check spacing between enemies).")]
        private LayerMask enemyLayerMask;

        [Header("Debug Settings")]
        [SerializeField, Tooltip("Enable debug logging for enemy spawning.")]
        private bool debugLog;

        private int currentEnemyCount;
        private Transform playerTransform;

        private void Start()
        {
            // Пытаемся найти игрока по тегу "Player".
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if(playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            else if (debugLog)
            {
                Debug.LogWarning("Player object not found. Check the tag 'Player'.");
            }

            // Запускаем цикл спавна врагов.
            StartCoroutine(SpawnEnemyLoop());
        }

        /// <summary>
        /// Циклично спавнит врагов с заданным интервалом, если текущее число врагов меньше максимума.
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
        /// Генерирует корректную позицию спавна с учётом заданных ограничений.
        /// </summary>
        /// <returns>Если позиция найдена, то возвращается корректная позиция; иначе исходная позиция спавнера.</returns>
        private Vector3 GetValidSpawnPosition()
        {
            Vector3 candidatePos = transform.position;
            int attempts = 0;
            bool valid = false;

            while (attempts < maxSpawnAttempts && !valid)
            {
                // Если включена случайная позиция - рассчитываем позицию в пределах spawnRadius
                if (randomizeSpawnPosition)
                {
                    Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
                    candidatePos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
                }
                else
                {
                    candidatePos = transform.position;
                }

                valid = true;

                // Проверка расстояния до игрока (если игрок найден)
                if (playerTransform != null)
                {
                    float distanceToPlayer = Vector3.Distance(candidatePos, playerTransform.position);
                    if (distanceToPlayer < minDistanceFromPlayer)
                    {
                        valid = false;
                        if(debugLog)
                        {
                            Debug.Log("Candidate spawn position too close to player: " + candidatePos + " (Distance: " + distanceToPlayer + ")");
                        }
                    }
                }

                // Проверка расстояния между врагами
                Collider2D hit = Physics2D.OverlapCircle(candidatePos, minDistanceBetweenEnemies, enemyLayerMask);
                if (hit != null)
                {
                    valid = false;
                    if(debugLog)
                    {
                        Debug.Log("Candidate spawn position too close to another enemy: " + candidatePos + " Overlap with: " + hit.name);
                    }
                }

                attempts++;
            }

            if (!valid && debugLog)
            {
                Debug.LogWarning("Could not find valid spawn position after " + attempts + " attempts, using fallback position.");
            }

            return candidatePos;
        }

        /// <summary>
        /// Спавнит врага по рассчитанной позиции.
        /// </summary>
        private void SpawnEnemy()
        {
            Vector3 spawnPosition = GetValidSpawnPosition();
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            currentEnemyCount++;

            if (debugLog)
            {
                Debug.Log("Spawned enemy at " + spawnPosition + ". Total count: " + currentEnemyCount);
            }
        }

        /// <summary>
        /// Уменьшает счётчик врагов. Вызывается, когда враг уничтожается.
        /// </summary>
        public void OnEnemyDestroyed()
        {
            currentEnemyCount = Mathf.Max(0, currentEnemyCount - 1);
        }
    }
}
