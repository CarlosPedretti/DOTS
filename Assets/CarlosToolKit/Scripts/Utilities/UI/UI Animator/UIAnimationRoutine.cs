using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Utilities.UI
{
    [Serializable]
    [CreateAssetMenu(fileName = "AnimationRoutine", menuName = "UI Animator/Animation Routine")]
    public class UIAnimationRoutine : ScriptableObject
    {
        [Tooltip("Defines how the animations inside this group are executed:\n" +
                 "- Sequential: animations play one after another.\n" +
                 "- Parallel: all animations play at the same time.\n")]
        public UIAnimationExecutionMode executionMode = UIAnimationExecutionMode.Parallel;

        [Tooltip("The list of animations included in this group.")]
        public UIAnimationSO[] animations;

        /// <summary>
        /// Gets the total duration of the routine depending on the execution mode.
        /// </summary>
        public float TotalDuration
        {
            get
            {
                if (animations == null || animations.Length == 0)
                    return 0f;

                switch (executionMode)
                {
                    case UIAnimationExecutionMode.Sequential:
                        return animations.Sum(a => a != null ? a.duration : 0f);

                    case UIAnimationExecutionMode.Parallel:
                        return animations.Max(a => a != null ? a.duration : 0f);

                    default:
                        return 0f;
                }
            }
        }

    }

    public enum UIAnimationExecutionMode
    {
        Parallel,
        Sequential,
    }
}

