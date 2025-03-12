using Resources.Scripts.Misc;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Resources.Scripts.GameManagers
{
    public class FirstPartGameManager : MonoBehaviour
    {
        [SerializeField, Range(3, 30)] private float sessionTimer = 10f;
        [SerializeField] private TextMeshProUGUI timerLabel;

        public static FirstPartGameManager singletone;

        private float _sessionDelay;
        private GameObject _player;

        private void Start()
        {
            singletone = this;
            _sessionDelay = sessionTimer;
            _player = GameObject.FindWithTag(ETag.Player.ToString());
        }

        private void Update()
        {
            UpdateSessionTimer();
            UpdateCheckPlayerAlive();
        }

        private void UpdateCheckPlayerAlive()
        {
            if (!_player.IsDestroyed()) {
                return;
            }

            ReloadScene();
        }

        private void UpdateSessionTimer()
        {
            timerLabel.text = "Timer: " + _sessionDelay.ToString("#.0");

            if (_sessionDelay <= 0)
            {
                SceneManager.LoadScene((int)EScene.SecondPart);
            }
            else 
            {
                _sessionDelay -= Time.deltaTime;
            }
        }

        private static void ReloadScene()
        {
            SceneManager.LoadScene((int)EScene.FirstPart);
        }
    }
}

