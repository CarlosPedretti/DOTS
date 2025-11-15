using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities.UI
{
    [CreateAssetMenu(fileName = "ColorAnimation", menuName = "UI Animator/Animations/Color")]
    public class ColorAnimationSO : UIAnimationSO
    {
        [Header("Color Settings")]
        public Color from = Color.white;
        public Color to = Color.red;

        [Header("Return Settings")]
        [Tooltip("If true, the element will return to its initial color after the animation.")]
        public bool returnToInitial = true;

        [Tooltip("Duration of the return to initial color.")]
        public float returnDuration = 0.2f;

        [Tooltip("Ease used for returning to initial color.")]
        public Ease returnEase = Ease.OutQuad;

        public override Tween Play(UIAnimator target)
        {
            var graphic = target.GetComponent<Graphic>();
            if (graphic == null)
            {
                Debug.LogWarning("No Graphic component found on target.");
                return null;
            }

            target.GetInitialColor(out Color initialColor);

            graphic.color = from;

            Tween mainTween = graphic.DOColor(to, duration).SetEase(ease);

            if (returnToInitial)
            {
                Sequence seq = DOTween.Sequence();
                seq.Append(mainTween);
                seq.Append(graphic.DOColor(initialColor, returnDuration).SetEase(returnEase));
                return seq;
            }

            return mainTween;
        }
    }
}
