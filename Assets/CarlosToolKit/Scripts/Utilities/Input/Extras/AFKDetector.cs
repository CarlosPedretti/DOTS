using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Utilities.Input
{
    public class AFKDetector : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool useAFKDetector = false;
        [SerializeField] private float afkTimeThreshold = 10f;
        [SerializeField] private float intervalCheck = 1f;

        [Header("Debug")]
        [SerializeField] private bool isAFK = false;
        [SerializeField] private bool wasWarned = false;
        [SerializeField] private float lastActivityTime;
        [SerializeField] private float elapsedTime;
        private float warningTime;

        [Header("Events")]
        public UnityEvent<int> OnWarning;  // int = remaining seconds
        public UnityEvent OnAFK;
        public UnityEvent OnBackFromAFK;

        private void Start()
        {
            lastActivityTime = Time.time;
            warningTime = afkTimeThreshold / 2;

            if (useAFKDetector)
            {
                StartCoroutine(CheckAFK());
            }
        }

        private void OnEnable()
        {
            LocalInputManager.Instance.MainInput.OnActionTriggered += OnInputActivity;
        }

        private void OnDisable()
        {
            LocalInputManager.Instance.MainInput.OnActionTriggered -= OnInputActivity;
        }

        private void OnInputActivity(InputAction.CallbackContext context)
        {
            lastActivityTime = Time.time;

            if (wasWarned && !isAFK)
            {
                wasWarned = false;
                OnBackFromAFK?.Invoke();
            }

            if (isAFK)
            {
                isAFK = false;
                wasWarned = false;
                OnBackFromAFK?.Invoke();
            }
        }

        private IEnumerator CheckAFK()
        {
            while (useAFKDetector)
            {
                yield return new WaitForSeconds(intervalCheck);
                HandleAFKState();
            }
        }

        private void HandleAFKState()
        {
            elapsedTime = Time.time - lastActivityTime;

            if (elapsedTime >= warningTime && elapsedTime < afkTimeThreshold)
            {
                if (!wasWarned)
                {
                    wasWarned = true;
                }

                int remainingTime = Mathf.RoundToInt(afkTimeThreshold - elapsedTime);
                OnWarning?.Invoke(remainingTime);
            }

            if (elapsedTime >= afkTimeThreshold && !isAFK)
            {
                isAFK = true;
                OnAFK?.Invoke();
            }
        }
    }
}
