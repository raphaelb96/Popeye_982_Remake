// GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int popeyeHearts = 0;
    public int popeyeHP = 3;
    private const int MAX_HEARTS = 24;

    public static event Action OnGameStart;
    public static event Action OnDamageTaken;
    public static event Action<bool> OnGameOver; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(StartRoundRoutine());
    }

    private IEnumerator StartRoundRoutine()
    {
        yield return new WaitForSeconds(3f); 
        OnGameStart?.Invoke();
    }

    public void AddHeart()
    {
        popeyeHearts++;
        if (popeyeHearts >= MAX_HEARTS) EndGame(true);
    }

    public void TakeDamage()
    {
        popeyeHP--;
        OnDamageTaken?.Invoke(); 
        if (popeyeHP <= 0) EndGame(false);
    }

    private void EndGame(bool popeyeWins)
    {
        Time.timeScale = 0;
        OnGameOver?.Invoke(popeyeWins);
        StartCoroutine(RestartRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        yield return new WaitUntil(() => InputManager.Instance.UIConfirmDown);
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}