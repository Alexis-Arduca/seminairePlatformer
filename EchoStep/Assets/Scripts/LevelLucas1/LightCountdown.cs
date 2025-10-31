using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightCountdown : MonoBehaviour
{
    public List<MeshRenderer> bulbs = new List<MeshRenderer>();

    public void StartCountdown(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(CountdownRoutine(duration));
    }

    private IEnumerator CountdownRoutine(float duration)
    {
        foreach (var bulb in bulbs)
            bulb.material.color = Color.yellow;

        float interval = duration / bulbs.Count;
        for (int i = bulbs.Count - 1; i >= 0; i--)
        {
            yield return new WaitForSeconds(interval);
            bulbs[i].material.color = Color.black;
        }
    }

    public void ResetLights()
    {
        StopAllCoroutines();
        foreach (var bulb in bulbs)
            bulb.material.color = Color.black;
    }
}
