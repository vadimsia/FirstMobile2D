using Resources.Scripts.Player;
using UnityEngine;
using System.Collections;

namespace Resources.Scripts.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField, Range(1, 15)]
        private int speed = 1;

        // Flag to enable or disable pushing the player on contact.
        [Tooltip("Enables or disables pushing the player on contact.")]
        public bool pushPlayer = true;

        private float currentSpeed;
        private Coroutine slowCoroutine;
        private Rigidbody2D rb;
        private PlayerController player;

        private void Start()
        {
            player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            currentSpeed = speed;
            rb = GetComponent<Rigidbody2D>(); // Used for physics interactions (e.g., push)
        }

        private void Update()
        {
            FollowPlayer();
        }

        // Moves the enemy toward the player if within range.
        private void FollowPlayer()
        {
            if (player == null)
            {
                Destroy(gameObject);
                return;
            }

            float distanceToPlayer = (transform.position - player.transform.position).magnitude;

            if (distanceToPlayer > 5)
                return;

            transform.position = Vector3.Lerp(transform.position, player.transform.position, Time.deltaTime * currentSpeed);

            if (distanceToPlayer > 1)
                return;

            player.TakeDamage(this);
            Debug.Log("Hit");
        }

        // Applies a slow effect to the enemy.
        public void ApplySlow(float slowFactor, float duration)
        {
            if (slowCoroutine != null)
                StopCoroutine(slowCoroutine);

            slowCoroutine = StartCoroutine(SlowEffect(slowFactor, duration));
        }

        // Coroutine to handle the slow effect duration.
        private IEnumerator SlowEffect(float slowFactor, float duration)
        {
            currentSpeed = speed * slowFactor;
            yield return new WaitForSeconds(duration);
            currentSpeed = speed;
        }

        // Applies a push force to the enemy (does not affect the player).
        public void ApplyPush(Vector2 force)
        {
            if (rb != null)
                rb.AddForce(force, ForceMode2D.Impulse);
        }
    }
}
