using UnityEngine;
using UnityEngine.EventSystems;

namespace DeckBuilding.UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class DraggableCardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Canvas _rootCanvas;

        private Transform _originalParent;
        private int _originalSiblingIndex;
        private Vector2 _originalAnchoredPosition;
        private bool _dropAccepted;

        /// <summary>Null when this card lives in the owned list; set to the deck slot index when placed in a deck slot.</summary>
        public int? SlotIndex { get; set; }

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _canvasGroup = GetComponent<CanvasGroup>();
            _rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _originalParent = transform.parent;
            _originalSiblingIndex = transform.GetSiblingIndex();
            _originalAnchoredPosition = _rectTransform.anchoredPosition;
            _dropAccepted = false;

            transform.SetParent(_rootCanvas.transform, worldPositionStays: true);
            transform.SetAsLastSibling();
            _canvasGroup.blocksRaycasts = false;

            // Normalize to a center point anchor so the ScreenPointToLocalPointInRectangle result
            // used in OnDrag can be assigned to anchoredPosition directly, regardless of whatever
            // anchor this card had while it was a layout-managed list item or slot child.
            _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);
            UpdatePosition(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdatePosition(eventData);
        }

        private void UpdatePosition(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_rootCanvas.transform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
            _rectTransform.anchoredPosition = localPoint;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = true;

            if (_dropAccepted)
            {
                // The drop target (deck slot / owned list) always creates its own fresh visual;
                // this dragged instance would otherwise be abandoned as a stray leftover under the canvas.
                Destroy(gameObject);
                return;
            }

            transform.SetParent(_originalParent, worldPositionStays: false);
            transform.SetSiblingIndex(_originalSiblingIndex);
            _rectTransform.anchoredPosition = _originalAnchoredPosition;
        }

        public void MarkDropAccepted() => _dropAccepted = true;
    }
}
