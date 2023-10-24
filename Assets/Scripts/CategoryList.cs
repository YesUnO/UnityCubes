
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class CategoryList : ListManager<Category>
{
    public static int GlobalItemId { get; set; }
    public float CubeDistance { get; set; } = 1.5f;
    public List<int> MissingYCoordinates { get; set; } = new();
    public int NextYCoordinates { get; set; } = 0;

    public async void AddToList()
    {
        var category = new Category();
        category.YCoordinate = GetYCoordinate();
        await AddToList(category);
        category.AddToList();
    }

    private int GetYCoordinate()
    {
        return MissingYCoordinates.Count > 0 ? MissingYCoordinates.FirstOrDefault(): NextYCoordinates++;
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
