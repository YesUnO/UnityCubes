using UnityEngine;

public class ItemMouseEvents : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

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
