using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEditor;

namespace Utilities
{
    public class Timer : MonoBehaviour
    {
        public enum TimerMode
        {
            CountUp,
            CountDown
        }
        public enum TimeFormat
        {
            MinutesSeconds,
            HoursMinutes,
            FullTime,
            SecondsOnly
        }


        public TimerMode mode = TimerMode.CountDown;
        public float duration = 10f;
        [SerializeField] private float currentTime;
        public bool isRunning = false;
        private bool isPaused = false;

        public UnityEvent OnTimerStarted;
        public UnityEvent<float> OnTimeChanged;
        public UnityEvent OnTimerEnded;
        public UnityEvent OnTimerPaused;

        private Coroutine timerCoroutine;

        [Tooltip("Show text options for the timer display")]
        public bool showTextOptions;

        public TimeFormat selectedFormat = TimeFormat.MinutesSeconds;

        public TMPro.TextMeshProUGUI timerText;

        private void Awake()
        {
            InitializeTimer();
        }

        private IEnumerator TimerRoutine()
        {
            if (duration <= 0f)
            {
                Debug.LogWarning("Timer duration is invalid or too short");
                isRunning = false;
                yield break;
            }

            InitializeTimer();
            isRunning = true;

            while ((mode == TimerMode.CountDown && currentTime > 0) || (mode == TimerMode.CountUp && currentTime < duration))
            {
                while (isPaused)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(1f);
                currentTime += (mode == TimerMode.CountDown) ? -1f : 1f;
                OnTimeChanged?.Invoke(currentTime);

                if (showTextOptions && timerText != null)
                {
                    timerText.text = FormatTime(currentTime);
                }
            }

            isRunning = false;
            OnTimerEnded?.Invoke();
        }

        private string FormatTime(float time)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(time);
            switch (selectedFormat)
            {
                case TimeFormat.MinutesSeconds:
                    return timeSpan.ToString(@"mm\:ss");
                case TimeFormat.HoursMinutes:
                    return timeSpan.ToString(@"hh\:mm");
                case TimeFormat.FullTime:
                    return timeSpan.ToString(@"hh\:mm\:ss");
                case TimeFormat.SecondsOnly:
                    return timeSpan.Seconds.ToString();
                default:
                    return timeSpan.ToString();
            }
        }

        void InitializeTimer()
        {
            currentTime = (mode == TimerMode.CountDown) ? duration : 0f;
        }

        #region Public Methods


        public void StartTimer()
        {
            if (timerCoroutine != null) StopCoroutine(timerCoroutine);
            timerCoroutine = StartCoroutine(TimerRoutine());

            OnTimerStarted?.Invoke();
        }


        public void StopTimer()
        {
            if (timerCoroutine != null) StopCoroutine(timerCoroutine);
            isRunning = false;

            InitializeTimer();
        }


        public void ResetTimer()
        {
            currentTime = (mode == TimerMode.CountDown) ? duration : 0f;

            StartTimer();
        }


        public void PauseTimer()
        {
            isPaused = true;
            isRunning = false;
            OnTimerPaused?.Invoke();
        }


        public void ResumeTimer()
        {
            if (isPaused)
            {
                isPaused = false;
                isRunning = true;
            }
        }

        #endregion
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Timer))]
    public class TimerEditor : Editor
    {
        private SerializedProperty mode;
        private SerializedProperty duration;
        private SerializedProperty currentTime;
        private SerializedProperty isRunning;
        private SerializedProperty OnTimerStarted;
        private SerializedProperty OnTimeChanged;
        private SerializedProperty OnTimerEnded;
        private SerializedProperty OnTimerPaused;

        private SerializedProperty showTextOptions;
        private SerializedProperty selectedFormat;
        private SerializedProperty timerText;

        private void OnEnable()
        {
            mode = serializedObject.FindProperty("mode");
            duration = serializedObject.FindProperty("duration");
            currentTime = serializedObject.FindProperty("currentTime");
            isRunning = serializedObject.FindProperty("isRunning");

            OnTimerStarted = serializedObject.FindProperty("OnTimerStarted");
            OnTimeChanged = serializedObject.FindProperty("OnTimeChanged");
            OnTimerEnded = serializedObject.FindProperty("OnTimerEnded");
            OnTimerPaused = serializedObject.FindProperty("OnTimerPaused");

            showTextOptions = serializedObject.FindProperty("showTextOptions");
            selectedFormat = serializedObject.FindProperty("selectedFormat");
            timerText = serializedObject.FindProperty("timerText");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(mode);
            EditorGUILayout.PropertyField(duration);

            GUI.enabled = false;
            EditorGUILayout.PropertyField(currentTime, new GUIContent("Current Time"));
            EditorGUILayout.PropertyField(isRunning);
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(OnTimerStarted);
            EditorGUILayout.PropertyField(OnTimeChanged);
            EditorGUILayout.PropertyField(OnTimerEnded);
            EditorGUILayout.PropertyField(OnTimerPaused);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Text Options", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(showTextOptions);

            if (showTextOptions.boolValue)
            {
                EditorGUILayout.PropertyField(selectedFormat);
                EditorGUILayout.PropertyField(timerText);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}


