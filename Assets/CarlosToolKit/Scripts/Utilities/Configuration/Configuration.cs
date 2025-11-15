using System;
using UnityEngine;
using UnityEngine.Events;

namespace Utilities
{
    public class Configuration : MonoBehaviour, IConfiguration
    {
        [Tooltip("The UI component that will be configured (e.g., Slider, Toggle, InputField, etc.).")]
        [SerializeField] private Component UIElement;

        [Tooltip("The unique key used to store and retrieve this value from PlayerPrefs.")]
        [SerializeField] private string prefsKey;

        [Tooltip("The default value to apply. If you don't have a saved value, the default value will be aplied.")]
        [SerializeField] private ConfigurableValue defaultValue;

        [Tooltip("Event triggered when the configuration is reset to its default value.")]
        [SerializeField] private UnityEvent onResetToDefault;

        [Tooltip("Event triggered after the configuration value has been saved.")]
        [SerializeField] private UnityEvent onSaved;

        public UnityEvent OnResetToDefault => onResetToDefault;
        public UnityEvent OnSaved => onSaved;

        public string PrefsKey { get { return prefsKey; } }
        public ConfigurableValue DefaultValue { get { return defaultValue; } }

        public Component GetUIElement() => UIElement;

        private bool isInitialized = false;

        [HideInInspector]
        [SerializeField] private ConfigurableValue savedValue;


        private void Awake()
        {
            SettingsManager.Instance.RegisterConfiguration(this);
        }

        private void Start()
        {
            //Waiting SettingsManager
            Initialize();
        }

        private void OnEnable()
        {
            //Must be called before Initialize();
            if (IsModified)
            {
                if (isInitialized) RevertToSaved();

            }
        }

        private void OnDestroy()
        {
            SettingsManager.Instance.UnRegisterConfiguration(this);
        }

        /// <summary>
        /// Returns true if the current UI value differs from the last saved value.
        /// </summary>
        public bool IsModified
        {
            get
            {
                if (UIElement == null || string.IsNullOrEmpty(PrefsKey)) return false;
                if (!UIElementAdapterRegistry.TryGetAdapter(UIElement.GetType(), out var adapter)) return false;

                object currentValue = adapter.GetValue(UIElement);
                return !Equals(currentValue, savedValue.GetValue());
            }
        }

        /// <summary>
        /// Initializes the Configuration component by applying the stored value from PlayerPrefs,
        /// or falling back to the default value. The value is applied to the UI component using its registered adapter.
        /// </summary>
        public void Initialize()
        {
            if(isInitialized) return;

            if (UIElement == null || string.IsNullOrEmpty(PrefsKey))
            {
                Debug.LogError($"Configuration '{UIElement?.gameObject.name}' is missing UIElement or PrefsKey.");
                return;
            }

            if (!UIElementAdapterRegistry.TryGetAdapter(UIElement.GetType(), out var adapter))
            {
                Debug.LogWarning($"No adapter registered for type {UIElement.GetType()}.");
                return;
            }

            object valueToSet = defaultValue.GetValue();

            if (NewPrefs.HasKey(PrefsKey))
            {
                if (IsStoredTypeValid(PrefsKey, defaultValue.valueType, out var storedTypeName))
                {
                    valueToSet = defaultValue.valueType switch
                    {
                        ConfigurableValue.ValueType.Int => NewPrefs.GetValue<int>(PrefsKey),
                        ConfigurableValue.ValueType.Float => NewPrefs.GetValue<float>(PrefsKey),
                        ConfigurableValue.ValueType.String => NewPrefs.GetValue<string>(PrefsKey),
                        ConfigurableValue.ValueType.Bool => NewPrefs.GetValue<bool>(PrefsKey),
                        ConfigurableValue.ValueType.Color => NewPrefs.GetValue<Color>(PrefsKey),
                        _ => valueToSet
                    };
                }

            }
            else
            {
                //We use the default value instead
                NewPrefs.SetValue(PrefsKey, valueToSet, defaultValue.valueType);
            }

            //Debug.Log($"[Configuration] Applying value to '{UIElement.gameObject.name}' ({UIElement.GetType().Name}): {valueToSet}");

            SetConfigurableValueFromObject(savedValue, valueToSet, defaultValue.valueType);
            adapter.SetValue(UIElement, savedValue.GetValue());

            isInitialized = true;
        }

