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

        [Header("Настройки камеры")]
        [Tooltip("Позиция камеры для данного лабиринта")]
        public Vector3 cameraPosition = Vector3.zero;

        [Tooltip("Угол поворота камеры (Euler) для данного лабиринта")]
        public Vector3 cameraRotation = Vector3.zero;

        [Tooltip("Размер камеры (orthographicSize) для данного лабиринта")]
        public float cameraSize = 5f;
    }
}