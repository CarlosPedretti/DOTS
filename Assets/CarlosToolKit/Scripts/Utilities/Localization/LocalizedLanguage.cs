using UnityEngine;
using UnityEngine.Localization;

namespace Utilities.Localization
{
    /// <summary>
    /// Represents a language option in the game, including its locale,
    /// localized display name, code, and optional flag sprite.
    /// </summary>
    [System.Serializable]
    public class LocalizedLanguage
    {
        [Header("Localization Data")]
        [Tooltip("The Locale associated with this language (e.g., English, Spanish).")]
        public Locale Locale;

        [Tooltip("Localized string representing the display name of this language.")]
        public LocalizedString LocalizedName;

        [Header("Metadata")]
        [Tooltip("The locale code (e.g., 'en', 'es'). Automatically populated from the Locale.")]
        public string Code => Locale.Identifier.Code;

        [Tooltip("The locale's display name (e.g., 'English (en)'). Automatically populated from the Locale.")]
        public string LocaleName => Locale.LocaleName;

        [Header("Visuals")]
        [Tooltip("Optional flag icon representing this language.")]
        public Sprite Flag;
    }
}
