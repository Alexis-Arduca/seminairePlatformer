using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    public TMPro.TMP_Text textComponent;
    public float typingSpeed = 0.05f;
    public float moveDuration = 2f;
    public float moveDistance = 50f;

    // private string currentText = "";

    public IEnumerator ShowText(string msg)
    {
        if (textComponent == null)
            yield break;

        textComponent.text = "";
        for (int i = 0; i <= msg.Length; i++)
        {
            textComponent.text = msg.Substring(0, i);
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(1);
        textComponent.text = "";
    }

    // IEnumerator MoveTextUp()
    // {
    //     Vector3 originalPosition = textComponent.rectTransform.anchoredPosition;
    //     Vector3 targetPosition = originalPosition + new Vector3(0, moveDistance, 0);
    //     float elapsedTime = 0f;

    //     while (elapsedTime < moveDuration)
    //     {
    //         textComponent.rectTransform.anchoredPosition = Vector3.Lerp(originalPosition, targetPosition, elapsedTime / moveDuration);
    //         elapsedTime += Time.deltaTime;
    //         yield return null;
    //     }

    //     textComponent.rectTransform.anchoredPosition = targetPosition;
    // }
}
