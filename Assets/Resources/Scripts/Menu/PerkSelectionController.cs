using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Resources.Scripts.Data;
using Resources.Scripts.GameManagers;

namespace Resources.Scripts.Menu
{
    public class PerkSelectionController : MonoBehaviour
    {
        [Header("Кнопки-опции (по порядку)")]
        [SerializeField] private Button[] optionButtons = null!;
        [SerializeField] private TextMeshProUGUI[] descriptionTexts = null!;

        private PerkDefinition[] options;

        /// <summary>
        /// Вызывается сразу после Instantiate панели.
        /// </summary>
        public void Setup(PerkDefinition[] perks)
        {
            options = perks;

            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (i >= perks.Length)
                {
                    optionButtons[i].gameObject.SetActive(false);
                    continue;
                }

                descriptionTexts[i].text = perks[i].GetDescription();
                int idx = i; // замыкание
                optionButtons[i].onClick.AddListener(() =>
                {
                    StageProgressionManager.Instance.OnPerkChosen(options[idx]);
                    Destroy(gameObject);
                });
            }
        }
    }
}