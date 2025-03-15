using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Resources.Scripts.Labyrinth
{
    /// <summary>
    /// Controls the minimap display at the beginning of the game.
    /// The map is shown at game start, and the player can toggle it via buttons.
    /// Two timer texts are displayed: one on the map panel and another on the player's panel.
    /// When the countdown expires, the minimap automatically closes.
    /// Also synchronizes the solution path display on the minimap.
    /// </summary>
    public class LabyrinthMapController : MonoBehaviour
    {
        public static LabyrinthMapController Instance { get; private set; }

        [SerializeField] private GameObject mapDisplay; // UI panel showing the map.
        [SerializeField] private Button showMapButton;
        [SerializeField] private Button hideMapButton;
        [SerializeField] private float toggleDuration = 100f; // Total time before auto-closing the map.

        // Timer texts for displaying remaining time.
        [SerializeField] private Text mapTimerText;      // Text displayed on the map panel.
        [SerializeField] private Text playerTimerText;   // Text displayed on the player's panel above the "Show Map" button.

        // Reference to the solution path drawer component.
        [SerializeField] private LabyrinthMinimapSolutionPathDrawer solutionPathDrawer;

        private float timer;
        private bool canToggle = true;

        /// <summary>
        /// Returns true if the map display is currently active.
        /// </summary>
        public bool IsMapActive => mapDisplay != null && mapDisplay.activeSelf;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (mapDisplay != null)
                mapDisplay.SetActive(true);

            if (hideMapButton != null)
                hideMapButton.onClick.AddListener(HideMap);
            if (showMapButton != null)
                showMapButton.onClick.AddListener(ShowMap);
        }

        private void Update()
        {
            timer += Time.deltaTime;
            // Calculate remaining time (not less than zero)
            float remainingTime = Mathf.Max(0, toggleDuration - timer);
            int secondsRemaining = Mathf.CeilToInt(remainingTime);

            // Update both timer texts.
            if (mapTimerText != null)
                mapTimerText.text = secondsRemaining.ToString();
            if (playerTimerText != null)
                playerTimerText.text = secondsRemaining.ToString();

            // If time has expired and toggling is still active.
            if (timer >= toggleDuration && canToggle)
            {
                // Close the minimap.
                if (mapDisplay != null)
                    mapDisplay.SetActive(false);

                // Remove the "Show Map" button.
                if (showMapButton != null)
                    Destroy(showMapButton.gameObject);

                canToggle = false;
            }
        }

        /// <summary>
        /// Hides the map display.
        /// </summary>
        public void HideMap()
        {
            if (mapDisplay != null)
                mapDisplay.SetActive(false);
        }

        /// <summary>
        /// Shows the map display (only allowed within toggleDuration seconds).
        /// </summary>
        public void ShowMap()
        {
            if (canToggle && mapDisplay != null)
                mapDisplay.SetActive(true);
        }

        /// <summary>
        /// Sets the solution path on the minimap by passing a list of world positions to the path drawer.
        /// </summary>
        /// <param name="positions">List of world-space positions representing the solution path.</param>
        public void SetSolutionPath(List<Vector3> positions)
        {
            if (solutionPathDrawer != null)
            {
                solutionPathDrawer.DrawSolutionPath(positions);
            }
        }
    }
}
