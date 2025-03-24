using UnityEngine;
using UnityEngine.UI;

namespace Resources.Scripts.UI
{
    /// <summary>
    /// Updates UI elements to display the player's mana.
    /// It updates a fill image and a text label in the format "current/max".
    /// </summary>
    public class ManaUIController : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("UI Image component with fill mode representing current mana.")]
        [SerializeField] private Image manaFillImage;
        [Tooltip("Legacy Text component displaying mana as 'current/max'.")]
        [SerializeField] private Text manaText;

        [Header("Player Stats")]
        [Tooltip("Reference to the PlayerStatsHandler for current and maximum mana.")]
        [SerializeField] private Resources.Scripts.Player.PlayerStatsHandler playerStats;

        private void Update()
        {
            if (playerStats != null && manaFillImage != null && manaText != null)
            {
                // Calculate fill amount (value between 0 and 1).
                float fillAmount = playerStats.CurrentMana / playerStats.MaxMana;
                manaFillImage.fillAmount = fillAmount;
                manaText.text = $"{(int)playerStats.CurrentMana}/{(int)playerStats.MaxMana}";
            }
        }
    }
}