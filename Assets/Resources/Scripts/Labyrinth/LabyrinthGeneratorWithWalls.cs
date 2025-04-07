using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Resources.Scripts.Data;
using Resources.Scripts.Misc;

namespace Resources.Scripts.Labyrinth
{
    /// <summary>
    /// Генерирует лабиринт, создавая объекты стен, бонусов и ловушек.
    /// Позиции стен вычисляются автоматически, а также задаётся возможность настройки смещения.
    /// Теперь стены получают коллайдеры, чтобы игрок не мог проходить сквозь них.
    /// При генерации вертикальных стен (левых и правых) производится проверка: если в ячейке ниже присутствует хотя бы одна стена,
    /// то используется вариант без изометричного вида, чтобы стены сочетались друг с другом.
    /// </summary>
    public class LabyrinthGeneratorWithWalls : MonoBehaviour
    {
        [Header("Настройки лабиринта (ScriptableObject)")]
        [Tooltip("Объект настроек лабиринта")]
        [SerializeField]
        private LabyrinthSettings labyrinthSettings;

        [Header("Параметры лабиринта (резервные, если нет настроек)")]
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

        [Header("Префабы стен")]
        [SerializeField, Tooltip("Префаб для верхней стены")]
        private GameObject topWallPrefab;
        [SerializeField, Tooltip("Префаб для нижней стены")]
        private GameObject bottomWallPrefab;
        [SerializeField, Tooltip("Префаб для левой стены с изометрией")]
        private GameObject leftWallPrefab;
        [SerializeField, Tooltip("Префаб для правой стены с изометрией")]
        private GameObject rightWallPrefab;
        [SerializeField, Tooltip("Префаб для левой стены без изометрии (dontIso)")]
        private GameObject leftNoIsoWallPrefab;
        [SerializeField, Tooltip("Префаб для правой стены без изометрии (dontIso)")]
        private GameObject rightNoIsoWallPrefab;

        [Header("Настройка смещения стен")]
        [SerializeField, Tooltip("Дополнительное смещение для верхней стены по оси Y")]
        public float topWallSpacingY = 0f;
        [SerializeField, Tooltip("Дополнительное смещение для нижней стены по оси Y")]
        public float bottomWallSpacingY = 0f;
        [SerializeField, Tooltip("Дополнительное смещение для левой стены по оси X")]
        public float leftWallSpacingX = 0f;
        [SerializeField, Tooltip("Дополнительное смещение для правой стены по оси X")]
        public float rightWallSpacingX = 0f;

        [Header("Настройки коллайдеров стен")]
        [SerializeField, Tooltip("Включить добавление коллайдеров к стенам")]
        private bool enableWallColliders = true;
        [SerializeField, Tooltip("Размер коллайдера для верхней и нижней стены")]
        private Vector2 horizontalColliderSize = new Vector2(1f, 0.2f);
        [SerializeField, Tooltip("Смещение коллайдера для верхней стены")]
        private Vector2 topColliderOffset = Vector2.zero;
        [SerializeField, Tooltip("Смещение коллайдера для нижней стены")]
        private Vector2 bottomColliderOffset = Vector2.zero;
        [SerializeField, Tooltip("Размер коллайдера для левой и правой стены")]
        private Vector2 verticalColliderSize = new Vector2(0.2f, 1f);
        [SerializeField, Tooltip("Смещение коллайдера для левой стены")]
        private Vector2 leftColliderOffset = Vector2.zero;
        [SerializeField, Tooltip("Смещение коллайдера для правой стены")]
        private Vector2 rightColliderOffset = Vector2.zero;

        [Header("Прочие ссылки")]
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
        [SerializeField, Tooltip("UI Стрелка таймера. Изначальный поворот по Z = -90")]
        private RectTransform clockHand;

        // Внутренние переменные
        private float labyrinthTimer;
        private float totalLabyrinthTime;
        private LabyrinthField labyrinth;
        private int rows, cols;
        private float cellSizeX, cellSizeY;

        // Смещение для сортировки (чем больше значение – тем корректнее перекрытия)
        [SerializeField, Tooltip("Смещение для расчёта порядка сортировки объектов")]
        private int sortingOffset = 10;

        private enum WallOrientation { Top, Bottom, Left, Right }

