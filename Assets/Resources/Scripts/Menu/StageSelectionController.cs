using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Resources.Scripts.Data;
using Resources.Scripts.GameManagers;
using Resources.Scripts.UI;
using System.Collections;

namespace Resources.Scripts.Menu
{
    public class StageSelectionController : MonoBehaviour
    {
        [Header("UI Элементы")]
        [SerializeField] private TextMeshProUGUI stageNameText;
        [SerializeField] private Image stageImage;
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Button closeButton;

        [Header("Фон панели")]
        [SerializeField] private Image panelBackground;
        [SerializeField] private Sprite[] backgroundSprites;

        [Header("Данные этапов")]
        [SerializeField] private StageData[] stages;

        [Header("Панель загрузки")]
        [Tooltip("Controller для панели загрузки с CanvasGroup и анимацией точек")]
        [SerializeField] private LoadingPanelController loadingPanelController;

        private int currentStageIndex;

        private void Start()
        {
            UpdateStageDisplay();
            leftButton.onClick.AddListener(OnLeftButton);
            rightButton.onClick.AddListener(OnRightButton);
            playButton.onClick.AddListener(OnPlayButton);
            closeButton.onClick.AddListener(OnCloseButton);

            if (loadingPanelController != null)
                loadingPanelController.gameObject.SetActive(false);
        }

        private void UpdateStageDisplay()
        {
            if (stages == null || stages.Length == 0) return;
            var stage = stages[currentStageIndex];
            stageNameText.text   = stage.stageName;
            stageImage.sprite    = stage.stageImage;
            leftButton.interactable  = currentStageIndex > 0;
            rightButton.interactable = currentStageIndex < stages.Length - 1;
            if (panelBackground != null &&
                currentStageIndex < backgroundSprites.Length)
                panelBackground.sprite = backgroundSprites[currentStageIndex];
        }

        private void OnLeftButton()
        {
            if (currentStageIndex > 0)
            {
                currentStageIndex--;
                UpdateStageDisplay();
            }
        }

        private void OnRightButton()
        {
            if (currentStageIndex < stages.Length - 1)
            {
                currentStageIndex++;
                UpdateStageDisplay();
            }
        }

        private void OnPlayButton()
        {
            // Показываем панель загрузки с анимацией
            loadingPanelController?.Show();

            // Сохраняем выбранный этап
            GameStageManager.currentStageData = stages[currentStageIndex];
            
            if (StageProgressionManager.Instance == null)
            {
                var go = new GameObject("StageProgressionManager");
                go.AddComponent<StageProgressionManager>();
            }

            // Запускаем с задержкой, чтобы панель оставалась видимой 2 секунды
            StartCoroutine(StartStageWithDelay());
        }

        private IEnumerator StartStageWithDelay()
        {
            // Ждём один кадр, чтобы Show() успел отработать
            yield return null;

            // Дополнительная задержка 2 секунды
            yield return new WaitForSecondsRealtime(2f);

            // Запуск этапов
            StageProgressionManager.Instance.StartStage();
        }

        private void OnCloseButton()
        {
            gameObject.SetActive(false);
        }
    }
}
