using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

namespace Resources.Scripts.UI
{
    public class UIButtonAnimator : MonoBehaviour
    {
        [Header("Настройки анимации")]
        [SerializeField] private float punchScale = 0.2f;
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private int vibrato = 10;
        [SerializeField] private float elasticity = 1f;

        [Header("Применять к кнопкам на сцене")]
        [SerializeField] private bool applyOnStart = true;

        private readonly HashSet<Button> animatedButtons = new();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject); // Переживает смену сцен
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            if (applyOnStart)
                AnimateAllSceneButtons();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// Вызывается при загрузке новой сцены.
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            AnimateAllSceneButtons();
        }

        /// <summary>
        /// Находит все кнопки в сцене и вешает на них анимацию.
        /// </summary>
        public void AnimateAllSceneButtons()
        {
            var buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var btn in buttons)
            {
                if (animatedButtons.Contains(btn)) continue;
                AddAnimation(btn);
                animatedButtons.Add(btn);
            }
        }

        /// <summary>
        /// Добавляет анимацию к переданной кнопке.
        /// </summary>
        private void AddAnimation(Button button)
        {
            // Удаляем старую анимацию, если была
            button.onClick.RemoveListener(() => Animate(button.transform));
            // Добавляем новую
            button.onClick.AddListener(() => Animate(button.transform));
        }

        /// <summary>
        /// Выполняет анимацию нажатия.
        /// </summary>
        private void Animate(Transform target)
        {
            target.DOKill(); // Убираем текущие анимации
            target.DOPunchScale(Vector3.one * punchScale, duration, vibrato, elasticity);
        }
    }
}
