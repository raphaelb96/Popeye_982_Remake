// SpinachItem.cs
// A spinach can collectible that activates Popeye's power-up mode when he touches it.
// Bluto can also destroy it via his MeleeHitbox punch (preventing Popeye from picking it up).
// Only one spinach effect can be active at a time — guard logic lives in PopeyeController.
using UnityEngine;
using System;

public class SpinachItem : MonoBehaviour
{
    // Static event fired when Popeye collects the spinach
    // PopeyeController subscribes to this to activate the 10-second speed+invincibility buff
    // AudioManager subscribes to this to play the spinach music
    public static event Action OnSpinachEaten;

    private void OnTriggerEnter(Collider other)
    {
        // Only Popeye (Player1) can eat spinach — Bluto cannot collect it, only destroy it
        if (other.CompareTag("Player1"))
        {
            OnSpinachEaten?.Invoke(); // Broadcast the event to all subscribers before destroying
            Destroy(gameObject);      // Remove the spinach can from the scene
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
