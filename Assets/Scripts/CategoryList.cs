using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CategoryList : ListManager<Category>
{
    public static int GlobalItemId { get; set; }
    public float CubeDistance { get; set; } = 1.5f;
    public List<int> MissingYCoordinates { get; set; } = new();
    public int NextYCoordinates { get; set; } = 0;
    public Vector3 Centroid { get; private set; }
    public int ItemCount { get; private set; }

    public void AddToCentroid(Vector3 vector)
    {
        Centroid = (Centroid * ItemCount + vector) / (ItemCount + 1);
        ItemCount++;
    }

    public void SubstractFromCentroid(Vector3 vector, int itemCount = 1)
    {
        Centroid = (Centroid * ItemCount - vector) / (ItemCount - 1);
        ItemCount-=itemCount;
    }

    public async void AddToList()
    {
        var category = new Category();
        category.YCoordinate = GetYCoordinate();
        await AddToList(category);
        await category.AddToList();
    }

    private int GetYCoordinate()
    {
        var res = 0;
        if (MissingYCoordinates.Count > 0)
        {
            res = MissingYCoordinates[0];
            MissingYCoordinates.RemoveAt(0);
        }
        else
        {
            res = NextYCoordinates++;
        }
        return res;
    }

    public List<ItemDetail> GetItemDetailsByGameObjects(List<GameObject> gameObjects)
    {
        return Items
            .SelectMany(x=>x.Items)
            .Where(x=>gameObjects.Contains(x.ItemObject))
            .OrderBy(x=>gameObjects.IndexOf(x.ItemObject))
            .ToList();
    }
}
