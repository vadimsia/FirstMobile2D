using System;
using UnityEngine;

namespace Resources.Scripts.Data
{
    [Serializable]
    public class StageData
    {
        #region General

        [Header("General")]
        [Tooltip("Name of the stage")]
        public string stageName = string.Empty;

        [Tooltip("Image representing the stage in the menu")]
        public Sprite stageImage = null!;

        #endregion

        #region Scenes

        [Header("Scenes")]
        [Tooltip("Name of the arena scene for this stage")]
        public string arenaSceneName = string.Empty;

        [Tooltip("Name of the labyrinth scene for this stage")]
        public string labyrinthSceneName = string.Empty;

        #endregion

        #region Settings

        [Header("Settings")]
        [Tooltip("Arena settings for this stage")]
        public ArenaSettings arenaSettings = null!;

        [Tooltip("Labyrinth settings for this stage")]
        public LabyrinthSettings labyrinthSettings = null!;

        #endregion
    }
}