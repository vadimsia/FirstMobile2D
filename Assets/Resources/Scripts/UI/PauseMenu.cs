using UnityEngine;
using UnityEngine.SceneManagement;

namespace Resources.Scripts.UI
{
    public class PauseMenu : MonoBehaviour
    {
        // Reference to the pause panel (assigned in the Inspector)
        public GameObject pausePanel;

        // Reference to the settings panel (currently for demonstration)
        public GameObject settingsPanel;

        // Flag indicating whether the game is paused
        private bool isPaused;

        private void Start()
        {
            // Hide panels when the game starts
            if (pausePanel != null)
                pausePanel.SetActive(false);

            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        // Called when the "Pause" button is pressed
        public void OnPauseButtonPressed()
        {
            if (!isPaused)
                PauseGame();
            else
                ResumeGame();
        }

        // Pauses the game
        public void PauseGame()
        {
            if (pausePanel != null)
                pausePanel.SetActive(true);

            // Stop game time
            Time.timeScale = 0f;
            isPaused = true;
        }

        // Resumes the game (called when "Resume" button is pressed)
        public void ResumeGame()
        {
            if (pausePanel != null)
                pausePanel.SetActive(false);

            // Resume game time
            Time.timeScale = 1f;
            isPaused = false;
        }

        // Opens the settings panel (pause panel remains active)
        public void OpenSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
            else
                Debug.Log("Settings panel is not assigned!");
        }

        // Closes the settings panel and returns to the pause panel
        public void CloseSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
            else
                Debug.Log("Settings panel is not assigned!");
        }

        // Exits to the main menu (scene named "Menu")
        // Note: Ensure the "Menu" scene is enabled in the build settings.
        public void ExitToMainMenu()
        {
            // Reset timeScale before transitioning to ensure the menu functions correctly
            Time.timeScale = 1f;
            SceneManager.LoadScene("Menu");
        }
    }
}
