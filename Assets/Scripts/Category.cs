using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Category : ListManager<ItemDetail>, IIdentifiable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public GameObject Prefab { get; set; }
    public int YCoordinate { get; set; }
    public List<Vector2> MissingCoordinates { get; set; } = new();
    public Vector2 LastAdded { get; set; } = new Vector2(-1, -1);

    public void AddToList()
    {
        var item = new ItemDetail();
        item.Category = this;
        AddToList(item);
    }
}
