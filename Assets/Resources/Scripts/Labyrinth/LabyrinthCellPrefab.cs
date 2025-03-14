using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal; // For ShadowCaster2D

namespace Resources.Scripts.Labyrinth
{
    public class LabyrinthCellPrefab : MonoBehaviour
    {
        [SerializeField] private GameObject topBorder;
        [SerializeField] private GameObject rightBorder;
        [SerializeField] private GameObject bottomBorder;
        [SerializeField] private GameObject leftBorder;
        [SerializeField] private Text arrayValueText; // Using Legacy Text

        // Toggle text visibility via Inspector
        [SerializeField] private bool showArrayValueText = true;
        [SerializeField, Range(0f, 1f)] private float defaultTextAlpha = 0.5f;

        private bool isFinishCell;

        private void Awake()
        {
            // Ensure the GameObject has a BoxCollider2D set as a trigger.
            BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider2D>();
            }
            boxCollider.isTrigger = true;
        }

        /// <summary>
        /// Initializes the cell appearance based on the labyrinth cell data.
        /// </summary>
        /// <param name="cell">The labyrinth cell data.</param>
        public void Init(LabyrinthCell cell)
        {
            // Remove borders that are not present; otherwise add shadow caster if missing.
            if (!cell.TopBorder)
            {
                Destroy(topBorder);
            }
            else if (topBorder != null && topBorder.GetComponent<ShadowCaster2D>() == null)
            {
                topBorder.AddComponent<ShadowCaster2D>();
            }

            if (!cell.RightBorder)
            {
                Destroy(rightBorder);
            }
            else if (rightBorder != null && rightBorder.GetComponent<ShadowCaster2D>() == null)
            {
                rightBorder.AddComponent<ShadowCaster2D>();
            }

            if (!cell.BottomBorder)
            {
                Destroy(bottomBorder);
            }
            else if (bottomBorder != null && bottomBorder.GetComponent<ShadowCaster2D>() == null)
            {
                bottomBorder.AddComponent<ShadowCaster2D>();
            }

            if (!cell.LeftBorder)
            {
                Destroy(leftBorder);
            }
            else if (leftBorder != null && leftBorder.GetComponent<ShadowCaster2D>() == null)
            {
                leftBorder.AddComponent<ShadowCaster2D>();
            }

            // Set text and color based on cell type.
            if (cell.IsStart)
            {
                arrayValueText.text = "S";
                arrayValueText.color = new Color(0f, 1f, 0f, defaultTextAlpha);
            }
            else if (cell.IsFinish)
            {
                arrayValueText.text = "F";
                arrayValueText.color = new Color(1f, 0f, 0f, defaultTextAlpha);
                isFinishCell = true;

                // Tag this GameObject as "Finish" for the PlayerController.
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
        /// Updates the visibility of the text based on the inspector setting.
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
            // When the player reaches the finish cell, load the next scene.
            if (isFinishCell && other.CompareTag("Player"))
            {
                SceneManager.LoadScene("FirstPartScene");
            }
        }
    }
}
