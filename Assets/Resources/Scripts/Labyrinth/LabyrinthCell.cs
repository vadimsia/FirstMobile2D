using UnityEngine;

namespace Resources.Scripts.Labyrinth
{
    /// <summary>
    /// Represents a single cell in the labyrinth grid.
    /// Stores information about cell borders, unique identifier, and special cell flags.
    /// </summary>
    public class LabyrinthCell
    {
        // Border flags for this cell.
        public bool TopBorder;
        public bool RightBorder;
        public bool BottomBorder;
        public bool LeftBorder;

        // Used to identify connected cells (for merging during maze generation).
        public int ArrayValue;

        // Flags to mark special cells.
        public bool IsStart;
        public bool IsFinish;
        public bool IsSolutionPath;

        /// <summary>
        /// Default constructor.
        /// Initializes all borders to false, ArrayValue to 0 and flags to false.
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
        /// <param name="reference">Reference cell to copy from.</param>
        public void Copy(LabyrinthCell reference)
        {
            RightBorder = reference.RightBorder;
            BottomBorder = reference.BottomBorder;
            ArrayValue = reference.ArrayValue;
        }
    }
}