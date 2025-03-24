using UnityEngine;
using System.Collections.Generic;

namespace Resources.Scripts.Labyrinth
{
    /// <summary>
    /// Generates the labyrinth field, instantiates cell prefabs and places bonus/trap objects.
    /// </summary>
    public class LabyrinthGenerator : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField, Range(4, 20), Tooltip("Number of rows in the labyrinth.")]
        private int rows = 5;
        [SerializeField, Range(4, 20), Tooltip("Number of columns in the labyrinth.")]
        private int cols = 5;
        [SerializeField, Tooltip("Prefab for the labyrinth cell.")]
        private LabyrinthCellPrefab cellPrefab;

        [Header("Cell Settings")]
        [SerializeField, Tooltip("Distance between cells in the labyrinth.")]
        private float cellSize = 1f;

        [Header("Bonus and Trap Settings")]
        [SerializeField, Tooltip("Prefab for bonus objects.")]
        private GameObject bonusPrefab;
        [SerializeField, Tooltip("Number of bonus objects to spawn.")]
        private int bonusCount = 1;
        [SerializeField, Tooltip("Prefab for trap objects.")]
        private GameObject trapPrefab;
        [SerializeField, Tooltip("Number of trap objects to spawn.")]
        private int trapCount = 3;
        [SerializeField, Range(1, 10), Tooltip("Minimum Manhattan distance between bonus/trap placements.")]
        private int minPlacementDistance = 5;

        private LabyrinthField labyrinth;

        /// <summary>
        /// Generates the labyrinth field, instantiates cell objects and places bonus/trap objects.
        /// </summary>
        private void GenerateField()
        {
            List<GameObject> solutionCells = new List<GameObject>();
            List<GameObject> nonSolutionCells = new List<GameObject>();

            // Create cell objects in the grid.
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    LabyrinthCell cell = labyrinth.Field[row, col];
                    // Calculate cell position based on cellSize.
                    Vector2 cellPosition = new Vector2(col * cellSize, -row * cellSize);
                    GameObject cellObj = Instantiate(cellPrefab, cellPosition, Quaternion.identity, transform).gameObject;
                    cellObj.name = "R" + row + "C" + col;
                    cellObj.GetComponent<LabyrinthCellPrefab>().Init(cell);

                    // Collect cells that are not start or finish for bonus/trap placement.
                    if (!cell.IsStart && !cell.IsFinish)
                    {
                        if (cell.IsSolutionPath)
                            solutionCells.Add(cellObj);
                        else
                            nonSolutionCells.Add(cellObj);
                    }
                }
            }

            // Place bonus objects on solution path cells.
            if (solutionCells.Count > 0 && bonusPrefab != null && bonusCount > 0)
            {
                List<GameObject> availableBonusCells = new List<GameObject>(solutionCells);
                int placedBonus = 0;
                while (placedBonus < bonusCount && availableBonusCells.Count > 0)
                {
                    int index = Random.Range(0, availableBonusCells.Count);
                    GameObject bonusCell = availableBonusCells[index];
                    Instantiate(bonusPrefab, bonusCell.transform.position, Quaternion.identity, bonusCell.transform);
                    placedBonus++;

                    // Calculate grid coordinates.
                    int gridRowBonus = Mathf.RoundToInt(-bonusCell.transform.position.y / cellSize);
                    int gridColBonus = Mathf.RoundToInt(bonusCell.transform.position.x / cellSize);

                    availableBonusCells.RemoveAll(cell =>
                    {
                        int cellRow = Mathf.RoundToInt(-cell.transform.position.y / cellSize);
                        int cellCol = Mathf.RoundToInt(cell.transform.position.x / cellSize);
                        return Mathf.Abs(cellRow - gridRowBonus) + Mathf.Abs(cellCol - gridColBonus) < minPlacementDistance;
                    });
                }
            }

            // Place trap objects on non-solution path cells.
            if (nonSolutionCells.Count > 0 && trapPrefab != null && trapCount > 0)
            {
                List<GameObject> availableTrapCells = new List<GameObject>(nonSolutionCells);
                int placedTrap = 0;
                while (placedTrap < trapCount && availableTrapCells.Count > 0)
                {
                    int index = Random.Range(0, availableTrapCells.Count);
                    GameObject trapCell = availableTrapCells[index];
                    Instantiate(trapPrefab, trapCell.transform.position, Quaternion.identity, trapCell.transform);
                    placedTrap++;

                    int gridRowTrap = Mathf.RoundToInt(-trapCell.transform.position.y / cellSize);
                    int gridColTrap = Mathf.RoundToInt(trapCell.transform.position.x / cellSize);

                    availableTrapCells.RemoveAll(cell =>
                    {
                        int cellRow = Mathf.RoundToInt(-cell.transform.position.y / cellSize);
                        int cellCol = Mathf.RoundToInt(cell.transform.position.x / cellSize);
                        return Mathf.Abs(cellRow - gridRowTrap) + Mathf.Abs(cellCol - gridColTrap) < minPlacementDistance;
                    });
                }
            }
        }

        /// <summary>
        /// Initializes the labyrinth field, sets the player spawn point and sets up the mini-map solution path.
        /// </summary>
        private void Start()
        {
            labyrinth = new LabyrinthField(rows, cols);

            // Find the player by tag and set its position to the labyrinth start cell.
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector2 spawnPos = new Vector2(labyrinth.StartCellCoordinates.y * cellSize, -labyrinth.StartCellCoordinates.x * cellSize);
                player.transform.position = spawnPos;
            }

            GenerateField();

            // If LabyrinthMapController exists, configure the mini-map solution path.
            if (LabyrinthMapController.Instance != null)
            {
                List<Vector3> solutionPath = labyrinth.GetSolutionPathWorldPositions();
                LabyrinthMapController.Instance.SetSolutionPath(solutionPath);
            }
        }
    }
}
