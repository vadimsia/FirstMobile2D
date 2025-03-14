using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Resources.Scripts.TrapSkill
{
    public class TrapPlacer : MonoBehaviour
    {
        [Header("Trap Settings")] 
        public GameObject trapPrefab;        // Trap prefab (with attached Trap script)
        public Transform trapSpawnPoint;     // Trap spawn point (e.g., player's position)
        public float cooldownTime = 10f;     // Cooldown time in seconds

        [Header("UI Elements for Cooldown Display")]
        public Image cooldownFillImage;      // Cooldown image (Fill mode, e.g., radial)
        public Text cooldownText;            // Text displaying remaining cooldown time

        private bool canPlace = true;        // Can the trap be placed now?
        private float cooldownTimer;         // Cooldown timer

        private void Start()
        {
            // Disable cooldown UI elements at game start
            if (cooldownFillImage != null)
                cooldownFillImage.gameObject.SetActive(false);

            if (cooldownText != null)
                cooldownText.gameObject.SetActive(false);
        }

        private void Update()
        {
            // If the trap is on cooldown – update timer and UI
            if (!canPlace)
            {
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer < 0)
                    cooldownTimer = 0f;

                if (cooldownFillImage != null)
                {
                    // Fill from 1 to 0
                    cooldownFillImage.fillAmount = cooldownTimer / cooldownTime;
                }

                if (cooldownText != null)
                {
                    cooldownText.text = Mathf.Ceil(cooldownTimer).ToString();
                }

                if (cooldownTimer <= 0f)
                {
                    canPlace = true;
                    
                    if (cooldownFillImage != null)
                    {
                        cooldownFillImage.fillAmount = 0f;
                        cooldownFillImage.gameObject.SetActive(false);
                    }

                    if (cooldownText != null)
                    {
                        cooldownText.text = "";
                        cooldownText.gameObject.SetActive(false);
                    }
                }
            }

            // Example: Place a trap when pressing the T key
            if (Input.GetKeyDown(KeyCode.T))
            {
                PlaceTrap();
            }
        }

        public void PlaceTrap()
        {
            // Traps can only be placed in the "FirstPartScene"
            if (SceneManager.GetActiveScene().name != "FirstPartScene")
            {
                Debug.Log("Trap cannot be placed in this scene.");
                return;
            }

            if (canPlace)
            {
                Instantiate(trapPrefab, trapSpawnPoint.position, trapSpawnPoint.rotation);
                Debug.Log("Trap placed.");
                
                canPlace = false;
                cooldownTimer = cooldownTime;

                if (cooldownFillImage != null)
                {
                    cooldownFillImage.gameObject.SetActive(true);
                    cooldownFillImage.fillAmount = 1f;
                }

                if (cooldownText != null)
                {
                    cooldownText.gameObject.SetActive(true);
                    cooldownText.text = cooldownTime.ToString();
                }
            }
            else
            {
                Debug.Log("Trap is on cooldown.");
            }
        }
    }
}
