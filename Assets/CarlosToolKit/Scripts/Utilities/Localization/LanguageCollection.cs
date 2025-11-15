using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Localization
{
    [CreateAssetMenu(fileName = "LanguageCollection", menuName = "CarlosToolKit/Localization/Language Collection")]
    public class LanguageCollection : ScriptableObject
    {
        public List<LocalizedLanguage> Languages = new();

        public LocalizedLanguage GetByCode(string code)
        {
            return Languages.Find(lang => lang.Code == code);
        }

        public LocalizedLanguage GetByIndex(int index)
        {
            if (index >= 0 && index < Languages.Count)
            {
                return Languages[index];
            }

            return null;
        }
    }
}
