using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class SlotManager : MonoBehaviour
{
    private const int MaxDeckCost = 10;
    public static SlotManager Instance { get; private set; }

    [SerializeField] private DeckSlot[] deckSlots;
    [SerializeField] private List<InventorySlot> inventorySlots;

    [SerializeField] private Card cardPrefab;
    [SerializeField] private CardData[] cardDataList;

    [SerializeField] private TMP_Text deckCostText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        //ゲーム開始時、インベントリスロットにランダムなカードを配布する
        foreach (var slot in inventorySlots)
        {
            var data = cardDataList[UnityEngine.Random.Range(0, cardDataList.Length)];
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
        int deckCost = CountingDeckCost();

        deckCostText.text = $"Cost: {deckCost}";
        deckCostText.color = deckCost > MaxDeckCost ? Color.red : Color.black;

        return true;
    }

    /// <summary>
    /// デッキのコストを計算し返り値として戻す
    /// </summary>
    private int CountingDeckCost()
    {
        var totalCost = 0;
        foreach (var slot in deckSlots)
        {
            if (slot.CurrentCard != null)
            {
                totalCost += slot.CurrentCard.Data.cost;
            }
        }
        return totalCost;
    }

    public void SortInventoryByCost()
    {
        SortInventory((a, b) => a.Data.cost.CompareTo(b.Data.cost));
    }

    public void SortInventoryByHp()
    {
        SortInventory((a, b) => a.Data.hp.CompareTo(b.Data.hp));
    }

    private void SortInventory(Comparison<Card> comparison)
    {
        var cards = new List<Card>();
        foreach (var slot in inventorySlots)
        {
            if (slot.CurrentCard != null)
            {
                cards.Add(slot.CurrentCard);
                slot.RemoveCard();
            }
        }

        cards.Sort(comparison);

        for (int i = 0; i < cards.Count; i++)
        {
            var slot = inventorySlots[i];
            var card = cards[i];

            slot.TrySetCard(card);
            card.transform.SetParent(slot.transform);
            (card.transform as RectTransform).anchoredPosition = Vector2.zero;
        }
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
