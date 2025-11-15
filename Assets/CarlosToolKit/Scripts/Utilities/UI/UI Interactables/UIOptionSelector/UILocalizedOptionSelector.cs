using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Utilities.UI
{
    public class UILocalizedOptionSelector : UIOptionSelector<LocalizedString>
    {
        private LocalizedString currentLocalized;

        protected override void OnEnable()
        {
            base.OnEnable();
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            SubscribeToCurrentString();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            UnsubscribeFromCurrentString();
        }

        private void OnLocaleChanged(UnityEngine.Localization.Locale locale)
        {
            UpdateText();
        }

        protected override void UpdateOption()
        {
            base.UpdateOption();
            SubscribeToCurrentString();
        }

        private void SubscribeToCurrentString()
        {
            UnsubscribeFromCurrentString();

            if (options == null || options.Count == 0)
                return;

            currentLocalized = options[currentIndex];

            if (currentLocalized != null)
                currentLocalized.StringChanged += OnLocalizedStringChanged;
        }

        private void UnsubscribeFromCurrentString()
        {
            if (currentLocalized != null)
            {
                currentLocalized.StringChanged -= OnLocalizedStringChanged;
                currentLocalized = null;
            }
        }

        private void OnLocalizedStringChanged(string value)
        {
            optionText.text = value;
        }

        protected override void UpdateText()
        {
            if (options == null || options.Count == 0)
            {
                optionText.text = string.Empty;
                return;
            }

            var localized = options[currentIndex];
            if (localized == null)
            {
                optionText.text = "LOCALIZED OPTION IS NULL";
                return;
            }

            optionText.text = localized.GetLocalizedString();
        }
    }
}
