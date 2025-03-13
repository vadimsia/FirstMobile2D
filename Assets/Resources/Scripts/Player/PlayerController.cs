using UnityEngine;
using System.Collections;
using Resources.Scripts.Enemy;
using Resources.Scripts.Fairy;
using Resources.Scripts.Misc;

namespace Resources.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField, Range(3, 20)] float keyboardSpeed = 5f;
        [SerializeField, Range(1, 10)] float joystickSpeed = 3f;
        [SerializeField] private Joystick joystick;
        [SerializeField] private GameObject trapPrefab;

        private PlayerStatsHandler playerStats;
        private float currentSlowMultiplier = 1f;
        private Coroutine slowCoroutine;

        void Start()
        {
            playerStats = GetComponent<PlayerStatsHandler>();     
        }

        void Update()
        {
            UpdateMove();
        }

        void UpdateMove()
        {
            if (Input.GetKey(KeyCode.Space))
                return;

            float horizontal = joystick != null ? joystick.Horizontal() : Input.GetAxis("Horizontal");
            float vertical   = joystick != null ? joystick.Vertical()   : Input.GetAxis("Vertical");

            float currentSpeed = (joystick != null ? joystickSpeed : keyboardSpeed) * currentSlowMultiplier;

            transform.Translate(new Vector2(horizontal, vertical) * currentSpeed * Time.deltaTime);
        }

        // Вызывается при достижении 0 здоровья
        private void Die()
        {
            // Отключаем все UI-элементы (Canvas) с использованием нового метода
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases)
            {
                canvas.gameObject.SetActive(false);
            }

            Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("Fairy"))
            {
                FairyController controller = collider.transform.GetComponent<FairyController>();
                controller.Destroy();
                playerStats.FairyCount += 1;
            }
        }

        public void TakeDamage(EnemyController enemy)
        {
            EnemyStatsHandler stats = enemy.GetComponent<EnemyStatsHandler>();
            playerStats.Health -= stats.Damage;

            if (playerStats.Health <= 0)
                Die();

            EntityUtils.MakeDash(transform, transform.position - enemy.transform.position);
        }

        public void ApplySlow(float slowFactor, float duration)
        {
            if (slowCoroutine != null)
                StopCoroutine(slowCoroutine);
            slowCoroutine = StartCoroutine(SlowEffect(slowFactor, duration));
        }

        private IEnumerator SlowEffect(float slowFactor, float duration)
        {
            currentSlowMultiplier = slowFactor;
            yield return new WaitForSeconds(duration);
            currentSlowMultiplier = 1f;
        }
    }
}
