using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

namespace Resources.Scripts.Labyrinth
{
    /// <summary>
    /// Инициализирует внешний вид ячейки лабиринта, устанавливает бордюры и теги.
    /// </summary>
    public class LabyrinthCellPrefab : MonoBehaviour
    {
        [SerializeField] private GameObject topBorder;
        [SerializeField] private GameObject rightBorder;
        [SerializeField] private GameObject bottomBorder;
        [SerializeField] private GameObject leftBorder;
        [SerializeField] private Text arrayValueText; // Компонент для отображения текста

        [SerializeField] private bool showArrayValueText = true;             // Отображение текста
        [SerializeField, Range(0f, 1f)] private float defaultTextAlpha = 0.5f; // Прозрачность текста

        private bool isFinishCell;

        private void Awake()
        {
            // Проверяем наличие BoxCollider2D и устанавливаем его как триггер
            BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider2D>();
            }
            boxCollider.isTrigger = true;
        }

        /// <summary>
        /// Обрабатывает бордюр: если он не нужен, удаляет объект (и удаляет тени);
        /// если нужен – добавляет компонент ShadowCaster2D только если его ещё нет.
        /// </summary>
        /// <param name="borderObject">Объект бордюра</param>
        /// <param name="hasBorder">Нужно ли отображать бордюр</param>
        private void ProcessBorder(ref GameObject borderObject, bool hasBorder)
        {
            if (!hasBorder)
            {
                // Если бордюр не нужен, удаляем ShadowCaster2D, если он есть, и уничтожаем объект
                if (borderObject != null)
                {
                    ShadowCaster2D sc = borderObject.GetComponent<ShadowCaster2D>();
                    if (sc != null)
                    {
                        Destroy(sc);
                    }
                    Destroy(borderObject);
                    borderObject = null;
                }
            }
            else
            {
                // Если бордюр нужен, добавляем ShadowCaster2D, если его нет
                if (borderObject != null && borderObject.GetComponent<ShadowCaster2D>() == null)
                {
                    borderObject.AddComponent<ShadowCaster2D>();
                }
            }
        }

        /// <summary>
        /// Инициализирует внешний вид ячейки на основе данных.
        /// </summary>
        /// <param name="cell">Данные ячейки лабиринта</param>
        public void Init(LabyrinthCell cell)
        {
            // Обрабатываем бордюры
            ProcessBorder(ref topBorder, cell.TopBorder);
            ProcessBorder(ref rightBorder, cell.RightBorder);
            ProcessBorder(ref bottomBorder, cell.BottomBorder);
            ProcessBorder(ref leftBorder, cell.LeftBorder);

            // Устанавливаем текст и цвет в зависимости от типа ячейки
            if (cell.IsStart)
            {
                arrayValueText.text = "S";
                arrayValueText.color = new Color(0f, 1f, 0f, defaultTextAlpha);
                // Убедитесь, что тег "Start" добавлен в Tags Manager (Edit → Project Settings → Tags and Layers)
                gameObject.tag = "Start";
            }
            else if (cell.IsFinish)
            {
                arrayValueText.text = "F";
                arrayValueText.color = new Color(1f, 0f, 0f, defaultTextAlpha);
                isFinishCell = true;
                // Убедитесь, что тег "Finish" добавлен в Tags Manager (Edit → Project Settings → Tags and Layers)
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
        /// Обновляет видимость текста на основе настроек в инспекторе.
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

        private void OnTriggerEnter2D(Collider2D other)
        {
            // При столкновении с игроком загружаем следующую сцену, если это финишная ячейка
            if (isFinishCell && other.CompareTag("Player"))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("FirstPartScene");
            }
        }
    }
}
