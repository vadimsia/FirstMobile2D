using UnityEngine;
using UnityEngine.UI;
using Resources.Scripts.SpellMode.Skills;
using System.Globalization;

namespace Resources.Scripts.SpellMode.UI
{
    /// <summary>
    /// ----------------------------------------------------------------------------
    /// UI controller for displaying the skill's cooldown.
    /// ----------------------------------------------------------------------------
    /// </summary>
    public class SkillUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("Image component used to display the cooldown fill.")]
        public Image cooldownImage;
        [Tooltip("Text component used to display the remaining cooldown time.")]
        public Text cooldownText;

        [Header("Skill Settings")]
        [Tooltip("Reference to the skill component.")]
        public SkillBase skill;

        private void Update()
        {
            if (skill == null)
                return;

            if (skill.GetIsOnCooldown())
            {
                // Update UI if the skill is on cooldown.
                if (cooldownImage != null)
                    cooldownImage.fillAmount = skill.GetCooldownTimer() / skill.cooldownDuration;
                if (cooldownText != null)
                    cooldownText.text = Mathf.Ceil(skill.GetCooldownTimer())
                        .ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                // Reset UI when the cooldown is finished.
                if (cooldownImage != null)
                    cooldownImage.fillAmount = 0f;
                if (cooldownText != null)
                    cooldownText.text = "";
            }
        }
    }
}