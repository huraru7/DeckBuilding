using UnityEngine;
using UnityEngine.EventSystems;

public class DeckSlot : MonoBehaviour, ISlot, IDropHandler
{
    private Card card;

    public Card CurrentCard => card;

    public bool TrySetCard(Card newCard)
    {
        if (card != null)
            return false;

        card = newCard;
        return true;
    }

    public void RemoveCard()
    {
        card = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var droppedCard = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<Card>() : null;
        if (droppedCard == null)
        {
            return;
        }

        var from = SlotManager.Instance.GetSlot(droppedCard);
        if (from == null || ReferenceEquals(from, this))
        {
            return;
        }

        if (SlotManager.Instance.MoveCard(droppedCard, from, this))
        {
            droppedCard.transform.SetParent(transform);
            (droppedCard.transform as RectTransform).anchoredPosition = Vector2.zero;
        }
    }
}
