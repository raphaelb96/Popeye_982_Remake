// PauseManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;
    private bool isPaused = false;
    private bool canPause = false;

    private void OnEnable() => GameManager.OnGameStart += () => canPause = true;
    private void OnDisable() => GameManager.OnGameStart -= () => canPause = false;

    private void Start() => pausePanel.SetActive(false);

    private void Update()
    {
        if (!canPause) return;

        if (InputManager.Instance.PauseDown)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0;
        pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1;
        pausePanel.SetActive(false);
    }

    public void QuitGame() => Application.Quit();
}