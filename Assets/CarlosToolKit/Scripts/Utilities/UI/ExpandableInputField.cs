using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Utilities.UI
{
    public class ExpandableInputField : MonoBehaviour
    {
        [SerializeField] TMP_InputField inputField;
        [SerializeField] RectTransform rectTransform;
        [SerializeField] RectTransform frameRectTransform;

        [SerializeField] bool enableHeight = true;
        [SerializeField] float maxHeight = 300f;
        [SerializeField] bool enableWidth = true;
        [SerializeField] float maxWidth = 500f;

        private float minHeight;
        private float minWidth;

        void Start()
        {
            inputField.onValueChanged.AddListener(AdjustSize);

            minWidth = rectTransform.sizeDelta.x;
            minHeight = rectTransform.sizeDelta.y;
        }

        void AdjustSize(string text)
        {

            if (inputField == null || rectTransform == null || frameRectTransform == null)
            {
                Debug.LogError("Null parameters");
                return;
            }

            float preferredWidth = Mathf.Max(LayoutUtility.GetPreferredWidth(inputField.textComponent.rectTransform), minWidth);
            float preferredHeight = Mathf.Max(LayoutUtility.GetPreferredHeight(inputField.textComponent.rectTransform), minHeight);

            float newWidth = enableWidth ? Mathf.Min(preferredWidth, maxWidth) : minWidth;
            float newHeight = enableHeight ? Mathf.Min(preferredHeight, maxHeight) : minHeight;

            rectTransform.sizeDelta = new Vector2(newWidth, newHeight);
            frameRectTransform.sizeDelta = new Vector2(newWidth, newHeight);
        }
    }
}

