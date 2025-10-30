using System;
using UnityEngine;

public class PlayerEvents
{
    public event Action onPlayerActiveEcho;
    public void OnPlayerActiveEcho()
    {
        if (onPlayerActiveEcho != null)
        {
            onPlayerActiveEcho();
        }
    }
}
