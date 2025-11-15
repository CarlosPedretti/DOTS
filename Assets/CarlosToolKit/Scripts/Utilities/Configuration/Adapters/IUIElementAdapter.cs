using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public interface IUIElementAdapter
{
    void SetValue(Component component, object value);
    object GetValue(Component component);
    void RegisterCallback(Component component, Action callback);
    void UnregisterCallback(Component component, Action callback);
    void RegisterCallback(Component component, Action<object> callback);
    void UnregisterCallback(Component component, Action<object> callback);
    void Invoke(Component component);

    List<ConfigurableValue.ValueType> SupportedValueTypes();
}


