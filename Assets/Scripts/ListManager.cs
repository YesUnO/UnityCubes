using System;
using System.Collections.Generic;

public class ListManager<T> : IDisposable where T : IIdentifiable, IDisposable
{
    //properties
    public List<T> Items { get; set; } = new();
    public int NextAvailableId { get; set; }
    public int ActiveId { get; set; }

    //events
    public Action<T> ItemAdded { get; set; }
    private List<Action<T>> _itemAddedSubscribers = new();
    public void SubscribeToItemAdded(Action<T> itemAdded, int? priority = null)
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
    public void PublishItemAdded(T item)
    {
        foreach (var subscriber in _itemAddedSubscribers)
        {
            subscriber.Invoke(item);
        }
    }


    public Action<int> ItemActivated { get; set; }
    private List<Action<int>> _itemActivatedSubscribers = new();



    //methods
    public void AddToList(T item)
    {
        item.Id = item.Id == -1 ? NextAvailableId++ : CategoryList.GlobalItemId++;
        item.Name = item.Id.ToString();
        Items.Add(item);
        PublishItemAdded(item);
    }
    public void RemoveActiveFromList()
    {

    }

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

    
    //Dispose
    public void Dispose()
    {
        ClearSubscriptions();
        foreach (var item in Items)
        {
            item.Dispose();
        }
    }
}

public interface IIdentifiable
{
    int Id { get; set; }
    string Name { get; set; }
}
