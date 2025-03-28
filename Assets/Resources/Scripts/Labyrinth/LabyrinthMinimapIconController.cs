using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

namespace Game.Scripts.Labyrinth
{
    /// <summary>
    /// Manages the display of minimap icons: start, finish and dynamic player icon.
    /// The prefabs must be UI Images and should be children of the RawImage displaying the minimap.
    /// </summary>
    public class LabyrinthMinimapIconController : MonoBehaviour
    {
        [Header("Icon Prefabs (UI Image)")]
        [SerializeField, Tooltip("Prefab for the start icon.")]
        private GameObject startIconPrefab;
        [SerializeField, Tooltip("Prefab for the finish icon.")]
        private GameObject finishIconPrefab;
        [SerializeField, Tooltip("Prefab for the dynamic player icon.")]
        private GameObject playerIconPrefab;

        [Header("Minimap Settings")]
        [SerializeField, Tooltip("Camera used exclusively for the minimap.")]
        private Camera minimapCamera;
        [SerializeField, Tooltip("RawImage that displays the minimap.")]
        private RawImage minimapImage;
        [SerializeField, Tooltip("Maximum time (in seconds) to wait for start and finish cells to appear.")]
        private float maxIconWaitTime = 5f;

        private RectTransform rawImageRectTransform;
        private GameObject startIconInstance;
        private GameObject finishIconInstance;
        private GameObject playerIconInstance;
        private Transform playerTransform;

        private void Start()
        {
            // Get the RectTransform from the minimap RawImage.
            if (minimapImage != null)
            {
                rawImageRectTransform = minimapImage.GetComponent<RectTransform>();
            }
            else
            {
                Debug.LogWarning("Minimap RawImage is not assigned!");
                return;
            }

            // Check for the minimap camera assignment.
            if (minimapCamera == null)
            {
                Debug.LogWarning("Minimap Camera is not assigned!");
                return;
            }

            // Find the player object by tag.
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("Player object not found!");
            }

            // Start coroutine to wait for the appearance of the start and finish cells.
            StartCoroutine(InitializeIconsCoroutine());
        }

        /// <summary>
        /// Coroutine that waits for objects with tags "Start" and "Finish" to appear, then instantiates their icons.
        /// </summary>
        private IEnumerator InitializeIconsCoroutine()
        {
            float timer = 0f;
            GameObject startCell = null;
            GameObject finishCell = null;
            // Wait up to maxIconWaitTime seconds for objects with "Start" and "Finish" tags.
            while (timer < maxIconWaitTime)
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
                Debug.LogWarning("Start cell not found within " + maxIconWaitTime + " seconds!");
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
                Debug.LogWarning("Finish cell not found within " + maxIconWaitTime + " seconds!");
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

            // Create the dynamic player icon if the player object is found.
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
            // Update the player icon position relative to the RawImage.
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
        /// Converts world coordinates to UI coordinates relative to the minimap RawImage using the minimap camera.
        /// </summary>
        /// <param name="worldPos">World position</param>
        /// <returns>Local coordinates (anchoredPosition) for the RawImage</returns>
        private Vector2 WorldToUISpace(Vector3 worldPos)
        {
            // Get viewport coordinates (values between 0 and 1)
            Vector3 viewportPos = minimapCamera.WorldToViewportPoint(worldPos);
            // Convert viewport coordinates to local coordinates of the RawImage.
            Vector2 uiPos = new Vector2(
                (viewportPos.x - 0.5f) * rawImageRectTransform.sizeDelta.x,
                (viewportPos.y - 0.5f) * rawImageRectTransform.sizeDelta.y
            );
            return uiPos;
        }
    }
}
