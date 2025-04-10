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

        [Tooltip("Время жизни снаряда (сек).")]
        public float lifeTime = 5f;

        /// <summary>
        /// Устанавливает параметры снаряда:
        /// - направление движения,
        /// - скорость,
        /// - длительность заморозки (binding) игрока,
        /// - время жизни снаряда,
        /// - потенциальный урон (если требуется).
        /// </summary>
        /// <param name="direction">Направление полёта снаряда.</param>
        /// <param name="projectileSpeed">Скорость снаряда.</param>
        /// <param name="bindDuration">Длительность эффекта заморозки игрока.</param>
        /// <param name="projectileLifeTime">Время жизни снаряда.</param>
        /// <param name="projectileDamage">Урон снаряда (если требуется).</param>
        public void SetParameters(Vector2 direction, float projectileSpeed, float bindDuration, float projectileLifeTime, float projectileDamage)
        {
            moveDirection = direction;
            speed = projectileSpeed;
            bindingDuration = bindDuration;
            lifeTime = projectileLifeTime;
            // Если нужен дополнительный урон – сохранить его в поле (если потребуется)

            Destroy(gameObject, lifeTime);
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            // Рекомендуется использовать непрерывную детекцию коллизий для быстродвижущихся объектов.
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void FixedUpdate()
        {
            // Перемещаем снаряд через Rigidbody2D, чтобы физика учитывалась
            rb.linearVelocity = moveDirection * speed;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Если снаряд попадает по игроку
            if (collision.CompareTag("Player"))
            {
                PlayerController player = collision.GetComponent<PlayerController>();
                if (player != null)
                {
                    // Применяем эффект заморозки (binding) на время bindingDuration.
                    // Важно, чтобы метод ApplyBinding в PlayerController корректно реализовывал блокирование движения.
                    player.ApplyBinding(bindingDuration);
                    Debug.Log($"Снаряд гоблина попал в игрока. Заморозка на {bindingDuration} сек.");
                }
                // Снаряд уничтожается при попадании в игрока
                Destroy(gameObject);
            }
            // Уничтожаем снаряд при столкновении с любыми объектами, кроме врагов
            else if (!collision.CompareTag("Enemy"))
            {
                Destroy(gameObject);
            }
        }
    }
}
