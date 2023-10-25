using System;
using UnityEngine;

public class ItemDetail : IIdentifiable, IDisposable
{
    public int Id { get; set; } = -1;
    public string Name { get; set; }
    public Vector3 OriginalPosition { get; set; }
    public Vector3 ChangedPosition { get; set; }
    public GameObject ItemObject { get; set; }
    public Category Category { get; set; }
    public string UiElName { get { return $"ItemDetail#{Id}"; } }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(ItemObject);
    }
}
