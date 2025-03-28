using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Resources.Scripts.Data;

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

        [Header("Данные этапов")]
        [SerializeField] private StageData[] stages;

        private int currentStageIndex = 0;

        private void Start()
        {
            UpdateStageDisplay();
            leftButton.onClick.AddListener(OnLeftButton);
            rightButton.onClick.AddListener(OnRightButton);
            playButton.onClick.AddListener(OnPlayButton);
            closeButton.onClick.AddListener(OnCloseButton);
        }

        private void UpdateStageDisplay()
        {
            if (stages == null || stages.Length == 0)
                return;

            StageData currentStage = stages[currentStageIndex];
            stageNameText.text = currentStage.stageName;
            stageImage.sprite = currentStage.stageImage;

            leftButton.interactable = currentStageIndex > 0;
            rightButton.interactable = currentStageIndex < stages.Length - 1;
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
            // Сохраняем выбранный этап в глобальный менеджер
            GameStageManager.currentStageData = stages[currentStageIndex];
            // Загружаем сцену арены выбранного этапа
            SceneManager.LoadScene(GameStageManager.currentStageData.arenaSceneName);
        }

        private void OnCloseButton()
        {
            // Закрыть панель выбора этапов
            gameObject.SetActive(false);
        }
    }
}
