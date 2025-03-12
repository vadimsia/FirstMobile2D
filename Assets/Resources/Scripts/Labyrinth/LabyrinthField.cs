using UnityEngine;

namespace Resources.Scripts.Labyrinth
{
    public class LabyrinthCell
    {
        public bool topBorder;
        public bool rightBorder;
        public bool bottomBorder;
        public bool leftBorder;
        public int arrayValue;

        public LabyrinthCell()
        {
            topBorder = false;
            rightBorder = false;
            bottomBorder = false;
            leftBorder = false;
            arrayValue = 0;
        }

        public void Copy(LabyrinthCell reference)
        {
            rightBorder = reference.rightBorder;
            bottomBorder = reference.bottomBorder;
            arrayValue = reference.arrayValue;
        }

        public void DebugLog()
        {
            Debug.Log("Top: " + topBorder);
            Debug.Log("Right: " + rightBorder);
            Debug.Log("Bottom: " + bottomBorder);
            Debug.Log("Left: " + leftBorder);
            Debug.Log("ArrayValue: " + arrayValue);
        }
    }

    public class LabyrinthField
    {
        public LabyrinthCell[,] field;

        public readonly int rows;
        public readonly int cols;

        int uniqueCounter = 1;

        void SetBorders()
        {
            for (int row = 0; row < rows; row++) {
                for (int col = 0; col < cols; col++) {
                    var cell = field[row, col];

                    if (row == 0)
                        cell.topBorder = true;
                    if (col == 0)
                        cell.leftBorder = true;
                    if (col == cols - 1)
                        cell.rightBorder = true;
                }
            }
        }

        void InitField()
        {
            field = new LabyrinthCell[rows, cols];
            for (int row = 0; row < rows; row++) {
                for (int col = 0; col < cols; col++) {
                    field[row, col] = new LabyrinthCell();
                }
            }
        }

        bool CanSetBottomBorder(int row, int arrayValue)
        {
            int count = 0;
            int borderCount = 0;
            for (int col = 0; col < cols; col++) {
                var cell = field[row, col];
                if (cell.arrayValue == arrayValue)
                    count++;
                if (cell.arrayValue == arrayValue && cell.bottomBorder)
                    borderCount++;
            }

            if (count - 1 == borderCount)
                return false;

            return true;
        }

        void ChangeArrayValues(int row, int oldArrayValye, int newArrayValue)
        {

            for (int col = 0; col < cols; col++) {
                if (field[row, col].arrayValue == oldArrayValye)
                    field[row, col].arrayValue = newArrayValue;
            }
        }

        void PreprocessRow(int row)
        {
            if (row == 0)
                return;

            for (int col = 0; col < cols; col++) {
                field[row, col].Copy(field[row - 1, col]);

                field[row, col].rightBorder = false;
                if (field[row, col].bottomBorder) {
                    field[row, col].arrayValue = 0;
                    field[row, col].bottomBorder = false;
                }
            }        
        }

        void ProcessRow(int row)
        {
            Debug.Log("Process row");
            // Set array value
            for (int col = 0; col < cols; col++) {
                var cell = field[row, col];

                if (cell.arrayValue != 0)
                    continue;
        
                // Set prev cell value + 1
                cell.arrayValue = uniqueCounter;
                uniqueCounter++;
            }

            // Create right borders
            for (int col = 0; col < cols - 1; col++) {
                var cell = field[row, col];
                var nextCell = field[row, col + 1];

                if (cell.arrayValue == nextCell.arrayValue && cell.arrayValue != 0) {
                    cell.rightBorder = true;
                    continue;
                }

                // Create border if 1
                if (Random.Range(0, 2) == 1) {
                    cell.rightBorder = true;
                    continue;
                }

                nextCell.arrayValue = cell.arrayValue;
            }

            // Create bottom borders
            for (int col = 0; col < cols; col++) {
                var cell = field[row, col];

                // Last row
                if (row == rows - 1) {
                    cell.bottomBorder = true;
                    continue;
                }

                if (!CanSetBottomBorder(row, cell.arrayValue))
                    continue;

                if (Random.Range(0, 2) == 1)
                    cell.bottomBorder = true;
            }
        }

        void PostProcess()
        {
            int row = rows - 1;
            for (int col = 0; col < cols - 1; col++) {
                var cell = field[row, col];
                var nextCell = field[row, col + 1];

                if (cell.arrayValue != nextCell.arrayValue && cell.rightBorder) {
                    cell.rightBorder = false;

                    ChangeArrayValues(row, nextCell.arrayValue, cell.arrayValue);
                }
            }

            SetBorders();
        }

        void CreateLabytinth()
        {
            for (int row = 0; row < rows; row++) {
                PreprocessRow(row);
                ProcessRow(row);
            }

            PostProcess();
        }

        public LabyrinthField(int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;

            InitField();
            CreateLabytinth();
        }
    }
}