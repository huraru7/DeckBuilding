using DeckBuilding.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeckBuilding.UI
{
    public class CharacterCardView : MonoBehaviour
    {
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text quantityText;

        public OwnedCharacter BoundCharacter { get; private set; }

        public void Bind(OwnedCharacter ownedCharacter, bool showQuantity)
        {
            BoundCharacter = ownedCharacter;
            CharacterData master = ownedCharacter.Master;

            if (portraitImage != null) portraitImage.sprite = master.Portrait;
            if (nameText != null) nameText.text = master.CharacterName;
            if (costText != null) costText.text = $"Cost:{master.Cost}";
            if (hpText != null) hpText.text = $"HP:{master.Hp}";

            if (quantityText != null)
            {
                quantityText.gameObject.SetActive(showQuantity);
                if (showQuantity) quantityText.text = $"x{ownedCharacter.Quantity}";
            }
        }
    }
}
