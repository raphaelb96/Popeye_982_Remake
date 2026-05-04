// ScreenWrapTrigger.cs
// Teleports any player or item that enters this trigger to the paired trigger on the opposite side.
// Two of these exist in the scene: ScreenWrapLeft (X = -12) and ScreenWrapRight (X = 20.98).
// Each one has its teleportDestination set to the other in the Inspector, forming a loop.
//
// A cooldown on the DESTINATION trigger (not the source) prevents the player from
// being immediately teleported back after arriving — without it, they'd flicker between edges.
using UnityEngine;

public class ScreenWrapTrigger : MonoBehaviour
{
    // The ScreenWrapTrigger on the opposite side of the stage
    // Drag the paired trigger GameObject here in the Inspector
    public Transform teleportDestination;

    // Countdown in seconds. When > 0, this trigger ignores all collisions.
    // Set on the DESTINATION trigger when a teleport happens to prevent bounce-back.
    private float cooldown = 0f;

    // ─── UNITY LOOP ──────────────────────────────────────────────────────────

    private void Update()
    {
        // Count down the cooldown each frame (frame-rate independent)
        if (cooldown > 0) cooldown -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only teleport if this trigger is not on cooldown (prevents double-teleport on arrival)
        // Accepted tags: Player1, Player2, and Item (hearts, bottles, spinach)
        if (cooldown <= 0 && (other.CompareTag("Player1") || other.CompareTag("Player2") || other.CompareTag("Item")))
        {
            // Get the X position of the destination trigger
            Vector3 targetPos = teleportDestination.position;

            // Teleport: keep the object's current Y and Z (height and depth), only change X
            other.transform.position = new Vector3(
                targetPos.x,
                other.transform.position.y,
                other.transform.position.z
            );

            // Apply cooldown to the DESTINATION trigger (not this one) so the teleported object
            // doesn't immediately trigger the destination and get sent back
            ScreenWrapTrigger destTrigger = teleportDestination.GetComponent<ScreenWrapTrigger>();
            if (destTrigger != null) destTrigger.cooldown = 0.5f; // 0.5s immunity on arrival
        }
    }
}
