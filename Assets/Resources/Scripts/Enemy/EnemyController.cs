using Resources.Scripts.Misc;
using Resources.Scripts.Player;
using Unity.VisualScripting;
using UnityEngine;

namespace Resources.Scripts.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField, Range(3, 15)] private int speed = 5;

        private PlayerController _player;

        private void Start()
        {
            _player = GameObject.FindWithTag(ETag.Player.ToString()).GetComponent<PlayerController>();
        }

        private void Update()
        {
            UpdateFollow();
        }

        private void UpdateFollow()
        {
            if (_player.IsDestroyed())
            {
                Destroy(gameObject);
                return;
            }

            var distanceToPlayer = (transform.position - _player.transform.position).magnitude; 

            if (distanceToPlayer > 5)
                return;
        
            transform.position = Vector3.Lerp(transform.position, _player.transform.position, Time.deltaTime * speed);

            if (distanceToPlayer > 1)
                return;
        
            _player.TakeDamage(this);
            Debug.Log("Hit");
        }
    }
}
