using UnityEngine;
using DG.Tweening;

namespace Resources.Scripts.Menu
{
    public class MenuManager : MonoBehaviour
    {
        [Header("Ссылка на панель выбора этапов")]
        [SerializeField] private GameObject stageSelectionPanel;

        // Вызывается кнопкой "Играть" на главном меню
        public void OnPlayButton()
        {
            if (stageSelectionPanel != null)
            {
                stageSelectionPanel.transform.localScale = Vector3.zero;
                stageSelectionPanel.SetActive(true);
                stageSelectionPanel.transform
                    .DOScale(1f, 0.4f)
                    .SetEase(Ease.OutBack);
            }
        }

        // Заглушка для "Опций"
        public void OnOptionsButton()
        {
            Debug.Log("Опции пока не реализованы");
            // можно добавить всплывающее уведомление позже
        }

        // Вызывается кнопкой "Выход"
        public void OnExitButton()
        {
            // анимация небольшого "пранча"
            transform
                .DOPunchScale(Vector3.one * 0.05f, 0.3f, 10, 0.5f)
                .OnComplete(() =>
                {
                    Application.Quit();
                    Debug.Log("Выход из игры");
                });
        }
    }
}