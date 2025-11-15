using UnityEngine;
using UnityEngine.UI;

namespace Utilities.UI
{
    [RequireComponent(typeof(Outline))]
    public class UIOutlineController : MonoBehaviour
    {
        private Outline outline;

        void Awake()
        {
            outline = GetComponent<Outline>();
        }

        /// <summary>
        /// Clears the outline by making it fully transparent.
        /// </summary>
        public void ClearOutline()
        {
            outline.effectColor = new Color(0, 0, 0, 0);
        }

        /// <summary>
        /// Disables the outline component entirely.
        /// </summary>
        public void DisableOutline()
        {
            outline.enabled = false;
        }

        /// <summary>
        /// Enables the outline component if it was previously disabled.
        /// </summary>
        public void EnableOutline()
        {
            outline.enabled = true;
        }

        /// <summary>
        /// Sets the outline to a custom color.
        /// </summary>
        /// <param name="r">Red value (0-1).</param>
        /// <param name="g">Green value (0-1).</param>
        /// <param name="b">Blue value (0-1).</param>
        public void SetCustomColor(float r, float g, float b)
        {
            outline.effectColor = new Color(r, g, b, 1f);
        }

        /// <summary>
        /// Sets the outline to a custom color with transparency.
        /// </summary>
        /// <param name="r">Red value (0-1).</param>
        /// <param name="g">Green value (0-1).</param>
        /// <param name="b">Blue value (0-1).</param>
        /// <param name="a">Alpha value (0-1).</param>
        public void SetCustomColorWithAlpha(float r, float g, float b, float a)
        {
            outline.effectColor = new Color(r, g, b, a);
        }
    }
}

