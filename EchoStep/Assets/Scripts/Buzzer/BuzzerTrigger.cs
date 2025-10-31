using UnityEngine;

public class BuzzerTrigger : MonoBehaviour
{
    [Header("Références")]
    public PuzzleManager_Level1 puzzleManager;  // Le manager du puzzle
    public Buzzer buzzerScript;                 // Optionnel : pour animation ou son du buzzer
    public KeyCode activationKey = KeyCode.F;   // Touche d’activation

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(activationKey))
        {
            Debug.Log("🔘 Bouton activé avec F !");

            // Lance l'animation visuelle du buzzer (si présente)
            if (buzzerScript != null)
                buzzerScript.PressOnce();

            // Informe le puzzle manager
            if (puzzleManager != null)
                puzzleManager.OnButtonPressed();
            else
                Debug.LogWarning("⚠️ Aucun PuzzleManager assigné !");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("➡️ Le joueur peut appuyer sur F pour activer le buzzer.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("⬅️ Le joueur s’éloigne du buzzer.");
        }
    }
}
