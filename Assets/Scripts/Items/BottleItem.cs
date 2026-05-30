// BottleItem.cs
// Dual-mode object: can act as a pickup on the floor OR as a flying projectile thrown by Bluto.
// - Pickup mode:    Bluto walks over it → gains 1 bottle in his inventory.
// - Projectile mode: Bluto throws it → flies in a straight line → stuns Popeye on contact.
// Popeye can destroy any bottle (floor or flying) by punching it.
using UnityEngine;
using System;

public class BottleItem : MonoBehaviour
{
    // Speed (units/sec) at which the bottle travels when thrown as a projectile
    public float projectileSpeed = 8f;

    // Tracks whether this bottle is currently flying (true) or sitting on the floor as a pickup (false)
    private bool isProjectile = false;

    // Direction of travel: +1 = right, -1 = left
    // Set at throw time by reading Bluto's facing direction (sign of his localScale.x)
    private float moveDirection;

    // Static event fired whenever any bottle is destroyed by any means
    // AudioManager subscribes to this to play the bottle smash sound effect
    public static event Action OnBottleSmashed;

    // ─── INITIALIZATION ──────────────────────────────────────────────────────

    // Called by OliveController when spawning a bottle collectible on the floor
    // Sets the bottle to passive pickup mode (no movement)
    public void InitializePickup() => isProjectile = false;

    // Called by BlutoController.SpawnBottleProjectile() at the animation event frame
    // dir: +1 for right, -1 for left — read from Bluto's current localScale.x sign
    public void InitializeProjectile(float dir)
    {
        isProjectile = true;
        moveDirection = dir;
    }

    // ─── UNITY LOOP ──────────────────────────────────────────────────────────

    private void Update()
    {
        // Only bottles in projectile mode need to move every frame
        if (isProjectile)
        {
            // Vector3.right * moveDirection gives a horizontal direction vector (+X or -X)
            // * projectileSpeed sets the distance to cover per second
            // * Time.deltaTime converts it to the correct distance for this frame (frame-rate independent)
            transform.Translate(Vector3.right * moveDirection * projectileSpeed * Time.deltaTime);

            // Self-destroy if the bottle flies off screen (beyond X = ±15 world units)
            if (Mathf.Abs(transform.position.x) > 15f) Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // PICKUP MODE: Bluto (Player2) walks into a floor bottle → add 1 to his inventory
        if (!isProjectile && other.CompareTag("Player2"))
        {
            other.GetComponent<BlutoController>().AddBottle();
            Destroy(gameObject); // Remove the pickup from the scene
        }
        // PROJECTILE MODE: the flying bottle hits Popeye (Player1) → stun only, no HP damage
        else if (isProjectile && other.CompareTag("Player1"))
        {
            PopeyeController popeye = other.GetComponent<PopeyeController>();
            // Stun duration set on Popeye's Inspector for easy tuning
            popeye.ApplyStun(popeye.bottleStunDuration);
            OnBottleSmashed?.Invoke(); // Notify AudioManager → play smash SFX
            Destroy(gameObject);       // Remove the bottle after hitting Popeye
        }
    }

    // ─── PUBLIC API ──────────────────────────────────────────────────────────

    // Called by MeleeHitbox when Popeye punches this bottle (works in both pickup and projectile mode)
    // Also used by any other system that needs to destroy a bottle (e.g., future power-ups)
    public void Smash()
    {
        OnBottleSmashed?.Invoke(); // Notify listeners before destroying
        Destroy(gameObject);
    }
}
