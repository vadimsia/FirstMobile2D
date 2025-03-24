using Resources.Scripts.Misc;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Resources.Scripts.GameManagers
{
    /// <summary>
    /// Manages the first part of the game, including session timer and player status check.
    /// </summary>
    public class FirstPartGameManager : MonoBehaviour
    {
        [Header("Session Settings")]
        [SerializeField, Range(3, 30), Tooltip("Duration of the session before switching to the second part.")]
        private float sessionTimer = 10f;
        [SerializeField, Tooltip("UI label to display the remaining session time.")]
        private TextMeshProUGUI timerLabel;

        [Header("Debug Settings")]
        [SerializeField, Tooltip("Enable debug logging for session events.")]
        private bool debugLog = false;

        public static FirstPartGameManager singletone;

        private float sessionDelay;
        private GameObject player;

        private void Start()
        {
            singletone = this;
            sessionDelay = sessionTimer;
            // Find the player using a tag defined in ETag enumeration.
            player = GameObject.FindWithTag(ETag.Player.ToString());
        }

        private void Update()
        {
            UpdateSessionTimer();
            CheckPlayerAlive();
        }

        /// <summary>
        /// Checks if the player is destroyed and reloads the scene if so.
        /// </summary>
        private void CheckPlayerAlive()
        {
            // If player is null (destroyed), reload the scene.
            if (player != null)
            {
                return;
            }

            if (debugLog)
            {
                Debug.Log("Player destroyed. Reloading FirstPart scene.");
            }
            ReloadScene();
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
                    Debug.Log("Session timer ended. Switching to SecondPart scene.");
                }
                // Load the scene corresponding to the second part.
                SceneManager.LoadScene((int)EScene.SecondPart);
            }
            else
            {
                // Decrease the session delay based on elapsed time.
                sessionDelay -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Reloads the first part of the game.
        /// </summary>
        private static void ReloadScene()
        {
            SceneManager.LoadScene((int)EScene.FirstPart);
        }
    }
}
