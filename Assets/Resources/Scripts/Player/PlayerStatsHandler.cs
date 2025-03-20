using UnityEngine;

namespace Resources.Scripts.Player
{
    /// <summary>
    /// Handles player's stats: health, mana, and fairy count.
    /// </summary>
    public class PlayerStatsHandler : MonoBehaviour
    {
        // Count of fairies collected by the player.
        [field: SerializeField] public int FairyCount { get; set; }

        // Health fields.
        [SerializeField, Range(5, 50)]
        private int health = 20;
        [SerializeField]
        private int maxHealth = 50;

        /// <summary>
        /// Gets or sets the player's health.
        /// The value is clamped between 0 and maxHealth.
        /// </summary>
        public int Health
        {
            get { return health; }
            set { health = Mathf.Clamp(value, 0, maxHealth); }
        }

        // Mana properties.
        [SerializeField]
        private float maxMana = 100f;
        [SerializeField]
        private float currentMana; // No need to initialize (default is 0)
        [SerializeField]
        private float manaRegenRate = 10f; // Mana regenerated per second.

        // Timer for delaying mana regeneration after spell usage.
        private float manaRegenDelayTimer; // Default value is 0
        // Delay time (in seconds) after using mana before regeneration resumes.
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
            // If the regeneration delay is active, count down the timer.
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
        /// Regenerates mana over time.
        /// </summary>
        private void RegenerateMana()
        {
            currentMana = Mathf.Min(currentMana + manaRegenRate * Time.deltaTime, maxMana);
        }

        /// <summary>
        /// Attempts to use a specified amount of mana.
        /// If successful, subtracts the mana and triggers the regeneration delay.
        /// </summary>
        /// <param name="amount">Amount of mana to use.</param>
        /// <returns>True if mana was successfully used; otherwise, false.</returns>
        public bool UseMana(float amount)
        {
            if (currentMana >= amount)
            {
                currentMana -= amount;
                // Trigger the regeneration delay after using mana.
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
