using System;
using UnityEngine;
using UnityEngine.Events;

namespace Utilities
{
    public class ScriptableConfiguration : MonoBehaviour, IConfiguration
    {
        [SerializeField] private string prefsKey;
        [SerializeField] private ConfigurableValue defaultValue;

        [Header("Events")]
        [SerializeField] private UnityEvent<object> onValueChanged = new UnityEvent<object>();
        [SerializeField] private UnityEvent onSaved = new UnityEvent();
        [SerializeField] private UnityEvent onResetToDefault = new UnityEvent();

        private ConfigurableValue currentValue;
        private ConfigurableValue savedValue;

        private bool isInitialized = false;

        public string PrefsKey => prefsKey;
        public ConfigurableValue DefaultValue => defaultValue;
        public bool IsModified => !AreValuesEqual(currentValue, savedValue);

        public UnityEvent<object> OnValueChanged => onValueChanged;
        public UnityEvent OnSaved => onSaved;
        public UnityEvent OnResetToDefault => onResetToDefault;

        private void Awake()
        {
            SettingsManager.Instance.RegisterConfiguration(this);
        }

        private void OnDestroy() => SettingsManager.Instance.UnRegisterConfiguration(this);

        /// <summary>
        /// Loads the value from PlayerPrefs or sets it to default if not present.
        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;

            if (string.IsNullOrEmpty(PrefsKey))
            {
                Debug.LogError($"[ScriptableConfig] Missing PrefsKey in {gameObject.name}");
                return;
            }

            currentValue = new ConfigurableValue { valueType = defaultValue.valueType };
            savedValue = new ConfigurableValue { valueType = defaultValue.valueType };

            object loadedValue = defaultValue.GetValue();

            if (NewPrefs.HasKey(PrefsKey))
            {
                if (IsStoredTypeValid(PrefsKey, defaultValue.valueType, out _))
                {
                    loadedValue = LoadValueFromPrefs(PrefsKey, defaultValue.valueType);
                }
            }
            else
            {
                NewPrefs.SetValue(PrefsKey, loadedValue, defaultValue.valueType);
            }

            SetConfigurableValueFromObject(currentValue, loadedValue, defaultValue.valueType);
            SetConfigurableValueFromObject(savedValue, loadedValue, defaultValue.valueType);

            Debug.Log($"[ScriptableConfig] Initialized {PrefsKey} with value: {loadedValue}");

            isInitialized = true;
        }

        /// <summary>
        /// Assings a new value (not saved yet).
        /// </summary>
        public void SetValue(object newValue, bool invokeEvents = true)
        {
            if (newValue == null) return;

            SetConfigurableValueFromObject(currentValue, newValue, defaultValue.valueType);

            if (invokeEvents)
                onValueChanged?.Invoke(newValue);
        }

        /// <summary>
        /// Saves the current value in PlayerPrefs.
        /// </summary>
        public void SaveConfiguration()
        {
            if (string.IsNullOrEmpty(PrefsKey)) return;

            var value = currentValue.GetValue();
            NewPrefs.SetValue(PrefsKey, value, defaultValue.valueType);
            SetConfigurableValueFromObject(savedValue, value, defaultValue.valueType);
            InvokeEvent();
            onSaved?.Invoke();
            Debug.Log($"[ScriptableConfig] Saved {PrefsKey}: {value}");
        }

        /// <summary>
        /// Reverts the current value to the last saved value.
        /// </summary>
        public void RevertToSaved()
        {
            SetValue(savedValue.GetValue(), invokeEvents: true);
            Debug.Log($"[ScriptableConfig] Reverted {PrefsKey} to saved value");
        }

        /// <summary>
        /// Resets the current value to the default value.
        /// </summary>
        public void SetToDefault()
        {
            var value = defaultValue.GetValue();
            SetValue(value, invokeEvents: true);

            NewPrefs.SetValue(PrefsKey, value, defaultValue.valueType);
            SetConfigurableValueFromObject(savedValue, value, defaultValue.valueType);

            onResetToDefault?.Invoke();
            Debug.Log($"[ScriptableConfig] Reset {PrefsKey} to default ({value})");
        }

