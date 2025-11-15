using UnityEngine;

namespace Utilities.UI
{
    public class UIStringOptionSelector : UIOptionSelector<string>
    {
        protected override void UpdateText()
        {
            if (options == null || options.Count == 0)
            {
                optionText.text = string.Empty;
                return;
            }

            optionText.text = options[currentIndex];
        }
    }
}
