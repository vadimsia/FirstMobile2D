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
        /// Устанавливает параметры снаряда: направление, скорость и время связывания.
        /// </summary>
        public void SetParameters(Vector2 direction, float projectileSpeed, float bindDuration)
        {
            moveDirection = direction;
            speed = projectileSpeed;
            bindingDuration = bindDuration;
            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            // Снаряд летит по заданному направлению с указанной скоростью
            transform.Translate(moveDirection * speed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                // Если снаряд попадает по игроку, применяем эффект связывания
                PlayerController player = collision.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.ApplyBinding(bindingDuration);
                }
                Destroy(gameObject);
            }
            else if (!collision.CompareTag("Enemy"))
            {
                // При столкновении с другими объектами (кроме врагов) снаряд уничтожается
                Destroy(gameObject);
            }
        }
    }
}