using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal; // Для ShadowCaster2D
using TMPro;
using Resources.Scripts.Data;
using Resources.Scripts.Misc;

namespace Resources.Scripts.Labyrinth
{
    public class LabyrinthGeneratorWithWalls : MonoBehaviour
    {
        [Header("Настройки лабиринта (ScriptableObject)")]
        [SerializeField] private LabyrinthSettings labyrinthSettings;

        [Header("Резервные параметры")]
        [SerializeField, Range(4, 20)] private int defaultRows = 5;
        [SerializeField, Range(4, 20)] private int defaultCols = 5;
        [SerializeField] private float defaultCellSizeX = 1f;
        [SerializeField] private float defaultCellSizeY = 1f;
        [SerializeField] private float defaultTimeLimit = 30f;

        [Header("Префабы стен")]
        [SerializeField] private GameObject topWallPrefab;
        [SerializeField] private GameObject bottomWallPrefab;
        [SerializeField] private GameObject leftWallPrefab;       // изометрическая
        [SerializeField] private GameObject leftNoIsoWallPrefab;  // без изометрии
        [SerializeField] private GameObject rightWallPrefab;
        [SerializeField] private GameObject rightNoIsoWallPrefab;

        [Header("Смещения")]
        [SerializeField] private float topWallSpacingY;
        [SerializeField] private float bottomWallSpacingY;
        [SerializeField] private float leftWallSpacingX;
        [SerializeField] private float rightWallSpacingX;

        [Header("Коллайдеры")]
        [SerializeField] private bool enableWallColliders = true;
        [SerializeField] private Vector2 horizontalColliderSize = new Vector2(1f, 0.2f);
        [SerializeField] private Vector2 topColliderOffset = Vector2.zero;
        [SerializeField] private Vector2 bottomColliderOffset = Vector2.zero;
        [SerializeField] private Vector2 verticalColliderSize = new Vector2(0.2f, 1f);
        [SerializeField] private Vector2 leftColliderOffset = Vector2.zero;
        [SerializeField] private Vector2 rightColliderOffset = Vector2.zero;

        [Header("Бонусы и ловушки")]
        [SerializeField] private GameObject bonusPrefab;
        [SerializeField] private GameObject trapPrefab;
        [SerializeField, Range(1, 10)] private int minPlacementDistance = 5;
        [SerializeField] private int bonusCount = 1;
        [SerializeField] private int trapCount = 3;

