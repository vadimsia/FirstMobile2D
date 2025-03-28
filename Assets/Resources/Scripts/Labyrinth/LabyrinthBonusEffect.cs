using UnityEngine;
using Resources.Scripts.Player;

namespace Resources.Scripts.Labyrinth
{
    /// <summary>
    /// Applies a bonus effect to the player when triggered.
    /// </summary>
    public class LabyrinthBonusEffect : MonoBehaviour
    {
        [Header("Bonus Settings")]
        [SerializeField, Tooltip("Multiplier to increase the player's speed.")]
        public float speedMultiplier = 1.5f;

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