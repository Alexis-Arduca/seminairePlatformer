using UnityEngine;
using System.Collections;

public class Buzzer : MonoBehaviour
{
    public Transform buttonTop;        // la partie mobile
    public float pressDepth = 0.1f;    // profondeur d’enfoncement
    public float pressDuration = 0.2f; // durée avant relâchement
    public AudioSource clickSound;     // optionnel

    private Vector3 startPos;
    private bool isPressed = false;

    void Start()
    {
        if (buttonTop == null)
            Debug.LogError("Assigne le ButtonTop dans l’inspector !");
        startPos = buttonTop.localPosition;
    }

    public void PressOnce()
    {
        if (!isPressed)
            StartCoroutine(PressBuzzer());
    }

    private IEnumerator PressBuzzer()
    {
        isPressed = true;

        // Descendre le dôme
        buttonTop.localPosition = startPos - new Vector3(0, pressDepth, 0);
        buttonTop.GetComponent<Renderer>().material.color = Color.gray; // feedback visuel
        if (clickSound != null) clickSound.Play();

        yield return new WaitForSeconds(pressDuration);

        // Remonter
        buttonTop.localPosition = startPos;
        buttonTop.GetComponent<Renderer>().material.color = Color.red;

        isPressed = false;
    }
}
