using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Resources.Scripts.Labyrinth
{
    /// <summary>
    /// Controls the minimap display at the beginning of the game.
    /// The map is shown at game start, and the player can toggle it via buttons.
    /// Two timer texts are displayed: one on the map panel and another on the player's panel.
    /// When the countdown expires, the minimap automatically closes.
    /// </summary>
    public class LabyrinthMapController : MonoBehaviour
    {
        public static LabyrinthMapController Instance { get; private set; }

        [SerializeField] private GameObject mapDisplay; // UI panel showing the map.
        [SerializeField] private Button showMapButton;
        [SerializeField] private Button hideMapButton;
        [SerializeField] private float toggleDuration = 10f; // Total time before auto-closing the map.

        // Timer texts for displaying remaining time.
        [SerializeField] private Text mapTimerText;      // Text displayed on the map panel.
        [SerializeField] private Text playerTimerText;   // Text displayed on the player's panel above the "Show Map" button.

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
            // Вычисляем оставшееся время (не меньше нуля)
            float remainingTime = Mathf.Max(0, toggleDuration - timer);
            int secondsRemaining = Mathf.CeilToInt(remainingTime);

            // Обновляем оба текстовых поля с таймером
            if (mapTimerText != null)
                mapTimerText.text = secondsRemaining.ToString();
            if (playerTimerText != null)
                playerTimerText.text = secondsRemaining.ToString();

            // Если время истекло и переключение ещё активно
            if (timer >= toggleDuration && canToggle)
            {
                // Закрываем миникарту
                if (mapDisplay != null)
                    mapDisplay.SetActive(false);

                // Удаляем кнопку "Show Map"
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
    }
}
