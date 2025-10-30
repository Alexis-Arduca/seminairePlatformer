using System;
using UnityEngine;

public class CollectibleEvents
{
    public event Action<int> onCollectibleGet;
    public void OnCollectibleGet(int id)
    {
        if (onCollectibleGet != null)
        {
            onCollectibleGet(id);
        }
    }
}
