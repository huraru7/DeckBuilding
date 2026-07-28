using UnityEngine;

//カード1種類分のステータス元データ
[CreateAssetMenu(fileName = "CardData", menuName = "DeckBuilding/CardData")]
public class CardData : ScriptableObject
{
    public int hp;
    public int cost;
    public Sprite cardImage;
}
