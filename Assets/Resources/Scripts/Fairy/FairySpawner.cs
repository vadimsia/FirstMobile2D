using System.Collections.Generic;
using UnityEngine;

namespace Resources.Scripts.Fairy
{
    public class FairySpawner : MonoBehaviour
    {
        [SerializeField] private List<GameObject> fairyPrefabs;
        [SerializeField, Range(3, 10)] private int maxFairies = 5;
        [SerializeField, Range(3, 15)] private float spawnTimer = 5f;

        private float timerDelay;
    
        private void Start()
        {
            timerDelay = spawnTimer;

        }

        private void Spawn(GameObject prefab) {
            if (fairyPrefabs.Count == 0) {
                return;
            }

            if (transform.childCount >= maxFairies) {
                return;
            }

            var fairy = Instantiate(prefab, transform.position, Quaternion.identity, transform).GetComponent<FairyController>();
            fairy.Init(transform.position);
        }

        private void Update()
        {
            timerDelay -= Time.deltaTime;
            if (timerDelay <= 0) {
                Spawn(fairyPrefabs[Random.Range(0, fairyPrefabs.Count - 1)]);
                timerDelay = spawnTimer;
            } 
        }
    }
}
