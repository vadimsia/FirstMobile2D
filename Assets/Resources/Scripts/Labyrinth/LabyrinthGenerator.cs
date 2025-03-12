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
            for (int row = 0; row < rows; row++) {
                for (int col = 0; col < cols; col++) {
                    var cell = labyrinth.field[row, col];
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
        }

        void Update()
        {
    
        }
    }
}
