using UnityEngine;

namespace DeckBuilding.Data
{
    [CreateAssetMenu(fileName = "Char_New", menuName = "DeckBuilding/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [SerializeField] private int id;
        [SerializeField] private string characterName;
        [SerializeField] private int cost;
        [SerializeField] private int hp;
        [SerializeField] private Sprite portrait;

        public int Id => id;
        public string CharacterName => characterName;
        public int Cost => cost;
        public int Hp => hp;
        public Sprite Portrait => portrait;
    }
}
