using UnityEditor;
using UnityEngine;
using Utilities;

[CustomEditor(typeof(Configuration))]
public class ConfigurationEditor : Editor
{
    private SerializedProperty uiElementProp;
    private SerializedProperty prefsKeyProp;
    private SerializedProperty defaultValueProp;
    private SerializedProperty onResetProp;
    private SerializedProperty onSavedProp;

    private void OnEnable()
    {
        uiElementProp = serializedObject.FindProperty("UIElement");
        prefsKeyProp = serializedObject.FindProperty("prefsKey");
        defaultValueProp = serializedObject.FindProperty("defaultValue");
        onResetProp = serializedObject.FindProperty("onResetToDefault");
        onSavedProp = serializedObject.FindProperty("onSaved");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(uiElementProp);
        EditorGUILayout.PropertyField(prefsKeyProp);

        Component uiElement = (target as Configuration)?.GetUIElement();

        if (uiElement != null && UIElementAdapterRegistry.TryGetAdapter(uiElement.GetType(), out var adapter))
        {
            var supportedTypes = adapter.SupportedValueTypes();

            SerializedProperty valueTypeProp = defaultValueProp.FindPropertyRelative("valueType");
            ConfigurableValue.ValueType currentType = (ConfigurableValue.ValueType)valueTypeProp.enumValueIndex;

            if (!supportedTypes.Contains(currentType))
                valueTypeProp.enumValueIndex = (int)supportedTypes[0];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Default Value", EditorStyles.boldLabel);

            ConfigurableValue.ValueType newType = ShowFilteredEnumPopup("Type", currentType, supportedTypes);
            if (newType != currentType)
                valueTypeProp.enumValueIndex = (int)newType;

            DrawValueField(defaultValueProp, newType);

            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("Assign a UIElement with a registered adapter to configure the default value.", MessageType.Info);
        }

        EditorGUILayout.PropertyField(onResetProp);
        EditorGUILayout.PropertyField(onSavedProp);

        serializedObject.ApplyModifiedProperties();
    }


    private ConfigurableValue.ValueType ShowFilteredEnumPopup(
        string label,
        ConfigurableValue.ValueType current,
        System.Collections.Generic.List<ConfigurableValue.ValueType> allowed)
    {
        string[] displayedOptions = allowed.ConvertAll(v => v.ToString()).ToArray();
        int currentIndex = allowed.IndexOf(current);
        if (currentIndex < 0) currentIndex = 0;

        int newIndex = EditorGUILayout.Popup(label, currentIndex, displayedOptions);
        return allowed[newIndex];
    }

    private void DrawValueField(SerializedProperty defaultValueProp, ConfigurableValue.ValueType valueType)
    {
        switch (valueType)
        {
            case ConfigurableValue.ValueType.Int:
                EditorGUILayout.PropertyField(defaultValueProp.FindPropertyRelative("intValue"), new GUIContent("Value"));
                break;
            case ConfigurableValue.ValueType.Float:
                EditorGUILayout.PropertyField(defaultValueProp.FindPropertyRelative("floatValue"), new GUIContent("Value"));
                break;
            case ConfigurableValue.ValueType.String:
                EditorGUILayout.PropertyField(defaultValueProp.FindPropertyRelative("stringValue"), new GUIContent("Value"));
                break;
            case ConfigurableValue.ValueType.Bool:
                EditorGUILayout.PropertyField(defaultValueProp.FindPropertyRelative("boolValue"), new GUIContent("Value"));
                break;
            case ConfigurableValue.ValueType.Color:
                EditorGUILayout.PropertyField(defaultValueProp.FindPropertyRelative("colorValue"), new GUIContent("Value"));
                break;
        }
    }
}
