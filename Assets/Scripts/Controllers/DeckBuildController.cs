using System.Collections.Generic;
using DeckBuilding.Data;
using DeckBuilding.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeckBuilding.Controllers
{
    public class DeckBuildController : MonoBehaviour
    {
        [SerializeField] private CharacterData[] allCharacterMaster;
        [SerializeField] private OwnedListView ownedListView;
        [SerializeField] private DeckSlotDropZone[] deckSlots;
        [SerializeField] private CharacterCardView slotCardPrefab;
        [SerializeField] private TMP_Text totalCostText;
        [SerializeField] private Color normalCostColor = Color.black;
        [SerializeField] private Color overLimitCostColor = Color.red;
        [SerializeField] private Button sortButton;
        [SerializeField] private TMP_Text sortButtonLabel;
        [SerializeField] private Button decideButton;
        [SerializeField] private ErrorMessagePopup messagePopup;

        private List<OwnedCharacter> _ownedCharacters;
        private OwnedCharacter[] _deckSlotContents;
        private SortMode _currentSortMode = SortMode.AcquisitionOrder;

        private void Awake()
        {
            _deckSlotContents = new OwnedCharacter[GameConstants.MaxDeckSize];
            _ownedCharacters = CharacterRandomizer.GenerateOwnedCharacters(allCharacterMaster);

            sortButton.onClick.AddListener(OnSortButtonPressed);
            decideButton.onClick.AddListener(OnDecidePressed);

            RefreshOwnedList();
            UpdateSortButtonLabel();
            UpdateCostDisplay();
        }

        public bool TryPlaceInSlot(int slotIndex, OwnedCharacter character)
        {
            if (_deckSlotContents[slotIndex] != null) return false;

            _deckSlotContents[slotIndex] = character;
            RefreshSlotVisual(slotIndex);
            UpdateCostDisplay();
            return true;
        }

        public void RemoveFromSlot(int slotIndex)
        {
            if (_deckSlotContents[slotIndex] == null) return;

            _deckSlotContents[slotIndex] = null;
            RefreshSlotVisual(slotIndex);
            UpdateCostDisplay();
        }

        private void RefreshSlotVisual(int slotIndex)
        {
            Transform container = deckSlots[slotIndex].CardContainer;
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Destroy(container.GetChild(i).gameObject);
            }

            OwnedCharacter character = _deckSlotContents[slotIndex];
            if (character == null) return;

            CharacterCardView card = Instantiate(slotCardPrefab, container);
            card.Bind(character, showQuantity: false);
            if (card.TryGetComponent(out DraggableCardView draggable))
            {
                draggable.SlotIndex = slotIndex;
            }

            // The shared card prefab has a fixed absolute size sized for the owned-character list;
            // a deck slot has no layout group to resize it, so stretch it to the slot's own bounds
            // instead of leaving it at its authored size (which can overflow a smaller slot).
            var cardRect = (RectTransform)card.transform;
            cardRect.anchorMin = Vector2.zero;
            cardRect.anchorMax = Vector2.one;
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;
        }

        private int CalculateTotalCost()
        {
            int total = 0;
            foreach (OwnedCharacter character in _deckSlotContents)
            {
                if (character != null) total += character.Master.Cost;
            }
            return total;
        }

        private void UpdateCostDisplay()
        {
            int totalCost = CalculateTotalCost();
            totalCostText.text = $"Total cost:{totalCost}";
            totalCostText.color = totalCost >= GameConstants.CostLimit ? overLimitCostColor : normalCostColor;
        }

        private void OnSortButtonPressed()
        {
            _currentSortMode = OwnedCharacterSorter.Next(_currentSortMode);
            UpdateSortButtonLabel();
            RefreshOwnedList();
        }

        private void UpdateSortButtonLabel()
        {
            if (sortButtonLabel != null) sortButtonLabel.text = OwnedCharacterSorter.ToDisplayLabel(_currentSortMode);
        }

        private void RefreshOwnedList()
        {
            List<OwnedCharacter> sorted = OwnedCharacterSorter.Sort(_ownedCharacters, _currentSortMode);
            ownedListView.Rebuild(sorted);
        }

        private void OnDecidePressed()
        {
            if (CalculateTotalCost() >= GameConstants.CostLimit)
            {
                messagePopup.Show("コストが上限値を越えています");
            }
            else
            {
                messagePopup.Show("デッキを保存しました");
            }
        }
    }
}
