using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GifSpritePlayer : MonoBehaviour
{
    private Coroutine playRoutine;

    [Header("Sprite Frames")]
    public RawImage targetImage;
    public float frameRate = 0.1f;
    public bool loop = false;

    void Start()
    {
        GameEventsManager.instance.playerEvents.onPlayerActiveRecord += PlayGifSprite;
    }
    
    private void OnDisable()
    {
        GameEventsManager.instance.playerEvents.onPlayerActiveRecord -= PlayGifSprite;
    }

    /// <summary>
    /// Play animation for a specific sprite.
    /// </summary>
    /// <param name="frameRate">Time for each frame</param>
    /// <param name="loop">If true, loop</param>
    public void PlayGifSprite(List<Texture> frames)
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine = StartCoroutine(PlayRoutine(frames));
    }

    /// <summary>
    /// Stop the current GIF.
    /// </summary>
    public void StopGif()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }
    }

    private IEnumerator PlayRoutine(List<Texture> frames)
    {
        int index = 0;

        do
        {
            targetImage.texture = frames[index];
            index = (index + 1) % frames.Count;

            yield return new WaitForSeconds(frameRate);
        }
        while (loop || index < frames.Count - 1);

        playRoutine = null;
    }
}
