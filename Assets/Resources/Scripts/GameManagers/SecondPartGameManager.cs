using Resources.Scripts.Misc;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Resources.Scripts.GameManagers
{
    /// <summary>
    /// Manages the second part of the game, handling session timing and scene switching.
    /// </summary>
    public class SecondPartGameManager : MonoBehaviour
    {
        [Header("Session Settings")]
        [SerializeField, Range(3, 30), Tooltip("Duration of the session before switching back to the first part.")]
        private float sessionTimer = 10f;
        [SerializeField, Tooltip("UI label to display the remaining session time.")]
        private TextMeshProUGUI timerLabel;

        [Header("Debug Settings")]
        [SerializeField, Tooltip("Enable debug logging for session events.")]
        private bool debugLog = false;

        public static SecondPartGameManager singletone;

        private float sessionDelay;

        private void Start()
        {
            singletone = this;
            sessionDelay = sessionTimer;
        }

        private void Update()
        {
            UpdateSessionTimer();
        }

        /// <summary>
        /// Updates the session timer and switches scene when time is up.
        /// </summary>
        private void UpdateSessionTimer()
        {
            // Update UI label with the remaining time.
            timerLabel.text = "Timer: " + sessionDelay.ToString("#.0");

            if (sessionDelay <= 0)
            {
                if (debugLog)
                {
                    Debug.Log("Session timer ended. Switching back to FirstPart scene.");
                }
                // Load the scene corresponding to the first part.
                SceneManager.LoadScene((int)EScene.FirstPart);
            }
            else
            {
                // Decrease the session delay based on elapsed time.
                sessionDelay -= Time.deltaTime;
            }
        }
    }
}