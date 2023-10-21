
using UnityEngine;

public class Category : ListManager<ItemDetail>, IIdentifiable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public GameObject Prefab { get; set; }

    public void AddToList()
    {
        var item = new ItemDetail();
        item.Category = this;
        AddToList(item);
    }
}
