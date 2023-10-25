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

    public void RemoveActiveCategory()
    {
        var category = Categories.ActiveItem;
        var categoryCentroid = category.Centroid;
        var categoryCount = category.ItemCount;
        Categories.RemoveActiveFromList();

        Categories.MissingYCoordinates.Add(category.YCoordinate);
        var prefab = Prefabs.FirstOrDefault(x => x.gameObj == category.Prefab);
        prefab.used = false;

        Categories.SubstractFromCentroid(categoryCentroid, categoryCount);
        AdjustCamera();
    }

    public void RemoveActiveItem()
    {
        var itemPos = Categories.ActiveItem.ActiveItem.ChangedPosition;
        Categories.ActiveItem.RemoveActiveFromList();

        AddToMissingCoordinates(itemPos);

        Categories.SubstractFromCentroid(itemPos);
        Categories.ActiveItem.SubstractFromCentroid(itemPos);
        AdjustCamera();
    }

    private void AddToMissingCoordinates(Vector3 pos)
    {
        var category = Categories.Items.FirstOrDefault(x => x.YCoordinate == pos.y);
        if (category != null)
        {
            category.MissingCoordinates.Add(pos);
        }
    }

    public void AdjustCamera()
    {
        _cameraControlls.AdjustTargetPosition(Categories.Centroid, Categories.CubeDistance);
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

    private void SetActiveItemGameObj(ItemDetail itemDetail)
    {
        if (itemDetail == null)
        {
            return;
        }
        if (_selectedItem != null && _selectedItem.ItemObject != null)
        {
            SetItemState(_selectedItem.ItemObject, ItemState.Selected, false);
        }
        SetItemState(itemDetail.ItemObject, ItemState.Selected);
        _selectedItem = itemDetail;
    }
    #endregion

    #region EventHandlers
    private Task<bool> HandleCategoryAdded(Category category)
    {
        var prefab = Prefabs.FirstOrDefault(x => !x.used);
        if (prefab == null)
        {
            Categories.RemoveFromList(category);
            Categories.MissingYCoordinates.Add(category.YCoordinate);
            return Task.FromResult(false);
        }
        category.Prefab = prefab.gameObj;
        prefab.used = true;

        category.SubscribeToItemAdded((itemDetail) => HandleItemAdded(itemDetail), 0);
        category.SubscribeToItemActivated((itemDetail) => HandleItemActivated(itemDetail));
        return Task.FromResult(true);
    }

    private void HandleItemActivated(ItemDetail itemDetail)
    {
        SetActiveItemGameObj(itemDetail);
    }

    private Task<bool> HandleItemAdded(ItemDetail itemDetail)
    {
        var category = itemDetail.Category;
        var position = GetXZCoordinates(category);
        var cube = Instantiate(category.Prefab, position * Categories.CubeDistance, Quaternion.identity);

        category.AddToCentroid(position);
        Categories.AddToCentroid(position);
        AdjustCamera();

        itemDetail.ItemObject = cube;
        itemDetail.OriginalPosition = position;
        itemDetail.ChangedPosition = position;
        return Task.FromResult(true);
    }

    private Vector3 GetXZCoordinates(Category category)
    {
        if (category.MissingCoordinates.Count > 0)
        {
            var res = category.MissingCoordinates[0];
            category.MissingCoordinates.RemoveAt(0);
            return res;

        }
        var xyCoordinates = new Vector2();

        var lastAdded = category.LastAdded;

        if (lastAdded.x < lastAdded.y)
        {
            xyCoordinates = new Vector2(lastAdded.y, lastAdded.x);
        }
        else if (lastAdded.x > lastAdded.y)
        {
            xyCoordinates = new Vector2(lastAdded.y + 1, lastAdded.x);
        }
        else if (lastAdded.x != -1 && lastAdded.x == lastAdded.y)
        {
            xyCoordinates = new Vector2(0, lastAdded.y + 1);
        }

        category.LastAdded = xyCoordinates;
        return new Vector3(xyCoordinates.x, category.YCoordinate, xyCoordinates.y);
    }
    #endregion

}
