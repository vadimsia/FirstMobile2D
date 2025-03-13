using UnityEngine;
using System.Collections.Generic;

namespace Resources.Scripts.Labyrinth
{
    public class LabyrinthCell
    {
        public bool topBorder;
        public bool rightBorder;
        public bool bottomBorder;
        public bool leftBorder;
        public int arrayValue;

        // Флаги для обозначения специальных ячеек
        public bool isStart;
        public bool isFinish;
        public bool isSolutionPath;

        public LabyrinthCell()
        {
            topBorder = false;
            rightBorder = false;
            bottomBorder = false;
            leftBorder = false;
            arrayValue = 0;
            isStart = false;
            isFinish = false;
            isSolutionPath = false;
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
        private Vector2Int startCell;
        private Vector2Int finishCell;

        // Публичное свойство для получения координат стартовой ячейки (индексы в сетке)
        public Vector2Int StartCell
        {
            get { return startCell; }
        }

        public LabyrinthField(int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;

            InitField();
            CreateLabytinth();

            // Определяем углы лабиринта
            List<Vector2Int> corners = new List<Vector2Int>()
            {
                new Vector2Int(0, 0),
                new Vector2Int(0, cols - 1),
                new Vector2Int(rows - 1, 0),
                new Vector2Int(rows - 1, cols - 1)
            };

            // Выбираем случайный угол для старта
            int startIndex = Random.Range(0, corners.Count);
            startCell = corners[startIndex];
            corners.RemoveAt(startIndex);

            // Выбираем случайный угол для финиша из оставшихся
            int finishIndex = Random.Range(0, corners.Count);
            finishCell = corners[finishIndex];

            // Помечаем выбранные клетки как старт и финиш
            field[startCell.x, startCell.y].isStart = true;
            field[finishCell.x, finishCell.y].isFinish = true;

            // Вычисляем решающий путь от стартовой клетки до финишной
            SolveMaze();
        }

        void SetBorders()
        {
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
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
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    field[row, col] = new LabyrinthCell();
                }
            }
        }

        bool CanSetBottomBorder(int row, int arrayValue)
        {
            int count = 0;
            int borderCount = 0;
            for (int col = 0; col < cols; col++)
            {
                var cell = field[row, col];
                if (cell.arrayValue == arrayValue)
                    count++;
                if (cell.arrayValue == arrayValue && cell.bottomBorder)
                    borderCount++;
            }
            return count - 1 != borderCount;
        }

        void ChangeArrayValues(int row, int oldArrayValue, int newArrayValue)
        {
            for (int col = 0; col < cols; col++)
            {
                if (field[row, col].arrayValue == oldArrayValue)
                    field[row, col].arrayValue = newArrayValue;
            }
        }

        void PreprocessRow(int row)
        {
            if (row == 0)
                return;

            for (int col = 0; col < cols; col++)
            {
                field[row, col].Copy(field[row - 1, col]);
                field[row, col].rightBorder = false;
                if (field[row, col].bottomBorder)
                {
                    field[row, col].arrayValue = 0;
                    field[row, col].bottomBorder = false;
                }
            }
        }

        void ProcessRow(int row)
        {
            Debug.Log("Process row " + row);
            // Назначаем значение для ячейки, если оно ещё не установлено
            for (int col = 0; col < cols; col++)
            {
                var cell = field[row, col];
                if (cell.arrayValue != 0)
                    continue;
                cell.arrayValue = uniqueCounter;
                uniqueCounter++;
            }

            // Создаём правые стены
            for (int col = 0; col < cols - 1; col++)
            {
                var cell = field[row, col];
                var nextCell = field[row, col + 1];

                if (cell.arrayValue == nextCell.arrayValue && cell.arrayValue != 0)
                {
                    cell.rightBorder = true;
                    continue;
                }
                if (Random.Range(0, 2) == 1)
                {
                    cell.rightBorder = true;
                    continue;
                }
                nextCell.arrayValue = cell.arrayValue;
            }

            // Создаём нижние стены
            for (int col = 0; col < cols; col++)
            {
                var cell = field[row, col];
                if (row == rows - 1)
                {
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
            for (int col = 0; col < cols - 1; col++)
            {
                var cell = field[row, col];
                var nextCell = field[row, col + 1];
                if (cell.arrayValue != nextCell.arrayValue && cell.rightBorder)
                {
                    cell.rightBorder = false;
                    ChangeArrayValues(row, nextCell.arrayValue, cell.arrayValue);
                }
            }
            SetBorders();
        }

        void CreateLabytinth()
        {
            for (int row = 0; row < rows; row++)
            {
                PreprocessRow(row);
                ProcessRow(row);
            }
            PostProcess();
        }

        // Поиск пути методом DFS от стартовой до финишной клетки
        void SolveMaze()
        {
            bool[,] visited = new bool[rows, cols];
            List<Vector2Int> path = new List<Vector2Int>();
            if (DFS(startCell.x, startCell.y, visited, path))
            {
                // Помечаем все ячейки, принадлежащие решающему (верному) пути
                foreach (Vector2Int pos in path)
                {
                    field[pos.x, pos.y].isSolutionPath = true;
                }
            }
        }

        bool DFS(int row, int col, bool[,] visited, List<Vector2Int> path)
        {
            if (row < 0 || row >= rows || col < 0 || col >= cols || visited[row, col])
                return false;

            visited[row, col] = true;
            path.Add(new Vector2Int(row, col));

            // Если достигли финиша
            if (row == finishCell.x && col == finishCell.y)
                return true;

            List<Vector2Int> neighbors = GetNeighbors(row, col);
            foreach (Vector2Int n in neighbors)
            {
                if (!visited[n.x, n.y] && DFS(n.x, n.y, visited, path))
                    return true;
            }
            path.RemoveAt(path.Count - 1);
            return false;
        }

        List<Vector2Int> GetNeighbors(int row, int col)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();
            LabyrinthCell cell = field[row, col];

            // Верхний сосед
            if (row > 0)
            {
                LabyrinthCell neighbor = field[row - 1, col];
                if (!cell.topBorder && !neighbor.bottomBorder)
                    neighbors.Add(new Vector2Int(row - 1, col));
            }
            // Правый сосед
            if (col < cols - 1)
            {
                LabyrinthCell neighbor = field[row, col + 1];
                if (!cell.rightBorder && !neighbor.leftBorder)
                    neighbors.Add(new Vector2Int(row, col + 1));
            }
            // Нижний сосед
            if (row < rows - 1)
            {
                LabyrinthCell neighbor = field[row + 1, col];
                if (!cell.bottomBorder && !neighbor.topBorder)
                    neighbors.Add(new Vector2Int(row + 1, col));
            }
            // Левый сосед
            if (col > 0)
            {
                LabyrinthCell neighbor = field[row, col - 1];
                if (!cell.leftBorder && !neighbor.rightBorder)
                    neighbors.Add(new Vector2Int(row, col - 1));
            }
            return neighbors;
        }
    }
}
