// PopeyeController.cs
// Controls all of Popeye's gameplay behaviour: movement, jumping, dropping through platforms,
// punching, ladder climbing, stun/invincibility, and the 10-second spinach power-up.
//
// Architecture: reads input from InputManager each frame (polling),
// fires static events so AudioManager can react without tight coupling.
// Movement is only enabled after GameManager.OnGameStart fires (prevents input during countdown).
using UnityEngine;
using System;

// RequireComponent ensures Unity automatically adds these components if missing
[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(Animator))]
public class PopeyeController : MonoBehaviour
{
    // ─── INSPECTOR SETTINGS ──────────────────────────────────────────────────

    [Header("Movement Settings")]
    public float moveSpeed = 5f;    // Horizontal speed in units per second (doubled during spinach mode)
    public float jumpForce = 8.5f;  // Upward impulse applied to Rigidbody when jumping

    [Header("Combat Settings")]
    // The BoxCollider on the PunchHitbox child object — enabled/disabled by animation events
    // Drag the PunchHitbox child's Collider here in the Inspector
    public Collider meleeHitbox;

    // ─── PRIVATE STATE ───────────────────────────────────────────────────────

    private Rigidbody rb;
    private CapsuleCollider col;
    private Animator animator;

    [Header("State")]
    public bool isGrounded;          // True when ground raycast detects a surface below feet
    public bool isClimbing;          // True when attached to a ladder
    public bool isStunned;           // True during stun — all input blocked
    public bool isInvincible = false;// True during spinach mode — blocks damage and stun
    private bool canMove = false;    // Locked until OnGameStart fires
    private Vector3 originalScale;   // Cached Inspector scale — used for directional flipping

    // Static event fired when Popeye executes a punch
    // AudioManager subscribes to play the punch thud sound
    public static event Action OnPunch;

    // ─── UNITY LIFECYCLE ─────────────────────────────────────────────────────

    private void Awake()
    {
        rb       = GetComponent<Rigidbody>();
        col      = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();

        // FreezePositionZ: lock Popeye to Z = 0 (the 2.5D play plane)
        // FreezeRotation: prevent physics from tumbling the character on impact
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        rb.useGravity = true;

        // Save the original scale before any flipping modifies it
        originalScale = transform.localScale;

        // Ensure the punch hitbox starts disabled — enabled only during animation frames 5–10
        if (meleeHitbox != null) meleeHitbox.enabled = false;
    }

    private void OnEnable()
    {
        // Subscribe to OnGameStart → allow movement after the 3s countdown
        GameManager.OnGameStart += EnableMovement;
        // Subscribe to OnSpinachEaten → activate the power-up when a spinach can is collected
        SpinachItem.OnSpinachEaten += ActivateSpinachMode;
    }

    private void OnDisable()
    {
        // Always unsubscribe on disable to prevent ghost callbacks after scene reload
        GameManager.OnGameStart -= EnableMovement;
        SpinachItem.OnSpinachEaten -= ActivateSpinachMode;
    }

    // Unlocks movement when the game officially starts
    private void EnableMovement() => canMove = true;

    private void Update()
    {
        // Block all processing while stunned or before the round starts
        if (isStunned || !canMove) return;

        CheckGrounded();
        HandleMovement();
        HandleJumpAndDrop();
        HandleAction();
    }

    // ─── INPUT HANDLERS ──────────────────────────────────────────────────────

    private void HandleMovement()
    {
        // PopeyeMove.x: -1 = left (A key), 0 = idle, +1 = right (D key)
        float moveX = InputManager.Instance.PopeyeMove.x;

        if (isClimbing)
        {
            // On a ladder: move freely in all 4 directions using both X and Y input
            float moveY = InputManager.Instance.PopeyeMove.y;
            rb.linearVelocity = new Vector3(moveX * moveSpeed, moveY * moveSpeed, 0);
        }
        else
        {
            // On the ground: horizontal movement only; keep current Y velocity (gravity)
            rb.linearVelocity = new Vector3(moveX * moveSpeed, rb.linearVelocity.y, 0);

            // Drive the "Speed" parameter in the Animator (0 = Idle, >0 = Walk)
            animator.SetFloat("Speed", Mathf.Abs(moveX));

            // Flip the sprite by negating localScale.x — preserves the original size
            if (moveX != 0)
            {
                transform.localScale = new Vector3(
                    Mathf.Sign(moveX) * Mathf.Abs(originalScale.x),
                    originalScale.y,
                    originalScale.z
                );
            }
        }
    }

