using DG.Tweening;
using UnityEngine;

namespace Resources.Scripts.GameManagers
{
    public static class DOTweenSafeExtensions
    {
        public static Tweener DoAnchorPosSafe(this RectTransform rect, Vector2 target, float duration)
        {
            if (rect == null || !rect.gameObject.activeInHierarchy) 
                return null;

            DOTween.Kill(rect);

            // Сначала получаем твин
            var tw = rect.DOAnchorPos(target, duration);

            // Конфигурируем безопасность
            tw.SetAutoKill(true);
            tw.OnUpdate(() =>
            {
                // Если объект исчез — убиваем твин
                if (rect == null || rect.gameObject == null)
                    tw.Kill();
            });

            return tw;
        }

        public static Tweener DoScaleSafe(this Transform tr, Vector3 scale, float duration)
        {
            if (tr == null || !tr.gameObject.activeInHierarchy) 
                return null;

            DOTween.Kill(tr);

            var tw = tr.DOScale(scale, duration);
            tw.SetAutoKill(true);
            tw.OnUpdate(() =>
            {
                if (tr == null || tr.gameObject == null)
                    tw.Kill();
            });

            return tw;
        }

        public static Tweener DoFadeSafe(this CanvasGroup cg, float alpha, float duration)
        {
            if (cg == null || !cg.gameObject.activeInHierarchy) 
                return null;

            DOTween.Kill(cg);

            var tw = cg.DOFade(alpha, duration);
            tw.SetAutoKill(true);
            tw.OnUpdate(() =>
            {
                if (cg == null || cg.gameObject == null)
                    tw.Kill();
            });

            return tw;
        }

        public static Tweener DoMoveSafe(this Transform tr, Vector3 position, float duration)
        {
            if (tr == null || !tr.gameObject.activeInHierarchy) 
                return null;

            DOTween.Kill(tr);

            var tw = tr.DOMove(position, duration);
            tw.SetAutoKill(true);
            tw.OnUpdate(() =>
            {
                if (tr == null || tr.gameObject == null)
                    tw.Kill();
            });

            return tw;
        }
    }
}
