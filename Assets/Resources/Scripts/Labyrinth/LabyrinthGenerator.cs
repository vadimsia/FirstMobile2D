using UnityEngine;

namespace Resources.Scripts.Labyrinth
{
    public class LabyrinthGenerator : MonoBehaviour
    {
        [SerializeField, Range(4, 20)] int rows = 5;
        [SerializeField, Range(4, 20)] int cols = 5;
        [SerializeField] LabyrinthCellPrefab cellPrefab;

        LabyrinthField labyrinth;

        void GenerateField()
        {
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    var cell = labyrinth.field[row, col];
                    // Преобразуем индексы в мировые координаты: x = col, y = -row
                    LabyrinthCellPrefab obj = Instantiate(cellPrefab, new Vector2(col, -row), Quaternion.identity, transform);
                    obj.name = "R" + row + "C" + col;
                    obj.Init(cell);
                }
            }
        }

        void Start()
        {
            labyrinth = new LabyrinthField(rows, cols);
            GenerateField();

            // Перемещаем игрока на точку старта лабиринта
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                // Преобразуем координаты: в Instantiate мы использовали (col, -row)
                Vector2 startPos = new Vector2(labyrinth.StartCell.y, -labyrinth.StartCell.x);
                player.transform.position = startPos;
            }
        }
    }
}