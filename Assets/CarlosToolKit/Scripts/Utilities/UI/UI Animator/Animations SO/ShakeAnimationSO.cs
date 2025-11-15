using DG.Tweening;
using UnityEngine;

namespace Utilities.UI
{
    [CreateAssetMenu(fileName = "ShakeAnimation", menuName = "UI Animator/Animations/Shake")]
    public class ShakeAnimationSO : UIAnimationSO
    {
        [Tooltip("Shake strength.")]
        public Vector3 strength = new Vector3(10f, 10f, 0f);

        [Tooltip("Number of shakes.")]
        public int vibrato = 10;

        [Tooltip("Randomness factor.")]
        [Range(0, 180)] public float randomness = 90f;

        [Header("Return Settings")]
        [Tooltip("If true, the element will return to its initial anchored position after shaking.")]
        public bool returnToInitial = true;

        [Tooltip("Duration of the return to initial position (if enabled).")]
        public float returnDuration = 0.2f;

        [Tooltip("Easing of the return movement.")]
        public Ease returnEase = Ease.OutQuad;

        public override Tween Play(UIAnimator target)
        {
            var rect = target.GetComponent<RectTransform>();
            target.GetInitialPosition(out Vector2 initialPosition);

            Sequence seq = DOTween.Sequence();

            seq.Append(rect.DOShakeAnchorPos(duration, strength, vibrato, randomness));

            if (returnToInitial)
            {
                seq.Append(rect.DOAnchorPos(initialPosition, returnDuration).SetEase(returnEase));
            }

            return seq;
        }
    }
}
