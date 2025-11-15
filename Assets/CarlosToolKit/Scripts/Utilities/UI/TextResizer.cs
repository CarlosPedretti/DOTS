using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VInspector;

namespace Utilities.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class TextResizer : MonoBehaviour
    {
        [SerializeField] private TMP_Text targetText;
        [Variants("Horizontal", "Vertical")]
        [SerializeField] private string fit = "Horizontal";
        [SerializeField] private float padding = 10f;

        private RectTransform rectTransform;
        private Vector2 lastSize;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (targetText == null)
                targetText = GetComponentInChildren<TMP_Text>();
        }

        void LateUpdate()
        {
            if (!targetText) return;

            targetText.ForceMeshUpdate();

            float preferredWidth = targetText.preferredWidth + padding;
            float preferredHeight = targetText.preferredHeight + padding;

            Vector2 newSize = rectTransform.sizeDelta;

            if (fit == "Horizontal")
                newSize.x = preferredWidth;
            if (fit == "Vertical")
                newSize.y = preferredHeight;

            if (Vector2.Distance(newSize, lastSize) > 0.5f)
            {
                rectTransform.sizeDelta = newSize;
                LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
                lastSize = newSize;
            }
        }
    }
}

