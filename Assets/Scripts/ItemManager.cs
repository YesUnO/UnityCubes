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
        Categories.SubscribeToItemAdded((category)=>HandleCategoryAdded(category));
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
        category.AddToList();
    }

    private void HandleItemAdded(ItemDetail itemDetail)
    {

    }
}
