using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public abstract class ListManager<T> : IDisposable where T : class, IIdentifiable, IDisposable
{
    //properties
    public List<T> Items { get; set; } = new();
    public int NextAvailableId { get; set; }
    public T ActiveItem { get; set; }

    //events
    public Func<T, Task<bool>> ItemAdded { get; set; }
    private List<Func<T, Task<bool>>> _itemAddedSubscribers = new();
    public void SubscribeToItemAdded(Func<T, Task<bool>> itemAdded, int? priority = null)
    {
        var count = _itemAddedSubscribers.Count;
        if (priority != null && count > 0 && count >= priority)
        {
            _itemAddedSubscribers.Insert((int)priority, itemAdded);
        }
        else
        {
            _itemAddedSubscribers.Add(itemAdded);
        }
        ItemAdded += itemAdded;
    }
    public async Task<bool> PublishItemAdded(T item)
    {
        var res = true;
        foreach (var subscriber in _itemAddedSubscribers)
        {
            var subRes = await subscriber.Invoke(item);
            if (!subRes)
            {
                item = null;
            }
            res &= subRes;
        }
        return res;
    }


    public Action<T> ItemActivated { get; set; }
    private List<Action<T>> _itemActivatedSubscribers = new();
    public void SubscribeToItemActivated(Action<T> itemActivated, int? priority = null)
    {
        var count = _itemAddedSubscribers.Count;
        if (priority != null && count > 0 && count >= priority)
        {
            _itemActivatedSubscribers.Insert((int)priority, itemActivated);
        }
        else
        {
            _itemActivatedSubscribers.Add(itemActivated);
        }
        ItemActivated += itemActivated;
    }
    public void PublishItemActivated(T item)
    {
        foreach (var subscriber in _itemActivatedSubscribers)
        {
            subscriber.Invoke(item);
        }
    }


    //methods
    public async Task AddToList(T item, bool activate = true)
    {
        item.Id = item.Id == -1 ? CategoryList.GlobalItemId++ : NextAvailableId++;
        item.Name = item.Id.ToString();
        Items.Add(item);
        var itemAdded = await PublishItemAdded(item);
        if (itemAdded && activate)
        {
            ActivateItem(item);
        }
    }

    public void ActivateItem(int id)
    {
        ActivateItem(Items.FirstOrDefault(x => x.Id == id));
    }
    private void ActivateItem(T item = null)
    {
        if (item == null)
        {
            ActiveItem = Items.FirstOrDefault();
        }
        if (item == ActiveItem)
        {
            //TODO: not yet
            //return;
        }
        else if (Items.Contains(item))
        {
            ActiveItem = item;
        }
        if (ActiveItem != null)
        {
            PublishItemActivated(ActiveItem);
            if (ActiveItem is Category category)
            {
                category.ActivateSubsequent();
            }
            if (ActiveItem is ItemDetail itemDetail)
            {
                itemDetail.Category.CategoryList.ActivateItem(itemDetail.Category);
            }
        }
    }

    private void ActivateSubsequent(T item = null)
    {
        if (item != null)
        {
            ActiveItem = item;
        }
        if (ActiveItem != null)
        {
            PublishItemActivated(ActiveItem);
        }
    }

    public void RemoveActiveFromList()
    {
        if (Items.Count <= 1 || ActiveItem == null)
        {
            throw new InvalidOperationException("Cant remove last item.");
        }
        RemoveFromList(ActiveItem);
        ActivateItem();
    }

    public void RemoveFromList(T item)
    {
        var index = Items.IndexOf(item);
        item.Dispose();
        Items.RemoveAt(index);
    }

    //Dispose
    private void ClearSubscriptions()
    {
        foreach (var subscriber in _itemAddedSubscribers)
        {
            ItemAdded -= subscriber;
        }
        _itemAddedSubscribers.Clear();
        foreach (var subscriber in _itemActivatedSubscribers)
        {
            ItemActivated -= subscriber;
        }
        _itemActivatedSubscribers.Clear();
    }
    public void Dispose()
    {
        ClearSubscriptions();
        foreach (var item in Items)
        {
            item.Dispose();
        }
        Items.Clear();
    }
}

public interface IIdentifiable
{
    int Id { get; set; }
    string Name { get; set; }
    string UiElName { get; }
}
