using UnityEngine;
using Resources.Scripts.Player;

namespace Resources.Scripts.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class GoblinProjectile : MonoBehaviour
    {
        private Vector2 moveDirection;
        private float speed;
        private float bindingDuration;
        private Rigidbody2D rb;

        [Tooltip("Lifetime of the projectile in seconds.")]
        [SerializeField] private float lifeTime = 5f;
        
        public void SetParameters(Vector2 direction, float projectileSpeed, float bindDuration, float projectileLifeTime, float projectileDamage)
        {
            moveDirection = direction;
            speed = projectileSpeed;
            bindingDuration = bindDuration;
            lifeTime = projectileLifeTime;

            // Schedule automatic destruction after lifespan expires.
            Destroy(gameObject, lifeTime);
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            // Use continuous collision detection for fast-moving projectiles.
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void FixedUpdate()
        {
            // Move the projectile via physics.
            rb.linearVelocity = moveDirection * speed;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                var player = collision.GetComponent<PlayerController>();
                if (player != null)
                {
                    // Apply freezing effect on the player.
                    player.ApplyBinding(bindingDuration);
                    Debug.Log($"Goblin projectile hit player. Freezing for {bindingDuration} seconds.");
                }

                Destroy(gameObject);
            }
            // Destroy projectile on collision with anything except other enemies.
            else if (!collision.CompareTag("Enemy"))
            {
                Destroy(gameObject);
            }
        }
    }
}
