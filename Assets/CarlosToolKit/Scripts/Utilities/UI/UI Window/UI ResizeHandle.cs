using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utilities.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class ResizeHandle : MonoBehaviour, IDragHandler
    {
        [SerializeField] private UIResizable _resizableUI;
        [SerializeField] private ResizeDirection _direction;
        [SerializeField] private float handleThickness = 5f;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            UpdateHandlePosition();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                EditorApplication.delayCall += () =>
                {
                    if (this != null)
                    {
                        _rectTransform = GetComponent<RectTransform>();
                        UpdateHandlePosition();
                    }
                };
            }
        }
#endif

        public void OnDrag(PointerEventData eventData)
        {
            if (_resizableUI == null) return;
            _resizableUI.Resize(eventData, _direction);
        }

        private void UpdateHandlePosition()
        {
            if (_resizableUI == null || _rectTransform == null) return;

            switch (_direction)
            {
                case ResizeDirection.Left:
                    _rectTransform.anchorMin = new Vector2(0, 0);
                    _rectTransform.anchorMax = new Vector2(0, 1);
                    _rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    _rectTransform.sizeDelta = new Vector2(handleThickness, 0);
                    _rectTransform.anchoredPosition = Vector2.zero;
                    break;

                case ResizeDirection.Right:
                    _rectTransform.anchorMin = new Vector2(1, 0);
                    _rectTransform.anchorMax = new Vector2(1, 1);
                    _rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    _rectTransform.sizeDelta = new Vector2(handleThickness, 0);
                    _rectTransform.anchoredPosition = Vector2.zero;
                    break;

                case ResizeDirection.Top:
                    _rectTransform.anchorMin = new Vector2(0, 1);
                    _rectTransform.anchorMax = new Vector2(1, 1);
                    _rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    _rectTransform.sizeDelta = new Vector2(0, handleThickness);
                    _rectTransform.anchoredPosition = Vector2.zero;
                    break;

                case ResizeDirection.Bottom:
                    _rectTransform.anchorMin = new Vector2(0, 0);
                    _rectTransform.anchorMax = new Vector2(1, 0);
                    _rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    _rectTransform.sizeDelta = new Vector2(0, handleThickness);
                    _rectTransform.anchoredPosition = Vector2.zero;
                    break;
            }
        }
    }
}
