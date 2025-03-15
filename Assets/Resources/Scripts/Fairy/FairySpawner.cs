using System.Collections.Generic;
using UnityEngine;

namespace Resources.Scripts.Fairy
{
    /// <summary>
    /// Spawns fairies in the scene at specified intervals.
    /// </summary>
    public class FairySpawner : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> fairyPrefabs;
        [SerializeField, Range(3, 10)]
        private int maxFairies = 5;
        [SerializeField, Range(3, 15)]
        private float spawnTimer = 5f;

        private float timerDelay;

        private void Start()
        {
            timerDelay = spawnTimer;
        }

        /// <summary>
        /// Spawns a fairy prefab at the spawner's position.
        /// </summary>
        /// <param name="prefab">The fairy prefab to spawn.</param>
        private void Spawn(GameObject prefab)
        {
            if (fairyPrefabs.Count == 0)
                return;

            if (transform.childCount >= maxFairies)
                return;

            var fairy = Instantiate(prefab, transform.position, Quaternion.identity, transform)
                .GetComponent<FairyController>();
            fairy.Init(transform.position);
        }

        private void Update()
        {
            timerDelay -= Time.deltaTime;
            if (timerDelay <= 0)
            {
                Spawn(fairyPrefabs[Random.Range(0, fairyPrefabs.Count)]);
                timerDelay = spawnTimer;
            }
        }
    }
}