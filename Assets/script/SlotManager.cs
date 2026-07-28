using UnityEngine;
using System.Collections.Generic;

public class SlotManager : MonoBehaviour
{
    public static SlotManager Instance { get; private set; }

    [SerializeField] private DeckSlot[] deckSlots;
    [SerializeField] private List<InventorySlot> inventorySlots;

    private void Awake()
    {
        Instance = this;
    }

    public bool MoveCard(Card card, ISlot from, ISlot to)
    {
        if (!to.TrySetCard(card))
        {
            return false;
        }

        from.RemoveCard();
        return true;
    }

    public ISlot GetSlot(Card card)
    {
        foreach (var slot in deckSlots)
        {
            if (slot.CurrentCard == card)
            {
                return slot;
            }
        }

        foreach (var slot in inventorySlots)
        {
            if (slot.CurrentCard == card)
            {
                return slot;
            }
        }

        return null;
    }
}
