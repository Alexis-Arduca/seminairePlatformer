using System;
using UnityEngine;
using TMPro;

public class MessagePopup : MonoBehaviour
{
    [Header("Message Configuration")]
    public string popUpMessage;
    public TMP_Text messagePopUp;
    private GameObject objectPopUp;

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (objectPopUp != null)
            {
                messagePopUp.text = popUpMessage;
            }
            else
            {
                messagePopUp.text = "";
            }
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (objectPopUp != null)
            {
                messagePopUp.text = "";
            }
        }
    }
}