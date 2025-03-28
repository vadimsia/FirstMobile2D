using UnityEngine;

namespace Resources.Scripts.Labyrinth
{
    /// <summary>
    /// Представляет отдельную ячейку лабиринта: содержит флаги границ,
    /// числовое значение для генерации и маркеры специальных ячеек.
    /// </summary>
    public class LabyrinthCell
    {
        // Флаги границ.
        public bool TopBorder;
        public bool RightBorder;
        public bool BottomBorder;
        public bool LeftBorder;

        // Значение, используемое для объединения ячеек при генерации.
        public int ArrayValue;

        // Специальные маркеры ячеек.
        public bool IsStart;
        public bool IsFinish;
        public bool IsSolutionPath;

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public LabyrinthCell()
        {
            TopBorder = false;
            RightBorder = false;
            BottomBorder = false;
            LeftBorder = false;
            ArrayValue = 0;
            IsStart = false;
            IsFinish = false;
            IsSolutionPath = false;
        }

        /// <summary>
        /// Копирует некоторые свойства из другой ячейки.
        /// </summary>
        public void Copy(LabyrinthCell reference)
        {
            RightBorder = reference.RightBorder;
            BottomBorder = reference.BottomBorder;
            ArrayValue = reference.ArrayValue;
        }
    }
}