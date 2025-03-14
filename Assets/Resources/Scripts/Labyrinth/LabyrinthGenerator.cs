using UnityEngine;
using System.Collections.Generic;

namespace Resources.Scripts.Labyrinth
{
    public class LabyrinthGenerator : MonoBehaviour
    {
        [SerializeField, Range(4, 20)]
        private int rows = 5;
        [SerializeField, Range(4, 20)]
        private int cols = 5;
        [SerializeField]
        private LabyrinthCellPrefab cellPrefab;

        // Bonus and Trap Settings
        [Header("Bonus and Trap Settings")]
        [SerializeField]
        private GameObject bonusPrefab;
        [SerializeField]
        private int bonusCount = 1;
        [SerializeField]
        private GameObject trapPrefab;
        [SerializeField]
        private int trapCount = 3;

        private LabyrinthField labyrinth;

        /// <summary>
        /// Generates the labyrinth field and instantiates cell objects.
        /// </summary>
        private void GenerateField()
        {
            List<GameObject> solutionCells = new List<GameObject>();
            List<GameObject> nonSolutionCells = new List<GameObject>();

            // Instantiate labyrinth cells
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    var cell = labyrinth.Field[row, col];
                    GameObject cellObj = Instantiate(cellPrefab, new Vector2(col, -row), Quaternion.identity, transform).gameObject;
                    cellObj.name = "R" + row + "C" + col;
                    cellObj.GetComponent<LabyrinthCellPrefab>().Init(cell);

                    // Exclude start and finish cells from bonus/trap placement
                    if (!cell.IsStart && !cell.IsFinish)
                    {
                        if (cell.IsSolutionPath)
                            solutionCells.Add(cellObj);
                        else
                            nonSolutionCells.Add(cellObj);
                    }
                }
            }

            // Place bonus items on solution path with distance check
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

                    // Get grid coordinates (row = -y, col = x)
                    int gridRowBonus = Mathf.RoundToInt(-bonusCell.transform.position.y);
                    int gridColBonus = Mathf.RoundToInt(bonusCell.transform.position.x);

                    availableBonusCells.RemoveAll(cell =>
                    {
                        int cellRow = Mathf.RoundToInt(-cell.transform.position.y);
                        int cellCol = Mathf.RoundToInt(cell.transform.position.x);
                        return Mathf.Abs(cellRow - gridRowBonus) + Mathf.Abs(cellCol - gridColBonus) < 5;
                    });
                }
            }

            // Place traps on non-solution cells with distance check
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

                    int gridRowTrap = Mathf.RoundToInt(-trapCell.transform.position.y);
                    int gridColTrap = Mathf.RoundToInt(trapCell.transform.position.x);

                    availableTrapCells.RemoveAll(cell =>
                    {
                        int cellRow = Mathf.RoundToInt(-cell.transform.position.y);
                        int cellCol = Mathf.RoundToInt(cell.transform.position.x);
                        return Mathf.Abs(cellRow - gridRowTrap) + Mathf.Abs(cellCol - gridColTrap) < 5;
                    });
                }
            }
        }

        /// <summary>
        /// Initializes the labyrinth field and sets the player position to the start cell.
        /// </summary>
        private void Start()
        {
            labyrinth = new LabyrinthField(rows, cols);

            // Find the player by tag "Player" and set its position to the start cell
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector2 spawnPos = new Vector2(labyrinth.StartCellCoordinates.y, -labyrinth.StartCellCoordinates.x);
                player.transform.position = spawnPos;
            }

            GenerateField();
        }
    }
}
