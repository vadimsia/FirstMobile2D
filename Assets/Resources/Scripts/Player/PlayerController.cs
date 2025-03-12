using Resources.Scripts.Enemy;
using Resources.Scripts.Fairy;
using Resources.Scripts.Misc;
using UnityEngine;

namespace Resources.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField, Range(3, 20)] float speed = 5f;


        PlayerStatsHandler playerStats;

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
        
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
        
            transform.Translate(speed * Time.deltaTime * new Vector2(horizontal, vertical));
        }

        void Destroy()
        {
            Destroy(gameObject);    
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag(ETag.Fairy.ToString())) {
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
                Destroy();

            EntityUtils.MakeDash(transform, transform.position - enemy.transform.position);
        }
    }
}
