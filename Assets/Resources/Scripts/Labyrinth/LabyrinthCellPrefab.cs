using UnityEngine;
using UnityEngine.UI;

namespace Resources.Scripts.Labyrinth
{
    /// <summary>
    /// Initializes the visual representation of a labyrinth cell,
    /// setting borders, text and tags based on cell data.
    /// </summary>
    public class LabyrinthCellPrefab : MonoBehaviour
    {
        [Header("Border GameObjects")]
        [SerializeField, Tooltip("GameObject for the top border.")]
        private GameObject topBorder;
        [SerializeField, Tooltip("GameObject for the right border.")]
        private GameObject rightBorder;
        [SerializeField, Tooltip("GameObject for the bottom border.")]
        private GameObject bottomBorder;
        [SerializeField, Tooltip("GameObject for the left border.")]
        private GameObject leftBorder;

        [Header("Text Settings")]
        [SerializeField, Tooltip("UI Text component for displaying the cell's array value.")]
        private Text arrayValueText;
        [SerializeField, Tooltip("Toggle text visibility.")]
        private bool showArrayValueText = true;
        [SerializeField, Range(0f, 1f), Tooltip("Default text alpha (transparency).")]
        private float defaultTextAlpha = 0.5f;

        [Header("Cell Type Settings")]
        [SerializeField, Tooltip("If true, this cell represents the finish point.")]
        private bool isFinishCell = false;

        private void Awake()
        {
            // Ensure a BoxCollider2D is attached and set as trigger.
            BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider2D>();
            }
            boxCollider.isTrigger = true;
        }

        /// <summary>
        /// Processes a border GameObject: enables/disables it based on hasBorder.
        /// </summary>
        /// <param name="borderObject">Border GameObject reference.</param>
        /// <param name="hasBorder">Should the border be visible?</param>
        private void ProcessBorder(GameObject borderObject, bool hasBorder)
        {
            if (borderObject != null)
            {
                borderObject.SetActive(hasBorder);
            }
        }

        /// <summary>
        /// Initializes the cell prefab based on provided cell data.
        /// </summary>
        /// <param name="cell">Labyrinth cell data.</param>
        public void Init(LabyrinthCell cell)
        {
            ProcessBorder(topBorder, cell.TopBorder);
            ProcessBorder(rightBorder, cell.RightBorder);
            ProcessBorder(bottomBorder, cell.BottomBorder);
            ProcessBorder(leftBorder, cell.LeftBorder);

            // Set cell text and color based on cell type.
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
        /// Updates the text visibility by adjusting its alpha.
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
            // If the finish cell collides with the player, load the designated scene.
            if (isFinishCell && other.CompareTag("Player"))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("FirstPartScene");
            }
        }
    }
}
