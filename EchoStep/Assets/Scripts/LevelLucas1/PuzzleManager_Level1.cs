using UnityEngine;
using System.Collections;

public class PuzzleManager_Level1 : MonoBehaviour
{
    [Header("Paramètres du puzzle")]
    public GameObject door;
    public LightCountdown lightCountdown;
    public float timeLimit = 4f;

    private bool plate1Pressed;
    private bool plate2Pressed;
    private bool buttonPressed;
    private bool timerRunning;
    private bool puzzleCompleted; // ✅ Nouveau flag pour empêcher les relancements

    private void Start()
    {
        ResetPuzzle();
    }

    public void OnPlate1Pressed()
    {
        if (puzzleCompleted) return; // ✅ Ignore si déjà réussi
        plate1Pressed = true;
        TryStartTimer();
    }

    public void OnPlate2Pressed()
    {
        if (puzzleCompleted) return;
        plate2Pressed = true;
        TryStartTimer();
    }

    public void OnButtonPressed()
    {
        if (puzzleCompleted) return;
        buttonPressed = true;
        TryStartTimer();
    }

    private void TryStartTimer()
    {
        if (puzzleCompleted) return;

        if (!timerRunning)
        {
            timerRunning = true;
            StartCoroutine(TimerRoutine());
            if (lightCountdown)
                lightCountdown.StartCountdown(timeLimit);
        }

        CheckCompletion();
    }

    private void CheckCompletion()
    {
        if (plate1Pressed && plate2Pressed && buttonPressed)
        {
            Debug.Log("✅ Puzzle réussi !");
            puzzleCompleted = true; // ✅ Marque le puzzle comme terminé
            StopAllCoroutines();
            lightCountdown?.ResetLights();
            OpenDoor();
        }
    }

    private IEnumerator TimerRoutine()
    {
        float timer = timeLimit;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        if (!puzzleCompleted)
        {
            Debug.Log("⏱ Temps écoulé !");
            ResetPuzzle();
        }
    }

    private void ResetPuzzle()
    {
        plate1Pressed = plate2Pressed = buttonPressed = false;
        timerRunning = false;
        if (lightCountdown != null)
            lightCountdown.ResetLights();
    }

    private void OpenDoor()
    {
        if (door == null) return;

        Debug.Log("🚪 Ouverture de la porte !");
        StartCoroutine(OpenDoorAnim());
    }

    private IEnumerator OpenDoorAnim()
    {
        // ✅ Seule la porte mobile doit bouger (pas le cadre)
        Vector3 start = door.transform.position;
        Vector3 end = start + Vector3.up * 3f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime;
            door.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
    }
}
