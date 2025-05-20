using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Resources.Scripts.Data;
using Resources.Scripts.GameManagers;

namespace Resources.Scripts.Menu
{
    public class PerkSelectionController : MonoBehaviour
    {
        [Header("Кнопки-опции (по порядку)")]
        [SerializeField] private Button[] optionButtons = null!;
        [SerializeField] private TextMeshProUGUI[] descriptionTexts = null!;

        [Header("Иконки для опций (соответствие кнопкам)")]
        [SerializeField] private Image[] iconImages = null!;

        private PerkDefinition[] options;

        /// <summary>
        /// Вызывается сразу после Instantiate панели.
        /// Добавляет анимацию появления кнопок и подставляет иконки.
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

                var btn = optionButtons[i];
                descriptionTexts[i].text = perks[i].GetDescription();
                
                // Устанавливаем иконку из ScriptableObject
                if (i < iconImages.Length && perks[i].Icon != null)
                {
                    iconImages[i].sprite = perks[i].Icon;
                    iconImages[i].SetNativeSize();
                }

                int idx = i; // замыкание

                // подготовка к анимации
                btn.transform.localScale = Vector3.zero;
                btn.onClick.AddListener(() =>
                {
                    StageProgressionManager.Instance.OnPerkChosen(options[idx]);
                    Destroy(gameObject);
                });

                // появление с задержкой
                btn.transform
                    .DOScale(1f, 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(0.1f * i);
            }
        }
    }
}
