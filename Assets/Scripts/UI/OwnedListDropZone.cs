using DeckBuilding.Controllers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DeckBuilding.UI
{
    public class OwnedListDropZone : MonoBehaviour, IDropHandler
    {
        [SerializeField] private DeckBuildController controller;

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null) return;
            if (!eventData.pointerDrag.TryGetComponent(out DraggableCardView draggedCard)) return;
            if (!draggedCard.SlotIndex.HasValue) return;

            controller.RemoveFromSlot(draggedCard.SlotIndex.Value);
            draggedCard.MarkDropAccepted();
        }
    }
}
