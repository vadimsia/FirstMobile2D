using UnityEngine;
using UnityEngine.UI;

namespace Resources.Scripts.Labyrinth
{
    /// <summary>
    /// Инициализирует визуальное представление ячейки лабиринта:
    /// выставляет видимость границ, текстовое значение и теги на основе данных ячейки.
    /// Также рассчитывает порядок отрисовки стен для корректного перекрытия.
    /// </summary>
    public class LabyrinthCellPrefab : MonoBehaviour
    {
        [Header("Border GameObjects")]
        [SerializeField, Tooltip("Объект для верхней границы.")]
        private GameObject topBorder;
        [SerializeField, Tooltip("Объект для правой границы (с изометрией).")]
        private GameObject rightBorder;
        [SerializeField, Tooltip("Объект для нижней границы.")]
        private GameObject bottomBorder;
        [SerializeField, Tooltip("Объект для левой границы (с изометрией).")]
        private GameObject leftBorder;

        [Header("Дополнительные стены без изометрии")]
        [SerializeField, Tooltip("Объект для левой стены без изометрического эффекта.")]
        private GameObject leftNoIsoWall;
        [SerializeField, Tooltip("Объект для правой стены без изометрического эффекта.")]
        private GameObject rightNoIsoWall;

        [Header("Text Settings")]
        [SerializeField, Tooltip("UI-текст для отображения значения ячейки.")]
        private Text arrayValueText;
        [SerializeField, Tooltip("Отображать ли текст с значением.")]
        private bool showArrayValueText = true;
        [SerializeField, Range(0f, 1f), Tooltip("Прозрачность текста по умолчанию.")]
        private float defaultTextAlpha = 0.5f;

        [Header("Cell Type Settings")]
        [SerializeField, Tooltip("Если true, эта ячейка является точкой финиша.")]
        private bool isFinishCell;

        [Header("Sorting Settings")]
        [SerializeField, Tooltip("Высота ячейки для расчёта порядка отрисовки (должна совпадать с cellSizeY в генераторе).")]
        private float cellSizeY = 1f;
        [SerializeField, Tooltip("Смещение для сортировки между рядами (рекомендуется значение больше 1).")]
        private int sortingOffset = 10;

        private void Awake()
        {
            // Убеждаемся, что присутствует компонент BoxCollider2D, настроенный как триггер.
            BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider2D>();
            }
            boxCollider.isTrigger = true;
        }

        private void Start()
        {
            // После позиционирования ячейки обновляем порядок сортировки стен.
            UpdateSortingOrders();
        }

        /// <summary>
        /// Рассчитывает и назначает порядки сортировки для стен ячейки.
        /// </summary>
        private void UpdateSortingOrders()
        {
            int rowIndex = Mathf.RoundToInt(-transform.position.y / cellSizeY);
            int colIndex = Mathf.RoundToInt(transform.position.x / cellSizeY);
            int baseOrder = rowIndex * sortingOffset + colIndex;

            if (topBorder != null)
            {
                SpriteRenderer sr = topBorder.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.sortingOrder = baseOrder;
            }
            if (leftBorder != null)
            {
                SpriteRenderer sr = leftBorder.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.sortingOrder = baseOrder + 1;
            }
            if (rightBorder != null)
            {
                SpriteRenderer sr = rightBorder.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.sortingOrder = baseOrder + 1;
            }
            if (bottomBorder != null)
            {
                SpriteRenderer sr = bottomBorder.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.sortingOrder = baseOrder + 2;
            }
            if (leftNoIsoWall != null)
            {
                SpriteRenderer sr = leftNoIsoWall.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.sortingOrder = baseOrder + 1;
            }
            if (rightNoIsoWall != null)
            {
                SpriteRenderer sr = rightNoIsoWall.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.sortingOrder = baseOrder + 1;
            }
        }

        /// <summary>
        /// Включает или отключает объект границы в зависимости от флага.
        /// </summary>
        private void ProcessBorder(GameObject borderObject, bool active)
        {
            if (borderObject != null)
            {
                borderObject.SetActive(active);
            }
        }

