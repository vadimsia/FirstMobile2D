using UnityEngine;

namespace Resources.Scripts.Data
{
    [CreateAssetMenu(fileName = "LabyrinthSettings", menuName = "GameSettings/Labyrinth Settings")]
    public class LabyrinthSettings : ScriptableObject
    {
        [Header("Параметры лабиринта")]
        [Tooltip("Количество строк")]
        public int rows = 5;

        [Tooltip("Количество столбцов")]
        public int cols = 5;

        [Tooltip("Размер ячейки по X")]
        public float cellSizeX = 1f;

        [Tooltip("Размер ячейки по Y")]
        public float cellSizeY = 1f;

        [Tooltip("Время на прохождение лабиринта (в секундах)")]
        public float labyrinthTimeLimit = 30f;
    }
}