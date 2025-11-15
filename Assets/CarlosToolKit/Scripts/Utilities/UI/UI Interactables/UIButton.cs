using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

namespace Utilities.UI
{
    [RequireComponent(typeof(AudioSource))]
    public class UIButton : Button
    {
        [Header("Button pressed down")]
        [Tooltip("Event invoked when the button is pressed down.")]
        [SerializeField] private UnityEvent onPressEvent;

        [Header("Button released")]
        [Tooltip("Event invoked when the button is released.")]
        [SerializeField] private UnityEvent onReleaseEvent;

        [Header("Pointer enters the button")]
        [Tooltip("Event invoked when the pointer enters the button.")]
        [SerializeField] private UnityEvent onPointerEnterEvent;

        [Header("Pointer exits the button")]
        [Tooltip("Event invoked when the pointer exits the button.")]
        [SerializeField] private UnityEvent onPointerExitEvent;

        [Header("Button selected")]
        [Tooltip("Event invoked when the button is selected.")]
        [SerializeField] private UnityEvent onSelectedEvent;

        [Header("Button deselected")]
        [Tooltip("Event invoked when the button is deselected.")]
        [SerializeField] private UnityEvent onDeselectedEvent;

        [Space]

        [Header("State Functionality")]
        [Tooltip("Enable state toggle functionality for the button.")]
        [SerializeField] private bool useStateFunctionality;
        [Tooltip("Image to show when the button is in the initial state.")]
        [SerializeField] private Image initialStateImage;
        [Tooltip("Image to show when the button is in the final state.")]
        [SerializeField] private Image finalStateImage;
        [Tooltip("Event invoked when the button enters the first state.")]
        [SerializeField] private UnityEvent onFirstState;
        [Tooltip("Event invoked when the button enters the second state.")]
        [SerializeField] private UnityEvent onSecondState;

        private bool isFirstState = false;


        [Space]

        [Header("Hold Functionality")]
        [Tooltip("Enable button hold functionality.")]
        [SerializeField] private bool useHoldFunctionality;
        [Tooltip("Time in seconds the button must be held to trigger the hold finished event.")]
        [SerializeField] private float holdTimeThreshold = 1.0f;
        [Tooltip("Event invoked when hold starts.")]
        [SerializeField] private UnityEvent onHoldStarted;
        [Tooltip("Event invoked when hold finishes.")]
        [SerializeField] private UnityEvent onHoldFinished;
        [Tooltip("Event invoked if hold is canceled before threshold time.")]
        [SerializeField] private UnityEvent onHoldCanceled;

        public bool IsHolding { get; private set; }

        private float pointerDownTime;
        private Coroutine holdCoroutine;

        private TMP_Text buttonText;


        protected override void Awake()
        {
            base.Awake();

            Initialize();
        }

        #region Pointer Events

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            onPointerEnterEvent?.Invoke();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            onPointerExitEvent?.Invoke();
        }

        public override void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            ToggleState();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            CheckHold(isOnPointerDown: true);

            onPressEvent?.Invoke();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            CheckHold(isOnPointerDown: false);

            onReleaseEvent?.Invoke();
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            onSelectedEvent?.Invoke();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);

            onDeselectedEvent?.Invoke();
        }

        #endregion





        // State Management
        private void ToggleState()
        {
            if (!useStateFunctionality) return;

            isFirstState = !isFirstState;
            UpdateState();
        }

        private void UpdateState()
        {
            bool first = isFirstState;
            SetImageState(first ? initialStateImage : finalStateImage, true);
            SetImageState(first ? finalStateImage : initialStateImage, false);
            (first ? onFirstState : onSecondState)?.Invoke();
        }

        private void SetImageState(Image image, bool isActive)
        {
            if (image) image.gameObject.SetActive(isActive);
        }

        public void ResetToInitialState()
        {
            isFirstState = false;

            SetImageState(finalStateImage, true);
            SetImageState(initialStateImage, false);
        }

        //Hold
        private void CheckHold(bool isOnPointerDown)
        {
            if (!useHoldFunctionality) return;

            if (isOnPointerDown)
            {
                pointerDownTime = Time.time;
                IsHolding = true;

                if (holdCoroutine != null) StopCoroutine(holdCoroutine);

                holdCoroutine = StartCoroutine(CheckHoldCO());
                ;
            }
            else
            {
                bool holdCompleted = (Time.time - pointerDownTime) >= holdTimeThreshold;

                IsHolding = false;

                if (holdCoroutine != null)
                {
                    StopCoroutine(holdCoroutine);
                    holdCoroutine = null;
                }

                if (!holdCompleted)
                {
                    onHoldCanceled?.Invoke();
                }
            }
        }

        private IEnumerator CheckHoldCO()
        {
            onHoldStarted?.Invoke();

            while (IsHolding)
            {
                float elapsedTime = Time.time - pointerDownTime;

                if (elapsedTime >= holdTimeThreshold)
                {
                    onHoldFinished?.Invoke();
                    yield break;
                }

                yield return null;
            }
        }


        //Initialize
        private void Initialize()
        {
            if (useStateFunctionality) UpdateState();

            buttonText = GetComponentInChildren<TMP_Text>();
        }

    }


