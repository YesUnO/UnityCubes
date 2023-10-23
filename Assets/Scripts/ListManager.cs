using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ListManager<T> : IDisposable where T : IIdentifiable, IDisposable
{
    //properties
    public List<T> Items { get; set; } = new();
    public int NextAvailableId { get; set; }
    public T ActiveItem { get; set; }

    //events
    public Func<T, Task> ItemAdded { get; set; }
    private List<Func<T,  Task>> _itemAddedSubscribers = new();
    public void SubscribeToItemAdded(Func<T, Task> itemAdded, int? priority = null)
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
    public async Task PublishItemAdded(T item)
    {
        foreach (var subscriber in _itemAddedSubscribers)
        {
            await subscriber.Invoke(item);
        }
    }


    public Action<int> ItemActivated { get; set; }
    private List<Action<int>> _itemActivatedSubscribers = new();
    public void SubscribeToItemActivated(Action<int> itemActivated, int? priority = null)
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
    public void PublishItemActivated(int itemId)
    {
        foreach (var subscriber in _itemActivatedSubscribers)
        {
            subscriber.Invoke(itemId);
        }
    }


    //methods
    public async Task AddToList(T item)
    {
        item.Id = item.Id == -1 ? NextAvailableId++ : CategoryList.GlobalItemId++;
        item.Name = item.Id.ToString();
        Items.Add(item);
        await PublishItemAdded(item);
    }
    public void RemoveActiveFromList()
    {

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
    }
}

public interface IIdentifiable
{
    int Id { get; set; }
    string Name { get; set; }
}
