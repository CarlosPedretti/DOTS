using DG.Tweening;
using UnityEngine;

namespace Utilities.UI
{
    [CreateAssetMenu(fileName = "ScaleAnimation", menuName = "UI Animator/Animations/Scale")]
    public class ScaleAnimationSO : UIAnimationSO
    {
        [Header("Mode")]
        [Tooltip("Choose whether to use Absolute mode (fixed from -> to) or Relative mode (current scale × Target Scale).")]
        public ScaleMode mode = ScaleMode.Absolute;

        [Header("Absolute Mode (From -> To)")]
        [Tooltip("Starting scale when using Absolute Mode.")]
        public Vector3 from = Vector3.one;

        [Tooltip("Target scale when using Absolute Mode.")]
        public Vector3 to = Vector3.one * 1.2f;

        [Header("Relative Mode (Based on the current object scale)")]
        [Tooltip("Increment applied to the current scale when using Relative Mode. " +
                 "Example: (0.2, 0.2, 0) = increase X and Y by 20%, keep Z unchanged. " +
                 "Negative values will shrink the scale.")]
        public Vector3 targetScale = new Vector3(0.2f, 0.2f, 0);

        public override Tween Play(UIAnimator uiAnimator)
        {
            var tr = uiAnimator.transform;

            switch (mode)
            {
                case ScaleMode.Relative:
                    {
                        uiAnimator.GetScaleData(out Vector3 initialScale);

                        Vector3 end = initialScale + Vector3.Scale(initialScale, targetScale);
                        return tr.DOScale(end, duration).SetEase(ease);
                    }


                case ScaleMode.Absolute:
                default:
                    {
                        tr.localScale = from;
                        return tr.DOScale(to, duration).SetEase(ease);
                    }
            }
        }

        public enum ScaleMode
        {
            Absolute,   // Uses fixed values (from -> to)
            Relative    // Uses current scale × targetScale
        }
    }
}

