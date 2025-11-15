using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities.UI
{
    public class UIElementStyler : MonoBehaviour
    {
        [Header("Optional References")]
        [SerializeField] private TMP_Text text;
        [SerializeField] private Image image;

        /// <summary>
        /// Sets the color of the assigned TMP_Text component using a HEX color code, if one is assigned.
        /// Example: "#FF0000" for red.
        /// </summary>
        public void SetTextColor(string hexColor)
        {
            if (text != null && ColorUtility.TryParseHtmlString(hexColor, out Color color))
            {
                text.color = color;
            }
            else
            {
                Debug.LogWarning($"Invalid HEX color: {hexColor}");
            }
        }

        /// <summary>
        /// Sets the color of the assigned Image component using a HEX color code, if one is assigned.
        /// Example: "#FF0000" for red.
        /// </summary>
        public void SetImageColor(string hexColor)
        {
            if (image != null && ColorUtility.TryParseHtmlString(hexColor, out Color color))
            {
                image.color = color;
            }
            else
            {
                Debug.LogWarning($"Invalid HEX color: {hexColor}");
            }
        }

        /// <summary>
        /// Sets the text content of the assigned TMP_Text (if any).
        /// </summary>
        public void SetText(string newText)
        {
            if (text != null)
                text.text = newText;
        }

        /// <summary>
        /// Changes the sprite of the assigned image (if any).
        /// </summary>
        public void SetImageSprite(Sprite sprite)
        {
            if(sprite != null)
                image.sprite = sprite;
        }

        /// <summary>
        /// Enables or disables the GameObject this component is attached to.
        /// </summary>
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        /// <summary>
        /// Sets the alpha (transparency) of the assigned image and/or text.
        /// </summary>
        public void SetAlpha(float alpha)
        {
            if (image != null)
            {
                var c = image.color;
                c.a = alpha;
                image.color = c;
            }

            if (text != null)
            {
                var c = text.color;
                c.a = alpha;
                text.color = c;
            }
        }
    }

}
