using UnityEngine;
using UnityEngine.SceneManagement;

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
                stageSelectionPanel.SetActive(true);
            }
        }

        // Заглушка для "Опций"
        public void OnOptionsButton()
        {
            Debug.Log("Опции пока не реализованы");
        }

        // Вызывается кнопкой "Выход"
        public void OnExitButton()
        {
            Application.Quit();
            Debug.Log("Выход из игры");
        }
    }
}