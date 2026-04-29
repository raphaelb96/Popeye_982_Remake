// OneWayPlatform3D.cs
// Implements one-way platform behaviour for 3D physics.
// Players can jump up through the platform from below, land on it from above,
// and intentionally drop through it by pressing down (calls FallThrough()).
//
// How it works:
//   FixedUpdate() checks each player's feet position every physics frame.
//   If a player's feet are BELOW the platform's top surface, collision is ignored (pass-through).
//   If a player's feet are ABOVE the surface, collision is restored (they can stand on it).
//   FallThrough() temporarily forces collision off for 0.4s so the player drops through intentionally.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OneWayPlatform3D : MonoBehaviour
{
    // The collider of this platform — cached to avoid repeated GetComponent calls
    private Collider platformCollider;

    // Collider references for both players — found by tag in Start()
    private Collider player1Collider;
    private Collider player2Collider;

    // Tracks which players are currently in an intentional fall-through (drop button pressed)
    // Using a HashSet prevents duplicate entries if FallThrough is called multiple times quickly
    private HashSet<Collider> fallingPlayers = new HashSet<Collider>();

    // ─── UNITY LIFECYCLE ─────────────────────────────────────────────────────

    private void Awake()
    {
        platformCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        // Find both players by tag and cache their Colliders
        // Done in Start() (not Awake()) to ensure player GameObjects are initialized first
        GameObject p1 = GameObject.FindGameObjectWithTag("Player1");
        if (p1 != null) player1Collider = p1.GetComponent<Collider>();

        GameObject p2 = GameObject.FindGameObjectWithTag("Player2");
        if (p2 != null) player2Collider = p2.GetComponent<Collider>();
    }

    // FixedUpdate runs in sync with the physics engine (default: 50 times/sec)
    // Must be used here because Physics.IgnoreCollision is a physics operation
    private void FixedUpdate()
    {
        HandlePlayerCollision(player1Collider);
        HandlePlayerCollision(player2Collider);
    }

    // ─── COLLISION LOGIC ─────────────────────────────────────────────────────

    // Determines each frame whether a player's collider should pass through or stand on this platform
    private void HandlePlayerCollision(Collider playerCol)
    {
        // Skip if no collider found, or if player is currently in a deliberate fall-through
        if (playerCol == null || fallingPlayers.Contains(playerCol)) return;

        // Compare the player's foot position (collider.bounds.min.y) to the platform's top surface
        // The -0.2f offset creates a small tolerance zone to prevent flickering at the exact boundary
        if (playerCol.bounds.min.y < platformCollider.bounds.max.y - 0.2f)
        {
            // Player's feet are below the platform top → they're jumping up through it → ignore collision
            Physics.IgnoreCollision(playerCol, platformCollider, true);
        }
        else
        {
            // Player's feet are above the platform top → they've landed on it → restore collision
            Physics.IgnoreCollision(playerCol, platformCollider, false);
        }
    }

    // ─── PUBLIC API ──────────────────────────────────────────────────────────

    // Called by PopeyeController / BlutoController when the player presses down on a platform
    // Temporarily disables collision so the player drops through deliberately
    public void FallThrough(Collider playerCollider)
    {
        // Guard: only start one coroutine per player at a time
        if (!fallingPlayers.Contains(playerCollider))
        {
            StartCoroutine(FallRoutine(playerCollider));
        }
    }

    // Coroutine: ignores collision for 0.4 seconds (enough time to fall through the platform geometry)
    private IEnumerator FallRoutine(Collider playerCollider)
    {
        fallingPlayers.Add(playerCollider);                              // Mark as falling through
        Physics.IgnoreCollision(playerCollider, platformCollider, true); // Force pass-through

        yield return new WaitForSeconds(0.4f); // Wait long enough to clear the platform geometry

        fallingPlayers.Remove(playerCollider); // Unmark — FixedUpdate resumes normal logic
        // Note: FixedUpdate will restore collision automatically once the player is above the platform
    }
}
