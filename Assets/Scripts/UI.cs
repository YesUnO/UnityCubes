using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    [SerializeField]
    private UIDocument _uiDocument;

    [SerializeField]
    private StyleSheet _uiStyleSheet;

    private VisualElement _categoryDetailContainer;
    private VisualElement _categoriesContainer;
    private VisualElement _itemDetailContainer;

    private int _activeCategoryId;
    private int _activeItemDetailId;

    // Start is called before the first frame update
    void Start()
    {
        var root = _uiDocument.rootVisualElement;

        _categoryDetailContainer = root.Q("Category");
        _categoriesContainer = root.Q("CategoryList");
        _itemDetailContainer = root.Q("ItemDetail");

        root.styleSheets.Add(_uiStyleSheet);


        var cubeDistanceSlider = root.Q<Slider>("CubeDistance");
        cubeDistanceSlider.focusable = false;
        cubeDistanceSlider.RegisterCallback<ChangeEvent<float>>((evt) => ItemManager.Instance.ChangeCubeDistance(evt.newValue));

        var addCategoryBtn = root.Q<Button>("AddCategory");
        addCategoryBtn.RegisterCallback<ClickEvent>(delegate { ItemManager.Instance.Categories.AddToList(); });

        var deleteCategoryBtn = root.Q<Button>("DeleteActiveCategory");
        deleteCategoryBtn.RegisterCallback<ClickEvent>(delegate { DeleteActiveCategoryCallback(); });

        var deleteItemBtn = root.Q<Button>("DeleteActiveItem");
        deleteItemBtn.RegisterCallback<ClickEvent>(delegate { DeleteActiveItemCallback(); });

        var addItemBtn = root.Q<Button>("AddItem");
        addItemBtn.RegisterCallback<ClickEvent>(delegate { ItemManager.Instance.Categories.ActiveItem.AddToList(); });

        ItemManager.Instance.Categories.SubscribeToItemAdded((category) => HandleCategoryCreated(category));
        ItemManager.Instance.Categories.SubscribeToItemActivated((categoryId) => HandleCategoryActivated(categoryId));
    }

    // Update is called once per frame
    void Update()
    {

    }

    #region VisualElementsCreation
    private Button CreateListItemVisualEl<T>(T item, Color color, ListManager<T> list) where T : class, IIdentifiable, IDisposable
    {
        var btn = new Button();
        btn.AddToClassList("genericBtn");
        btn.name = item.UiElName;
        btn.text = item.Id.ToString();
        btn.style.color = color;
        btn.RegisterCallback<ClickEvent>(delegate { list.ActivateItem(item.Id); });
        return btn;
    }

    private Color GetGameObjectColor(GameObject gameObj)
    {
        Renderer prefabRenderer = gameObj.GetComponent<Renderer>();

        if (prefabRenderer != null)
        {
            Material prefabMaterial = prefabRenderer.sharedMaterial;
            Color materialColor = prefabMaterial.color;
            return materialColor;

        }
        return Color.black;
    }

    private void CreateCategoryItemContainerVisualEl(string name)
    {
        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Row;
        container.name = name;
        var categoriesList = _categoryDetailContainer.Q<VisualElement>("ListItemsContainer");
        categoriesList.Add(container);
    }
    #endregion


    private void DeleteActiveCategoryCallback()
    {
        try
        {
            var category = ItemManager.Instance.Categories.ActiveItem;
            ItemManager.Instance.RemoveActiveCategory();
            var btn = _categoriesContainer.Q<Button>(category.UiElName);
            var container = _categoryDetailContainer.Q(category.ContainerUiElName);
            btn.RemoveFromHierarchy();
            container.RemoveFromHierarchy();
        }
        catch (InvalidOperationException ex)
        {
            //Debug.LogError(ex.Message);
        }

    }

    private void DeleteActiveItemCallback()
    {
        try
        {
            var item = ItemManager.Instance.Categories.ActiveItem.ActiveItem;
            ItemManager.Instance.RemoveActiveItem();
            var btn = _categoryDetailContainer.Q<Button>(item.UiElName);
            btn.RemoveFromHierarchy();
        }
        catch (InvalidOperationException ex)
        {
            //Debug.LogError(ex.Message);
        }

    }


    #region EventHandlers
    private Task HandleCategoryCreated(Category category)
    {
        category.SubscribeToItemAdded((item) => HandleItemCreated(item));
        category.SubscribeToItemActivated((id) => HandleItemActivated(id));

        CreateCategoryItemContainerVisualEl(category.ContainerUiElName);

        var color = GetGameObjectColor(category.Prefab);
        var btn = CreateListItemVisualEl(category, color, ItemManager.Instance.Categories);

        var categoriesList = _categoriesContainer.Q<VisualElement>("ListItemsContainer");
        categoriesList.Add(btn);

        return Task.CompletedTask;
    }

    private Task HandleItemCreated(ItemDetail itemDetail)
    {
        var color = GetGameObjectColor(itemDetail.Category.Prefab);
        var btn = CreateListItemVisualEl(itemDetail, color, itemDetail.Category);
        var categoryContainer = _categoryDetailContainer.Q<VisualElement>(itemDetail.Category.ContainerUiElName);
        categoryContainer.Add(btn);
        return Task.CompletedTask;
    }

    private void HandleCategoryActivated(Category category)
    {
        if (category.Id == _activeCategoryId)
        {
            var item = _categoriesContainer.Q<Button>(category.UiElName);
            if (!item.ClassListContains("highlighted"))
            {
                item.AddToClassList("highlighted");
            }
            return;
        }
        foreach (var item in _categoryDetailContainer.Q<VisualElement>("ListItemsContainer").Children())
        {
            if (item.name == category.ContainerUiElName)
            {
                item.style.display = DisplayStyle.Flex;
            }
            else
            {
                item.style.display = DisplayStyle.None;
            }
        }
        foreach (var item in _categoriesContainer.Q<VisualElement>("ListItemsContainer").Children())
        {
            if (item.name == category.UiElName)
            {
                item.AddToClassList("highlighted");
            }
            else
            {
                item.RemoveFromClassList("highlighted");
            }
        }
        _activeCategoryId = category.Id;
    }

    private void HandleItemActivated(ItemDetail itemDetail)
    {
        if (itemDetail.Id == _activeItemDetailId)
        {
            var item = _categoryDetailContainer.Q<Button>(itemDetail.UiElName);
            if (!item.ClassListContains("highlighted"))
            {
                item.AddToClassList("highlighted");
            }
            return;
        }
        foreach (var item in _categoryDetailContainer.Q<VisualElement>(itemDetail.Category.ContainerUiElName).Children())
        {
            if (item.name == itemDetail.UiElName)
            {
                item.AddToClassList("highlighted");
            }
            else
            {
                item.RemoveFromClassList("highlighted");
            }
        }
        _activeItemDetailId = itemDetail.Id;

    }
    #endregion

}
