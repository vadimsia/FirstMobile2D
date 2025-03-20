using UnityEngine;
using Resources.Scripts.Enemy;

namespace Resources.Scripts.SpellMode.Skills
{
    /// <summary>
    /// ----------------------------------------------------------------------------
    /// Skill that slows enemies within a specified radius.
    /// ----------------------------------------------------------------------------
    /// </summary>
    public class SlowEnemiesSkill : SkillBase
    {
        [Header("Slow Enemies Settings")]
        [Tooltip("Radius within which enemies will be slowed.")]
        public float effectRadius = 5f;
        [Tooltip("Slow multiplier (e.g., 0.5 reduces enemy speed to 50%).")]
        public float slowMultiplier = 0.5f;
        [Tooltip("Duration of the slow effect in seconds.")]
        public float slowDuration = 3f;

        // Pre-allocated array to store collider results.
        private Collider2D[] resultsBuffer = new Collider2D[50];

        /// <summary>
        /// Activates the slow effect on all enemies within the effect radius.
        /// </summary>
        protected override void ActivateSkill()
        {
            // Create a default filter. Adjust filter options (e.g., layerMask, triggers) as needed.
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = false;
            // Use the non-allocating overload with a ContactFilter2D.
            int count = Physics2D.OverlapCircle(transform.position, effectRadius, filter, resultsBuffer);
            for (int i = 0; i < count; i++)
            {
                Collider2D hit = resultsBuffer[i];
                if (hit.CompareTag("Enemy"))
                {
                    EnemyController enemy = hit.GetComponent<EnemyController>();
                    if (enemy != null)
                    {
                        enemy.ApplySlow(slowMultiplier, slowDuration);
                    }
                }
            }
            Debug.Log("SlowEnemiesSkill activated.");
        }

        /// <summary>
        /// Draws the effect radius in the editor.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, effectRadius);
        }
    }
}
