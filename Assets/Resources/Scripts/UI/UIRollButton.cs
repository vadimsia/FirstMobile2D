using UnityEngine;
using UnityEngine.UI;
using Resources.Scripts.Player;
namespace Resources.Scripts.UI
{
    public class UIRollButton : MonoBehaviour
    {
        public Image cooldownImage;
        public TMPro.TextMeshProUGUI cooldownText;
        public PlayerController player;

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnRollButtonClicked);
        }

        private void OnRollButtonClicked()
        {
            player?.TryRoll();
        }

        public void SetCooldownUI(float value)
        {
            cooldownImage.fillAmount = value;
            cooldownText.text = Mathf.CeilToInt(value).ToString();
            cooldownText.enabled = value > 0f;
        }
    }
}