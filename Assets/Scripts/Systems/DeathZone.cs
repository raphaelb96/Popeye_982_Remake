// DeathZone.cs
// A large invisible trigger placed below the stage (Y = -3, Size 50×5×20).
// Any player who falls off a platform and passes below this trigger is respawned
// at their designated respawn point Transform (configured in the Inspector).
//
// Known previous bug: respawn was hardcoded to (0,0,0) — now uses configurable Transforms.
// Assign popeyeRespawn and blutoRespawn to empty GameObjects placed at valid spawn positions in the scene.
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [Header("Respawn Points")]
    // Drag an empty GameObject here positioned where Popeye should respawn (e.g., bottom-left platform)
    [Tooltip("The world position where Popeye respawns after falling")]
    public Transform popeyeRespawn;

    // Drag an empty GameObject here positioned where Bluto should respawn (e.g., bottom-right platform)
    [Tooltip("The world position where Bluto respawns after falling")]
    public Transform blutoRespawn;

    // ─── COLLISION DETECTION ─────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        // Popeye fell into the death zone
        if (other.CompareTag("Player1"))
        {
            // Use the configured respawn Transform, or a safe fallback if not assigned
            Vector3 target = popeyeRespawn != null ? popeyeRespawn.position : new Vector3(0, 2, 0);
            Respawn(other.gameObject, target);
        }
        // Bluto fell into the death zone
        else if (other.CompareTag("Player2"))
        {
            Vector3 target = blutoRespawn != null ? blutoRespawn.position : new Vector3(3, 2, 0);
            Respawn(other.gameObject, target);
        }
        // Note: items (hearts, bottles, spinach) that fall off screen are handled by their own scripts
        // (HeartItem self-destructs below Y = -10, etc.) — they don't need respawning here
    }

    // ─── RESPAWN LOGIC ───────────────────────────────────────────────────────

    // Resets the player's physics velocity and teleports them to the target position
    private void Respawn(GameObject player, Vector3 position)
    {
        // Zero out velocity before teleporting — otherwise the player arrives still moving
        // at the speed they had when they fell, which can cause immediate re-falling
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.zero;

        // Snap the player to the respawn position
        player.transform.position = position;
    }
}
