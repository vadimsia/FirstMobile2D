using UnityEngine;
using TMPro;
using System.Collections;

namespace Resources.Scripts.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class LoadingPanelController : MonoBehaviour
    {
        [Header("Fade Settings")]
        [Tooltip("Длительность появления/исчезновения")]
        [SerializeField] private float fadeDuration = 0.5f;

        [Header("Text Animation")]
        [Tooltip("Ссылка на TextMeshProUGUI с текстом \"Загрузка\"")]
        [SerializeField] private TMP_Text loadingText;
        [Tooltip("Интервал между изменениями точек (сек)")]
        [SerializeField] private float dotInterval = 0.5f;

        private CanvasGroup canvasGroup;
        private Coroutine dotsCoroutine;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        /// <summary>Показывает панель: fade-in + анимация точек.</summary>
        public void Show()
        {
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(FadeIn());
            dotsCoroutine = StartCoroutine(AnimateDots());
        }

        /// <summary>Скрывает панель: fade-out и останавливает анимацию точек.</summary>
        public void Hide()
        {
            StopCoroutine(dotsCoroutine);
            StartCoroutine(FadeOut());
        }

        private IEnumerator FadeIn()
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOut()
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private IEnumerator AnimateDots()
        {
            const string baseText = "Загрузка";
            int dots = 0;
            while (true)
            {
                loadingText.text = baseText + new string('.', dots);
                dots = (dots + 1) % 4;
                yield return new WaitForSecondsRealtime(dotInterval);
            }
        }
    }
}
