using System;
using UnityEngine;

public class PauseEvents
{
    public event Action onPauseButtonPressed;
    public void OnPauseButtonPressed()
    {
        if (onPauseButtonPressed != null)
        {
            onPauseButtonPressed();
        }
    }
}
