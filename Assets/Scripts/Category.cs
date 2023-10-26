using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Category : ListManager<ItemDetail>, IIdentifiable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public GameObject Prefab { get; set; }
    public int YCoordinate { get; set; }
    public Vector2 LastAdded { get; set; } = new Vector2(-1, -1);
    public CategoryList CategoryList { get; }

    public string UiElName { get { return $"Category#{Id}"; } }
    public string ContainerUiElName { get { return $"CategoryContainer#{Id}"; } }

    public Category(CategoryList categoryList)
    {
        CategoryList = categoryList;
    }


    public async Task AddToList()
    {
        var item = new ItemDetail();
        item.Category = this;
        await AddToList(item);
    }
}
