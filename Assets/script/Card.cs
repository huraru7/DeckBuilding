using UnityEngine;
using UnityEngine.UI;

//カード1枚を表すコンポーネント
[RequireComponent(typeof(Image))]
public class Card : MonoBehaviour
{
    private Image image;

    public CardData Data { get; private set; }
    public int Id { get; private set; }

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void Setup(CardData data, int id)
    {
        Data = data;
        Id = id;
        image.sprite = data.cardImage;
    }
}
