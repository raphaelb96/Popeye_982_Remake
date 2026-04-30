// GameManager.cs
// Central game state authority. Tracks Popeye's heart count and HP,
// fires global events that all other systems react to, and handles
// the end-of-game flow (freeze time → show result → wait for input → reload scene).
//
// Win conditions:
//   Popeye wins → collects 24 hearts (popeyeHearts >= MAX_HEARTS)
//   Bluto wins  → Popeye reaches 0 HP (popeyeHP <= 0)
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // ─── SINGLETON ───────────────────────────────────────────────────────────
    // Static reference so any script can call GameManager.Instance.AddHeart() etc.
    // without needing a direct Inspector reference
    public static GameManager Instance { get; private set; }

    // ─── GAME STATE ──────────────────────────────────────────────────────────
    // Number of hearts Popeye has collected this round (starts at 0, win at 24)
    public int popeyeHearts = 0;

    // Number of lives Popeye has remaining (starts at 3, lose at 0)
    public int popeyeHP = 3;

    // The win threshold — Popeye must collect exactly this many hearts to win
    private const int MAX_HEARTS = 24;

    // ─── STATIC EVENTS ───────────────────────────────────────────────────────
    // These events are the backbone of the event-driven architecture.
    // No script polls GameManager every frame — they react only when something changes.

    // Fired 3 seconds after scene load — signals all controllers to start accepting input
    public static event Action OnGameStart;

    // Fired every time Popeye takes damage — UIManager listens to refresh the HP display
    public static event Action OnDamageTaken;

    // Fired when the game ends — bool parameter: true = Popeye wins, false = Bluto wins
    // UIManager listens to show the correct victory/defeat message
    public static event Action<bool> OnGameOver;

    // ─── UNITY LIFECYCLE ─────────────────────────────────────────────────────

    private void Awake()
    {
        // Singleton enforcement: if another GameManager already exists, destroy this duplicate
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Delay the game start by 3 seconds to give players time to read the screen
        StartCoroutine(StartRoundRoutine());
    }

    // ─── COROUTINES ──────────────────────────────────────────────────────────

    // Waits 3 seconds then fires OnGameStart — all player controllers and NPCs
    // subscribe to this event to know when they're allowed to move
    private IEnumerator StartRoundRoutine()
    {
        yield return new WaitForSeconds(3f);
        OnGameStart?.Invoke(); // The ?. (null-conditional) prevents crash if nobody is subscribed
    }

    // Waits for the player to press the confirm button (Space/Enter), then reloads the scene
    // yield return new WaitUntil() pauses the coroutine without blocking the main thread
    private IEnumerator RestartRoutine()
    {
        yield return new WaitUntil(() => InputManager.Instance.UIConfirmDown);
        Time.timeScale = 1;   // Unfreeze time before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reload current scene
    }

    // ─── PUBLIC API ──────────────────────────────────────────────────────────

    // Called by HeartItem when Popeye collects a heart
    public void AddHeart()
    {
        popeyeHearts++;
        // Check win condition immediately after incrementing
        if (popeyeHearts >= MAX_HEARTS) EndGame(true); // Popeye wins
    }

    // Called by MeleeHitbox (Bluto's punch) and by BlutoController via SeaHagProjectile
    public void TakeDamage()
    {
        popeyeHP--;
        OnDamageTaken?.Invoke(); // Notify UIManager to refresh the HP display
        if (popeyeHP <= 0) EndGame(false); // Bluto wins
    }

    // ─── PRIVATE LOGIC ───────────────────────────────────────────────────────

    // Freezes the game, fires the game-over event, then starts the restart wait loop
    private void EndGame(bool popeyeWins)
    {
        Time.timeScale = 0;            // Pause all physics and Update() calls
        OnGameOver?.Invoke(popeyeWins); // UIManager shows the winner screen
        StartCoroutine(RestartRoutine()); // Wait for input to restart
    }
}
