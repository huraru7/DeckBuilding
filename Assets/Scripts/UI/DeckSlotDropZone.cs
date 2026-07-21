using DeckBuilding.Controllers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DeckBuilding.UI
{
    public class DeckSlotDropZone : MonoBehaviour, IDropHandler
    {
        [SerializeField] private int slotIndex;
        [SerializeField] private DeckBuildController controller;
        [SerializeField] private Transform cardContainer;

        public int SlotIndex => slotIndex;
        public Transform CardContainer => cardContainer;

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null) return;
            if (!eventData.pointerDrag.TryGetComponent(out DraggableCardView draggedCard)) return;
            if (!eventData.pointerDrag.TryGetComponent(out CharacterCardView cardView)) return;
            if (cardView.BoundCharacter == null) return;

            bool placed = controller.TryPlaceInSlot(slotIndex, cardView.BoundCharacter);
            if (!placed) return;

            draggedCard.MarkDropAccepted();

            // Dragging directly from one deck slot into another: clear the source slot so the
            // character isn't left double-counted in both slots.
            if (draggedCard.SlotIndex.HasValue && draggedCard.SlotIndex.Value != slotIndex)
            {
                controller.RemoveFromSlot(draggedCard.SlotIndex.Value);
            }
        }
    }
}
