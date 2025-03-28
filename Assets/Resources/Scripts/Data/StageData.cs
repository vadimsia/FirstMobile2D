using UnityEngine;

namespace Resources.Scripts.Data
{
    [System.Serializable]
    public class StageData
    {
        [Header("Общее")]
        [Tooltip("Название этапа")]
        public string stageName;

        [Tooltip("Изображение этапа для отображения в меню")]
        public Sprite stageImage;

        [Header("Сцены")]
        [Tooltip("Название сцены арены")]
        public string arenaSceneName;
        [Tooltip("Название сцены лабиринта")]
        public string labyrinthSceneName;

        [Header("Настройки")]
        [Tooltip("Настройки арены для этого этапа")]
        public ArenaSettings arenaSettings;
        [Tooltip("Настройки лабиринта для этого этапа")]
        public LabyrinthSettings labyrinthSettings;
    }
}