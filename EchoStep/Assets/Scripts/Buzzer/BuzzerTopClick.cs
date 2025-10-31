using UnityEngine;

public class BuzzerTopClick : MonoBehaviour
{
    public Buzzer parentBuzzer;

    void OnMouseDown()
    {
        Debug.Log("Clic sur le buzzer !");
        if (parentBuzzer != null)
            parentBuzzer.PressOnce();
        else
            Debug.LogWarning("Parent Buzzer non assigné sur " + gameObject.name);
    }
}
