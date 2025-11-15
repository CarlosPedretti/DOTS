using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Utilities.UI;

namespace Utilities
{
    public static class UIElementAdapterRegistry
    {
        private static Dictionary<Type, IUIElementAdapter> adapters = new();

        public static void Register<T>(IUIElementAdapter adapter) where T : Component
        {
            adapters[typeof(T)] = adapter;
        }

        public static bool TryGetAdapter(Type type, out IUIElementAdapter adapter)
        {
            return adapters.TryGetValue(type, out adapter);
        }

        public static void EnsureRegistered()
        {
            if (adapters.Count > 0) return;

            Register<Slider>(new SliderAdapter());
            Register<UISlider>(new UISliderAdapter());
            Register<Toggle>(new ToggleAdapter());
            Register<Dropdown>(new DropdownAdapter());
            Register<TMP_Dropdown>(new TMPDropdownAdapter());
            Register<UIStringOptionSelector>(new UIStringOptionSelectorAdapter());
            Register<UILocalizedOptionSelector>(new UILocalizedOptionSelectorAdapter());
        }
    }


    public static class UIElementAdapterRegistryInit
    {
#if UNITY_EDITOR

        [InitializeOnLoadMethod]
        private static void EditorInit()
        {
            UIElementAdapterRegistry.EnsureRegistered();
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInit()
        {
            UIElementAdapterRegistry.EnsureRegistered();
        }
    }
}


