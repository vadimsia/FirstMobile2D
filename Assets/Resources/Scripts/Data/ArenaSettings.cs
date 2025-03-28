using UnityEngine;

namespace Resources.Scripts.Data
{
    [CreateAssetMenu(fileName = "ArenaSettings", menuName = "GameSettings/Arena Settings")]
    public class ArenaSettings : ScriptableObject
    {
        [Header("Параметры арены")]
        [Tooltip("Размер арены (например, радиус или длина стороны)")]
        public float arenaSize = 10f;

        [Tooltip("Время выживания на арене (в секундах)")]
        public float survivalTime = 10f;

        [Tooltip("Количество врагов на арене")]
        public int enemyCount = 5;

        [Tooltip("Типы врагов (префабы)")]
        public GameObject[] enemyPrefabs;
    }
}