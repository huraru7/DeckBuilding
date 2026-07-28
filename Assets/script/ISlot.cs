//インベントリスロット・デッキスロット共通のインターフェース
//スロット自身は移動ルールを持たず、カードの出し入れのみを行う
public interface ISlot
{
    Card CurrentCard { get; }

    bool TrySetCard(Card card);

    void RemoveCard();
}