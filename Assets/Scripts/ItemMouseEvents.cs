using UnityEngine;

public class ItemMouseEvents : MonoBehaviour
{
    private void OnMouseOver()
    {
        if (!DragAndDropManager.Instance.IsDragging)
        {
            ItemManager.Instance.SetItemState(gameObject, ItemState.Hovered);
        }
    }

    private void OnMouseExit()
    {
        ItemManager.Instance.SetItemState(gameObject, ItemState.Hovered, false);
    }
}
