
using System.Collections.Generic;

public class CategoryList : ListManager<Category>
{
    public static int GlobalItemId { get; set; }
    public float CubeDistance { get; set; }
    public List<int> MissingZ { get; set; } = new();
}
