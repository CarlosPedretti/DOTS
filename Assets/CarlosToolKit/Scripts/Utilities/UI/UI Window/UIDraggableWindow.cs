using UnityEngine;
using UnityEngine.EventSystems;

namespace Utilities.UI
{
    [DisallowMultipleComponent]
    public class UIDraggableWindow : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform _target = null;
        [SerializeField] private Canvas _canvas = null;
        [SerializeField] private bool _isDraggable = true;
        [SerializeField] private RectTransform _dragArea = null;

        private bool _isDragging = false;
        private Vector2 _dragOffset;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_isDraggable || _target == null || _canvas == null) return;

            bool insideDragArea = _dragArea == null ||
                RectTransformUtility.RectangleContainsScreenPoint(_dragArea, eventData.position, eventData.pressEventCamera);

            if (!insideDragArea) return;

            _isDragging = true;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_target, eventData.position, eventData.pressEventCamera, out _dragOffset);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || _canvas == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                _target.anchoredPosition = localPoint - _dragOffset;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
        }
    }
}
