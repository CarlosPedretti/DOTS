using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Utilities.UI
{
    public abstract class UIOptionSelector<T> : MonoBehaviour
    {
        [SerializeField] protected List<T> options = new();
        [SerializeField] protected TextMeshProUGUI optionText;

        [Header("Events")]
        public UnityEvent<int> OnOptionChangedIndex;
        public UnityEvent<T> OnOptionChangedValue;

        public IReadOnlyList<T> GetOptions() => options;

        protected int currentIndex = 0;

        protected virtual void OnEnable()
        {
            UpdateOption();
        }

        protected virtual void OnDisable()
        {

        }

        public virtual void NextOption()
        {
            if (options == null || options.Count == 0)
                return;

            currentIndex = (currentIndex + 1) % options.Count;
            UpdateOption();
        }

        public virtual void PreviousOption()
        {
            if (options == null || options.Count == 0)
                return;

            currentIndex--;
            if (currentIndex < 0)
                currentIndex = options.Count - 1;

            UpdateOption();
        }

        protected virtual void UpdateOption()
        {
            UpdateText();
            OnOptionChangedIndex?.Invoke(currentIndex);

            if (options != null && options.Count > 0)
                OnOptionChangedValue?.Invoke(options[currentIndex]);
        }

        protected abstract void UpdateText();

        public int GetCurrentIndex() => currentIndex;

        public void SetOption(int index)
        {
            currentIndex = Mathf.Clamp(index, 0, options.Count - 1);
            UpdateOption();
        }

        public void SetOption(T option)
        {
            if (options == null || options.Count == 0 || option == null)
                return;

            int idx = options.IndexOf(option);
            if (idx >= 0)
                SetOption(idx);
        }

        public void ClearOptions()
        {
            options.Clear();
            currentIndex = 0;
            UpdateText();
        }

        public void AddOption(T value)
        {
            if (value == null) return;
            options.Add(value);
        }
    }
}
