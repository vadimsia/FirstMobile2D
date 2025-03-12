
using UnityEngine;

namespace Resources.Scripts.Player
{
    class PlayerStatsHandler : MonoBehaviour
    {
        [SerializeField] int fairyCount;
        [SerializeField, Range(5, 50)] int health = 20;

        public int FairyCount {
            get {
                return fairyCount;
            }
            set {
                fairyCount = value;
            }
        }

        public int Health {
            get {
                return health;
            }
            set {
                health = value;
            }
        }
    }
}