
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class CategoryList : ListManager<Category>
{
    public static int GlobalItemId { get; set; }
    public float CubeDistance { get; set; }
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
}