        private void Start()
        {
            // Чтение настроек из LabyrinthSettings или резервных настроек
            if (labyrinthSettings != null)
            {
                rows = labyrinthSettings.rows;
                cols = labyrinthSettings.cols;
                cellSizeX = labyrinthSettings.cellSizeX;
                cellSizeY = labyrinthSettings.cellSizeY;
                labyrinthTimer = labyrinthSettings.labyrinthTimeLimit;
            }
            else if (GameStageManager.currentStageData != null && GameStageManager.currentStageData.labyrinthSettings != null)
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

            totalLabyrinthTime = labyrinthTimer;

            if (clockHand != null)
            {
                clockHand.localRotation = Quaternion.Euler(0f, 0f, -90f);
            }

            // Создаём структуру лабиринта
            labyrinth = new LabyrinthField(rows, cols, cellSizeX, cellSizeY);
            GenerateWalls();

            // Расстановка игрока – ставим его в стартовую позицию
            GameObject player = GameObject.FindGameObjectWithTag(ETag.Player.ToString());
            if (player != null)
            {
                player.transform.position = labyrinth.GetStartWorldPosition();
            }

            // Передаём путь решения на миникарту
            if (LabyrinthMapController.Instance != null)
            {
                List<Vector3> solutionPath = labyrinth.GetSolutionPathWorldPositions();
                LabyrinthMapController.Instance.SetSolutionPath(solutionPath);
            }
        }

        /// <summary>
        /// Генерирует лабиринт, создавая отдельные объекты стен.
        /// </summary>
        private void GenerateWalls()
        {
            List<Vector3> bonusPositions = new List<Vector3>();
            List<Vector3> trapPositions = new List<Vector3>();

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    Vector3 cellCenter = new Vector3(col * cellSizeX, -row * cellSizeY, 0f);
                    LabyrinthCell cell = labyrinth.Field[row, col];

                    // Верхняя стена
                    if (cell.TopBorder && topWallPrefab != null)
                    {
                        Vector3 pos = cellCenter + new Vector3(0f, cellSizeY / 2f + topWallSpacingY, 0f);
                        GameObject topWall = Instantiate(topWallPrefab, pos, Quaternion.identity, transform);
                        topWall.name = $"TopWall_R{row}_C{col}";
                        SetSortingOrder(topWall, row * sortingOffset + col);
                        AddCollider(topWall, WallOrientation.Top);
                    }

                    // Нижняя стена
                    if (cell.BottomBorder && bottomWallPrefab != null)
                    {
                        Vector3 pos = cellCenter - new Vector3(0f, cellSizeY / 2f + bottomWallSpacingY, 0f);
                        GameObject bottomWall = Instantiate(bottomWallPrefab, pos, Quaternion.identity, transform);
                        bottomWall.name = $"BottomWall_R{row}_C{col}";
                        SetSortingOrder(bottomWall, row * sortingOffset + col + 2);
                        AddCollider(bottomWall, WallOrientation.Bottom);
                    }

                    // Левая стена
                    if (cell.LeftBorder)
                    {
                        // Проверяем, есть ли в ячейке ниже (row+1) хотя бы одна стена
                        bool belowWallExists = false;
                        if (row < rows - 1)
                        {
                            LabyrinthCell belowCell = labyrinth.Field[row + 1, col];
                            belowWallExists = belowCell.LeftBorder || belowCell.RightBorder || belowCell.TopBorder || belowCell.BottomBorder;
                        }

                        Vector3 pos = cellCenter - new Vector3(cellSizeX / 2f + leftWallSpacingX, 0f, 0f);
                        GameObject leftWallObj = null;
                        if (belowWallExists && leftNoIsoWallPrefab != null)
                        {
                            leftWallObj = Instantiate(leftNoIsoWallPrefab, pos, Quaternion.identity, transform);
                            leftWallObj.name = $"LeftNoIsoWall_R{row}_C{col}";
                        }
                        else if (leftWallPrefab != null)
                        {
                            leftWallObj = Instantiate(leftWallPrefab, pos, Quaternion.identity, transform);
                            leftWallObj.name = $"LeftWall_R{row}_C{col}";
                        }
                        if (leftWallObj != null)
                        {
                            SetSortingOrder(leftWallObj, row * sortingOffset + col + 1);
                            AddCollider(leftWallObj, WallOrientation.Left);
                        }
                    }

                    // Правая стена
                    if (cell.RightBorder)
                    {
                        // Проверяем, есть ли в ячейке ниже хотя бы одна стена
                        bool belowWallExists = false;
                        if (row < rows - 1)
                        {
                            LabyrinthCell belowCell = labyrinth.Field[row + 1, col];
                            belowWallExists = belowCell.LeftBorder || belowCell.RightBorder || belowCell.TopBorder || belowCell.BottomBorder;
                        }

                        Vector3 pos = cellCenter + new Vector3(cellSizeX / 2f + rightWallSpacingX, 0f, 0f);
                        GameObject rightWallObj = null;
                        if (belowWallExists && rightNoIsoWallPrefab != null)
                        {
                            rightWallObj = Instantiate(rightNoIsoWallPrefab, pos, Quaternion.identity, transform);
                            rightWallObj.name = $"RightNoIsoWall_R{row}_C{col}";
                        }
                        else if (rightWallPrefab != null)
                        {
                            rightWallObj = Instantiate(rightWallPrefab, pos, Quaternion.identity, transform);
                            rightWallObj.name = $"RightWall_R{row}_C{col}";
                        }
                        if (rightWallObj != null)
                        {
                            SetSortingOrder(rightWallObj, row * sortingOffset + col + 1);
                            AddCollider(rightWallObj, WallOrientation.Right);
                        }
                    }

                    // Сохраняем позиции для бонусов и ловушек (если ячейка не стартовая или финишная)
                    if (!cell.IsStart && !cell.IsFinish)
                    {
                        if (cell.IsSolutionPath)
                            bonusPositions.Add(cellCenter);
                        else
                            trapPositions.Add(cellCenter);
                    }
                }
            }

