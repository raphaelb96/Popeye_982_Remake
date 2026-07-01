// ScreenWrapTrigger.cs
// Teleports any player or item that enters this trigger to the paired trigger on the opposite side.
// Two of these exist in the scene: ScreenWrapLeft (X = -12) and ScreenWrapRight (X = 20.98).
// Each one has its teleportDestination set to the other in the Inspector, forming a loop.
//
// Re-trigger guard: an object teleported ONTO a trigger is ignored by that trigger until it
// physically leaves the zone (OnTriggerExit). This lets the player turn around inside the
// arrival zone without being bounced back — replaces the old, fragile 0.5s timer.
using UnityEngine;
using System.Collections.Generic;

public class ScreenWrapTrigger : MonoBehaviour
{
    // The ScreenWrapTrigger on the opposite side of the stage (set in the Inspector)
    public Transform teleportDestination;

    // Colliders that just arrived here via teleport — ignored until they leave this trigger,
    // so turning around inside the arrival zone never sends them straight back.
    private readonly HashSet<Collider> arrivedHere = new HashSet<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        // Skip anything that was just teleported onto this trigger — it must exit first
        if (arrivedHere.Contains(other)) return;

        // Accepted tags: Player1, Player2, and Item (hearts, bottles, spinach)
        if (other.CompareTag("Player1") || other.CompareTag("Player2") || other.CompareTag("Item"))
        {
            // Teleport: keep the object's current Y and Z (height and depth), only change X
            Vector3 targetPos = teleportDestination.position;
            other.transform.position = new Vector3(
                targetPos.x,
                other.transform.position.y,
                other.transform.position.z
            );

            // Mark it as "arrived" on the DESTINATION trigger so that trigger won't bounce it back
            // until it has physically left the zone (cleared in OnTriggerExit)
            ScreenWrapTrigger destTrigger = teleportDestination.GetComponent<ScreenWrapTrigger>();
            if (destTrigger != null) destTrigger.arrivedHere.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Left the zone — allow it to wrap again next time it enters
        arrivedHere.Remove(other);
    }
}
