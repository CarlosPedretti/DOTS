using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif


namespace Utilities.UI
{
    public class UISlider : Slider
    {
        [SerializeField] private bool showNumbers;
        [SerializeField] private TextMeshProUGUI numbersText;
        [SerializeField] private Color numbersTextColor;
        [SerializeField] private string valueFormat = "F1";
        [SerializeField] private bool usePercentage = false;
        [SerializeField] private bool useScaledValue = false;
        [SerializeField] private bool useTimeFormat = false;

        public UnityEvent OnSliderBeingDragged;
        public UnityEvent OnSliderBeingDraggedEnd;

        public bool isBeingDragged;


        protected override void Awake()
        {
            base.Awake();

            onValueChanged.AddListener(UpdateNumbersText);

            UpdateNumbersText(value);
        }

        private void UpdateNumbersText(float value)
        {
            if (showNumbers && numbersText != null)
            {
                float displayValue = value;

                if (useScaledValue)
                {
                    displayValue = Mathf.Lerp(0, 100, Mathf.InverseLerp(0.0001f, 1f, value));
                }

                if (useTimeFormat)
                {
                    int totalSeconds = Mathf.FloorToInt(displayValue);
                    int minutes = totalSeconds / 60;
                    int seconds = totalSeconds % 60;
                    numbersText.text = $"{minutes:D2}:{seconds:D2}";
                    return;
                }


                if (!usePercentage)
                {
                    numbersText.text = displayValue.ToString(valueFormat);
                    numbersText.color = numbersTextColor;
                }
                else
                {
                    numbersText.text = $"{displayValue.ToString(valueFormat)} %";
                    numbersText.color = numbersTextColor;
                }
            }
        }


        public override void OnPointerDown(PointerEventData eventData)
        {
            isBeingDragged = true;
            OnSliderBeingDragged?.Invoke();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            isBeingDragged = false;
            OnSliderBeingDraggedEnd?.Invoke();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(UISlider))]
    public class UISliderEditor : SliderEditor
    {
        SerializedProperty showNumbersProp;
        SerializedProperty numbersTextProp;
        SerializedProperty numbersTextColorProp;
        SerializedProperty valueFormatProp;
        SerializedProperty usePercentageProp;
        SerializedProperty useScaledValueProp;
        SerializedProperty useTimeFormatProp;

        SerializedProperty onSliderBeingDragged;
        SerializedProperty onSliderBeingDraggedEnd;


        protected override void OnEnable()
        {
            base.OnEnable();

            showNumbersProp = serializedObject.FindProperty("showNumbers");
            numbersTextProp = serializedObject.FindProperty("numbersText");
            numbersTextColorProp = serializedObject.FindProperty("numbersTextColor");
            valueFormatProp = serializedObject.FindProperty("valueFormat");
            usePercentageProp = serializedObject.FindProperty("usePercentage");
            useScaledValueProp = serializedObject.FindProperty("useScaledValue");
            useTimeFormatProp = serializedObject.FindProperty("useTimeFormat");

            onSliderBeingDragged = serializedObject.FindProperty("OnSliderBeingDragged");
            onSliderBeingDraggedEnd = serializedObject.FindProperty("OnSliderBeingDraggedEnd");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(showNumbersProp);

            if (showNumbersProp.boolValue)
            {
                EditorGUILayout.PropertyField(numbersTextProp);
                EditorGUILayout.PropertyField(numbersTextColorProp);
                EditorGUILayout.PropertyField(valueFormatProp);
                EditorGUILayout.PropertyField(usePercentageProp);
                EditorGUILayout.PropertyField(useScaledValueProp);
                EditorGUILayout.PropertyField(useTimeFormatProp);
                EditorGUILayout.PropertyField(onSliderBeingDragged);
                EditorGUILayout.PropertyField(onSliderBeingDraggedEnd);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}

