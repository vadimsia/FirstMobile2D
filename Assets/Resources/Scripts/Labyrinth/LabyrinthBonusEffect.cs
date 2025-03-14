using UnityEngine;
using Resources.Scripts.Player;

namespace Resources.Scripts.Labyrinth
{
    public class LabyrinthBonusEffect : MonoBehaviour
    {
        public float speedMultiplier = 1.5f; // Speed UP on 50%

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.IncreaseSpeed(speedMultiplier);
                }

                Destroy(gameObject);
            }
        }
    }
}