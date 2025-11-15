using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.UI;
using Utilities.UI;

namespace Utilities
{
    public class CallbackGroup<T>
    {
        public List<Action> actions = new();
        public List<Action<object>> objectActions = new();
        public UnityAction<T> listener;

        public static void Register(Dictionary<Component, CallbackGroup<T>> callbacks, Component component, Action callback, UnityEvent<T> unityEvent)
        {
            if (!callbacks.TryGetValue(component, out var group))
            {
                group = new CallbackGroup<T>();
                group.listener = (val) =>
                {
                    foreach (var cb in group.actions)
                        cb();

                    foreach (var objCb in group.objectActions)
                        objCb(val);
                };
                unityEvent.AddListener(group.listener);
                callbacks[component] = group;
            }

            if (!group.actions.Contains(callback))
                group.actions.Add(callback);
        }

        public static void Register(Dictionary<Component, CallbackGroup<T>> callbacks, Component component, Action<object> callback, UnityEvent<T> unityEvent)
        {
            if (!callbacks.TryGetValue(component, out var group))
            {
                group = new CallbackGroup<T>();
                group.listener = (val) =>
                {
                    foreach (var cb in group.actions)
                        cb();

                    foreach (var objCb in group.objectActions)
                        objCb(val);
                };
                unityEvent.AddListener(group.listener);
                callbacks[component] = group;
            }

            if (!group.objectActions.Contains(callback))
                group.objectActions.Add(callback);
        }

        public static void Unregister(Dictionary<Component, CallbackGroup<T>> callbacks, Component component, Action callback, UnityEvent<T> unityEvent)
        {
            if (!callbacks.TryGetValue(component, out var group)) return;

            group.actions.Remove(callback);

            if (group.actions.Count == 0 && group.objectActions.Count == 0)
            {
                unityEvent.RemoveListener(group.listener);
                callbacks.Remove(component);
            }
        }

        public static void Unregister(Dictionary<Component, CallbackGroup<T>> callbacks, Component component, Action<object> callback, UnityEvent<T> unityEvent)
        {
            if (!callbacks.TryGetValue(component, out var group)) return;

            group.objectActions.Remove(callback);

            if (group.actions.Count == 0 && group.objectActions.Count == 0)
            {
                unityEvent.RemoveListener(group.listener);
                callbacks.Remove(component);
            }
        }
    }
    public class SliderAdapter : IUIElementAdapter
    {
        private readonly Dictionary<Component, CallbackGroup<float>> callbacks = new();

        public void SetValue(Component component, object value)
        {
            ((Slider)component).value = Convert.ToSingle(value);
        }

        public object GetValue(Component component)
        {
            return ((Slider)component).value;
        }

        public void RegisterCallback(Component component, Action callback)
        {
            var slider = (Slider)component;
            CallbackGroup<float>.Register(callbacks, component, callback, slider.onValueChanged);
        }

        public void RegisterCallback(Component component, Action<object> callback)
        {
            var slider = (Slider)component;
            CallbackGroup<float>.Register(callbacks, component, callback, slider.onValueChanged);
        }

        public void UnregisterCallback(Component component, Action callback)
        {
            var slider = (Slider)component;
            CallbackGroup<float>.Unregister(callbacks, component, callback, slider.onValueChanged);
        }

        public void UnregisterCallback(Component component, Action<object> callback)
        {
            var slider = (Slider)component;
            CallbackGroup<float>.Unregister(callbacks, component, callback, slider.onValueChanged);
        }

        public void Invoke(Component component)
        {
            var slider = (Slider)component;
            slider.onValueChanged.Invoke(slider.value);
        }

        public List<ConfigurableValue.ValueType> SupportedValueTypes()
        {
            return new List<ConfigurableValue.ValueType> { ConfigurableValue.ValueType.Int, ConfigurableValue.ValueType.Float };
        }
    }
    public class UISliderAdapter : IUIElementAdapter
    {
        private readonly Dictionary<Component, CallbackGroup<float>> callbacks = new();

        public void SetValue(Component component, object value)
        {
            ((UISlider)component).value = Convert.ToSingle(value);
        }

        public object GetValue(Component component)
        {
            return ((UISlider)component).value;
        }

        public void RegisterCallback(Component component, Action callback)
        {
            var slider = (UISlider)component;
            CallbackGroup<float>.Register(callbacks, component, callback, slider.onValueChanged);
        }

        public void RegisterCallback(Component component, Action<object> callback)
        {
            var slider = (UISlider)component;
            CallbackGroup<float>.Register(callbacks, component, callback, slider.onValueChanged);
        }

