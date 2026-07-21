using System;

namespace DeckBuilding.Data
{
    [Serializable]
    public class OwnedCharacter
    {
        public CharacterData Master;
        public byte Quantity;
        public int AcquisitionOrder;

        public OwnedCharacter(CharacterData master, byte quantity, int acquisitionOrder)
        {
            Master = master;
            Quantity = quantity;
            AcquisitionOrder = acquisitionOrder;
        }
    }
}
