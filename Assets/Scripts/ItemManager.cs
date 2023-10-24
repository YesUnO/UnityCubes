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

public enum ItemState
{
    Collided,
    Selected,
    Hovered,
}

public class ItemManager : MonoBehaviour
{
    
    public List<UsedPrefab> Prefabs = new();
    public CategoryList Categories { get; private set; } = new();

    public static ItemManager Instance { get; private set; }

    private CameraControlls _cameraControlls;

    private ItemDetail _selectedItem;

    private void Awake() => Instance = this;

    // Start is called before the first frame update
    void Start()
    {
        _cameraControlls = FindObjectOfType<CameraControlls>();
        Categories.SubscribeToItemAdded((category) => HandleCategoryAdded(category), 0);
        Categories.AddToList();
    }

    // Update is called once per frame
    void Update()
    {

    }

    #region CubeProperties
    public void ChangeCubeDistance(float distance)
    {
        foreach (var item in Categories.Items.SelectMany(x => x.Items))
        {
            var cube = item.ItemObject;
            cube.transform.position = cube.transform.position / Categories.CubeDistance * distance;
        }
        Categories.CubeDistance = distance;
        _cameraControlls.AdjustTargetPosition(Categories.Centroid, distance);
    }

    public bool? GetItemState(GameObject gameObject, ItemState itemState)
    {
        var selectedChild = gameObject.transform.Find(itemState.ToString());
        if (selectedChild != null)
        {
            return selectedChild.gameObject.activeSelf;
        }
        return null;
    }

    public void SetItemState(GameObject gameObject, ItemState itemState, bool value = true)
    {
        var selectedChild = gameObject.transform.Find(itemState.ToString());
        if (selectedChild != null)
        {
            selectedChild.gameObject.SetActive(value);
        }
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
    #endregion

    #region EventHandlers
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
        category.SubscribeToItemActivated((itemDetail) => HandleItemActivated(itemDetail));
        return Task.CompletedTask;
    }

    private void HandleItemActivated(ItemDetail itemDetail)
    {
        if (_selectedItem != null && _selectedItem.ItemObject != null)
        {
            Debug.Log($"activating {itemDetail.Id} deactivating {_selectedItem.Id}");
            SetItemState(_selectedItem.ItemObject, ItemState.Selected, false);
        }
        SetItemState(itemDetail.ItemObject, ItemState.Selected);
        _selectedItem = itemDetail;
    }


    private Task HandleItemAdded(ItemDetail itemDetail)
    {
        var category = itemDetail.Category;
        var xz = GetXZCoordinates(category);
        var position = new Vector3(xz.x, category.YCoordinate, xz.y);
        var cube = Instantiate(category.Prefab, position * Categories.CubeDistance, Quaternion.identity);

        category.AddToCentroid(position);
        Categories.AddToCentroid(position);
        _cameraControlls.AdjustTargetPosition(Categories.Centroid, Categories.CubeDistance);

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
    #endregion

}
