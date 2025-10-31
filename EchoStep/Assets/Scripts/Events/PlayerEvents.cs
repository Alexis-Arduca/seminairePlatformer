using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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

    public event Action<List<Texture>> onPlayerActiveRecord;
    public void OnPlayerActiveRecord(List<Texture> frames)
    {
        if (onPlayerActiveRecord != null)
        {
            onPlayerActiveRecord(frames);
        }
    }
}
