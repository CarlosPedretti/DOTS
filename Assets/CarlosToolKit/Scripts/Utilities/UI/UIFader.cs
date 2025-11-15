using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace Utilities.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIFader : UnityEngine.EventSystems.UIBehaviour
    {
        [SerializeField] bool startFadeOnEnable = true;
        [SerializeField] bool playOnce = false;

        [SerializeField] private float _startDelay;
        [SerializeField] private EFadeDirection _direction = EFadeDirection.FadeIn;
        public EFadeDirection Direction { get => _direction; set => _direction = value; }

        [SerializeField] private float _duration = 0.5f;
        public float Duration { get => _duration; set => _duration = value; }

        [SerializeField] private Ease _ease = Ease.OutQuad;
        public Ease Ease { get => _ease; set => _ease = value; }

        [SerializeField] private bool _resetOnDisable = true;

        public UnityEvent OnFadeStarted;
        public UnityEvent OnFadeFinished;

        private float _resetValue;
        private bool _isFinished;
        public bool IsFinished => _isFinished;

        private Tween _fadeTween;

        private CanvasGroup _canvasGroup;

        protected override void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!startFadeOnEnable) return;
            StartFade();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (_resetOnDisable)
            {
                _canvasGroup.alpha = _resetValue;
            }

            _fadeTween?.Kill();
        }

        public void StartFade()
        {
            if (playOnce && _isFinished) return;

            _fadeTween?.Kill();

            _isFinished = false;
            ConfigurateFade();

            OnFadeStarted?.Invoke();

            _fadeTween = _canvasGroup
                .DOFade(_direction == EFadeDirection.FadeIn ? 1f : 0f, _duration)
                .SetDelay(_startDelay)
                .SetEase(_ease)
                .OnComplete(() =>
                {
                    _isFinished = true;
                    OnFadeFinished?.Invoke();
                });
        }

        public void StartFade(EFadeDirection direction)
        {
            if (playOnce && _isFinished) return;

            _direction = direction;
            StartFade();
        }

        public void StopFade()
        {
            _fadeTween?.Kill();
        }

        public void SetDuration(float duration) => _duration = duration;
        public void SetStartDelay(float delay) => _startDelay = delay;

        private void ConfigurateFade()
        {
            _resetValue = _canvasGroup.alpha;
            _canvasGroup.alpha = _direction == EFadeDirection.FadeIn ? 0f : 1f;
        }


    }
    public enum EFadeDirection
    {
        FadeIn,
        FadeOut,
    }
}
