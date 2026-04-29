// SeaHagProjectile.cs
// A magic projectile fired by the Sea Hag that travels in a straight line across the stage.
// It stuns BOTH players for 1 second on contact — it's an environmental hazard that affects everyone.
// Popeye can destroy it with a punch (via MeleeHitbox.canDestroyProjectiles = true).
// Self-destructs when it reaches the edge of the visible stage (X = ±15).
using UnityEngine;

public class SeaHagProjectile : MonoBehaviour
{
    // Travel speed in units per second
    public float speed = 6f;

    // +1 = moving right, -1 = moving left — set by SeaHagController.SpawnAndAttack()
    private float moveDirection;

    // ─── INITIALIZATION ──────────────────────────────────────────────────────

    // Called immediately after Instantiate() by SeaHagController
    // dir: +1 for right (spawned from left side), -1 for left (spawned from right side)
    public void Initialize(float dir)
    {
        moveDirection = dir;
    }

    // ─── UNITY LOOP ──────────────────────────────────────────────────────────

    private void Update()
    {
        // Move the projectile horizontally every frame (frame-rate independent via Time.deltaTime)
        transform.Translate(Vector3.right * moveDirection * speed * Time.deltaTime);

        // Self-destroy when the projectile exits the visible play area on either side
        if (Mathf.Abs(transform.position.x) > 15f) Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Stun Popeye for 1 second — the projectile is an equal-opportunity hazard
        if (other.CompareTag("Player1"))
        {
            other.GetComponent<PopeyeController>().ApplyStun(1f);
            Destroy(gameObject); // Destroy the projectile after it hits
        }
        // Stun Bluto for 1 second as well
        else if (other.CompareTag("Player2"))
        {
            other.GetComponent<BlutoController>().ApplyStun(1f);
            Destroy(gameObject);
        }
    }

    // ─── PUBLIC API ──────────────────────────────────────────────────────────

    // Called by MeleeHitbox when Popeye punches this projectile (canDestroyProjectiles = true)
    public void Smash()
    {
        Destroy(gameObject); // Simply destroy it — no extra effects needed
    }
}
