using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

namespace Game.Scripts.Labyrinth
{
    /// <summary>
    /// Управляет отображением иконок миникарты: иконка старта, финиша и динамичная иконка игрока.
    /// Префабы должны быть UI Image и находиться внутри RawImage, отображающего миникарту.
    /// </summary>
    public class LabyrinthMinimapIconController : MonoBehaviour
    {
        [Header("Префабы иконок (UI Image)")]
        [SerializeField] private GameObject startIconPrefab;
        [SerializeField] private GameObject finishIconPrefab;
        [SerializeField] private GameObject playerIconPrefab; // Префаб динамичной иконки игрока

        [Header("Настройки миникарты")]
        [SerializeField] private Camera minimapCamera; // Вторая камера для миникарты
        [SerializeField] private RawImage minimapImage;  // RawImage, на котором отображается миникарта

        private RectTransform rawImageRectTransform;   // RectTransform RawImage
        private GameObject startIconInstance;
        private GameObject finishIconInstance;
        private GameObject playerIconInstance;
        private Transform playerTransform;

        private void Start()
        {
            // Получаем RectTransform у RawImage миникарты
            if (minimapImage != null)
            {
                rawImageRectTransform = minimapImage.GetComponent<RectTransform>();
            }
            else
            {
                Debug.LogWarning("Minimap RawImage is not assigned!");
                return;
            }

            // Проверяем назначение миникамеры
            if (minimapCamera == null)
            {
                Debug.LogWarning("Minimap Camera is not assigned!");
                return;
            }

            // Находим объект игрока по тегу "Player"
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("Player object not found!");
            }

            // Запускаем корутину ожидания появления ячеек "Start" и "Finish"
            StartCoroutine(InitializeIconsCoroutine());
        }

        private IEnumerator InitializeIconsCoroutine()
        {
            float timer = 0f;
            GameObject startCell = null;
            GameObject finishCell = null;
            // Ждем появления объектов с тегами "Start" и "Finish" (до 5 секунд)
            while (timer < 5f)
            {
                startCell = GameObject.FindGameObjectWithTag("Start");
                finishCell = GameObject.FindGameObjectWithTag("Finish");
                if (startCell != null && finishCell != null)
                    break;
                timer += Time.deltaTime;
                yield return null;
            }

            if (startCell == null)
            {
                Debug.LogWarning("Start cell not found within 5 seconds!");
            }
            else if (startIconPrefab != null)
            {
                Vector2 uiPos = WorldToUISpace(startCell.transform.position);
                startIconInstance = Instantiate(startIconPrefab, rawImageRectTransform);
                RectTransform startRect = startIconInstance.GetComponent<RectTransform>();
                if (startRect == null)
                {
                    startRect = startIconInstance.AddComponent<RectTransform>();
                }
                startRect.anchoredPosition = uiPos;
            }

            if (finishCell == null)
            {
                Debug.LogWarning("Finish cell not found within 5 seconds!");
            }
            else if (finishIconPrefab != null)
            {
                Vector2 uiPos = WorldToUISpace(finishCell.transform.position);
                finishIconInstance = Instantiate(finishIconPrefab, rawImageRectTransform);
                RectTransform finishRect = finishIconInstance.GetComponent<RectTransform>();
                if (finishRect == null)
                {
                    finishRect = finishIconInstance.AddComponent<RectTransform>();
                }
                finishRect.anchoredPosition = uiPos;
            }

            // Создаем динамичную иконку игрока (если объект игрока найден)
            if (playerTransform != null && playerIconPrefab != null)
            {
                Vector2 uiPos = WorldToUISpace(playerTransform.position);
                playerIconInstance = Instantiate(playerIconPrefab, rawImageRectTransform);
                RectTransform playerRect = playerIconInstance.GetComponent<RectTransform>();
                if (playerRect == null)
                {
                    playerRect = playerIconInstance.AddComponent<RectTransform>();
                }
                playerRect.anchoredPosition = uiPos;
            }
        }

        private void Update()
        {
            // Обновляем позицию иконки игрока относительно RawImage
            if (playerTransform != null && playerIconInstance != null)
            {
                Vector2 uiPos = WorldToUISpace(playerTransform.position);
                RectTransform playerRect = playerIconInstance.GetComponent<RectTransform>();
                if (playerRect != null)
                {
                    playerRect.anchoredPosition = uiPos;
                }
            }
        }

        /// <summary>
        /// Преобразует мировые координаты в координаты UI для RawImage, используя камеру миникарты.
        /// Метод использует viewport-координаты для корректного позиционирования относительно RawImage.
        /// </summary>
        /// <param name="worldPos">Мировая позиция</param>
        /// <returns>Локальные координаты RawImage (anchoredPosition)</returns>
        private Vector2 WorldToUISpace(Vector3 worldPos)
        {
            // Получаем viewport-координаты (значения от 0 до 1)
            Vector3 viewportPos = minimapCamera.WorldToViewportPoint(worldPos);
            // Преобразуем viewport-координаты в локальные координаты RawImage
            Vector2 uiPos = new Vector2(
                (viewportPos.x - 0.5f) * rawImageRectTransform.sizeDelta.x,
                (viewportPos.y - 0.5f) * rawImageRectTransform.sizeDelta.y
            );
            return uiPos;
        }
    }
}
