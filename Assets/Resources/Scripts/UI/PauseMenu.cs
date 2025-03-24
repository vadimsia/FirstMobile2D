using UnityEngine;
using UnityEngine.SceneManagement;

namespace Resources.Scripts.UI
{
    /// <summary>
    /// Manages the pause menu, allowing the game to be paused/resumed,
    /// opening settings, and exiting to the main menu.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("UI Panels")]
        [Tooltip("UI panel displayed when the game is paused.")]
        public GameObject pausePanel;
        [Tooltip("UI panel for settings (currently for demonstration).")]
        public GameObject settingsPanel;

        private bool isPaused;

        private void Start()
        {
            // Hide both pause and settings panels at game start.
            if (pausePanel != null)
                pausePanel.SetActive(false);
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        /// <summary>
        /// Called when the Pause button is pressed.
        /// Toggles between pausing and resuming the game.
        /// </summary>
        public void OnPauseButtonPressed()
        {
            if (!isPaused)
                PauseGame();
            else
                ResumeGame();
        }

        /// <summary>
        /// Pauses the game by showing the pause panel and stopping time.
        /// </summary>
        public void PauseGame()
        {
            if (pausePanel != null)
                pausePanel.SetActive(true);

            Time.timeScale = 0f;
            isPaused = true;
        }

        /// <summary>
        /// Resumes the game by hiding the pause panel and restarting time.
        /// </summary>
        public void ResumeGame()
        {
            if (pausePanel != null)
                pausePanel.SetActive(false);

            Time.timeScale = 1f;
            isPaused = false;
        }

        /// <summary>
        /// Opens the settings panel while keeping the pause panel active.
        /// </summary>
        public void OpenSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
            else
                Debug.LogWarning("Settings panel is not assigned!");
        }

        /// <summary>
        /// Closes the settings panel and returns to the pause panel.
        /// </summary>
        public void CloseSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
            else
                Debug.LogWarning("Settings panel is not assigned!");
        }

        /// <summary>
        /// Exits to the main menu scene.
        /// </summary>
        public void ExitToMainMenu()
        {
            // Reset time scale before transitioning.
            Time.timeScale = 1f;
            SceneManager.LoadScene("Menu");
        }
    }
}
