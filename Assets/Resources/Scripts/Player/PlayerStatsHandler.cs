using UnityEngine;

namespace Resources.Scripts.Player
{
    public class PlayerStatsHandler : MonoBehaviour
    {
        [field: SerializeField] public int FairyCount { get; set; }
        [field: SerializeField, Range(5, 50)] public int Health { get; set; } = 20;
    }
}