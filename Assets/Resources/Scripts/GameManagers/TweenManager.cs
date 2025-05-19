using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Resources.Scripts.GameManagers
{
    public class TweenManager : MonoBehaviour
    {
        public static TweenManager Instance { get; private set; }
        private readonly Dictionary<string, List<Tween>> tweenGroups = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void RegisterTween(Tween tween, string groupId)
        {
            if (tween == null) return;

            tween.SetId(groupId)
                .SetAutoKill(true)
                .OnKill(() =>
                {
                    if (tweenGroups.TryGetValue(groupId, out var list))
                        list.Remove(tween);
                });

            if (!tweenGroups.ContainsKey(groupId))
                tweenGroups[groupId] = new List<Tween>();

            tweenGroups[groupId].Add(tween);
        }

        public void KillGroup(string groupId)
        {
            if (!tweenGroups.ContainsKey(groupId)) return;
            foreach (var tw in tweenGroups[groupId].ToArray())
                if (tw.IsActive()) tw.Kill();
            tweenGroups[groupId].Clear();
        }

        public void KillAll()
        {
            foreach (var kv in tweenGroups)
            {
                foreach (var tw in kv.Value.ToArray())
                    if (tw.IsActive()) tw.Kill();
                kv.Value.Clear();
            }
            tweenGroups.Clear();
        }

        private void OnDestroy()
        {
            KillAll();
        }
    }
}