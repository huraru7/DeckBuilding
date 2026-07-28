using UnityEngine;
using System.Collections.Generic;

public class SlotManager : MonoBehaviour
{
    public static SlotManager Instance { get; private set; }

    [SerializeField] private DeckSlot[] deckSlots;
    [SerializeField] private List<InventorySlot> inventorySlots;

    [SerializeField] private Card cardPrefab;
    [SerializeField] private CardData[] cardDataList;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        //ゲーム開始時、インベントリスロットにランダムなカードを配布する
        foreach (var slot in inventorySlots)
        {
            var data = cardDataList[Random.Range(0, cardDataList.Length)];
            var card = Instantiate(cardPrefab, slot.transform);
            card.Setup(data);
            (card.transform as RectTransform).anchoredPosition = Vector2.zero;

            slot.TrySetCard(card);
        }
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
