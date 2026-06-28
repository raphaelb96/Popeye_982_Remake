// GameManager.cs
// Central game state authority. Tracks Popeye's heart count and HP,
// fires global events that all other systems react to, and handles
// the end-of-game flow (freeze time → show result → wait for input → reload scene).
//
// Win conditions:
//   Popeye wins → collects all hearts (popeyeHearts >= heartsToWin)
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

    // The win threshold — Popeye must collect this many hearts to win.
    // Public field so the target can be tuned from the Inspector without touching code.
    [Header("Win Condition")]
    [Tooltip("Number of hearts Popeye must collect to win the round")]
    public int heartsToWin = 24;

    // Guards against starting the round twice (e.g. Play button clicked after Enter was already pressed)
    private bool roundStarted = false;

    // Set true once the round has started for the first time. Static = survives scene reloads,
    // so Restart replays immediately instead of bouncing back to the start menu.
    public static bool skipStartMenu = false;

    // ─── STATIC EVENTS ───────────────────────────────────────────────────────
    // These events are the backbone of the event-driven architecture.
    // No script polls GameManager every frame — they react only when something changes.

    // Fired once the player presses the confirm button to start the round — signals all controllers to start accepting input
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
        // Restart (flag set last life) → begin immediately. Fresh launch → wait for the menu.
        if (skipStartMenu) StartGame();
        else StartCoroutine(StartRoundRoutine());
    }

    // ─── COROUTINES ──────────────────────────────────────────────────────────

    // Waits for the player to press the confirm button (Space/Enter), then starts the round —
    // the Play button on the start menu can also trigger this directly via StartGame()
    private IEnumerator StartRoundRoutine()
    {
        yield return new WaitUntil(() => InputManager.Instance.UIConfirmDown);
        StartGame();
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

    // Called by the Play button on the start menu (or by pressing Enter/Space) to begin the round
    public void StartGame()
    {
        if (roundStarted) return;
        roundStarted = true;
        skipStartMenu = true; // Menu passed — future restarts skip straight to gameplay
        OnGameStart?.Invoke(); // The ?. (null-conditional) prevents crash if nobody is subscribed
    }

    // Called by HeartItem when Popeye collects a heart
    public void AddHeart()
    {
        popeyeHearts++;
        // Check win condition immediately after incrementing
        if (popeyeHearts >= heartsToWin) EndGame(true); // Popeye wins
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
