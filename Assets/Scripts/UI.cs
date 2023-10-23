using System.ComponentModel;
using System.Linq;
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

    // Start is called before the first frame update
    void Start()
    {
        var root = _uiDocument.rootVisualElement;

        _categoryDetailContainer = root.Q("Category");
        _categoriesContainer = root.Q("CategoryList");
        _itemDetailContainer = root.Q("ItemDetail");

        root.styleSheets.Add(_uiStyleSheet);

        var addCategoryBtn = root.Q<Button>("AddCategory");
        addCategoryBtn.RegisterCallback<ClickEvent>(delegate { ItemManager.Instance.Categories.AddToList(); });

        var deleteCategoryBtn = root.Q<Button>("DeleteActiveCategory");
        addCategoryBtn.RegisterCallback<ClickEvent>(delegate { DeleteActiveCategoryCallback(); });

        var addItemBtn = root.Q<Button>("AddItem");
        addCategoryBtn.RegisterCallback<ClickEvent>(delegate { ItemManager.Instance.Categories.ActiveItem.AddToList(); });

        ItemManager.Instance.Categories.SubscribeToItemAdded((category) => HandleCategoryCreated(category));
        ItemManager.Instance.Categories.SubscribeToItemActivated((categoryId) => HandleCategoryActivated(categoryId));
    }

    // Update is called once per frame
    void Update()
    {

    }

    private Button CreateListItemVisualEl(int id, Color color)
    {
        var btn = new Button();
        btn.AddToClassList("genericBtn");
        btn.name = id.ToString();
        btn.text= id.ToString();
        btn.style.color = color;
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

    private void CreateCategoryItemContainerVisualEl(int id)
    {
        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Row;
        container.name = id.ToString();
        var categoriesList = _categoryDetailContainer.Q<VisualElement>("ListItemsContainer");
        categoriesList.Add(container);
    }

    private void DeleteActiveCategoryCallback()
    {

    }


    #region EventHandlers
    private Task HandleCategoryCreated(Category category)
    {
        category.SubscribeToItemAdded((item) => HandleItemCreated(item));
        category.SubscribeToItemActivated((id) => HandleItemActivated(id));

        CreateCategoryItemContainerVisualEl(category.Id);
        var color = GetGameObjectColor(category.Prefab);
        var btn = CreateListItemVisualEl(category.Id, color);
        var categoriesList = _categoriesContainer.Q<VisualElement>("ListItemsContainer");
        categoriesList.Add(btn);
        return Task.CompletedTask;
    }

    private Task HandleItemCreated(ItemDetail itemDetail)
    {
        var color = GetGameObjectColor(itemDetail.Category.Prefab);
        var btn = CreateListItemVisualEl(itemDetail.Id, color);
        var categoryList = _categoryDetailContainer.Q<VisualElement>("ListItemsContainer").Q(itemDetail.Category.Id.ToString());
        categoryList.Add(btn);
        return Task.CompletedTask;
    }

    private void HandleCategoryActivated(int categoryId)
    {
    }

    private void HandleItemActivated(int itemDetailId)
    {

    }
    #endregion

}
