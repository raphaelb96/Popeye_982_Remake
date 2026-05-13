// SpinachItem.cs
// A spinach can collectible that activates Popeye's power-up mode when he touches it.
// Bluto can also destroy it via his MeleeHitbox punch (preventing Popeye from picking it up).
// Only one spinach effect can be active at a time — guard logic lives in PopeyeController.
// When one is collected, all other spinach cans currently on the board destroy themselves.
using UnityEngine;
using System;

public class SpinachItem : MonoBehaviour
{
    private bool isCollected = false;

    // Static event fired when Popeye collects the spinach
    // PopeyeController subscribes to this to activate the 10-second speed+invincibility buff
    // AudioManager subscribes to this to play the spinach music
    public static event Action OnSpinachEaten;

    private void OnEnable()
    {
        // Subscribe to its own class's static event. 
        // When ANY spinach fires this event, ALL spinaches hear it.
        OnSpinachEaten += ClearFromBoard;
    }

    private void OnDisable()
    {
        OnSpinachEaten -= ClearFromBoard;
    }

    private void ClearFromBoard()
    {
        // If this specific can was NOT the one Popeye just collected, destroy it silently.
        if (!isCollected)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        // Only Popeye (Player1) can eat spinach — Bluto cannot collect it, only destroy it
        if (other.CompareTag("Player1"))
        {
            isCollected = true;
            GetComponent<Collider>().enabled = false; // Lock out ghost collisions

            // Firing this event triggers the power-up AND tells all other cans to self-destruct
            OnSpinachEaten?.Invoke();

            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    // ─── PUBLIC API ──────────────────────────────────────────────────────────

    // Called by MeleeHitbox when Bluto's punch hits the spinach can
    // This denies Popeye the power-up without triggering the OnSpinachEaten event
    public void DestroyByBluto()
    {
        Destroy(gameObject); // Simply remove it — no event fired, no buff granted
    }
}