        /// <summary>
        /// Saves the current value from the UI component to PlayerPrefs using the assigned key.
        /// This ensures the value is persisted across sessions.
        /// </summary>
        public void SaveConfiguration()
        {
            if (UIElement == null || string.IsNullOrEmpty(PrefsKey)) return;

            if (UIElementAdapterRegistry.TryGetAdapter(UIElement.GetType(), out var adapter))
            {
                object currentValue = adapter.GetValue(UIElement);
                if (currentValue != null)
                {
                    SetConfigurableValueFromObject(savedValue, currentValue, defaultValue.valueType);
                    NewPrefs.SetValue(PrefsKey, currentValue, defaultValue.valueType);
                    InvokeEvent();
                    onSaved?.Invoke();

                    Debug.Log($"[CONFIG] Saving {PrefsKey} with value: {currentValue}");
                }

            }
            else
            {
                Debug.LogWarning($"[CONFIG] No adapter for {UIElement.GetType().Name} in '{gameObject.name}'");
            }

        }

        /// <summary>
        /// Reverts the UI component to the last saved value stored in memory.
        /// </summary>
        public void RevertToSaved()
        {
            if (UIElement == null || savedValue == null || string.IsNullOrEmpty(PrefsKey)) return;

            if (UIElementAdapterRegistry.TryGetAdapter(UIElement.GetType(), out var adapter))
            {
                adapter.SetValue(UIElement, savedValue.GetValue());
                InvokeEvent();
            }
            else
            {
                Debug.LogWarning($"[CONFIG] No adapter for {UIElement.GetType().Name} in '{gameObject.name}'");
            }

            Debug.Log("RevertToSaved() Called");
        }

        /// <summary>
        /// Forces the UI component to invoke its value change event manually,
        /// even if the GameObject is inactive or the value hasn't changed.
        /// Useful for triggering callbacks or listeners programmatically.
        /// </summary>
        public void InvokeEvent()
        {
            if (UIElement == null || string.IsNullOrEmpty(PrefsKey)) return;

            if (UIElementAdapterRegistry.TryGetAdapter(UIElement.GetType(), out var adapter))
            {
                adapter.Invoke(UIElement);
            }
            else
            {
                Debug.LogWarning($"[CONFIG] No adapter for {UIElement.GetType().Name} in '{gameObject.name}'");
            }
        }

        /// <summary>
        /// Resets the UI component to its default value and stores that value in PlayerPrefs.
        /// </summary>
        public void SetToDefault()
        {
            if (UIElement == null || string.IsNullOrEmpty(PrefsKey)) return;

            if (UIElementAdapterRegistry.TryGetAdapter(UIElement.GetType(), out var adapter))
            {
                object value = defaultValue != null ? defaultValue.GetValue() : default;

                if (value != null)
                {
                    SetConfigurableValueFromObject(savedValue, value, defaultValue.valueType);
                    NewPrefs.SetValue(PrefsKey, value, DefaultValue.valueType);
                    adapter.SetValue(UIElement, value);
                    InvokeEvent();
                    onResetToDefault?.Invoke();
                }
            }
            else
            {
                Debug.LogWarning($"[CONFIG] No adapter for {UIElement.GetType().Name} in '{gameObject.name}'");
            }
        }

        /// <summary>
        /// Registers a callback to be invoked when the UI component's value changes.
        /// This version accepts an Action with no parameters.
        /// </summary>
        /// <param name="callback">The callback to invoke when the value changes.</param>
        public void RegisterCallback(Action callback)
        {
            if (UIElement == null || string.IsNullOrEmpty(PrefsKey)) return;

            if (UIElementAdapterRegistry.TryGetAdapter(UIElement.GetType(), out var adapter))
            {
                adapter.RegisterCallback(UIElement, callback);
            }
        }

        /// <summary>
        /// Registers a callback to be invoked when the UI component's value changes.
        /// This version accepts an Action that receives the new value as a parameter.
        /// </summary>
        /// <param name="callback">The callback to invoke with the new value.</param>
        public void RegisterCallback(Action<object> callback)
        {
            if (UIElement == null || string.IsNullOrEmpty(PrefsKey)) return;

            if (UIElementAdapterRegistry.TryGetAdapter(UIElement.GetType(), out var adapter))
            {
                adapter.RegisterCallback(UIElement, callback);
            }
        }

        /// <summary>
        /// Unregisters a previously registered callback that had no parameters.
        /// </summary>
        /// <param name="callback">The callback to remove.</param>
        public void UnregisterCallback(Action callback)
        {
            if (UIElement == null || string.IsNullOrEmpty(PrefsKey)) return;

            if (UIElementAdapterRegistry.TryGetAdapter(UIElement.GetType(), out var adapter))
            {
                adapter.UnregisterCallback(UIElement, callback);
            }
        }

        /// <summary>
        /// Unregisters a previously registered callback that received the new value as a parameter.
        /// </summary>
        /// <param name="callback">The callback to remove.</param>
        public void UnregisterCallback(Action<object> callback)
        {
            if (UIElement == null || string.IsNullOrEmpty(PrefsKey)) return;

            if (UIElementAdapterRegistry.TryGetAdapter(UIElement.GetType(), out var adapter))
            {
                adapter.UnregisterCallback(UIElement, callback);
            }
        }