        /// <summary>
        /// Invokes the OnValueChanged event with the current value.
        /// </summary>
        public void InvokeEvent()
        {
            onValueChanged?.Invoke(currentValue.GetValue());
        }

        public object GetCurrentValue() => currentValue?.GetValue();
        public object GetSavedValue() => savedValue?.GetValue();

        public void SetDefaultValue(object newValue)
        {
            if (newValue == null) return;

            switch (defaultValue.valueType)
            {
                case ConfigurableValue.ValueType.Int: defaultValue.intValue = (int)newValue; break;
                case ConfigurableValue.ValueType.Float: defaultValue.floatValue = (float)newValue; break;
                case ConfigurableValue.ValueType.String: defaultValue.stringValue = (string)newValue; break;
                case ConfigurableValue.ValueType.Bool: defaultValue.boolValue = (bool)newValue; break;
                case ConfigurableValue.ValueType.Color: defaultValue.colorValue = (Color)newValue; break;
            }
        }

        #region Helpers
        private static object LoadValueFromPrefs(string key, ConfigurableValue.ValueType type)
        {
            return type switch
            {
                ConfigurableValue.ValueType.Int => NewPrefs.GetValue<int>(key),
                ConfigurableValue.ValueType.Float => NewPrefs.GetValue<float>(key),
                ConfigurableValue.ValueType.String => NewPrefs.GetValue<string>(key),
                ConfigurableValue.ValueType.Bool => NewPrefs.GetValue<bool>(key),
                ConfigurableValue.ValueType.Color => NewPrefs.GetValue<Color>(key),
                _ => null
            };
        }

        private bool IsStoredTypeValid(string key, ConfigurableValue.ValueType expectedValueType, out string storedTypeName)
        {
            storedTypeName = PlayerPrefs.GetString($"{key}_type", null);
            var expectedType = GetSystemType(expectedValueType);
            return !string.IsNullOrEmpty(storedTypeName) && storedTypeName == expectedType.FullName;
        }

        private static void SetConfigurableValueFromObject(ConfigurableValue target, object value, ConfigurableValue.ValueType type)
        {
            if (value == null) return;
            try
            {
                target.valueType = type;
                switch (type)
                {
                    case ConfigurableValue.ValueType.Int: target.intValue = Convert.ToInt32(value); break;
                    case ConfigurableValue.ValueType.Float: target.floatValue = Convert.ToSingle(value); break;
                    case ConfigurableValue.ValueType.String: target.stringValue = Convert.ToString(value); break;
                    case ConfigurableValue.ValueType.Bool: target.boolValue = Convert.ToBoolean(value); break;
                    case ConfigurableValue.ValueType.Color: target.colorValue = (Color)value; break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ScriptableConfig] Error converting value for '{value}': {ex.Message}");
            }
        }

        private static bool AreValuesEqual(ConfigurableValue a, ConfigurableValue b)
        {
            if (a == null || b == null || a.valueType != b.valueType)
                return false;

            return a.valueType switch
            {
                ConfigurableValue.ValueType.Float => Mathf.Approximately(a.floatValue, b.floatValue),
                ConfigurableValue.ValueType.Color => a.colorValue.Equals(b.colorValue),
                _ => Equals(a.GetValue(), b.GetValue())
            };
        }

        private static Type GetSystemType(ConfigurableValue.ValueType type) => type switch
        {
            ConfigurableValue.ValueType.Int => typeof(int),
            ConfigurableValue.ValueType.Float => typeof(float),
            ConfigurableValue.ValueType.String => typeof(string),
            ConfigurableValue.ValueType.Bool => typeof(bool),
            ConfigurableValue.ValueType.Color => typeof(Color),
            _ => null
        };

        #endregion
    }
}
