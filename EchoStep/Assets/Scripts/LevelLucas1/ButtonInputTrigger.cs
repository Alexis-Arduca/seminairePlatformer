using UnityEngine;

public class ButtonInputTrigger : MonoBehaviour
{
    public PuzzleManager_Level1 puzzleManager;
    private bool playerInRange;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            puzzleManager.OnButtonPressed();
            Debug.Log("🔘 Bouton activé !");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Appuyez sur E pour activer.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
