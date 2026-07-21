using System.Collections.Generic;
using DeckBuilding.Data;
using UnityEngine;

namespace DeckBuilding.Controllers
{
    public static class CharacterRandomizer
    {
        public static List<OwnedCharacter> GenerateOwnedCharacters(IReadOnlyList<CharacterData> masterData)
        {
            var owned = new List<OwnedCharacter>(masterData.Count);

            for (int i = 0; i < masterData.Count; i++)
            {
                byte quantity = (byte)Random.Range(1, GameConstants.MaxOwnedQuantity + 1);
                owned.Add(new OwnedCharacter(masterData[i], quantity, acquisitionOrder: i));
            }

            return owned;
        }
    }
}
