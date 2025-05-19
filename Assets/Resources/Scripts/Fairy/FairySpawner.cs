using System.Collections.Generic;
using UnityEngine;
using Resources.Scripts.Data;
using Resources.Scripts.GameManagers;

namespace Resources.Scripts.Fairy
{
    [DisallowMultipleComponent]
    public class FairySpawner : MonoBehaviour
    {
        [Header("Spawning Settings")]
        [SerializeField, Tooltip("ArenaSettings с кол-вом и префабами фей")]
        private ArenaSettings arenaSettings;

        [Header("Spawn Timing")]
        [SerializeField, Range(1f, 30f), Tooltip("Интервал спавна (сек)")]
        private float spawnInterval = 5f;

        [Header("Spawn Area")]
        [SerializeField, Tooltip("Радиус случайного спавна")]
        private float spawnAreaRadius;
        [SerializeField, Tooltip("Включить рандомизацию позиции")]
        private bool randomizeSpawnPosition;

        [Header("Debug")]
        [SerializeField, Tooltip("Логировать спавны")]
        private bool debugLog;

        private List<GameObject> fairyPrefabs = new List<GameObject>();
        private int maxFairies = 5;
        private float spawnTimer;

        private static readonly int ProgressID = Shader.PropertyToID("_Progress");
        private static readonly int AtrPosID   = Shader.PropertyToID("_AtrPos");

        private void Awake()
        {
            spawnTimer = spawnInterval;
            if (arenaSettings == null)
            {
                Debug.LogWarning("Нужен ArenaSettings.", this);
                return;
            }

            maxFairies = Mathf.Max(1, arenaSettings.fairyCount);
            if (arenaSettings.fairyPrefabs != null && arenaSettings.fairyPrefabs.Length > 0)
                fairyPrefabs = new List<GameObject>(arenaSettings.fairyPrefabs);
            else
                Debug.LogWarning("Нет префабов фей в ArenaSettings.", this);
        }

        private void Update()
        {
            if (fairyPrefabs.Count == 0) return;
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                Spawn(fairyPrefabs[Random.Range(0, fairyPrefabs.Count)]);
                spawnTimer = spawnInterval;
            }
        }

        private void Spawn(GameObject prefab)
        {
            if (prefab == null || transform.childCount >= maxFairies) return;

            var pos = transform.position;
            if (randomizeSpawnPosition && spawnAreaRadius > 0f)
            {
                var off = Random.insideUnitCircle * spawnAreaRadius;
                pos += new Vector3(off.x, off.y, 0f);
            }

            var fx = CharacterScaleManager.Factory.CreateCharacter(prefab, pos, Quaternion.identity);
            fx.transform.SetParent(transform, worldPositionStays: true);
            if (fx.TryGetComponent(out FairyController fairy))
                fairy.Init(pos);

            if (debugLog)
                Debug.Log($"Spawned fairy at {pos} ({transform.childCount}/{maxFairies})", this);
        }
    }
}