#if UNITY_EDITOR
    [CustomEditor(typeof(UIButton))]
    public class UIButtonEditor : ButtonEditor
    {
        private bool showEvents = true;

        //Press & Release
        SerializedProperty onPressEventProp;
        SerializedProperty onReleaseEventProp;

        //Pointer Enter & Exit
        SerializedProperty onPointerEnterEventProp;
        SerializedProperty onPointerExitEventProp;

        //Select & Deselect
        SerializedProperty onSelectedEventProp;
        SerializedProperty onDeselectedEventProp;

        //Image
        SerializedProperty useStateFunctionalityProp;
        SerializedProperty initialStateImageProp;
        SerializedProperty finalStateImageProp;
        SerializedProperty onFirstStateProp;
        SerializedProperty onSecondStateProp;

        // Hold
        SerializedProperty useHoldFunctionalityProp;
        SerializedProperty holdTimeThresholdProp;
        SerializedProperty onHoldStartedProp;
        SerializedProperty onHoldFinishedProp;
        SerializedProperty onHoldCanceledProp;

        protected override void OnEnable()
        {
            base.OnEnable();

            onPressEventProp = serializedObject.FindProperty("onPressEvent");
            onReleaseEventProp = serializedObject.FindProperty("onReleaseEvent");

            onPointerEnterEventProp = serializedObject.FindProperty("onPointerEnterEvent");
            onPointerExitEventProp = serializedObject.FindProperty("onPointerExitEvent");

            onSelectedEventProp = serializedObject.FindProperty("onSelectedEvent");
            onDeselectedEventProp = serializedObject.FindProperty("onDeselectedEvent");

            useStateFunctionalityProp = serializedObject.FindProperty("useStateFunctionality");
            initialStateImageProp = serializedObject.FindProperty("initialStateImage");
            finalStateImageProp = serializedObject.FindProperty("finalStateImage");
            onFirstStateProp = serializedObject.FindProperty("onFirstState");
            onSecondStateProp = serializedObject.FindProperty("onSecondState");

            useHoldFunctionalityProp = serializedObject.FindProperty("useHoldFunctionality");
            holdTimeThresholdProp = serializedObject.FindProperty("holdTimeThreshold");
            onHoldStartedProp = serializedObject.FindProperty("onHoldStarted");
            onHoldFinishedProp = serializedObject.FindProperty("onHoldFinished");
            onHoldCanceledProp = serializedObject.FindProperty("onHoldCanceled");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            showEvents = EditorGUILayout.Foldout(showEvents, "Button Events", true);
            if (showEvents)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(onPressEventProp, new GUIContent("On Press"));
                EditorGUILayout.PropertyField(onReleaseEventProp, new GUIContent("On Release"));
                EditorGUILayout.PropertyField(onPointerEnterEventProp, new GUIContent("On Pointer Enter"));
                EditorGUILayout.PropertyField(onPointerExitEventProp, new GUIContent("On Pointer Exit"));
                EditorGUILayout.PropertyField(onSelectedEventProp, new GUIContent("On Selected"));
                EditorGUILayout.PropertyField(onDeselectedEventProp, new GUIContent("On Deselected"));
                EditorGUI.indentLevel--;
            }


            EditorGUILayout.PropertyField(useStateFunctionalityProp);
            if (useStateFunctionalityProp.boolValue)
            {
                EditorGUILayout.PropertyField(initialStateImageProp);
                EditorGUILayout.PropertyField(finalStateImageProp);
                EditorGUILayout.PropertyField(onFirstStateProp);
                EditorGUILayout.PropertyField(onSecondStateProp);
            }
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(useHoldFunctionalityProp);
            if (useHoldFunctionalityProp.boolValue)
            {
                EditorGUILayout.PropertyField(holdTimeThresholdProp);
                EditorGUILayout.PropertyField(onHoldStartedProp);
                EditorGUILayout.PropertyField(onHoldFinishedProp);
                EditorGUILayout.PropertyField(onHoldCanceledProp);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}

