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
    public Vector3 Centroid { get; private set; }
    public int ItemCount { get; private set; }
    public CategoryList CategoryList { get; }

    public string UiElName { get { return $"Category#{Id}"; } }
    public string ContainerUiElName { get { return $"CategoryContainer#{Id}"; } }

    public Category(CategoryList categoryList)
    {
        CategoryList = categoryList;
    }


    public void AddToCentroid(Vector3 vector)
    {
        Centroid = (Centroid * ItemCount + vector) / (ItemCount + 1);
        ItemCount++;
    }

    public void SubstractFromCentroid(Vector3 vector)
    {
        Centroid = (Centroid * ItemCount - vector) / (ItemCount - 1);
        ItemCount--;
    }

    public async Task AddToList()
    {
        var item = new ItemDetail();
        item.Category = this;
        await AddToList(item);
    }
}
