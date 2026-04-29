// LadderTrigger.cs
// A trigger zone that enables climbing for any player who enters it while pressing up or down.
// Climbing is NOT activated by merely walking through the trigger — the player must
// intentionally press a vertical direction key to grab the ladder (prevents accidental attachment).
// Popeye can punch a ladder to disable it for 3 seconds (blocking Bluto from using it).
using UnityEngine;
using System.Collections;

public class LadderTrigger : MonoBehaviour
{
    // When true, the ladder is temporarily disabled (Popeye punched it)
    // Players inside the trigger still get OnTriggerStay calls but the guard at the top blocks them
    private bool isDisabled = false;

    // ─── TRIGGER CALLBACKS ───────────────────────────────────────────────────

    // OnTriggerStay fires every physics frame while a collider overlaps this trigger
    // Used instead of OnTriggerEnter so the player can grab the ladder at any point during overlap
    private void OnTriggerStay(Collider other)
    {
        if (isDisabled) return; // Ladder is temporarily disabled — ignore all input

        if (other.CompareTag("Player1"))
        {
            HandleClimbing(other.GetComponent<PopeyeController>());
        }
        else if (other.CompareTag("Player2"))
        {
            HandleClimbing(other.GetComponent<BlutoController>());
        }
    }

    // OnTriggerExit fires the moment a collider leaves this trigger's bounds (including from the sides)
    // Immediately detaches the player from climbing — they can't "hang" outside the ladder zone
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player1"))
        {
            other.GetComponent<PopeyeController>().SetClimbing(false);
        }
        else if (other.CompareTag("Player2"))
        {
            other.GetComponent<BlutoController>().SetClimbing(false);
        }
    }

    // ─── CLIMBING LOGIC ──────────────────────────────────────────────────────

    // Checks if the player is pressing a vertical direction and attaches them to the ladder if so
    // The 0.1f dead zone threshold prevents drift from zero-valued analog sticks triggering climbing
    private void HandleClimbing(MonoBehaviour controller)
    {
        // Check Popeye's vertical input — if pressing up or down AND is a PopeyeController, climb
        if (Mathf.Abs(InputManager.Instance.PopeyeMove.y) > 0.1f && controller is PopeyeController p)
            p.SetClimbing(true);

        // Check Bluto's vertical input — same logic for BlutoController
        if (Mathf.Abs(InputManager.Instance.BlutoMove.y) > 0.1f && controller is BlutoController b)
            b.SetClimbing(true);
    }

    // ─── PUBLIC API ──────────────────────────────────────────────────────────

    // Called by MeleeHitbox when Popeye punches this ladder trigger
    // Disables the ladder for 3 seconds to deny Bluto access
    public void DisableLadder()
    {
        // Guard: don't start a new coroutine if one is already running
        if (!isDisabled) StartCoroutine(DisableRoutine());
    }

    // Coroutine: sets isDisabled for 3 seconds then re-enables the ladder
    private IEnumerator DisableRoutine()
    {
        isDisabled = true;
        yield return new WaitForSeconds(3f); // Pause here without blocking the main thread
        isDisabled = false;
    }
}