        /// <summary>
        /// Инициализирует ячейку на основе переданных данных.
        /// Дополнительно принимает информацию о позиции ячейки и флаги наличия стен ниже.
        /// </summary>
        /// <param name="cell">Данные ячейки</param>
        /// <param name="row">Номер ряда (начиная с 0 сверху)</param>
        /// <param name="col">Номер столбца</param>
        /// <param name="totalCols">Общее число столбцов лабиринта</param>
        /// <param name="totalRows">Общее число рядов лабиринта</param>
        /// <param name="belowLeftWall">Есть ли у ячейки ниже левая стена</param>
        /// <param name="belowRightWall">Есть ли у ячейки ниже правая стена</param>
        public void Init(LabyrinthCell cell, int row, int col, int totalCols, int totalRows, bool belowLeftWall, bool belowRightWall)
        {
            ProcessBorder(topBorder, cell.TopBorder);
            ProcessBorder(bottomBorder, cell.BottomBorder);

            // ЛЕВАЯ СТЕНА
            if (cell.LeftBorder)
            {
                // Если ниже есть такая же стена – используем без изометрии,
                // иначе (закрывающая стена) – с изометрией.
                if (row < totalRows - 1 && belowLeftWall)
                {
                    if (leftNoIsoWall != null)
                        leftNoIsoWall.SetActive(true);
                    if (leftBorder != null)
                        leftBorder.SetActive(false);
                }
                else
                {
                    if (leftNoIsoWall != null)
                        leftNoIsoWall.SetActive(false);
                    ProcessBorder(leftBorder, true);
                }
            }
            else
            {
                if (leftNoIsoWall != null)
                    leftNoIsoWall.SetActive(false);
                ProcessBorder(leftBorder, false);
            }

            // ПРАВАЯ СТЕНА
            if (cell.RightBorder)
            {
                if (row < totalRows - 1 && belowRightWall)
                {
                    if (rightNoIsoWall != null)
                        rightNoIsoWall.SetActive(true);
                    if (rightBorder != null)
                        rightBorder.SetActive(false);
                }
                else
                {
                    if (rightNoIsoWall != null)
                        rightNoIsoWall.SetActive(false);
                    ProcessBorder(rightBorder, true);
                }
            }
            else
            {
                if (rightNoIsoWall != null)
                    rightNoIsoWall.SetActive(false);
                ProcessBorder(rightBorder, false);
            }

            // Текст и теги для специальных ячеек.
            if (cell.IsStart)
            {
                arrayValueText.text = "S";
                arrayValueText.color = new Color(0f, 1f, 0f, defaultTextAlpha);
                gameObject.tag = "Start";
            }
            else if (cell.IsFinish)
            {
                arrayValueText.text = "F";
                arrayValueText.color = new Color(1f, 0f, 0f, defaultTextAlpha);
                isFinishCell = true;
                gameObject.tag = "Finish";
            }
            else if (cell.IsSolutionPath)
            {
                arrayValueText.text = cell.ArrayValue.ToString();
                arrayValueText.color = new Color(0f, 1f, 0f, defaultTextAlpha);
            }
            else
            {
                arrayValueText.text = cell.ArrayValue.ToString();
                arrayValueText.color = new Color(1f, 1f, 1f, defaultTextAlpha);
            }
        }

        private void Update()
        {
            UpdateTextVisibility();
        }

        /// <summary>
        /// Обновляет прозрачность текста в зависимости от флага видимости.
        /// </summary>
        private void UpdateTextVisibility()
        {
            if (arrayValueText != null)
            {
                Color currentColor = arrayValueText.color;
                currentColor.a = showArrayValueText ? defaultTextAlpha : 0f;
                arrayValueText.color = currentColor;
            }
        }

        /// <summary>
        /// Если это финишная ячейка и игрок входит в неё, происходит загрузка сцены.
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isFinishCell && other.CompareTag("Player"))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
            }
        }
    }
}