        /// <summary>
        /// Gets the current value of the UI component via its adapter.
        /// </summary>
        /// <returns>The current value of the UI component, or null if invalid or no adapter is found.</returns>
        public object GetCurrentValue()
        {
            if (UIElement == null || string.IsNullOrEmpty(PrefsKey)) return null;

            if (UIElementAdapterRegistry.TryGetAdapter(UIElement.GetType(), out var adapter))
            {
                return adapter.GetValue(UIElement);
            }

            return null;
        }

        /// <summary>
        /// Sets a new default value for this configuration. This value will be used when resetting to default,
        /// but it does not immediately apply the value to the UI component.
        /// </summary>
        /// <param name="newValue">The new default value to assign.</param>
        public void SetDefaultValue(object newValue)
        {
            if (newValue == null) return;

            switch (defaultValue.valueType)
            {
                case ConfigurableValue.ValueType.Int:
                    defaultValue.intValue = (int)newValue;
                    break;
                case ConfigurableValue.ValueType.Float:
                    defaultValue.floatValue = (float)newValue;
                    break;
                case ConfigurableValue.ValueType.String:
                    defaultValue.stringValue = (string)newValue;
                    break;
                case ConfigurableValue.ValueType.Bool:
                    defaultValue.boolValue = (bool)newValue;
                    break;
                case ConfigurableValue.ValueType.Color:
                    defaultValue.colorValue = (Color)newValue;
                    break;
            }
        }


        #region Helpers
        private bool IsStoredTypeValid(string key, ConfigurableValue.ValueType expectedValueType, out string storedTypeName)
        {
            storedTypeName = PlayerPrefs.GetString($"{key}_type", null);
            System.Type expectedType = GetSystemType(expectedValueType);

            if (string.IsNullOrEmpty(storedTypeName))
            {
                Debug.LogWarning($"[Configuration] No type metadata found for key '{key}'. Assuming fallback.");
                return false;
            }

            if (expectedType == null)
            {
                Debug.LogError($"[Configuration] Could not resolve expected type for valueType '{expectedValueType}'.");
                return false;
            }

            if (storedTypeName != expectedType.FullName)
            {
                Debug.LogError(
                    $"[Configuration] Type mismatch for key '{key}'. Expected: {expectedType.FullName}, Found: {storedTypeName}.\n" +
                    $"This might be due to a misconfigured ConfigurableValue or a corrupted PlayerPref.");
                return false;
            }

            return true;
        }

        private void SetConfigurableValueFromObject(ConfigurableValue target, object value, ConfigurableValue.ValueType type)
        {
            target.valueType = type;

            try
            {
                switch (type)
                {
                    case ConfigurableValue.ValueType.Int:
                        target.intValue = Convert.ToInt32(value);
                        break;
                    case ConfigurableValue.ValueType.Float:
                        target.floatValue = Convert.ToSingle(value);
                        break;
                    case ConfigurableValue.ValueType.String:
                        target.stringValue = Convert.ToString(value);
                        break;
                    case ConfigurableValue.ValueType.Bool:
                        target.boolValue = Convert.ToBoolean(value);
                        break;
                    case ConfigurableValue.ValueType.Color:
                        target.colorValue = (Color)value;
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Config] Error converting value for '{PrefsKey}': {ex.Message} (Value: {value}, Type: {value?.GetType()})");
            }
        }

        private static System.Type GetSystemType(ConfigurableValue.ValueType type)
        {
            return type switch
            {
                ConfigurableValue.ValueType.Int => typeof(int),
                ConfigurableValue.ValueType.Float => typeof(float),
                ConfigurableValue.ValueType.String => typeof(string),
                ConfigurableValue.ValueType.Bool => typeof(bool),
                ConfigurableValue.ValueType.Color => typeof(Color),
                _ => null
            };
        }

        #endregion

    }

    [System.Serializable]
    public class ConfigurableValue
    {
        public enum ValueType
        {
            Int,
            Float,
            String,
            Bool,
            Color
        }

        public ValueType valueType;

        public int intValue;

        public float floatValue;

        public string stringValue;

        public bool boolValue;

        public Color colorValue;

        public object GetValue()
        {
            switch (valueType)
            {
                case ValueType.Int:
                    return intValue;
                case ValueType.Float:
                    return floatValue;
                case ValueType.String:
                    return stringValue;
                case ValueType.Bool:
                    return boolValue;
                case ValueType.Color:
                    return colorValue;
                default:
                    return null;
            }
        }
    }

}

