using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TMP_Text))]
public class TextFade : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float visibleDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private TMP_Text tmpText;
    private Coroutine currentRoutine;

    private void Awake()
    {
        tmpText = GetComponent<TMP_Text>();

        Color c = tmpText.color;
        c.a = 0f;
        tmpText.color = c;
    }

    /// <summary>
    /// Show the text in a fade animation
    /// </summary>
    /// <param name="message">Text to Display</param>
    public void PlayFade(string message)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(FadeRoutine(message));
    }

    private IEnumerator FadeRoutine(string message)
    {
        tmpText.text = message;

        yield return StartCoroutine(FadeAlpha(0f, 1f, fadeInDuration));

        yield return new WaitForSeconds(visibleDuration);

        yield return StartCoroutine(FadeAlpha(1f, 0f, fadeOutDuration));

        currentRoutine = null;
    }

    private IEnumerator FadeAlpha(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color c = tmpText.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            tmpText.color = c;
            yield return null;
        }

        c.a = endAlpha;
        tmpText.color = c;
    }
}