            PlaceItems(bonusPrefab, bonusCount, bonusPositions);
            PlaceItems(trapPrefab, trapCount, trapPositions);
        }

        /// <summary>
        /// Устанавливает порядок сортировки (если есть компонент SpriteRenderer) для корректного перекрытия.
        /// </summary>
        private void SetSortingOrder(GameObject obj, int order)
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = order;
            }
        }

        /// <summary>
        /// Добавляет BoxCollider2D к стене, если включены настройки коллайдеров.
        /// Настраивает его размер и смещение в зависимости от ориентации стены.
        /// </summary>
        /// <param name="wallObj">Объект стены</param>
        /// <param name="orientation">Ориентация стены</param>
        private void AddCollider(GameObject wallObj, WallOrientation orientation)
        {
            if (!enableWallColliders || wallObj == null)
                return;

            BoxCollider2D collider = wallObj.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = wallObj.AddComponent<BoxCollider2D>();
            }

            switch (orientation)
            {
                case WallOrientation.Top:
                    collider.size = horizontalColliderSize;
                    collider.offset = topColliderOffset;
                    break;
                case WallOrientation.Bottom:
                    collider.size = horizontalColliderSize;
                    collider.offset = bottomColliderOffset;
                    break;
                case WallOrientation.Left:
                    collider.size = verticalColliderSize;
                    collider.offset = leftColliderOffset;
                    break;
                case WallOrientation.Right:
                    collider.size = verticalColliderSize;
                    collider.offset = rightColliderOffset;
                    break;
            }
        }

        /// <summary>
        /// Размещает заданное количество объектов (бонусы или ловушки) на позициях с учётом минимального расстояния.
        /// </summary>
        private void PlaceItems(GameObject prefab, int count, List<Vector3> availablePositions)
        {
            if (prefab == null || availablePositions.Count == 0 || count <= 0)
                return;

            int placed = 0;
            while (placed < count && availablePositions.Count > 0)
            {
                int index = Random.Range(0, availablePositions.Count);
                Vector3 pos = availablePositions[index];
                Instantiate(prefab, pos, Quaternion.identity, transform);
                placed++;

                availablePositions.RemoveAll(p =>
                    Mathf.Abs(p.x - pos.x) / cellSizeX + Mathf.Abs(p.y - pos.y) / cellSizeY < minPlacementDistance);
            }
        }

        private void Update()
        {
            labyrinthTimer -= Time.deltaTime;
            UpdateTimerUI();

            if (labyrinthTimer <= 0f)
            {
                LoadArenaScene();
            }
        }

        private void UpdateTimerUI()
        {
            if (timerText != null)
            {
                timerText.text = $"{labyrinthTimer:F1}";
            }
            if (clockHand != null && totalLabyrinthTime > 0)
            {
                float normalizedTime = Mathf.Clamp01(labyrinthTimer / totalLabyrinthTime);
                float angle = -90f - (1f - normalizedTime) * 360f;
                clockHand.localRotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        /// <summary>
        /// Загружает сцену арены, передавая настройки из LabyrinthSettings.
        /// </summary>
        private void LoadArenaScene()
        {
            if (GameStageManager.currentStageData != null)
            {
                SceneManager.LoadScene(GameStageManager.currentStageData.arenaSceneName);
            }
        }
    }
}
