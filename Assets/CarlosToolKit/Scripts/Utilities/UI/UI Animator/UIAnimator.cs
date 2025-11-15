using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Utilities.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIAnimator : MonoBehaviour
    {
        [SerializeField] bool resetToInitialStateOnDisable = false;

        [Tooltip("Initial color of the element. Used as a reference for animations that return to the original color.")]
        [SerializeField] Color initialColor = Color.white;

        [Tooltip("Invoked when this GameObject is enabled.")]
        public UnityEvent OnEnableEvent;

        [Tooltip("Invoked when this GameObject is disabled.")]
        public UnityEvent OnDisableEvent;

        [Tooltip("Invoked during Awake(), before Start().")]
        public UnityEvent OnAwakeEvent;

        [Tooltip("Invoked during Start(), after Awake().")]
        public UnityEvent OnStartEvent;


        private RectTransform rectTransform;
        private Vector3 initialScale;
        private Vector2 initialPosition;

        private CanvasGroup canvasGroup;
        private float initialAplha;

        private Tween activeTween;

        public Tween ActiveTween { get { return activeTween; } }

        private void OnEnable()
        {
            OnEnableEvent?.Invoke();
        }

        private void OnDisable()
        {
            OnDisableEvent?.Invoke();

            KillActiveTween(resetToInitialStateOnDisable);
        }

        private void Awake()
        {
            initialScale = transform.localScale;

            canvasGroup = GetComponent<CanvasGroup>();
            initialAplha = canvasGroup.alpha;

            rectTransform = GetComponent<RectTransform>();
            initialPosition = rectTransform.anchoredPosition;
        }

        /// <summary>
        /// Plays the given animation routine on this UI element.
        /// <para>
        /// This method executes all animations contained in the provided <see cref="UIAnimationRoutine"/> 
        /// in either sequential or parallel mode, depending on the routine's configuration.
        /// </para>
        /// </summary>
        /// <param name="routine">
        /// The <see cref="UIAnimationRoutine"/> ScriptableObject to execute. 
        /// If null, the method logs a warning and does nothing.
        /// </param>
        public void PlayRoutine(UIAnimationRoutine routine)
        {
            if (routine == null)
            {
                Debug.LogWarning($"UIAnimator: Routine is null.");
                return;
            }

            if (routine.animations.Length == 0)
            {
                Debug.LogWarning($"UIAnimator: The current routine '{routine.name}' has no animations to play.");
                return;
            }

            if (activeTween != null && activeTween.IsActive())
                KillActiveTween(resetToInitial: false);

            activeTween = ExecuteRoutine(routine);
        }

        /// <summary>
        /// Plays the given animation routine on this UI element in a loop.
        /// </summary>
        /// <param name="routine">
        /// The <see cref="UIAnimationRoutine"/> ScriptableObject to execute. 
        /// If null, the method logs a warning and does nothing.
        /// </param>
        public void PlayRoutineInLoop(UIAnimationRoutine routine)
        {
            // Use -1 for infinite.
            int loops = -1;
            LoopType loopType = LoopType.Restart;

            if (routine == null)
            {
                Debug.LogWarning("UIAnimator: Routine is null.");
                return;
            }

            if (routine.animations.Length == 0)
            {
                Debug.LogWarning($"UIAnimator: The current routine '{routine.name}' has no animations to play.");
                return;
            }

            if (activeTween != null && activeTween.IsActive())
                KillActiveTween(resetToInitial: false);

            Sequence seq = DOTween.Sequence();

            if (routine.executionMode == UIAnimationExecutionMode.Sequential)
            {
                foreach (var anim in routine.animations)
                    if (anim != null)
                        seq.Append(anim.Play(this));
            }
            else
            {
                foreach (var anim in routine.animations)
                    if (anim != null)
                        seq.Join(anim.Play(this));
            }

            seq.SetLoops(loops, loopType);

            activeTween = seq;
            seq.Play();
        }


        private Tween ExecuteRoutine(UIAnimationRoutine routine)
        {
            Sequence seq = DOTween.Sequence();

            if (routine.executionMode == UIAnimationExecutionMode.Sequential)
            {
                foreach (var anim in routine.animations)
                    if (anim != null)
                        seq.Append(anim.Play(this));
            }
            else
            {
                foreach (var anim in routine.animations)
                    if (anim != null)
                        seq.Join(anim.Play(this));
            }

            seq.Play();
            return seq;
        }

        /// <summary>
        /// Kills the active tween if there is one.
        /// Optionally resets the transform to its initial state.
        /// </summary>
        public void KillActiveTween(bool resetToInitial = true)
        {
            if (activeTween != null && activeTween.IsActive())
            {
                activeTween.Kill();
                activeTween = null;
            }

            if (resetToInitial)
            {
                ResetToInitialState();
            }
        }

        /// <summary>
        /// Resets position, scale, and color to their initial values.
        /// </summary>
        public void ResetToInitialState()
        {
            transform.localScale = initialScale;
            transform.localPosition = initialPosition;

            canvasGroup.alpha = initialAplha;

            //if (TryGetComponent(out UnityEngine.UI.Graphic graphic))
            //{
            //    graphic.color = initialColor;
            //}
        }

        public void GetScaleData(out Vector3 scale)
        {
            scale = initialScale;
        }

        public void GetInitialPosition(out Vector2 initialPosition)
        {
            initialPosition = this.initialPosition;
        }

        public void GetInitialColor(out Color initialColor)
        {
            initialColor = this.initialColor;
        }
    }
}
