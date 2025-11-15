using DG.Tweening;
using UnityEngine;

namespace Utilities.UI
{
    [CreateAssetMenu(fileName = "FadeAnimation", menuName = "UI Animator/Animations/Fade")]
    public class FadeAnimationSO : UIAnimationSO
    {
        public float from = 0f;
        public float to = 1f;

        public override Tween Play(UIAnimator target)
        {
            var cg = target.GetComponent<CanvasGroup>();
            if (cg == null) cg = target.gameObject.AddComponent<CanvasGroup>();

            cg.alpha = from;
            return cg.DOFade(to, duration).SetEase(ease);
        }
    }
}

