using UnityEngine;

namespace Resources.Scripts.Enemy
{
    public class EnemyStatsHandler : MonoBehaviour
    {
        [SerializeField, Range(3, 15)] int damage;

        public int Damage {
            get { return damage; }
        }
    }
}