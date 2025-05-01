using UnityEngine;

namespace Resources.Scripts.Data
{
    [CreateAssetMenu(fileName = "LabyrinthSettings", menuName = "GameSettings/Labyrinth Settings")]
    public sealed class LabyrinthSettings : ScriptableObject
    {
        #region Maze Settings

        [Header("Labyrinth Dimensions")]
        [Tooltip("Number of rows in the labyrinth")]
        [Min(1)]
        public int rows = 5;

        [Tooltip("Number of columns in the labyrinth")]
        [Min(1)]
        public int cols = 5;

        [Header("Cell Size")]
        [Tooltip("Width of each cell (X axis)")]
        [Min(0.1f)]
        public float cellSizeX = 1f;

        [Tooltip("Height of each cell (Y axis)")]
        [Min(0.1f)]
        public float cellSizeY = 1f;

        [Header("Time Limit")]
        [Tooltip("Time limit to complete the labyrinth (in seconds)")]
        [Min(0f)]
        public float labyrinthTimeLimit = 30f;

        #endregion

        #region Camera Settings

        [Header("Camera Position")]
        [Tooltip("World-space position of the camera for this labyrinth")]
        public Vector3 cameraPosition = Vector3.zero;

        [Tooltip("Euler rotation of the camera for this labyrinth")]
        public Vector3 cameraRotation = Vector3.zero;

        [Tooltip("Orthographic size of the camera for this labyrinth")]
        [Min(0.1f)]
        public float cameraSize = 5f;

        #endregion
    }
}