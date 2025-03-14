using UnityEngine;
using Resources.Scripts.Player;

namespace Resources.Scripts.Labyrinth

{
    public class LabyrinthTrapEffect : MonoBehaviour
    {
        public float stunDuration = 2f; // Stun duration in seconds

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