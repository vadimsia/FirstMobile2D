using UnityEngine;
using Resources.Scripts.Player;

namespace Resources.Scripts.Labyrinth
{
    /// <summary>
    /// Applies a trap effect that stuns the player when triggered.
    /// </summary>
    public class LabyrinthTrapEffect : MonoBehaviour
    {
        [Header("Trap Settings")]
        [SerializeField, Tooltip("Duration of the stun effect in seconds.")]
        public float stunDuration = 2f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.Stun(stunDuration);
                }
                Destroy(gameObject);
            }
        }
    }
}