        [Header("UI Таймер")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private RectTransform clockHand;

        // Внутренние
        private float labyrinthTimer;
        private float totalLabyrinthTime;
        private LabyrinthField labyrinth;
        private int rows, cols;
        private float cellSizeX, cellSizeY;
        [SerializeField] private int sortingOffset = 10;

        private enum WallOrientation { Top, Bottom, Left, Right }

        private void Start()
        {
            // Загружаем параметры лабиринта
            if (labyrinthSettings != null)
            {
                rows = labyrinthSettings.rows;
                cols = labyrinthSettings.cols;
                cellSizeX = labyrinthSettings.cellSizeX;
                cellSizeY = labyrinthSettings.cellSizeY;
                labyrinthTimer = labyrinthSettings.labyrinthTimeLimit;
            }
            else if (GameStageManager.currentStageData?.labyrinthSettings != null)
            {
                var s = GameStageManager.currentStageData.labyrinthSettings;
                rows = s.rows;
                cols = s.cols;
                cellSizeX = s.cellSizeX;
                cellSizeY = s.cellSizeY;
                labyrinthTimer = s.labyrinthTimeLimit;
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
                clockHand.localRotation = Quaternion.Euler(0, 0, -90f);

            labyrinth = new LabyrinthField(rows, cols, cellSizeX, cellSizeY);
            GenerateWalls();

            // Ставим игрока
            var player = GameObject.FindWithTag(ETag.Player.ToString());
            if (player != null)
                player.transform.position = labyrinth.GetStartWorldPosition();

            // Решение на миникарте
            if (LabyrinthMapController.Instance != null)
                LabyrinthMapController.Instance.SetSolutionPath(labyrinth.GetSolutionPathWorldPositions());
        }

        private void GenerateWalls()
        {
            var bonusPositions = new List<Vector3>();
            var trapPositions  = new List<Vector3>();

            for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                Vector3 center = new Vector3(c * cellSizeX, -r * cellSizeY, 0f);
                var cell = labyrinth.Field[r, c];

                // Верхняя стена
                if (cell.TopBorder && topWallPrefab != null)
                    CreateWall(topWallPrefab, WallOrientation.Top, r, c, center,
                               Vector3.up * (cellSizeY/2 + topWallSpacingY), "TopWall");

                // Нижняя стена
                if (cell.BottomBorder && bottomWallPrefab != null)
                    CreateWall(bottomWallPrefab, WallOrientation.Bottom, r, c, center,
                               Vector3.down * (cellSizeY/2 + bottomWallSpacingY), "BottomWall");

                // Левая стена
                if (cell.LeftBorder)
                {
                    bool bottomHere    = cell.BottomBorder
                                         || (c > 0 && labyrinth.Field[r, c-1].BottomBorder);
                    bool verticalBelow = (r < rows - 1 && labyrinth.Field[r + 1, c].LeftBorder);

                    if (bottomHere || verticalBelow)
                    {
                        CreateWall(leftNoIsoWallPrefab, WallOrientation.Left, r, c, center,
                                   Vector3.left * (cellSizeX/2 + leftWallSpacingX), "LeftNoIsoWall");
                    }
                    else
                    {
                        CreateWall(leftWallPrefab, WallOrientation.Left, r, c, center,
                                   Vector3.left * (cellSizeX/2 + leftWallSpacingX), "LeftWall");
                    }
                }

                // Правая стена
                if (cell.RightBorder)
                {
                    bool bottomHere    = cell.BottomBorder
                                         || (c < cols-1 && labyrinth.Field[r, c+1].BottomBorder);
                    bool verticalBelow = (r < rows - 1 && labyrinth.Field[r + 1, c].RightBorder);

                    if (bottomHere || verticalBelow)
                    {
                        CreateWall(rightNoIsoWallPrefab, WallOrientation.Right, r, c, center,
                                   Vector3.right * (cellSizeX/2 + rightWallSpacingX), "RightNoIsoWall");
                    }
                    else
                    {
                        CreateWall(rightWallPrefab, WallOrientation.Right, r, c, center,
                                   Vector3.right * (cellSizeX/2 + rightWallSpacingX), "RightWall");
                    }
                }

                // Бонусы и ловушки
                if (!cell.IsStart && !cell.IsFinish)
                {
                    if (cell.IsSolutionPath) bonusPositions.Add(center);
                    else                     trapPositions.Add(center);
                }
            }

            PlaceItems(bonusPrefab, bonusCount, bonusPositions);
            PlaceItems(trapPrefab,  trapCount,  trapPositions);
        }

        private GameObject CreateWall(GameObject prefab, WallOrientation ori,
                                      int r, int c, Vector3 center, Vector3 offset, string baseName)
        {
            if (prefab == null) return null;
            var go = Instantiate(prefab, center + offset, Quaternion.identity, transform);
            go.name = $"{baseName}_R{r}_C{c}";
            int extra = ori == WallOrientation.Bottom ? 2 : (ori == WallOrientation.Top ? 0 : 1);
            SetSortingOrder(go, r * sortingOffset + c + extra);
            AddCollider(go, ori);
            AddShadowCaster(go);
            return go;
        }

        private void AddCollider(GameObject go, WallOrientation ori)
        {
            if (!enableWallColliders || go == null) return;

            // Ищем ненулевой физический коллайдер
            BoxCollider2D phys = null;
            foreach (var c in go.GetComponents<BoxCollider2D>())
                if (!c.isTrigger) { phys = c; break; }

            if (phys == null)
                phys = go.AddComponent<BoxCollider2D>();

            phys.isTrigger = false;

            switch (ori)
            {
                case WallOrientation.Top:
                    phys.size   = horizontalColliderSize;
                    phys.offset = topColliderOffset;
                    break;
                case WallOrientation.Bottom:
                    phys.size   = horizontalColliderSize;
                    phys.offset = bottomColliderOffset;
                    break;
                case WallOrientation.Left:
                    phys.size   = verticalColliderSize;
                    phys.offset = leftColliderOffset;
                    break;
                case WallOrientation.Right:
                    phys.size   = verticalColliderSize;
                    phys.offset = rightColliderOffset;
                    break;
            }
        }

        private void AddShadowCaster(GameObject go)
        {
            if (go == null) return;
            if (go.GetComponent<ShadowCaster2D>() == null)
                go.AddComponent<ShadowCaster2D>();
        }

        private void SetSortingOrder(GameObject go, int order)
        {
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = order;
        }

        private void PlaceItems(GameObject prefab, int count, List<Vector3> positions)
        {
            if (prefab == null || positions.Count == 0 || count <= 0) return;
            int placed = 0;
            while (placed < count && positions.Count > 0)
            {
                int idx = Random.Range(0, positions.Count);
                Instantiate(prefab, positions[idx], Quaternion.identity, transform);
                placed++;
                positions.RemoveAll(p =>
                    Mathf.Abs(p.x - positions[idx].x)/cellSizeX +
                    Mathf.Abs(p.y - positions[idx].y)/cellSizeY < minPlacementDistance);
            }
        }

        private void Update()
        {
            labyrinthTimer -= Time.deltaTime;
            UpdateTimerUI();
            if (labyrinthTimer <= 0f)
                LoadArenaScene();
        }

        private void UpdateTimerUI()
        {
            if (timerText != null)
                timerText.text = $"{labyrinthTimer:F1}";
            if (clockHand != null && totalLabyrinthTime > 0)
            {
                float norm = Mathf.Clamp01(labyrinthTimer / totalLabyrinthTime);
                clockHand.localRotation = Quaternion.Euler(0, 0, -90f - (1 - norm) * 360f);
            }
        }

        private void LoadArenaScene()
        {
            var data = GameStageManager.currentStageData;
            if (data != null)
                SceneManager.LoadScene(data.arenaSceneName);
        }
    }
}