    private void HandleJumpAndDrop()
    {
        // JUMP: only if standing on the ground and not climbing a ladder
        if (isGrounded && InputManager.Instance.PopeyeJumpDown && !isClimbing)
        {
            // Reset vertical velocity first so jump height is always consistent
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, 0);
            // ForceMode.Impulse: applies the full force in one instant (not over time)
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // DROP THROUGH ONE-WAY PLATFORM: raycast down to find the platform below Popeye's feet
        if (isGrounded && InputManager.Instance.PopeyeDropDown)
        {
            Vector3 rayStart = new Vector3(col.bounds.center.x, col.bounds.min.y + 0.1f, col.bounds.center.z);
            RaycastHit hit;
            if (Physics.Raycast(rayStart, Vector3.down, out hit, 1.0f))
            {
                OneWayPlatform3D platform = hit.collider.GetComponent<OneWayPlatform3D>();
                if (platform != null) platform.FallThrough(col); // Signal the platform to ignore Popeye briefly
            }
        }
    }

    private void HandleAction()
    {
        // PUNCH: trigger the animation — EnableHitbox/DisableHitbox are called by Animation Events
        if (InputManager.Instance.PopeyePunchDown)
        {
            animator.SetTrigger("Punch"); // Fires a one-shot transition to the Punch state
            OnPunch?.Invoke();            // Notify AudioManager to play punch sound
        }
    }

    // ─── DAMAGE SYSTEM ───────────────────────────────────────────────────────

    // Called by MeleeHitbox (Bluto punch hits Popeye) with an optional knockback direction
    // amount: reserved for future variable-damage system
    // knockbackVector: impulse force direction and magnitude applied to Popeye on hit
    public void TakeDamage(int amount, Vector3 knockbackVector)
    {
        // Ignore damage while invincible (spinach mode) or already stunned
        if (isInvincible || isStunned) return;

        GameManager.Instance.TakeDamage(); // Decrement HP counter and check for game over

        // Apply a physics impulse in the direction Bluto was facing (knockback)
        rb.linearVelocity = Vector3.zero; // Zero out current velocity first for consistent knockback
        rb.AddForce(knockbackVector, ForceMode.Impulse);

        // Brief 0.5s stun — plays the Hit animation and blocks input during knockback
        ApplyStun(0.5f);
        animator.SetTrigger("Hit"); // Visual feedback for taking damage
    }

    // ─── ANIMATION EVENTS ────────────────────────────────────────────────────
    // Called by Animation Events embedded in the Popeye_Punch clip

    // Frame 5 of Punch animation: open the damage window
    public void EnableHitbox()
    {
        if (meleeHitbox != null) meleeHitbox.enabled = true;
    }

    // Frame 10 of Punch animation: close the damage window
    public void DisableHitbox()
    {
        if (meleeHitbox != null) meleeHitbox.enabled = false;
    }

    // ─── GROUND CHECK ────────────────────────────────────────────────────────

    private void CheckGrounded()
    {
        float rayLength = 0.3f;
        // Cast the ray from slightly above the bottom of the collider to avoid self-hits
        Vector3 rayStart = new Vector3(col.bounds.center.x, col.bounds.min.y + 0.05f, col.bounds.center.z);

        // Exclude the Player layer so Popeye's ray doesn't detect Bluto as "ground"
        int layerMask = ~LayerMask.GetMask("Player"); // ~ (bitwise NOT) inverts the mask to exclude the layer

        isGrounded = Physics.Raycast(rayStart, Vector3.down, rayLength, layerMask);
        animator.SetBool("IsGrounded", isGrounded); // Keep the Animator in sync
    }

    // ─── PUBLIC API ──────────────────────────────────────────────────────────

    // Called by LadderTrigger to attach/detach Popeye from a ladder
    public void SetClimbing(bool state)
    {
        isClimbing = state;
        rb.useGravity = !state; // Disable gravity while climbing so Popeye doesn't slide down
        if (state) rb.linearVelocity = Vector3.zero; // Kill momentum when grabbing the ladder
        animator.SetBool("IsClimbing", state);
    }

    // Generic stun: freezes Popeye's input for the specified duration
    // Also blocked by isInvincible (spinach) so stuns don't apply during the buff
    public void ApplyStun(float duration)
    {
        if (isStunned || isInvincible) return;
        isStunned = true;
        animator.SetBool("IsStunned", true);
        Invoke(nameof(RecoverFromStun), duration); // Schedule recovery after 'duration' seconds
    }

    // Called automatically by Invoke when the stun timer expires
    private void RecoverFromStun()
    {
        isStunned = false;
        animator.SetBool("IsStunned", false);
    }

    // ─── SPINACH POWER-UP ────────────────────────────────────────────────────

    // Called when SpinachItem.OnSpinachEaten fires (Popeye touched a spinach can)
    // Grants invincibility and double movement speed for 10 seconds
    private void ActivateSpinachMode()
    {
        isInvincible = true;
        moveSpeed *= 2f; // Double speed for 10 seconds — reversed in DeactivateSpinachMode
        // Optional: add a visual effect here (e.g., aura particle, color tint) via VFXManager
        Invoke(nameof(DeactivateSpinachMode), 10f); // Schedule deactivation after 10 seconds
    }

    // Called 10 seconds after activation to restore Popeye to normal
    private void DeactivateSpinachMode()
    {
        isInvincible = false;
        moveSpeed /= 2f; // Restore original speed (reverse the ×2 from activation)
    }
}
