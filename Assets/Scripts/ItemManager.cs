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

    public void ChangeCubeDistance(float distance)
    {
        foreach (var item in Categories.Items.SelectMany(x => x.Items))
        {
            var cube = item.ItemObject;
            cube.transform.position = cube.transform.position / Categories.CubeDistance * distance;
        }
        Categories.CubeDistance = distance;
    }

    public void SwitchItemsPositions(GameObject obj1, GameObject obj2)
    {
        var itemDetails = Categories.GetItemDetailsByGameObjects(new List<GameObject> { obj1, obj2 });
        if (itemDetails.Count != 2)
        {
            return;
        }

        var item1 = itemDetails[0];
        var item2 = itemDetails[1];

        (obj1.transform.position, obj2.transform.position) = (item2.ChangedPosition * Categories.CubeDistance, item1.ChangedPosition * Categories.CubeDistance);
        (item1.ChangedPosition, item2.ChangedPosition) = (item2.ChangedPosition, item1.ChangedPosition);
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
        var cube = Instantiate(category.Prefab, position * Categories.CubeDistance, Quaternion.identity);
        itemDetail.ItemObject = cube;
        itemDetail.OriginalPosition = position;
        itemDetail.ChangedPosition = position;
        return Task.CompletedTask;
    }

    private Vector2 GetXZCoordinates(Category category)
    {
        if (category.MissingCoordinates.Count > 0)
        {
            return category.MissingCoordinates.FirstOrDefault();
        }
        var res = new Vector2();

        var lastAdded = category.LastAdded;

        if (lastAdded.x < lastAdded.y)
        {
            res = new Vector2(lastAdded.y, lastAdded.x);
        }
        else if (lastAdded.x > lastAdded.y)
        {
            res = new Vector2(lastAdded.y + 1, lastAdded.x);
        }
        else if (lastAdded.x != -1 && lastAdded.x == lastAdded.y)
        {
            res = new Vector2(0, lastAdded.y + 1);
        }

        category.LastAdded = res;
        return res;
    }
}
