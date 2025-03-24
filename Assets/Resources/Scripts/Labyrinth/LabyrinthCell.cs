using UnityEngine;

namespace Resources.Scripts.Labyrinth
{
    /// <summary>
    /// Represents a single cell in the labyrinth grid,
    /// including border flags, array value and special cell markers.
    /// </summary>
    public class LabyrinthCell
    {
        // Border flags.
        public bool TopBorder;
        public bool RightBorder;
        public bool BottomBorder;
        public bool LeftBorder;

        // Used to identify connected cells during maze generation.
        public int ArrayValue;

        // Special cell markers.
        public bool IsStart;
        public bool IsFinish;
        public bool IsSolutionPath;

        /// <summary>
        /// Default constructor.
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
        /// Copies selected properties from another cell.
        /// </summary>
        /// <param name="reference">Cell to copy data from.</param>
        public void Copy(LabyrinthCell reference)
        {
            RightBorder = reference.RightBorder;
            BottomBorder = reference.BottomBorder;
            ArrayValue = reference.ArrayValue;
        }
    }
}