        public void UnregisterCallback(Component component, Action callback)
        {
            var slider = (UISlider)component;
            CallbackGroup<float>.Unregister(callbacks, component, callback, slider.onValueChanged);
        }

        public void UnregisterCallback(Component component, Action<object> callback)
        {
            var slider = (UISlider)component;
            CallbackGroup<float>.Unregister(callbacks, component, callback, slider.onValueChanged);
        }

        public void Invoke(Component component)
        {
            var slider = (UISlider)component;
            slider.onValueChanged.Invoke(slider.value);
        }

        public List<ConfigurableValue.ValueType> SupportedValueTypes()
        {
            return new List<ConfigurableValue.ValueType> { ConfigurableValue.ValueType.Int, ConfigurableValue.ValueType.Float };
        }
    }
    public class ToggleAdapter : IUIElementAdapter
    {
        private readonly Dictionary<Component, CallbackGroup<bool>> callbacks = new();

        public void SetValue(Component component, object value)
        {
            ((Toggle)component).isOn = Convert.ToBoolean(value);
        }

        public object GetValue(Component component)
        {
            return ((Toggle)component).isOn;
        }

        public void RegisterCallback(Component component, Action callback)
        {
            var toggle = (Toggle)component;
            CallbackGroup<bool>.Register(callbacks, component, callback, toggle.onValueChanged);
        }

        public void RegisterCallback(Component component, Action<object> callback)
        {
            var toggle = (Toggle)component;
            CallbackGroup<bool>.Register(callbacks, component, callback, toggle.onValueChanged);
        }

        public void UnregisterCallback(Component component, Action callback)
        {
            var toggle = (Toggle)component;
            CallbackGroup<bool>.Unregister(callbacks, component, callback, toggle.onValueChanged);
        }

        public void UnregisterCallback(Component component, Action<object> callback)
        {
            var toggle = (Toggle)component;
            CallbackGroup<bool>.Unregister(callbacks, component, callback, toggle.onValueChanged);
        }

        public void Invoke(Component component)
        {
            var toggle = (Toggle)component;
            toggle.onValueChanged.Invoke(toggle.isOn);
        }

        public List<ConfigurableValue.ValueType> SupportedValueTypes()
        {
            return new List<ConfigurableValue.ValueType> { ConfigurableValue.ValueType.Bool };
        }
    }
    public class DropdownAdapter : IUIElementAdapter
    {
        private readonly Dictionary<Component, CallbackGroup<int>> callbacks = new();

        public void SetValue(Component component, object value)
        {
            ((Dropdown)component).value = Convert.ToInt32(value);
        }

        public object GetValue(Component component)
        {
            return ((Dropdown)component).value;
        }

        public void RegisterCallback(Component component, Action callback)
        {
            var dropdown = (Dropdown)component;
            CallbackGroup<int>.Register(callbacks, component, callback, dropdown.onValueChanged);
        }

        public void RegisterCallback(Component component, Action<object> callback)
        {
            var dropdown = (Dropdown)component;
            CallbackGroup<int>.Register(callbacks, component, callback, dropdown.onValueChanged);
        }

        public void UnregisterCallback(Component component, Action callback)
        {
            var dropdown = (Dropdown)component;
            CallbackGroup<int>.Unregister(callbacks, component, callback, dropdown.onValueChanged);
        }

        public void UnregisterCallback(Component component, Action<object> callback)
        {
            var dropdown = (Dropdown)component;
            CallbackGroup<int>.Unregister(callbacks, component, callback, dropdown.onValueChanged);
        }

        public void Invoke(Component component)
        {
            var dropdown = (Dropdown)component;
            dropdown.onValueChanged.Invoke(dropdown.value);
        }

        public List<ConfigurableValue.ValueType> SupportedValueTypes()
        {
            return new List<ConfigurableValue.ValueType> { ConfigurableValue.ValueType.Int };
        }
    }
    public class TMPDropdownAdapter : IUIElementAdapter
    {
        private readonly Dictionary<Component, CallbackGroup<int>> callbacks = new();
        public void SetValue(Component component, object value)
        {
            ((TMP_Dropdown)component).value = Convert.ToInt32(value);
        }

        public object GetValue(Component component)
        {
            return ((TMP_Dropdown)component).value;
        }

        public void RegisterCallback(Component component, Action callback)
        {
            var TMP_dropdown = (TMP_Dropdown)component;
            CallbackGroup<int>.Register(callbacks, component, callback, TMP_dropdown.onValueChanged);
        }

        public void RegisterCallback(Component component, Action<object> callback)
        {
            var TMP_dropdown = (TMP_Dropdown)component;
            CallbackGroup<int>.Register(callbacks, component, callback, TMP_dropdown.onValueChanged);
        }

