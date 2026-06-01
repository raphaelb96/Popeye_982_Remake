// MeleeHitbox.cs
// A reusable punch hitbox attached as a child of any character with melee attacks.
// The hitbox starts disabled and is only activated during the punch animation frames
// via Animation Events (EnableHitbox at frame 5, DisableHitbox at frame 10).
//
// Configured per-character via Inspector:
//   - Popeye's hitbox: targetTag = "Player2", canDestroyProjectiles = true
//   - Bluto's hitbox:  targetTag = "Player1", canDestroyProjectiles = false
//
// On hit, the hitbox checks what it collided with and routes to the correct response.
using UnityEngine;

// Automatically adds a BoxCollider if one isn't present — prevents null reference errors
[RequireComponent(typeof(BoxCollider))]
public class MeleeHitbox : MonoBehaviour
{
    // The Unity tag of the character this hitbox should damage
    // Set to "Player1" for Bluto's hitbox (damages Popeye), "Player2" for Popeye's (damages Bluto)
    public string targetTag;

    // Damage amount — currently unused in logic (TakeDamage is a fixed -1 HP call)
    // Reserved for future use if damage values become variable
    public int damageAmount = 1;

    // If true, this hitbox can also destroy flying projectiles (BottleItem, SeaHagProjectile)
    // Set to true for Popeye (can punch bottles away), false for Bluto
    public bool canDestroyProjectiles = true;

    // The BoxCollider on this same GameObject — cached in Awake for repeated use
    private BoxCollider hitCollider;

    public string hitText = "POW!";  // Set per-character in Inspector

    // ─── UNITY LIFECYCLE ─────────────────────────────────────────────────────

    private void Awake()
    {
        hitCollider = GetComponent<BoxCollider>();
        hitCollider.isTrigger = true;  // Trigger mode: detects overlaps without physical collision response
        hitCollider.enabled = false;   // Starts disabled — only active during the punch window
    }

    // ─── ANIMATION EVENT TARGETS ─────────────────────────────────────────────
    // These methods are called by Animation Events set on the Punch animation clips.
    // The method names must match exactly what is typed in the Animation Event inspector.

    // Called at frame 5 of Punch animation — opens the damage window
    public void EnableHitbox()  => hitCollider.enabled = true;

    // Called at frame 10 of Punch animation — closes the damage window
    public void DisableHitbox() => hitCollider.enabled = false;

    // ─── COLLISION DETECTION ─────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        // ── CASE 1: Hit the intended target (enemy player) ───────────────────
        if (other.CompareTag(targetTag))
        {
            // Spawn floating text at the hit point
            VFXManager vfx = FindFirstObjectByType<VFXManager>();
            if (vfx != null) vfx.SpawnFloatingText(other.transform.position, hitText);

            // Bluto's hitbox hit Popeye (targetTag = "Player1") → reduce Popeye's HP
            if (targetTag == "Player1")
            {
                // Call TakeDamage() directly on Popeye — handles HP, knockback and stun together
                PopeyeController popeye = other.GetComponent<PopeyeController>();
                if (popeye != null)
                {
                    // Push Popeye away from Bluto's hitbox parent
                    Vector3 knockbackDir = (other.transform.position - transform.parent.position).normalized;
                    popeye.TakeDamage(damageAmount, knockbackDir * 8f);
                }
            }
            // CASE 1b: Popeye's hitbox hit Bluto (targetTag = "Player2")
            // Only apply the spinach stun if Popeye is currently in spinach mode (isInvincible = true)
            // Without spinach, Popeye's punch is harmless to Bluto — no damage, no stun
            else if (targetTag == "Player2" && transform.parent.GetComponent<PopeyeController>())
            {
                // Get the PopeyeController to check if the spinach buff is active
                PopeyeController popeye = transform.parent.GetComponent<PopeyeController>();

                BlutoController bluto = other.GetComponent<BlutoController>();

                if (popeye.isInvincible)
                {
                    // Spinach active — stun duration set in Inspector on Bluto
                    bluto.ApplyStun(bluto.spinachStunDuration);
                }
                else
                {
                    // Normal punch — stun duration set in Inspector on Bluto
                    bluto.ApplyStun(bluto.normalStunDuration);
                }
            }
        }
        // ── CASE 2: Hit a bottle projectile or floor pickup → smash it ───────
        else if (canDestroyProjectiles && other.GetComponent<BottleItem>())
        {
            other.GetComponent<BottleItem>().Smash();
        }
        // ── CASE 3: Hit a Sea Hag magic projectile → destroy it ──────────────
        else if (canDestroyProjectiles && other.GetComponent<SeaHagProjectile>())
        {
            other.GetComponent<SeaHagProjectile>().Smash();
        }
        // ── CASE 4: Bluto's hitbox punches a spinach can → deny the pickup ───
        else if (other.GetComponent<SpinachItem>() && transform.parent.GetComponent<BlutoController>())
        {
            other.GetComponent<SpinachItem>().DestroyByBluto();
        }
        // ── CASE 4b: Bluto's hitbox punches a heart → destroy it before Popeye collects it ───
        else if (other.GetComponent<HeartItem>() && transform.parent.GetComponent<BlutoController>())
        {
            Destroy(other.gameObject);
        }
        // ── CASE 5: Popeye punches a ladder → disable it for 3 seconds ───────
        // This is a defensive mechanic to block Bluto from climbing
        else if (other.GetComponent<LadderTrigger>() && transform.parent.GetComponent<PopeyeController>())
        {
            other.GetComponent<LadderTrigger>().DisableLadder();
        }
    }
}
