using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Card))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class CardMovement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //ドロップ判定先のスロットにRaycastを通すため、自分自身を判定対象から外す
        originalParent = rectTransform.parent;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        //OnDropで移動が成功していれば親が変わっているはず。変わっていなければ失敗なので元の位置へ戻す
        if (rectTransform.parent == originalParent)
        {
            rectTransform.position = originalParent.position;
        }
    }
}
