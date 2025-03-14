using Resources.Scripts.Misc;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Resources.Scripts.GameManagers
{
    public class SecondPartGameManager : MonoBehaviour
    {
        [SerializeField, Range(3, 30)] private float sessionTimer = 10f;
        [SerializeField] private TextMeshProUGUI timerLabel;

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

        private void UpdateSessionTimer()
        {
            timerLabel.text = "Timer: " + sessionDelay.ToString("#.0");

            if (sessionDelay <= 0) {
                SceneManager.LoadScene((int)EScene.FirstPart);
            } else {
                sessionDelay -= Time.deltaTime;
            }
        }
    }
}
