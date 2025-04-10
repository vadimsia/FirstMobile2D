using Resources.Scripts.Player;
using UnityEngine;

namespace Resources.Scripts.Enemy
{
    public class GoblinProjectile : MonoBehaviour
    {
        private Vector2 moveDirection;
        private float speed;
        private float bindingDuration;
        
        [Tooltip("Время жизни снаряда (сек).")]
        public float lifeTime = 5f;

        /// <summary>
        /// Устанавливает параметры снаряда:
        /// - направление движения,
        /// - скорость,
        /// - длительность связывания игрока,
        /// - время жизни снаряда,
        /// - урон снаряда (если применяется).
        /// </summary>
        public void SetParameters(Vector2 direction, float projectileSpeed, float bindDuration, float projectileLifeTime, float projectileDamage)
        {
            moveDirection = direction;
            speed = projectileSpeed;
            bindingDuration = bindDuration;
            lifeTime = projectileLifeTime;
            // Если нужен дополнительный урон, можно сохранить его в отдельном поле и обработать
            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            transform.Translate(moveDirection * speed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                PlayerController player = collision.GetComponent<PlayerController>();
                if (player != null)
                {
                    // Применяем эффект связывания (например, заморозку) к игроку.
                    player.ApplyBinding(bindingDuration);
                    // Если нужен дополнительный урон, его можно обработать тут, например:
                    // player.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
            else if (!collision.CompareTag("Enemy"))
            {
                Destroy(gameObject);
            }
        }
    }
}