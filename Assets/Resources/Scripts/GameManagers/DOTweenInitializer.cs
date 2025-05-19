using UnityEngine;
using DG.Tweening;

namespace Resources.Scripts.GameManagers
{
    [DefaultExecutionOrder(-1000)]
    public class DOTweenInitializer : MonoBehaviour
    {
        [Header("DOTween Capacity Settings")]
        [Tooltip("Макс. количество одновременно активных твинов")]
        public int maxTweeners = 1000;

        [Tooltip("Макс. количество одновременно активных секвенций")]
        public int maxSequences = 100;

        private void Awake()
        {
            DOTween.SetTweensCapacity(maxTweeners, maxSequences);
            Debug.Log($"<color=#00bcd4><b>DOTween:</b></color> Capacity set to {maxTweeners}/{maxSequences}");
        }
    }
}