using System;
using UnityEngine;

namespace Utilities
{
    public static class NewPrefs
    {
        public static event Action<string> OnPrefChanged;
        public static event Action OnPrefsReset;

        private const string TypeSuffix = "_type";

        public static void SetValue<T>(string key, T value)
        {
            try
            {
                switch (value)
                {
                    case string str:
                        PlayerPrefs.SetString(key, str);
                        break;
                    case int i:
                        PlayerPrefs.SetInt(key, i);
                        break;
                    case float f:
                        PlayerPrefs.SetFloat(key, f);
                        break;
                    default:
                        string json = JsonUtility.ToJson(new SerializableValue<T>(value));
                        PlayerPrefs.SetString(key, json);
                        break;
                }

                PlayerPrefs.SetString($"{key}_type", typeof(T).FullName);

                PlayerPrefs.Save();
                OnPrefChanged?.Invoke(key);

                //Debug.Log($"Setting value to key '{key}', with a value of type '{typeof(T).FullName}'");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error setting key '{key}' in PlayerPrefs: {e.Message}");
            }
        }
        public static void SetValue(string key, object value, ConfigurableValue.ValueType valueType)
        {
            switch (valueType)
            {
                case ConfigurableValue.ValueType.Int:
                    if (value is int intValue)
                        SetValue<int>(key, intValue);
                    else
                        Debug.LogError($"Invalid value type for key '{key}': expected int, got {value?.GetType()}");
                    break;

                case ConfigurableValue.ValueType.Float:
                    if (value is float floatValue)
                        SetValue<float>(key, floatValue);
                    else
                        Debug.LogError($"Invalid value type for key '{key}': expected float, got {value?.GetType()}");
                    break;

                case ConfigurableValue.ValueType.String:
                    if (value is string strValue)
                        SetValue<string>(key, strValue);
                    else
                        Debug.LogError($"Invalid value type for key '{key}': expected string, got {value?.GetType()}");
                    break;

                case ConfigurableValue.ValueType.Bool:
                    if (value is bool boolValue)
                        SetValue<bool>(key, boolValue);
                    else
                        Debug.LogError($"Invalid value type for key '{key}': expected bool, got {value?.GetType()}");
                    break;

                case ConfigurableValue.ValueType.Color:
                    if (value is Color colorValue)
                        SetValue<Color>(key, colorValue);
                    else
                        Debug.LogError($"Invalid value type for key '{key}': expected Color, got {value?.GetType()}");
                    break;
            }
        }

        public static T GetValue<T>(string key)
        {
            try
            {
                if (!PlayerPrefs.HasKey(key))
                {
                    Debug.LogWarning($"Key '{key}' not found.");
                    return default;
                }

                string savedType = PlayerPrefs.GetString(key + TypeSuffix, null);
                if (!string.IsNullOrEmpty(savedType) && savedType != typeof(T).FullName)
                {
                    Debug.LogWarning($"Type mismatch for key '{key}': expected {typeof(T).FullName}, found {savedType}.");
                    return default;
                }

                if (typeof(T) == typeof(string))
                    return (T)(object)PlayerPrefs.GetString(key);

                if (typeof(T) == typeof(int))
                    return (T)(object)PlayerPrefs.GetInt(key);

                if (typeof(T) == typeof(float))
                    return (T)(object)PlayerPrefs.GetFloat(key);

                string json = PlayerPrefs.GetString(key);
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogWarning($"JSON string for key '{key}' is null or empty.");
                    return default;
                }

                var wrapper = JsonUtility.FromJson<SerializableValue<T>>(json);
                return wrapper.Value;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting key '{key}' in PlayerPrefs: {e.Message}");
                return default;
            }
        }

        public static bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public static void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.DeleteKey(key + TypeSuffix);
        }

        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
            OnPrefsReset?.Invoke();
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }
    }

    [Serializable]
    public class SerializableValue<T>
    {
        public T Value;

        public SerializableValue(T value)
        {
            Value = value;
        }
    }
}
