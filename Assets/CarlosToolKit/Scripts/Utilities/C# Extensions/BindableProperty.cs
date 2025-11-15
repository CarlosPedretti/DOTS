using System;

namespace Utilities
{
    /// <summary>
    /// Represents a reactive property that triggers an event when its value changes.
    /// </summary>
    /// <typeparam name="T">Type of the property.</typeparam>
    public class BindableProperty<T>
    {
        private T _value;

        /// <summary>
        /// Event triggered when the value changes.
        /// </summary>
        public event Action<T> OnChanged;

        /// <summary>
        /// Creates a new BindableProperty with an optional initial value.
        /// </summary>
        /// <param name="initialValue">Initial value for the property.</param>
        public BindableProperty(T initialValue = default)
        {
            _value = initialValue;
        }

        /// <summary>
        /// Gets or sets the current value. 
        /// Setting a new value triggers the OnChanged event if the value is different.
        /// </summary>
        public T Value
        {
            get => _value;
            set
            {
                if (!Equals(_value, value))
                {
                    _value = value;
                    OnChanged?.Invoke(_value);
                }
            }
        }

        /// <summary>
        /// Sets a new value without triggering the OnChanged event.
        /// </summary>
        /// <param name="newValue">Value to set silently.</param>
        public void SetSilently(T newValue)
        {
            _value = newValue;
        }
    }

}
