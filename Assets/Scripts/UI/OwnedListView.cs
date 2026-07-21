using System.Collections.Generic;
using DeckBuilding.Data;
using UnityEngine;
using UnityEngine.UI;

namespace DeckBuilding.UI
{
    public class OwnedListView : MonoBehaviour
    {
        [SerializeField] private CharacterCardView cardPrefab;
        [SerializeField] private Transform contentContainer;

        public void Rebuild(IReadOnlyList<OwnedCharacter> sortedOwnedCharacters)
        {
            for (int i = contentContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(contentContainer.GetChild(i).gameObject);
            }

            foreach (OwnedCharacter ownedCharacter in sortedOwnedCharacters)
            {
                CharacterCardView card = Instantiate(cardPrefab, contentContainer);
                card.Bind(ownedCharacter, showQuantity: true);
            }

            // The VerticalLayoutGroup only repositions children on its next scheduled rebuild pass;
            // without forcing it here, newly instantiated cards stay stacked at their prefab-authored
            // position until something else happens to trigger a layout pass.
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)contentContainer);
        }
    }
}
