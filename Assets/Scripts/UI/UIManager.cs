// UIManager.cs
// Manages all HUD elements and the game-over screen.
// Listens to game events and refreshes UI text whenever relevant state changes.
// Currently uses TextMeshPro text fields — these will be replaced with icon-based UI
// in the next sprint (bottle icons for Bluto, heart icons for Popeye's HP).
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{

    // ─── INSPECTOR REFERENCES ────────────────────────────────────────────────
    [Header("Bottle Icons")]
    // Array of 5 bottle icon GameObjects — shown/hidden based on Bluto's current bottle count
    public GameObject[] bottleIcons;

    [Header("Heart Icons")]
    // Array of 3 heart icon GameObjects — shown/hidden based on Popeye's remaining HP
    public GameObject[] heartIcons;

    [Header("Victory Panels")]
    // Activated at game end — only the winner's panel is shown
    public GameObject popeyeVictoryPanel;
    public GameObject blutoVictoryPanel;

    [Header("UI Elements")]
    

    // Displays "Hearts: X / 24" — Popeye's heart collection progress
    public TextMeshProUGUI heartsText;

    // Displays "Popeye HP: X" — Popeye's remaining lives
    public TextMeshProUGUI popeyeHpText;

    // Displays "Bottles: X" — Bluto's current bottle inventory count
    public TextMeshProUGUI blutoBottlesText;

    // The panel that appears when the game ends (contains the win/lose message)
    public GameObject gameOverPanel;

    // The text inside the game-over panel — updated with winner name and restart prompt
    public TextMeshProUGUI gameOverMessage;

    // Direct references needed to read current values (GameManager for HP/hearts, BlutoController for bottles)
    public GameManager gameManager;
    public BlutoController bluto;

    // ─── EVENT SUBSCRIPTIONS ─────────────────────────────────────────────────

    private void OnEnable()
    {
        // Refresh the HUD whenever any of these game events fire
        HeartItem.OnHeartCollected        += UpdateHUD; // Popeye collected a heart
        BlutoController.OnThrowBottle += UpdateHUD; // Bluto threw a bottle (count decreased)
        BlutoController.OnBottleCountChanged += UpdateHUD;
        GameManager.OnDamageTaken         += UpdateHUD; // Popeye took damage (HP changed)
        GameManager.OnGameStart += UpdateHUD; // Round started — show initial values

        // Show the game-over screen when the game ends
        GameManager.OnGameOver            += ShowGameOverScreen;
    }

    private void OnDisable()
    {
        // Mirror unsubscriptions — prevents ghost callbacks if this object is disabled
        HeartItem.OnHeartCollected        -= UpdateHUD;
        BlutoController.OnThrowBottle -= UpdateHUD;
        BlutoController.OnBottleCountChanged -= UpdateHUD;
        GameManager.OnDamageTaken         -= UpdateHUD;
        GameManager.OnGameStart           -= UpdateHUD;
        GameManager.OnGameOver            -= ShowGameOverScreen;
    }

    // ─── UNITY LIFECYCLE ─────────────────────────────────────────────────────

    private void Start()
    {
        gameOverPanel.SetActive(false); // Hide the game-over panel at scene start
        UpdateHUD();                    // Populate HUD with initial values before the round starts
    }

    // ─── HUD UPDATE ──────────────────────────────────────────────────────────

    // Refreshes all three text fields with current values from GameManager and BlutoController
    // Called by multiple events — uses null checks to avoid errors if references aren't set
    public void UpdateHUD()
    {
        // Show heart collection progress using a C# string interpolation ($"..." syntax)
        if (heartsText != null)
            heartsText.text = $"Hearts: {gameManager.popeyeHearts} / 24";

        // Show or hide each heart icon based on Popeye's remaining HP
        // Example: if popeyeHP = 2, icons 0/1 are shown, icon 2 is hidden
        if (heartIcons != null)
        {
            for (int i = 0; i < heartIcons.Length; i++)
            {
                heartIcons[i].SetActive(i < gameManager.popeyeHP);
            }
        }
        // Show or hide each bottle icon based on Bluto's current inventory count
        // Example: if currentBottles = 3, icons 0/1/2 are shown, icons 3/4 are hidden
        if (bluto != null && bottleIcons != null)
        {
            for (int i = 0; i < bottleIcons.Length; i++)
            {
                bottleIcons[i].SetActive(i < bluto.currentBottles);
            }
        }
    }

    // ─── GAME OVER SCREEN ────────────────────────────────────────────────────

    // Called by GameManager.OnGameOver(bool popeyeWins) when either win condition is met
    // popeyeWins: true = Popeye collected 24 hearts, false = Bluto reduced Popeye to 0 HP
    // Called by GameManager.OnGameOver(bool popeyeWins) when either win condition is met
    // popeyeWins = true  → Popeye collected 24 hearts
    // popeyeWins = false → Bluto reduced Popeye to 0 HP
    private void ShowGameOverScreen(bool popeyeWins)
    {
        // Show the main game over panel
        gameOverPanel.SetActive(true);

        // Show ONLY the winner's panel — hide the other
        if (popeyeVictoryPanel != null) popeyeVictoryPanel.SetActive(popeyeWins);
        if (blutoVictoryPanel != null) blutoVictoryPanel.SetActive(!popeyeWins);
    }
}
