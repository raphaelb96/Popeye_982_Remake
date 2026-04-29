// PopeyeHitbox.cs
// ⚠️ LEGACY SCRIPT — DO NOT USE IN THE CURRENT PROJECT ⚠️
//
// This was an early prototype hitbox written before MeleeHitbox.cs was created.
// It has been fully replaced by MeleeHitbox.cs which:
//   - Is reusable for both Popeye AND Bluto (configured via Inspector fields)
//   - Handles all collision cases: enemy player, bottles, Sea Hag projectiles, spinach, ladders
//   - Is properly integrated with the Animation Event system (EnableHitbox / DisableHitbox)
//
// This file should be DELETED from the Unity project.
// If it is still attached to any child GameObject of PlayerPopeye, remove the component.
using UnityEngine;

public class PopeyeHitbox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // PROTOTYPE: only handled bottle destruction — incomplete implementation
        // The hit-Bluto logic (commented out below) was never finished before this was replaced
        BottleItem bottle = other.GetComponent<BottleItem>();
        if (bottle != null)
        {
            bottle.Smash(); // Destroy any bottle Popeye's fist touches
        }

        // ── COMMENTED-OUT PROTOTYPE: future Bluto hit logic ─────────────────
        // This was never activated — MeleeHitbox.cs handles this correctly now
        /*
        else if (other.CompareTag("Player2"))
        {
            BlutoController bluto = other.GetComponent<BlutoController>();
            if (bluto != null) bluto.ApplyStun(1f);
        }
        */
    }
}
