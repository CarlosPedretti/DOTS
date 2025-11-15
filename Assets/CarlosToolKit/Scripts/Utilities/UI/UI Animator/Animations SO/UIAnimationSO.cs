using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Utilities.UI
{
    public enum StepExecutionType { Join, Append }

    public abstract class UIAnimationSO : ScriptableObject
    {
        [Min(0f)]
        public float duration = 0.5f;
        public Ease ease = Ease.OutQuad;
        public abstract Tween Play(UIAnimator target);
    }
}



