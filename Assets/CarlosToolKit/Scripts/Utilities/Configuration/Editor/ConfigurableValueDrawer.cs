using UnityEngine;
using UnityEditor;

namespace Utilities
{
    [CustomPropertyDrawer(typeof(ConfigurableValue))]
    public class ConfigurableValueDrawer : PropertyDrawer
    {
        private bool foldout = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            foldout = EditorGUI.Foldout(
                new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                foldout, label, true);

            if (!foldout)
                return;

            EditorGUI.indentLevel++;

            SerializedProperty valueType = property.FindPropertyRelative("valueType");
            SerializedProperty intValue = property.FindPropertyRelative("intValue");
            SerializedProperty floatValue = property.FindPropertyRelative("floatValue");
            SerializedProperty stringValue = property.FindPropertyRelative("stringValue");
            SerializedProperty boolValue = property.FindPropertyRelative("boolValue");
            SerializedProperty colorValue = property.FindPropertyRelative("colorValue");

            float lineHeight = EditorGUIUtility.singleLineHeight + 2;
            float yOffset = position.y + lineHeight;

            // Draw ValueType dropdown
            Rect enumRect = new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(enumRect, valueType);
            yOffset += lineHeight;

            // Draw value field based on selected type
            Rect valueRect = new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight);
            switch ((ConfigurableValue.ValueType)valueType.enumValueIndex)
            {
                case ConfigurableValue.ValueType.Int:
                    EditorGUI.PropertyField(valueRect, intValue);
                    break;
                case ConfigurableValue.ValueType.Float:
                    EditorGUI.PropertyField(valueRect, floatValue);
                    break;
                case ConfigurableValue.ValueType.String:
                    EditorGUI.PropertyField(valueRect, stringValue);
                    break;
                case ConfigurableValue.ValueType.Bool:
                    EditorGUI.PropertyField(valueRect, boolValue);
                    break;
                case ConfigurableValue.ValueType.Color:
                    EditorGUI.PropertyField(valueRect, colorValue);
                    break;
            }

            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight + 2;
            if (!foldout)
                return lineHeight;

            return lineHeight * 3;
        }
    }
}
