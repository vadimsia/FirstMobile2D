using UnityEngine;
using System.Collections.Generic;

namespace Resources.Scripts.Labyrinth
{
    /// <summary>
    /// Represents a single cell in the labyrinth grid.
    /// </summary>
    public class LabyrinthCell
    {
        // Border flags for this cell
        public bool TopBorder;
        public bool RightBorder;
        public bool BottomBorder;
        public bool LeftBorder;

        // Used to identify connected cells
        public int ArrayValue;

        // Flags to mark special cells
        public bool IsStart;
        public bool IsFinish;
        public bool IsSolutionPath;

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

    /// <summary>
    /// Generates and manages the labyrinth field.
    /// </summary>
    public class LabyrinthField
    {
        public LabyrinthCell[,] Field;
        public int Rows { get; }
        public int Cols { get; }

        private int uniqueCounter = 1;
        private Vector2Int startCell;
        private Vector2Int finishCell;

        /// <summary>
        /// Gets the coordinates of the start cell.
        /// </summary>
        public Vector2Int StartCellCoordinates => startCell;

        /// <summary>
        /// Constructor that initializes and creates the labyrinth.
        /// </summary>
        /// <param name="rows">Number of rows in the labyrinth.</param>
        /// <param name="cols">Number of columns in the labyrinth.</param>
        public LabyrinthField(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            InitField();
            CreateLabyrinth();
            SetStartAndFinish();
            SolveMaze();
        }

        /// <summary>
        /// Initializes the labyrinth field with new cells.
        /// </summary>
        private void InitField()
        {
            Field = new LabyrinthCell[Rows, Cols];
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    Field[row, col] = new LabyrinthCell();
                }
            }
        }

