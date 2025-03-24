using UnityEngine;

namespace Resources.Scripts.Player
{
    /// <summary>
    /// Handles the player's stats, including health, mana, and fairy collection.
    /// </summary>
    public class PlayerStatsHandler : MonoBehaviour
    {
        [Header("Fairy Collection")]
        [SerializeField, Tooltip("Number of fairies collected by the player.")]
        public int FairyCount { get; set; }

        [Header("Health Settings")]
        [SerializeField, Range(5, 50), Tooltip("Initial health of the player.")]
        private int health = 20;
        [SerializeField, Tooltip("Maximum health of the player.")]
        private int maxHealth = 50;

        /// <summary>
        /// Gets or sets the player's health, clamped between 0 and maxHealth.
        /// </summary>
        public int Health
        {
            get { return health; }
            set { health = Mathf.Clamp(value, 0, maxHealth); }
        }

        [Header("Mana Settings")]
        [SerializeField, Tooltip("Maximum mana available to the player.")]
        private float maxMana = 100f;
        [SerializeField, Tooltip("Current mana available to the player.")]
        private float currentMana;
        [SerializeField, Tooltip("Rate at which mana regenerates per second.")]
        private float manaRegenRate = 10f;

        // Timer for delaying mana regeneration after spell usage.
        private float manaRegenDelayTimer;
        // Constant delay time (in seconds) after mana usage.
        private const float ManaRegenDelayAfterSpell = 2f;

        /// <summary>
        /// Gets the current mana.
        /// </summary>
        public float CurrentMana => currentMana;

        /// <summary>
        /// Gets the maximum mana.
        /// </summary>
        public float MaxMana => maxMana;

        private void Update()
        {
            // Count down mana regeneration delay if active.
            if (manaRegenDelayTimer > 0)
            {
                manaRegenDelayTimer -= Time.deltaTime;
            }
            else
            {
                RegenerateMana();
            }
        }

        /// <summary>
        /// Regenerates mana over time, ensuring it does not exceed maxMana.
        /// </summary>
        private void RegenerateMana()
        {
            currentMana = Mathf.Min(currentMana + manaRegenRate * Time.deltaTime, maxMana);
        }

        /// <summary>
        /// Attempts to use a specified amount of mana.
        /// Returns true if successful and triggers a regeneration delay.
        /// </summary>
        /// <param name="amount">Amount of mana to use.</param>
        /// <returns>True if mana is successfully used; otherwise, false.</returns>
        public bool UseMana(float amount)
        {
            if (currentMana >= amount)
            {
                currentMana -= amount;
                manaRegenDelayTimer = ManaRegenDelayAfterSpell;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Restores a specified amount of mana.
        /// </summary>
        /// <param name="amount">Amount of mana to restore.</param>
        public void RestoreMana(float amount)
        {
            currentMana = Mathf.Min(currentMana + amount, maxMana);
        }
    }
}
