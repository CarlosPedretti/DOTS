using QFSW.QC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Utilities.UI
{
    public enum ResizeDirection
    {
        Left,
        Right,
        Top,
        Bottom
    }

    [DisallowMultipleComponent]
    public class UIResizable : MonoBehaviour, IDragHandler
    {
        [Header("References")]
        [SerializeField] private RectTransform _resizeRoot = null;
        [SerializeField] private Canvas _resizeCanvas = null;

        [Header("Canvas Sorting")]
        [SerializeField, Tooltip("Initial sorting order of the canvas.")]
        private int _initialSortingOrder = 0;


        [Header("Settings")]
        [SerializeField, Tooltip("Prevents resizing/moving outside the screen. Sets maxSize to screen size.")]
        private bool _lockInScreen = true;

        [SerializeField, Tooltip("Minimum width and height.")]
        private Vector2 _minSize = new(100f, 100f);

        [SerializeField, Tooltip("Maximum width and height. Ignored if 'Lock In Screen' is enabled.")]
        private Vector2 _maxSize = new(1920f, 1080f);

        [SerializeField, Tooltip("Prevents overlap with other resizable elements.")]
        private bool _blockOverlap = true;

        private static readonly List<UIResizable> _instances = new();

        private Vector2 _initialSize;
        private Vector2 _initialPosition;

        private Canvas _localCanvas;

        #region Unity Methods

        private void Awake()
        {
            StoreInitialState();
            AdjustMaxSizeToCanvas();
            _instances.Add(this);
        }

        private void OnDestroy()
        {
            _instances.Remove(this);
        }

        #endregion

        #region Public Methods

        public void OnDrag(PointerEventData eventData)
        {
            Resize(eventData, ResizeDirection.Right);
            Resize(eventData, ResizeDirection.Top);
        }

        public void Resize(PointerEventData eventData, ResizeDirection direction)
        {
            if (!IsValidSetup()) return;

            Vector2 delta = eventData.delta / _resizeCanvas.scaleFactor;
            (Vector2 size, Vector2 position) = GetResizedSizeAndPosition(delta, direction);

            if (IsOutOfBounds(size, direction))
            {
                size = ClampSize(size);
                position = KeepPositionIfClamped(size, direction, position);
            }

            if (_blockOverlap && WouldOverlap(size, position)) return;

            ApplyResize(size, position);

            if (_lockInScreen)
                ClampToScreen();
        }

        [Command]
        public void ResetToInitialSize()
        {
            ApplyResize(
                new Vector2(
                    Mathf.Clamp(_initialSize.x, _minSize.x, _maxSize.x),
                    Mathf.Clamp(_initialSize.y, _minSize.y, _maxSize.y)
                ),
                _initialPosition
            );

            if (_lockInScreen)
                ClampToScreen();
        }

        [Command]
        public void SetSortingOrder(int order)
        {
            _localCanvas.sortingOrder = order;
        }

        [Command]
        public void ResetSortingOrder()
        {
            if (_localCanvas != null)
            {
                _localCanvas.overrideSorting = false;
                _localCanvas.sortingOrder = _initialSortingOrder;
            }
        }

        #endregion

        #region Private Methods

        private bool IsValidSetup() => _resizeRoot != null && _resizeCanvas != null;

        private void StoreInitialState()
        {
            if (_resizeRoot == null) return;

            EnsureLocalCanvas();

            _initialSize = _resizeRoot.sizeDelta;
            _initialPosition = _resizeRoot.anchoredPosition;
        }

        private void EnsureLocalCanvas()
        {
            if (_resizeRoot == null) return;

            _localCanvas = _resizeRoot.GetComponent<Canvas>();

            if (_localCanvas == null)
            {
                _localCanvas = _resizeRoot.gameObject.AddComponent<Canvas>();
            }

            if (_resizeRoot.GetComponent<GraphicRaycaster>() == null)
            {
                _resizeRoot.gameObject.AddComponent<GraphicRaycaster>();
            }

            _localCanvas.overrideSorting = true;
            _localCanvas.sortingOrder = _initialSortingOrder;
        }

        private void AdjustMaxSizeToCanvas()
        {
            if (!_lockInScreen || _resizeCanvas == null) return;

            RectTransform canvasRect = _resizeCanvas.GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRect.rect.size;

            _maxSize = Vector2.Min(_maxSize, canvasSize);
        }

        private (Vector2 size, Vector2 position) GetResizedSizeAndPosition(Vector2 delta, ResizeDirection direction)
        {
            Vector2 size = _resizeRoot.sizeDelta;
            Vector2 position = _resizeRoot.anchoredPosition;

            switch (direction)
            {
                case ResizeDirection.Left:
                    size.x -= delta.x;
                    position.x += delta.x * 0.5f;
                    break;
                case ResizeDirection.Right:
                    size.x += delta.x;
                    position.x += delta.x * 0.5f;
                    break;
                case ResizeDirection.Top:
                    size.y += delta.y;
                    position.y += delta.y * 0.5f;
                    break;
                case ResizeDirection.Bottom:
                    size.y -= delta.y;
                    position.y += delta.y * 0.5f;
                    break;
            }

            return (size, position);
        }

        private Vector2 ClampSize(Vector2 size) => new(
            Mathf.Clamp(size.x, _minSize.x, _maxSize.x),
            Mathf.Clamp(size.y, _minSize.y, _maxSize.y)
        );

        private bool IsOutOfBounds(Vector2 size, ResizeDirection direction)
        {
            return (direction == ResizeDirection.Left || direction == ResizeDirection.Right) &&
                   (size.x <= _minSize.x || size.x >= _maxSize.x)
                ||
                   (direction == ResizeDirection.Top || direction == ResizeDirection.Bottom) &&
                   (size.y <= _minSize.y || size.y >= _maxSize.y);
        }

        private Vector2 KeepPositionIfClamped(Vector2 size, ResizeDirection direction, Vector2 position)
        {
            if ((direction == ResizeDirection.Left || direction == ResizeDirection.Right) &&
                (size.x == _minSize.x || size.x == _maxSize.x))
            {
                position.x = _resizeRoot.anchoredPosition.x;
            }

            if ((direction == ResizeDirection.Top || direction == ResizeDirection.Bottom) &&
                (size.y == _minSize.y || size.y == _maxSize.y))
            {
                position.y = _resizeRoot.anchoredPosition.y;
            }

            return position;
        }

        private void ApplyResize(Vector2 size, Vector2 position)
        {
            _resizeRoot.sizeDelta = size;
            _resizeRoot.anchoredPosition = position;
        }

        private void ClampToScreen()
        {
            if (!IsValidSetup()) return;

            Rect canvasRect = GetWorldRect(_resizeCanvas.GetComponent<RectTransform>());
            Rect rectWorld = GetWorldRect(_resizeRoot);

            Vector2 offset = CalculateOffset(rectWorld, canvasRect);

            if (offset != Vector2.zero)
            {
                Vector2 localOffset;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _resizeRoot.parent as RectTransform,
                    RectTransformUtility.WorldToScreenPoint(null, _resizeRoot.position + (Vector3)offset),
                    null,
                    out localOffset
                );

                _resizeRoot.anchoredPosition = localOffset;
            }
        }

        private Rect GetWorldRect(RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            return new Rect(
                corners[0].x,
                corners[0].y,
                corners[2].x - corners[0].x,
                corners[2].y - corners[0].y
            );
        }

        private Vector2 CalculateOffset(Rect rect, Rect bounds)
        {
            Vector2 offset = Vector2.zero;

            if (rect.xMin < bounds.xMin) offset.x = bounds.xMin - rect.xMin;
            if (rect.xMax > bounds.xMax) offset.x = bounds.xMax - rect.xMax;
            if (rect.yMin < bounds.yMin) offset.y = bounds.yMin - rect.yMin;
            if (rect.yMax > bounds.yMax) offset.y = bounds.yMax - rect.yMax;

            return offset;
        }

        private bool WouldOverlap(Vector2 newSize, Vector2 newPosition)
        {
            RectTransform tempRect = Instantiate(_resizeRoot, _resizeRoot.parent);
            tempRect.sizeDelta = newSize;
            tempRect.anchoredPosition = newPosition;

            Bounds newBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(_resizeCanvas.transform, tempRect);
            DestroyImmediate(tempRect.gameObject);

            foreach (var other in _instances)
            {
                if (other == this || other._resizeRoot == null) continue;

                Bounds otherBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(_resizeCanvas.transform, other._resizeRoot);
                if (newBounds.Intersects(otherBounds)) return true;
            }

            return false;
        }

        #endregion
    }
}