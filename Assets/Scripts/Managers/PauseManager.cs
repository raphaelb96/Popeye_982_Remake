// PauseManager.cs
// Handles game pausing: freezes time, shows the pause panel, and resumes on second press.
// Pausing is only allowed after the game has officially started (after the 3s countdown).
// Known issue: the lambda used in OnEnable/OnDisable creates a new delegate each time,
// so the -= in OnDisable won't match the += from OnEnable → canPause is never reset to false.
// This is a minor memory-leak risk to fix later. Functionally it works during a single session.
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    // Drag the PausePanel UI GameObject here in the Inspector
    public GameObject pausePanel;

    // Tracks whether the game is currently paused
    private bool isPaused = false;

    // Gate that prevents pausing during the 3-second startup countdown
    // Becomes true when GameManager.OnGameStart fires
    private bool canPause = false;

    // Subscribe to OnGameStart using a lambda — sets canPause to true when the round begins
    // NOTE: lambdas cannot be unsubscribed with -= (known bug, see class comment above)
    private void OnEnable()  => GameManager.OnGameStart += () => canPause = true;
    private void OnDisable() => GameManager.OnGameStart -= () => canPause = false; // Does not work as intended

    // Hide the pause panel on scene load before anything starts
    private void Start() => pausePanel.SetActive(false);

    private void Update()
    {
        // Do nothing if the round hasn't started yet
        if (!canPause) return;

        // Toggle pause on every Escape/P key press
        if (InputManager.Instance.PauseDown)
        {
            if (isPaused) ResumeGame(); // Second press = resume
            else          PauseGame();  // First press = pause
        }
    }

    // ─── PUBLIC METHODS (also callable from UI buttons in the pause panel) ───

    // Freeze time and show the pause overlay
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0;           // Setting timeScale to 0 stops all physics and Update() calls
        pausePanel.SetActive(true);   // Show the pause UI panel
    }

    // Unfreeze time and hide the pause overlay
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1;           // Restore normal time flow
        pausePanel.SetActive(false);  // Hide the pause UI panel
    }

    // Exit the application — works in builds, does nothing in the Editor
    public void QuitGame() => Application.Quit();
}
