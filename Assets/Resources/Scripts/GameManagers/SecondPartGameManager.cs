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

        private float _sessionDelay;

        private void Start()
        {
            singletone = this;
            _sessionDelay = sessionTimer;
        }
        
        private void Update()
        {
            UpdateSessionTimer();
        }

        private void UpdateSessionTimer()
        {
            timerLabel.text = "Timer: " + _sessionDelay.ToString("#.0");

            if (_sessionDelay <= 0) {
                SceneManager.LoadScene((int)EScene.FirstPart);
            } else {
                _sessionDelay -= Time.deltaTime;
            }
        }
    }
}