        public void UnregisterCallback(Component component, Action callback)
        {
            var TMP_dropdown = (TMP_Dropdown)component;
            CallbackGroup<int>.Unregister(callbacks, component, callback, TMP_dropdown.onValueChanged);
        }

        public void UnregisterCallback(Component component, Action<object> callback)
        {
            var TMP_dropdown = (TMP_Dropdown)component;
            CallbackGroup<int>.Unregister(callbacks, component, callback, TMP_dropdown.onValueChanged);
        }

        public void Invoke(Component component)
        {
            var tmp_dropdown = (TMP_Dropdown)component;
            tmp_dropdown.onValueChanged.Invoke(tmp_dropdown.value);
        }

        public List<ConfigurableValue.ValueType> SupportedValueTypes()
        {
            return new List<ConfigurableValue.ValueType> { ConfigurableValue.ValueType.Int };
        }
    }
    public class UIStringOptionSelectorAdapter : IUIElementAdapter
    {
        private readonly Dictionary<Component, CallbackGroup<int>> callbacks = new();
        public void SetValue(Component component, object value)
        {
            int newValue = Convert.ToInt32(value);
            ((UIStringOptionSelector)component).SetOption(newValue);
        }

        public object GetValue(Component component)
        {
            int index = ((UIStringOptionSelector)component).GetCurrentIndex();
            return index;
        }

        public void RegisterCallback(Component component, Action callback)
        {
            var optionSelector = (UIStringOptionSelector)component;
            CallbackGroup<int>.Register(callbacks, component, callback, optionSelector.OnOptionChangedIndex);
        }

        public void RegisterCallback(Component component, Action<object> callback)
        {
            var optionSelector = (UIStringOptionSelector)component;
            CallbackGroup<int>.Register(callbacks, component, callback, optionSelector.OnOptionChangedIndex);
        }

        public void UnregisterCallback(Component component, Action callback)
        {
            var optionSelector = (UIStringOptionSelector)component;
            CallbackGroup<int>.Register(callbacks, component, callback, optionSelector.OnOptionChangedIndex);
        }

        public void UnregisterCallback(Component component, Action<object> callback)
        {
            var optionSelector = (UIStringOptionSelector)component;
            CallbackGroup<int>.Register(callbacks, component, callback, optionSelector.OnOptionChangedIndex);
        }

        public void Invoke(Component component)
        {
            var optionSelector = (UIStringOptionSelector)component;
            optionSelector.OnOptionChangedIndex.Invoke(optionSelector.GetCurrentIndex());
        }

        public List<ConfigurableValue.ValueType> SupportedValueTypes()
        {
            return new List<ConfigurableValue.ValueType> { ConfigurableValue.ValueType.Int };
        }
    }
    public class UILocalizedOptionSelectorAdapter : IUIElementAdapter
    {
        private readonly Dictionary<Component, CallbackGroup<int>> callbacks = new();
        public void SetValue(Component component, object value)
        {
            int newValue = Convert.ToInt32(value);
            ((UILocalizedOptionSelector)component).SetOption(newValue);
        }

        public object GetValue(Component component)
        {
            int index = ((UILocalizedOptionSelector)component).GetCurrentIndex();
            return index;
        }

        public void RegisterCallback(Component component, Action callback)
        {
            var optionSelector = (UILocalizedOptionSelector)component;
            CallbackGroup<int>.Register(callbacks, component, callback, optionSelector.OnOptionChangedIndex);
        }

        public void RegisterCallback(Component component, Action<object> callback)
        {
            var optionSelector = (UILocalizedOptionSelector)component;
            CallbackGroup<int>.Register(callbacks, component, callback, optionSelector.OnOptionChangedIndex);
        }

        public void UnregisterCallback(Component component, Action callback)
        {
            var optionSelector = (UILocalizedOptionSelector)component;
            CallbackGroup<int>.Register(callbacks, component, callback, optionSelector.OnOptionChangedIndex);
        }

        public void UnregisterCallback(Component component, Action<object> callback)
        {
            var optionSelector = (UILocalizedOptionSelector)component;
            CallbackGroup<int>.Register(callbacks, component, callback, optionSelector.OnOptionChangedIndex);
        }

        public void Invoke(Component component)
        {
            var optionSelector = (UILocalizedOptionSelector)component;
            optionSelector.OnOptionChangedIndex.Invoke(optionSelector.GetCurrentIndex());
        }

        public List<ConfigurableValue.ValueType> SupportedValueTypes()
        {
            return new List<ConfigurableValue.ValueType> { ConfigurableValue.ValueType.Int };
        }
    }

}