        /// <summary>
        /// Sets the outer borders for the labyrinth.
        /// </summary>
        private void SetBorders()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    var cell = Field[row, col];
                    if (row == 0)
                        cell.TopBorder = true;
                    if (col == 0)
                        cell.LeftBorder = true;
                    if (col == Cols - 1)
                        cell.RightBorder = true;
                }
            }
        }

        /// <summary>
        /// Determines whether a bottom border can be set for cells with the given array value.
        /// </summary>
        private bool CanSetBottomBorder(int row, int arrayValue)
        {
            int count = 0;
            int borderCount = 0;
            for (int col = 0; col < Cols; col++)
            {
                var cell = Field[row, col];
                if (cell.ArrayValue == arrayValue)
                {
                    count++;
                    if (cell.BottomBorder)
                        borderCount++;
                }
            }
            return count - 1 != borderCount;
        }

        /// <summary>
        /// Changes the array values for all cells in the specified row.
        /// </summary>
        private void ChangeArrayValues(int row, int oldArrayValue, int newArrayValue)
        {
            for (int col = 0; col < Cols; col++)
            {
                if (Field[row, col].ArrayValue == oldArrayValue)
                    Field[row, col].ArrayValue = newArrayValue;
            }
        }

        /// <summary>
        /// Preprocesses a row before generating its maze structure.
        /// </summary>
        private void PreprocessRow(int row)
        {
            if (row == 0)
                return;

            for (int col = 0; col < Cols; col++)
            {
                Field[row, col].Copy(Field[row - 1, col]);
                Field[row, col].RightBorder = false;
                if (Field[row, col].BottomBorder)
                {
                    Field[row, col].ArrayValue = 0;
                    Field[row, col].BottomBorder = false;
                }
            }
        }

        /// <summary>
        /// Processes a row by assigning array values and random borders.
        /// </summary>
        private void ProcessRow(int row)
        {
            // Assign unique array values where needed
            for (int col = 0; col < Cols; col++)
            {
                var cell = Field[row, col];
                if (cell.ArrayValue == 0)
                    cell.ArrayValue = uniqueCounter++;
            }

            // Decide right borders and merge cells
            for (int col = 0; col < Cols - 1; col++)
            {
                var cell = Field[row, col];
                var nextCell = Field[row, col + 1];

                if (cell.ArrayValue == nextCell.ArrayValue && cell.ArrayValue != 0)
                {
                    cell.RightBorder = true;
                }
                else if (Random.Range(0, 2) == 1)
                {
                    cell.RightBorder = true;
                }
                else
                {
                    nextCell.ArrayValue = cell.ArrayValue;
                }
            }

            // Decide bottom borders randomly if allowed
            for (int col = 0; col < Cols; col++)
            {
                var cell = Field[row, col];
                if (row == Rows - 1)
                {
                    cell.BottomBorder = true;
                    continue;
                }
                if (!CanSetBottomBorder(row, cell.ArrayValue))
                    continue;
                if (Random.Range(0, 2) == 1)
                    cell.BottomBorder = true;
            }
        }

        /// <summary>
        /// Final adjustments to ensure proper maze connectivity.
        /// </summary>
        private void PostProcess()
        {
            int lastRow = Rows - 1;
            for (int col = 0; col < Cols - 1; col++)
            {
                var cell = Field[lastRow, col];
                var nextCell = Field[lastRow, col + 1];
                if (cell.ArrayValue != nextCell.ArrayValue && cell.RightBorder)
                {
                    cell.RightBorder = false;
                    ChangeArrayValues(lastRow, nextCell.ArrayValue, cell.ArrayValue);
                }
            }
            SetBorders();
        }

        /// <summary>
        /// Creates the labyrinth by processing all rows.
        /// </summary>
        private void CreateLabyrinth()
        {
            for (int row = 0; row < Rows; row++)
            {
                PreprocessRow(row);
                ProcessRow(row);
            }
            PostProcess();
        }

        /// <summary>
        /// Randomly selects and marks the start and finish cells from the four corners.
        /// </summary>
        private void SetStartAndFinish()
        {
            List<Vector2Int> corners = new List<Vector2Int>
            {
                new Vector2Int(0, 0),
                new Vector2Int(0, Cols - 1),
                new Vector2Int(Rows - 1, 0),
                new Vector2Int(Rows - 1, Cols - 1)
            };

            int startIndex = Random.Range(0, corners.Count);
            startCell = corners[startIndex];
            corners.RemoveAt(startIndex);

            int finishIndex = Random.Range(0, corners.Count);
            finishCell = corners[finishIndex];

            Field[startCell.x, startCell.y].IsStart = true;
            Field[finishCell.x, finishCell.y].IsFinish = true;
        }

        /// <summary>
        /// Marks the solution path (shortest path) in the labyrinth using Breadth-First Search (BFS).
        /// </summary>
        private void SolveMaze()
        {
            List<Vector2Int> shortestPath = FindShortestPath();
            foreach (Vector2Int pos in shortestPath)
            {
                Field[pos.x, pos.y].IsSolutionPath = true;
            }
        }

        /// <summary>
        /// Finds the shortest path from start to finish using BFS.
        /// </summary>
        /// <returns>A list of cell coordinates representing the path.</returns>
        private List<Vector2Int> FindShortestPath()
        {
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            bool[,] visited = new bool[Rows, Cols];
            Vector2Int[,] prev = new Vector2Int[Rows, Cols];

            // Initialize previous positions
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Cols; j++)
                    prev[i, j] = new Vector2Int(-1, -1);

            queue.Enqueue(startCell);
            visited[startCell.x, startCell.y] = true;
            bool found = false;

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                if (current == finishCell)
                {
                    found = true;
                    break;
                }

                foreach (Vector2Int neighbor in GetNeighbors(current.x, current.y))
                {
                    if (!visited[neighbor.x, neighbor.y])
                    {
                        visited[neighbor.x, neighbor.y] = true;
                        prev[neighbor.x, neighbor.y] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            List<Vector2Int> path = new List<Vector2Int>();
            if (!found)
                return path;

            Vector2Int curPos = finishCell;
            while (curPos.x != -1 && curPos.y != -1)
            {
                path.Add(curPos);
                curPos = prev[curPos.x, curPos.y];
            }
            path.Reverse();
            return path;
        }

        /// <summary>
        /// Returns the accessible neighbor cells for the cell at the specified row and column.
        /// </summary>
        private List<Vector2Int> GetNeighbors(int row, int col)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();
            LabyrinthCell cell = Field[row, col];

            if (row > 0 && !cell.TopBorder && !Field[row - 1, col].BottomBorder)
                neighbors.Add(new Vector2Int(row - 1, col));
            if (col < Cols - 1 && !cell.RightBorder && !Field[row, col + 1].LeftBorder)
                neighbors.Add(new Vector2Int(row, col + 1));
            if (row < Rows - 1 && !cell.BottomBorder && !Field[row + 1, col].TopBorder)
                neighbors.Add(new Vector2Int(row + 1, col));
            if (col > 0 && !cell.LeftBorder && !Field[row, col - 1].RightBorder)
                neighbors.Add(new Vector2Int(row, col - 1));

            return neighbors;
        }

        /// <summary>
        /// Returns the world position for the finish cell (assumes each cell is 1 unit in size).
        /// </summary>
        public Vector3 GetFinishWorldPosition()
        {
            return new Vector3(finishCell.x, finishCell.y, 0f);
        }
    }
}
