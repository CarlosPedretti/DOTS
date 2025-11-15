using DG.Tweening;
using UnityEngine;

namespace Utilities.UI
{
    [CreateAssetMenu(fileName = "MoveAnimation", menuName = "UI Animator/Animations/Move")]
    public class MoveAnimationSO : UIAnimationSO
    {
        [Header("Mode")]
        [Tooltip("Absolute = From -> To. Relative = Offset from Initial Position.")]
        public MoveMode moveMode = MoveMode.Absolute;

        [Header("Absolute Mode")]
        [Tooltip("Start anchored position (only used in Absolute mode).")]
        public Vector2 from;

        [Tooltip("End anchored position (only used in Absolute mode).")]
        public Vector2 to;

        [Header("RelativeOffset Mode")]
        [Tooltip("Offset applied to the initial position.\n" +
                 "(0,0) = initial position.\n" +
                 "(50,0) = initial +50px on X.\n" +
                 "(0,-100) = initial -100px on Y.")]
        public Vector2 relativeOffset = Vector2.zero;

        [Header("Return Settings")]
        [Tooltip("If true, the object will return to its initial position.")]
        public bool returnToInitial = false;

        [Tooltip("Duration to return to initial position.")]
        public float returnDuration = 0.2f;

        [Tooltip("Ease used for returning to initial scale.")]
        public Ease returnEase = Ease.OutQuad;

        public override Tween Play(UIAnimator target)
        {
            var rect = target.GetComponent<RectTransform>();
            Vector2 startPos = Vector2.zero;
            Vector2 endPos = Vector2.zero;

            switch (moveMode)
            {
                case MoveMode.Absolute:
                    rect.anchoredPosition = from;
                    startPos = from;
                    endPos = to;
                    break;

                case MoveMode.RelativeOffset:
                default:
                    startPos = rect.anchoredPosition;
                    endPos = startPos + relativeOffset;
                    break;
            }

            var seq = DOTween.Sequence();
            seq.Append(rect.DOAnchorPos(endPos, duration).SetEase(ease));

            if (returnToInitial)
                seq.Append(rect.DOAnchorPos(startPos, returnDuration).SetEase(returnEase));

            return seq;
        }


        public enum MoveMode
        {
            Absolute,
            RelativeOffset
        }
    }
}
