using Resources.Scripts.Misc;
using Resources.Scripts.Player;
using UnityEngine;
using System.Collections;

namespace Resources.Scripts.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField, Range(1, 15)] private int speed = 1;
        private float currentSpeed;
        private Coroutine slowCoroutine;

        private PlayerController _player;

        private void Start()
        {
            _player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            currentSpeed = speed;
        }

        private void Update()
        {
            UpdateFollow();
        }

        private void UpdateFollow()
        {
            if (_player == null)
            {
                Destroy(gameObject);
                return;
            }

            float distanceToPlayer = (transform.position - _player.transform.position).magnitude; 

            if (distanceToPlayer > 5)
                return;
        
            transform.position = Vector3.Lerp(transform.position, _player.transform.position, Time.deltaTime * currentSpeed);

            if (distanceToPlayer > 1)
                return;
        
            _player.TakeDamage(this);
            Debug.Log("Hit");
        }

        public void ApplySlow(float slowFactor, float duration)
        {
            if (slowCoroutine != null)
                StopCoroutine(slowCoroutine);
            slowCoroutine = StartCoroutine(SlowEffect(slowFactor, duration));
        }

        private IEnumerator SlowEffect(float slowFactor, float duration)
        {
            currentSpeed = speed * slowFactor;
            yield return new WaitForSeconds(duration);
            currentSpeed = speed;
        }
    }
}