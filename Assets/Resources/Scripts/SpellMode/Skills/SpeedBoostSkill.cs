using UnityEngine;
using Resources.Scripts.Player;

namespace Resources.Scripts.SpellMode.Skills
{
    /// <summary>
    /// ----------------------------------------------------------------------------
    /// Skill that temporarily boosts the player's speed.
    /// ----------------------------------------------------------------------------
    /// </summary>
    public class SpeedBoostSkill : SkillBase
    {
        [Header("Speed Boost Settings")]
        [Tooltip("Multiplier to increase the player's speed (e.g., 1.5 increases speed by 50%).")]
        public float boostMultiplier = 1.5f;
        [Tooltip("Duration of the speed boost in seconds.")]
        public float boostDuration = 5f;

        /// <summary>
        /// Activates the speed boost on the player.
        /// </summary>
        protected override void ActivateSkill()
        {
            // Use the recommended method to find any instance of PlayerController
            PlayerController player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                player.IncreaseSpeed(boostMultiplier);
            }
            Debug.Log("SpeedBoostSkill activated.");
        }
    }
}