using UnityEngine;

namespace Resources.Scripts.Data
{
    [System.Serializable]
    public class ObstacleTypeSettings {
        [Tooltip("Уникальное имя препятствия")]
        public string obstacleName;
        
        [Tooltip("Префаб препятствия (с SpriteRenderer и Collider)")]
        public GameObject obstaclePrefab;
        
        [Tooltip("Вероятность спавна (от 0 до 1) для данного типа")]
        [Range(0f, 1f)]
        public float spawnProbability = 1f;
        
        [Tooltip("Минимальное количество, которое может появиться")]
        public int minCount = 1;
        
        [Tooltip("Максимальное количество, которое может появиться")]
        public int maxCount = 3;
        
        [Tooltip("Минимальный масштаб при спавне")]
        public float minScale = 1f;
        
        [Tooltip("Максимальный масштаб при спавне")]
        public float maxScale = 1f;
    }

    [CreateAssetMenu(fileName = "ArenaSettings", menuName = "GameSettings/Arena Settings")]
    public class ArenaSettings : ScriptableObject {
        [Header("Параметры арены")]
        [Tooltip("Время выживания на арене (в секундах)")]
        public float survivalTime = 10f;

        [Tooltip("Количество врагов на арене")]
        public int enemyCount = 5;

        [Tooltip("Префабы врагов")]
        public GameObject[] enemyPrefabs;

        [Header("Параметры фей")]
        [Tooltip("Количество фей на арене")]
        public int fairyCount = 3;

        [Tooltip("Префабы фей")]
        public GameObject[] fairyPrefabs;
        
        [Header("Параметры препятствий")]
        [Tooltip("Список настроек для препятствий, которые будут спавниться на арене")]
        public ObstacleTypeSettings[] obstacleTypes;

        [Header("Дополнительные параметры препятствий")]
        [Tooltip("Минимальное расстояние между препятствиями")]
        public float obstacleMinDistance = 1f;
    }
}