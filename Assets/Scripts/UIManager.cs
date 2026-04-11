// UIManager.cs
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI heartsText;
    public TextMeshProUGUI popeyeHpText;
    public TextMeshProUGUI blutoBottlesText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverMessage;

    public GameManager gameManager;
    public BlutoController bluto;

    private void OnEnable()
    {
        HeartItem.OnHeartCollected += UpdateHUD;
        BlutoController.OnBottleThrow += UpdateHUD;
        GameManager.OnDamageTaken += UpdateHUD;
        GameManager.OnGameStart += UpdateHUD; 
        GameManager.OnGameOver += ShowGameOverScreen;
    }

    private void OnDisable()
    {
        HeartItem.OnHeartCollected -= UpdateHUD;
        BlutoController.OnBottleThrow -= UpdateHUD;
        GameManager.OnDamageTaken -= UpdateHUD;
        GameManager.OnGameStart -= UpdateHUD;
        GameManager.OnGameOver -= ShowGameOverScreen;
    }

    private void Start()
    {
        gameOverPanel.SetActive(false);
        UpdateHUD();
    }

    public void UpdateHUD()
    {
        if (heartsText != null) heartsText.text = $"Hearts: {gameManager.popeyeHearts} / 24";
        if (popeyeHpText != null) popeyeHpText.text = $"Popeye HP: {gameManager.popeyeHP}";
        if (bluto != null && blutoBottlesText != null) blutoBottlesText.text = $"Bottles: {bluto.currentBottles}";
    }

    private void ShowGameOverScreen(bool popeyeWins)
    {
        gameOverPanel.SetActive(true);
        if (popeyeWins)
        {
            gameOverMessage.text = "POPEYE WINS!\nPress SPACE to Restart";
            gameOverMessage.color = Color.green;
        }
        else
        {
            gameOverMessage.text = "BLUTO WINS!\nPress SPACE to Restart";
            gameOverMessage.color = Color.red;
        }
    }
}