using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using Resources.Scripts.Data;
using Resources.Scripts.Misc;
namespace Resources.Scripts.Labyrinth
{
    public class LabyrinthGenerator : MonoBehaviour
    {
        [Header("Параметры лабиринта по умолчанию (на случай отсутствия настроек)")]
        [SerializeField, Range(4, 20)]
        private int defaultRows = 5;
        [SerializeField, Range(4, 20)]
        private int defaultCols = 5;
        [SerializeField]
        private float defaultCellSizeX = 1f;
        [SerializeField]
        private float defaultCellSizeY = 1f;
        [SerializeField]
        private float defaultTimeLimit = 30f;

        [Header("Прочие ссылки")]
        [SerializeField, Tooltip("Префаб для ячейки лабиринта")]
        private LabyrinthCellPrefab cellPrefab;
        [SerializeField, Tooltip("Префаб бонуса")]
        private GameObject bonusPrefab;
        [SerializeField, Tooltip("Префаб ловушки")]
        private GameObject trapPrefab;
        [SerializeField, Range(1, 10)]
        private int minPlacementDistance = 5;
        [SerializeField, Tooltip("Количество бонусов")]
        private int bonusCount = 1;
        [SerializeField, Tooltip("Количество ловушек")]
        private int trapCount = 3;

        [Header("UI Таймер")]
        [SerializeField] private TextMeshProUGUI timerText;

        // Время на прохождение лабиринта
        private float labyrinthTimer;

        private LabyrinthField labyrinth;
        private int rows, cols;
        private float cellSizeX, cellSizeY;

        private void Start()
        {
            // Получаем настройки лабиринта из выбранного этапа, если они заданы
            if (GameStageManager.currentStageData != null && GameStageManager.currentStageData.labyrinthSettings != null)
            {
                rows = GameStageManager.currentStageData.labyrinthSettings.rows;
                cols = GameStageManager.currentStageData.labyrinthSettings.cols;
                cellSizeX = GameStageManager.currentStageData.labyrinthSettings.cellSizeX;
                cellSizeY = GameStageManager.currentStageData.labyrinthSettings.cellSizeY;
                labyrinthTimer = GameStageManager.currentStageData.labyrinthSettings.labyrinthTimeLimit;
            }
            else
            {
                rows = defaultRows;
                cols = defaultCols;
                cellSizeX = defaultCellSizeX;
                cellSizeY = defaultCellSizeY;
                labyrinthTimer = defaultTimeLimit;
            }

            labyrinth = new LabyrinthField(rows, cols, cellSizeX, cellSizeY);
            GenerateField();

            // Устанавливаем позицию игрока на стартовую ячейку лабиринта.
            GameObject player = GameObject.FindGameObjectWithTag(ETag.Player.ToString());
            if (player != null)
            {
                player.transform.position = labyrinth.GetStartWorldPosition();
            }

            if (LabyrinthMapController.Instance != null)
            {
                List<Vector3> solutionPath = labyrinth.GetSolutionPathWorldPositions();
                LabyrinthMapController.Instance.SetSolutionPath(solutionPath);
            }
        }

        private void GenerateField()
        {
            List<GameObject> solutionCells = new List<GameObject>();
            List<GameObject> nonSolutionCells = new List<GameObject>();

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    LabyrinthCell cell = labyrinth.Field[row, col];
                    Vector2 cellPosition = new Vector2(col * cellSizeX, -row * cellSizeY);
                    GameObject cellObj = Instantiate(cellPrefab, cellPosition, Quaternion.identity, transform).gameObject;
                    cellObj.name = "R" + row + "C" + col;

                    cellObj.GetComponent<LabyrinthCellPrefab>().Init(cell);
                    cellObj.GetComponent<LabyrinthCellPrefab>().SendMessage("setCellSizeY", cellSizeY, SendMessageOptions.DontRequireReceiver);

                    if (!cell.IsStart && !cell.IsFinish)
                    {
                        if (cell.IsSolutionPath)
                            solutionCells.Add(cellObj);
                        else
                            nonSolutionCells.Add(cellObj);
                    }
                }
            }

            // Расстановка бонусов
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

                    int gridRowBonus = Mathf.RoundToInt(-bonusCell.transform.position.y / cellSizeY);
                    int gridColBonus = Mathf.RoundToInt(bonusCell.transform.position.x / cellSizeX);

                    availableBonusCells.RemoveAll(cell =>
                    {
                        int cellRow = Mathf.RoundToInt(-cell.transform.position.y / cellSizeY);
                        int cellCol = Mathf.RoundToInt(cell.transform.position.x / cellSizeX);
                        return Mathf.Abs(cellRow - gridRowBonus) + Mathf.Abs(cellCol - gridColBonus) < minPlacementDistance;
                    });
                }
            }

            // Расстановка ловушек
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

                    int gridRowTrap = Mathf.RoundToInt(-trapCell.transform.position.y / cellSizeY);
                    int gridColTrap = Mathf.RoundToInt(trapCell.transform.position.x / cellSizeX);

                    availableTrapCells.RemoveAll(cell =>
                    {
                        int cellRow = Mathf.RoundToInt(-cell.transform.position.y / cellSizeY);
                        int cellCol = Mathf.RoundToInt(cell.transform.position.x / cellSizeX);
                        return Mathf.Abs(cellRow - gridRowTrap) + Mathf.Abs(cellCol - gridColTrap) < minPlacementDistance;
                    });
                }
            }
        }

        private void Update()
        {
            labyrinthTimer -= Time.deltaTime;
            UpdateTimerUI();

            if (labyrinthTimer <= 0f)
            {
                // Если время вышло – возвращаем игрока на арену
                LoadArenaScene();
            }
        }

        private void UpdateTimerUI()
        {
            if (timerText != null)
            {
                timerText.text = $"Время: {labyrinthTimer:F1}";
            }
        }

        public void OnLabyrinthComplete()
        {
            SceneManager.LoadScene("Menu");
        }

        private void LoadArenaScene()
        {
            if (GameStageManager.currentStageData != null)
            {
                SceneManager.LoadScene(GameStageManager.currentStageData.arenaSceneName);
            }
        }
    }
}
