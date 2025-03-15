using UnityEngine;
using UnityEngine.UI;

namespace Resources.Scripts.UI
{
    /// <summary>
    /// Updates the UI elements displaying the player's mana.
    /// </summary>
    public class ManaUIController : MonoBehaviour
    {
        [SerializeField]
        private Image manaFillImage; // UI Image with fill mode.
        [SerializeField]
        private Text manaText; // Legacy Text showing current mana in the format "current/max".

        [SerializeField]
        private Resources.Scripts.Player.PlayerStatsHandler playerStats;

        private void Update()
        {
            if (playerStats != null && manaFillImage != null && manaText != null)
            {
                float fillAmount = playerStats.CurrentMana / playerStats.MaxMana;
                manaFillImage.fillAmount = fillAmount;
                manaText.text = $"{(int)playerStats.CurrentMana}/{(int)playerStats.MaxMana}";
            }
        }
    }
}