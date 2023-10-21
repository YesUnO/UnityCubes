using System;
using System.Collections.Generic;
using System.Linq;
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
    public CategoryList Categories { get; private set; } = new();

    public ItemManager Instance { get; private set; }

    private void Awake() => Instance = this;

    // Start is called before the first frame update
    void Start()
    {
        Categories.SubscribeToItemAdded((category) => HandleCategoryAdded(category));
        Categories.AddToList();
    }

    // Update is called once per frame
    void Update()
    {

    }


    private void HandleCategoryAdded(Category category)
    {
        var prefab = Prefabs.FirstOrDefault(x => !x.used);
        if (prefab == null)
        {
            //error handling?
            return;
        }
        category.Prefab = prefab.gameObj;
        prefab.used = true;

        category.SubscribeToItemAdded((itemDetail) => HandleItemAdded(itemDetail), 0);
        category.YCoordinate = GetYCoordinate();
        category.AddToList();
    }

    private int GetYCoordinate()
    {
        return Categories.MissingYCoordinates.Count > 0 ? Categories.NextZ++ : Categories.MissingYCoordinates.FirstOrDefault();
    }

    private void HandleItemAdded(ItemDetail itemDetail)
    {
        var category = itemDetail.Category;
        var xz = GetXZCoordinates(category);
        var position = new Vector3(xz.x, category.YCoordinate, xz.y);
        var cube = Instantiate(category.Prefab, position, Quaternion.identity);
        itemDetail.ItemObject = cube;
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
