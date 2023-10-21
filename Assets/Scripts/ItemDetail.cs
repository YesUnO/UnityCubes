using System;
using UnityEngine;

public class ItemDetail : IIdentifiable, IDisposable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Vector3 OriginalPosition { get; set; }
    public Vector3 ChangedPosition { get; set; }
    public GameObject ItemObject { get; set; }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
