using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using QFSW.QC;

namespace Utilities.Localization
{
    public static class LocalizationUtils
    {
        public static LanguageCollection LanguageCollection { get; private set; }

        public static List<LocalizedLanguage> Languages => LanguageCollection?.Languages;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            LanguageCollection = Resources.Load<LanguageCollection>("Localization/LanguageCollection");

            if (LanguageCollection == null)
            {
                Debug.LogWarning("[LocalizationUtils] No LanguageCollection found. Please create one in your project.");
                return;
            }

            Debug.Log($"[LocalizationUtils] Loaded {LanguageCollection.Languages.Count} languages from asset.");
        }

        [Command]
        public static void DebugAvailableLanguages()
        {
            if (Languages == null)
            {
                Debug.LogWarning("Languages list not loaded!");
                return;
            }

            foreach (var lang in Languages)
            {
                Debug.Log($"Code: {lang.Code}, Name: {lang.LocaleName}, Flag: {(lang.Flag ? "Exists" : "Null")}");
            }
        }

        public static IEnumerator ChangeLanguageByCode(string code)
        {
            yield return LocalizationSettings.InitializationOperation;

            var lang = LanguageCollection?.GetByCode(code);
            if (lang != null)
            {
                LocalizationSettings.SelectedLocale = lang.Locale;

                Debug.Log($"Language changed to: {lang.Code}");
            }
            else
            {
                Debug.LogWarning($"Locale not found: {code}");
            }
        }

        public static IEnumerator ChangeLanguageByIndex(int index)
        {
            yield return LocalizationSettings.InitializationOperation;

            if (Languages == null || index < 0 || index >= Languages.Count)
            {
                Debug.LogWarning($"Invalid language index: {index}");
                yield break;
            }

            var lang = Languages[index];
            LocalizationSettings.SelectedLocale = lang.Locale;

            Debug.Log($"Language changed to: {lang.Code}");
        }
    }
}
