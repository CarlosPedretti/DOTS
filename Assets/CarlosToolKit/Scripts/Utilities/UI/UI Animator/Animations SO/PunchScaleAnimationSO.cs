using DG.Tweening;
using UnityEngine;

namespace Utilities.UI
{
    [CreateAssetMenu(fileName = "PunchScaleAnimation", menuName = "UI Animator/Animations/Punch Scale")]
    public class PunchScaleAnimationSO : UIAnimationSO
    {
        [Tooltip("Amount to punch scale (relative).")]
        public Vector3 punch = new Vector3(0.2f, 0.2f, 0f);

        [Tooltip("Number of vibrato oscillations.")]
        public int vibrato = 10;

        [Tooltip("Elasticity (0 = no stretch, 1 = full stretch).")]
        [Range(0, 1)] public float elasticity = 1f;

        [Header("Return Settings")]
        [Tooltip("If true, the object will return to its initial scale after punching.")]
        public bool returnToInitial = true;

        [Tooltip("Duration to return to initial scale.")]
        public float returnDuration = 0.2f;

        [Tooltip("Ease used for returning to initial scale.")]
        public Ease returnEase = Ease.OutQuad;

        public override Tween Play(UIAnimator target)
        {
            var tr = target.transform;

            target.GetScaleData(out Vector3 initialScale);

            Tween punchTween = tr.DOPunchScale(punch, duration, vibrato, elasticity);

            if (returnToInitial)
            {
                Sequence seq = DOTween.Sequence();
                seq.Append(punchTween);
                seq.Append(tr.DOScale(initialScale, returnDuration).SetEase(returnEase));
                return seq;
            }

            return punchTween;
        }
    }
}
