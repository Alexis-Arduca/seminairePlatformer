using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;
    private bool pauseState = false;
    private float fixedDeltaTime;

    void Start()
    {
        pausePanel.SetActive(pauseState);

        GameEventsManager.instance.pauseEvents.onPauseButtonPressed += GamePauseState;
    }

    void Awake()
    {
        this.fixedDeltaTime = Time.fixedDeltaTime;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.pauseEvents.onPauseButtonPressed -= GamePauseState;
    }

    private void GamePauseState()
    {
        pauseState = !pauseState;
        pausePanel.SetActive(pauseState);

        if (pauseState == true)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Time.timeScale = 0f;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1.0f;
        }
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        pauseState = false;

        Time.timeScale = 1.0f;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public bool GetOnPause()
    {
        return pauseState;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
