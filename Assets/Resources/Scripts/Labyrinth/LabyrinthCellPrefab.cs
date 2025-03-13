using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Resources.Scripts.Labyrinth
{
    public class LabyrinthCellPrefab : MonoBehaviour
    {
        [SerializeField] private GameObject topBorder;
        [SerializeField] private GameObject rightBorder;
        [SerializeField] private GameObject bottomBorder;
        [SerializeField] private GameObject leftBorder;
        [SerializeField] private Text arrayValueText; // Используем Legacy Text

        // Флаг для определения, что эта клетка является финишной
        private bool isFinishCell;

        private void Awake()
        {
            // Проверяем наличие BoxCollider2D на родительском объекте и добавляем его, если отсутствует
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<BoxCollider2D>();
            }
            // Устанавливаем флаг Is Trigger, чтобы обрабатывать столкновения через OnTriggerEnter2D
            collider.isTrigger = true;
        }

        public void Init(LabyrinthCell cell)
        {
            if (!cell.topBorder)
                Destroy(topBorder);
            if (!cell.rightBorder)
                Destroy(rightBorder);
            if (!cell.bottomBorder)
                Destroy(bottomBorder);
            if (!cell.leftBorder)
                Destroy(leftBorder);

            // Настраиваем отображение текста и запоминаем, если клетка финишная
            if (cell.isStart)
            {
                arrayValueText.text = "S";
                arrayValueText.color = Color.green;
            }
            else if (cell.isFinish)
            {
                arrayValueText.text = "F";
                arrayValueText.color = Color.red;
                isFinishCell = true;
            }
            else if (cell.isSolutionPath)
            {
                arrayValueText.text = cell.arrayValue.ToString();
                arrayValueText.color = Color.yellow;
            }
            else
            {
                arrayValueText.text = cell.arrayValue.ToString();
                arrayValueText.color = Color.white;
            }
        }

        // Обработка столкновения с игроком
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Если клетка финишная и столкновение произошло с объектом, у которого тег "Player"
            if (isFinishCell && other.CompareTag("Player"))
            {
                SceneManager.LoadScene("FirstPartScene");
            }
        }
    }
}
