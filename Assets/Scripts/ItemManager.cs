using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class UsedPrefab
{
    public GameObject gameObj;
    public bool used;
}

public class ItemManager : MonoBehaviour
{
    public List<UsedPrefab> Prefabs = new();
    public CategoryList Categories { get; set; } = new();

    public static ItemManager Instance { get; private set; }

    private void Awake() => Instance = this;

    // Start is called before the first frame update
    void Start()
    {
        Categories.SubscribeToItemAdded((category) => HandleCategoryAdded(category), 0);
        Categories.AddToList();
    }

    // Update is called once per frame
    void Update()
    {

    }


    private Task HandleCategoryAdded(Category category)
    {
        var prefab = Prefabs.FirstOrDefault(x => !x.used);
        if (prefab == null)
        {
            //error handling?
            return Task.CompletedTask;
        }
        category.Prefab = prefab.gameObj;
        prefab.used = true;

        category.SubscribeToItemAdded((itemDetail) => HandleItemAdded(itemDetail), 0);
        return Task.CompletedTask;
    }


    private Task HandleItemAdded(ItemDetail itemDetail)
    {
        var category = itemDetail.Category;
        var xz = GetXZCoordinates(category);
        var position = new Vector3(xz.x, category.YCoordinate, xz.y);
        var cube = Instantiate(category.Prefab, position, Quaternion.identity);
        itemDetail.ItemObject = cube;
        return Task.CompletedTask;
    }

    private Vector2 GetXZCoordinates(Category category)
    {
        if (category.MissingCoordinates.Count > 0)
        {
            return category.MissingCoordinates.FirstOrDefault();
        }
        var res = new Vector2();

        var last = category.LastAdded;

        if (last.x == last.y && last.x != -1)
        {
            res.x = 0;
            res.y += last.y;
        }
        if (last.y > last.x)
        {
            res.x = last.y;
            res.y = last.x;
        }
        if (last.x > last.y)
        {
            res.x = last.y;
            res.y = last.x + 1;
        }

        category.LastAdded = res;
        return res;
    }
}
