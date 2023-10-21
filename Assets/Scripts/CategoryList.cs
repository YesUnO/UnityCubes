
using System.Collections.Generic;

public class CategoryList : ListManager<Category>
{
    public static int GlobalItemId { get; set; }
    public float CubeDistance { get; set; }
    public List<int> MissingYCoordinates { get; set; } = new();
    public int NextZ { get; set; } = 0;

    public void AddToList()
    {
        var category = new Category();
        AddToList(category);
    }
}
