using System;
using System.Collections;
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
    private ScrollView _itemsScrollView;

    private VisualElement _scrollTo = null;
    private Label _errors;
    private Button _addItemBtn;

    private int _multiplier = 1;
    private bool _isShowMsgCoroutineRunning = false;

    // Start is called before the first frame update
    void Start()
    {
        var root = _uiDocument.rootVisualElement;

        root.styleSheets.Add(_uiStyleSheet);

        _categoryDetailContainer = root.Q("Category");
        _categoriesContainer = root.Q("CategoryList");
        _itemDetailContainer = root.Q("ItemDetail");
        _errors = root.Q<Label>("Errors");
        _itemsScrollView = _categoryDetailContainer.Q<ScrollView>("ListItemsContainer");

        var cubeDistanceSlider = root.Q<Slider>("CubeDistance");
        cubeDistanceSlider.focusable = false;
        cubeDistanceSlider.RegisterCallback<ChangeEvent<float>>((evt) => ItemManager.Instance.ChangeCubeDistance(evt.newValue));

        var multiplierSlider = root.Q<SliderInt>("Multiplier");
        multiplierSlider.focusable = false;
        multiplierSlider.RegisterCallback<ChangeEvent<int>>((evt) => ChangeMultiplier(evt.newValue));


        var addCategoryBtn = root.Q<Button>("AddCategory");
        addCategoryBtn.RegisterCallback<ClickEvent>(delegate { ItemManager.Instance.Categories.AddToList(); });

        var deleteCategoryBtn = root.Q<Button>("DeleteActiveCategory");
        deleteCategoryBtn.RegisterCallback<ClickEvent>(delegate { DeleteActiveCategoryCallback(); });

        var deleteItemBtn = root.Q<Button>("DeleteActiveItem");
        deleteItemBtn.RegisterCallback<ClickEvent>(delegate { DeleteActiveItemCallback(); });

        _addItemBtn = root.Q<Button>("AddItem");
        _addItemBtn.style.width = StyleKeyword.Auto;
        _addItemBtn.RegisterCallback<ClickEvent>(delegate { AddItemCallback(); });

        ItemManager.Instance.Categories.SubscribeToItemAdded((category) => HandleCategoryCreated(category));
        ItemManager.Instance.Categories.SubscribeToItemActivated((categoryId) => HandleCategoryActivated(categoryId));
    }

    // Update is called once per frame
    void Update()
    {
        if (_scrollTo != null)
        {
            _itemsScrollView.ScrollTo(_scrollTo);
            _scrollTo = null;
        }
    }

    #region VisualElementsManagment
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
        _itemsScrollView.Add(container);
    }

    private IEnumerator ShowMsgCoroutine(string msg)
    {
        _isShowMsgCoroutineRunning = true;
        _errors.text = msg;

        yield return new WaitForSeconds(5f);

        _errors.text = string.Empty;
        _isShowMsgCoroutineRunning = false;
    }

    private void ShowNewMsg(string msg) 
    {
        if (_isShowMsgCoroutineRunning)
        {
            StopAllCoroutines();
        }
        StartCoroutine(ShowMsgCoroutine(msg));
    }

    #endregion

    #region ClickCallbacks
    private void AddItemCallback()
    {
        for (int i = 0; i < _multiplier; i++)
        {
            var activateItem = i == _multiplier - 1;
            ItemManager.Instance.Categories.ActiveItem.AddToList(activateItem).GetAwaiter().GetResult();

        }
    }

    private void ChangeMultiplier(int multiplier)
    {
        _multiplier = multiplier;

        _addItemBtn.text = _multiplier == 1? "Add Item" : $"Add {_multiplier} Items";
    }

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
            ShowNewMsg(ex.Message);
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
            ShowNewMsg(ex.Message);
        }

    }
    #endregion

    #region EventHandlers
    private Task<bool> HandleCategoryCreated(Category category)
    {
        if (category == null)
        {
            ShowNewMsg("Cannot create more categories.");
            return Task.FromResult(false);
        }
        category.SubscribeToItemAdded((item) => HandleItemCreated(item));
        category.SubscribeToItemActivated((id) => HandleItemActivated(id));

        CreateCategoryItemContainerVisualEl(category.ContainerUiElName);

        var color = GetGameObjectColor(category.Prefab);
        var btn = CreateListItemVisualEl(category, color, ItemManager.Instance.Categories);

        var categoriesList = _categoriesContainer.Q<ScrollView>("ListItemsContainer");
        categoriesList.Add(btn);

        return Task.FromResult(true);
    }

    private Task<bool> HandleItemCreated(ItemDetail itemDetail)
    {
        if (itemDetail == null)
        {
            ShowNewMsg("Cannot create more items in category.");
            return Task.FromResult(false);
        }
        var color = GetGameObjectColor(itemDetail.Category.Prefab);
        var btn = CreateListItemVisualEl(itemDetail, color, itemDetail.Category);
        var categoryContainer = _categoryDetailContainer.Q<VisualElement>(itemDetail.Category.ContainerUiElName);
        categoryContainer.Add(btn);
        return Task.FromResult(true);
    }

    private void HandleCategoryActivated(Category category)
    {
        foreach (var item in _itemsScrollView.Children())
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
        foreach (var item in _categoriesContainer.Q<ScrollView>("ListItemsContainer").Children())
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
        Debug.Log($"category {category.Id} showwn");
    }

    private void HandleItemActivated(ItemDetail itemDetail)
    {

        foreach (var item in _categoryDetailContainer.Q<VisualElement>(itemDetail.Category.ContainerUiElName).Children())
        {
            if (item.name == itemDetail.UiElName)
            {
                item.AddToClassList("highlighted");
                if (item.layout.width > 0 && _itemsScrollView.layout.width > 0)
                {
                    _itemsScrollView.ScrollTo(item);
                }
                else
                {
                    _scrollTo = item;
                }
            }
            else
            {
                item.RemoveFromClassList("highlighted");
            }
        }
    }
    #endregion
}
