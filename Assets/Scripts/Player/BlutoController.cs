// BlutoController.cs
// Controls all of Bluto's gameplay behaviour: movement, jumping, dropping through platforms,
// punching, throwing bottles, ladder climbing, and stun recovery.
//
// Architecture: reads input from InputManager each frame (polling),
// fires static events so AudioManager / UIManager / VFXManager can react without tight coupling.
// Movement is only enabled after GameManager.OnGameStart fires (prevents input during countdown).
using UnityEngine;
using System;

// RequireComponent ensures Unity automatically adds these components if missing,
// and guarantees GetComponent won't return null in Awake()
[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(Animator))]
public class BlutoController : MonoBehaviour
{
    // ─── INSPECTOR SETTINGS ──────────────────────────────────────────────────

    [Header("Movement Settings")]
    public float moveSpeed = 4f;    // Horizontal movement speed in units per second
    public float jumpForce = 8f;    // Upward impulse force applied to Rigidbody when jumping

    [Header("Inventory")]
    public int maxBottles = 5;      // Maximum bottles Bluto can carry at once
    public int currentBottles = 5;  // Current bottle count — shown in UI and consumed on throw

    [Header("References")]
    // The child Transform from which thrown bottles are spawned (ThrowPoint child object)
    public Transform throwPoint;

    // The bottle prefab instantiated when Bluto throws — assign Michael's BottlePrefab here
    public GameObject bottlePrefab;

    // The BoxCollider on the PunchHitbox child object — enabled/disabled by animation events
    // Drag the child PunchHitbox's Collider component here in the Inspector
    public Collider meleeHitbox;

    // ─── PRIVATE STATE ───────────────────────────────────────────────────────

    private Rigidbody rb;
    private CapsuleCollider col;
    private Animator animator;

    [Header("State")]
    public bool isGrounded;   // True when the ground raycast detects a surface below Bluto
    public bool isClimbing;   // True when Bluto is attached to a ladder
    private bool isStunned;   // True during stun — blocks all input
    private bool canMove = false; // Locked until OnGameStart fires (stays false during countdown)
    private Vector3 originalScale; // Cached scale from Inspector — used to flip without squashing

    // ─── STATIC EVENTS ───────────────────────────────────────────────────────
    // Static events allow decoupled communication: AudioManager/VFXManager react without
    // needing a direct reference to BlutoController

    public static event Action OnHeavyPunch;          // Fired when Bluto starts a punch animation → camera shake
    public static event Action OnBottleThrow;         // Fired when a bottle is actually thrown (at animation event)
    public static event Action OnBottleCountChanged;  // Fired when bottle count changes → UIManager refreshes
    public static event Action OnBlutoStunned;        // Fired when spinach stun is applied (longer stun)

    // ─── UNITY LIFECYCLE ─────────────────────────────────────────────────────

    private void Awake()
    {
        // Cache component references once — much cheaper than GetComponent() every frame
        rb       = GetComponent<Rigidbody>();
        col      = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();

        // FreezePositionZ: keep Bluto locked to Z = 0 (2.5D plane) — physics can't push him into the screen
        // FreezeRotation: prevent the Rigidbody from tumbling when hit
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        rb.useGravity = true;

        // Store the Inspector scale before any flipping happens
        originalScale = transform.localScale;

        // Ensure the punch hitbox starts disabled — it's only enabled during the punch animation frame
        if (meleeHitbox != null) meleeHitbox.enabled = false;
    }

    // Subscribe to OnGameStart when this object is enabled
    private void OnEnable() => GameManager.OnGameStart += EnableMovement;

    // Always unsubscribe when disabled to avoid ghost callbacks after scene reload
    private void OnDisable() => GameManager.OnGameStart -= EnableMovement;

    // Called when GameManager fires OnGameStart (3 seconds after scene load)
    private void EnableMovement()
    {
        canMove = true;
        OnBottleCountChanged?.Invoke(); // Push initial bottle count to UIManager immediately
    }

    private void Update()
    {
        // Block all input while stunned or before the round has officially started
        if (isStunned || !canMove) return;

        CheckGrounded();
        HandleMovement();
        HandleJumpAndDrop();
        HandleAction();
    }

    // ─── INPUT HANDLERS ──────────────────────────────────────────────────────

    private void HandleMovement()
    {
        // InputManager.BlutoMove is a Vector2 from arrow keys: X = horizontal, Y = vertical
        float moveX = InputManager.Instance.BlutoMove.x;

        if (isClimbing)
        {
            // On a ladder: move freely in all 4 directions
            float moveY = InputManager.Instance.BlutoMove.y;
            rb.linearVelocity = new Vector3(moveX * moveSpeed, moveY * moveSpeed, 0);
        }
        else
        {
            // On the ground: only horizontal movement; preserve the current vertical velocity (gravity)
            rb.linearVelocity = new Vector3(moveX * moveSpeed, rb.linearVelocity.y, 0);

            // Drive the "Speed" float in the Animator to transition between Idle and Walk states
            animator.SetFloat("Speed", Mathf.Abs(moveX));

            // Flip the sprite based on movement direction
            // Mathf.Sign(moveX) returns +1 or -1; multiplied by Abs(originalScale.x) to keep size constant
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
        // JUMP: only if grounded and not on a ladder — prevents double-jumping
        if (isGrounded && InputManager.Instance.BlutoJumpDown && !isClimbing)
        {
            // Reset vertical velocity before applying jump force to ensure consistent jump height
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, 0);
            // ForceMode.Impulse applies force in a single instant (not spread over time)
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // DROP THROUGH ONE-WAY PLATFORM: raycast down from feet to detect a OneWayPlatform3D below
        if (isGrounded && InputManager.Instance.BlutoDropDown)
        {
            // Raycast from slightly above the bottom of the collider to avoid self-hits
            Vector3 rayStart = new Vector3(col.bounds.center.x, col.bounds.min.y + 0.1f, col.bounds.center.z);
            RaycastHit hit;
            if (Physics.Raycast(rayStart, Vector3.down, out hit, 1.0f))
            {
                // If the platform below is a OneWayPlatform3D, tell it to temporarily ignore Bluto's collider
                OneWayPlatform3D platform = hit.collider.GetComponent<OneWayPlatform3D>();
                if (platform != null) platform.FallThrough(col);
            }
        }
    }

    private void HandleAction()
    {
        // PUNCH: trigger the punch animation (animation events will call EnableHitbox/DisableHitbox)
        if (InputManager.Instance.BlutoPunchDown)
        {
            animator.SetTrigger("Punch"); // Transitions the Animator to the Punch state
            OnHeavyPunch?.Invoke();       // Notify VFXManager to trigger camera shake
        }
        // THROW BOTTLE: only if Bluto has at least 1 bottle in inventory
        // The actual bottle spawn happens in SpawnBottleProjectile() called by the Throw animation event
        else if (InputManager.Instance.BlutoThrowDown && currentBottles > 0)
        {
            animator.SetTrigger("Throw"); // Transitions to Throw state — SpawnBottleProjectile called at frame 5
        }
    }

    // ─── ANIMATION EVENTS ────────────────────────────────────────────────────
    // These methods are called by Animation Events embedded in the Animator clips.
    // They must be public and match the exact function name set in the Animation Event.

    // Called at frame 5 of Bluto_Throw animation — spawns the bottle projectile
    public void SpawnBottleProjectile()
    {
        if (currentBottles <= 0) return; // Safety check in case the count is wrong

        currentBottles--;                 // Consume 1 bottle from inventory
        OnBottleCountChanged?.Invoke();   // Notify UIManager to refresh bottle icons
        OnBottleThrow?.Invoke();          // Notify AudioManager to play throw sound

        // Instantiate the bottle at the ThrowPoint child position
        GameObject bottle = Instantiate(bottlePrefab, throwPoint.position, Quaternion.identity);

        // Read Bluto's facing direction from localScale.x sign (+1 = right, -1 = left)
        float direction = Mathf.Sign(transform.localScale.x);
        bottle.GetComponent<BottleItem>().InitializeProjectile(direction);
    }

    // Called at frame 5 of Bluto_Punch animation — activates the punch hitbox
    public void EnableHitbox()
    {
        if (meleeHitbox != null) meleeHitbox.enabled = true;
    }

    // Called at frame 10 of Bluto_Punch animation — deactivates the punch hitbox
    public void DisableHitbox()
    {
        if (meleeHitbox != null) meleeHitbox.enabled = false;
    }

    // ─── PHYSICS / GROUNDED CHECK ────────────────────────────────────────────

    private void CheckGrounded()
    {
        float rayLength = 0.3f;
        // Cast the ray from slightly above the bottom of the collider (to avoid self-intersection)
        Vector3 rayStart = new Vector3(col.bounds.center.x, col.bounds.min.y + 0.05f, col.bounds.center.z);

        // Exclude the Player layer so the ray can't detect the other player as "ground"
        int layerMask = ~LayerMask.GetMask("Player");

        isGrounded = Physics.Raycast(rayStart, Vector3.down, rayLength, layerMask);
        animator.SetBool("IsGrounded", isGrounded); // Keep the Animator in sync
    }

    // ─── PUBLIC API ──────────────────────────────────────────────────────────

    // Called by LadderTrigger to attach/detach Bluto from a ladder
    public void SetClimbing(bool state)
    {
        isClimbing = state;
        rb.useGravity = !state; // Disable gravity while climbing so Bluto doesn't slide down
        if (state) rb.linearVelocity = Vector3.zero; // Stop momentum when grabbing the ladder
        animator.SetBool("IsClimbing", state);
    }

    // Called by BottleItem when Bluto walks over a floor bottle pickup
    public void AddBottle()
    {
        if (currentBottles < maxBottles)
        {
            currentBottles++;
            OnBottleCountChanged?.Invoke(); // Refresh UI
        }
    }

    // Called by MeleeHitbox when Popeye (in spinach mode) punches Bluto
    // Applies the long spinach stun (10s) and fires the stun event
    public void ApplySpinachStun()
    {
        ApplyStun(10f);
        OnBlutoStunned?.Invoke(); // Can be used for VFX or audio feedback
    }

    // Generic stun: freezes Bluto's input for the specified duration
    // Called by SeaHagProjectile (1s), BottleItem indirect via GameManager, and ApplySpinachStun (10s)
    public void ApplyStun(float duration)
    {
        if (isStunned) return; // Ignore new stuns while already stunned (no stun extension)
        isStunned = true;
        animator.SetBool("IsStunned", true);
        rb.linearVelocity = Vector3.zero; // Stop all movement immediately
        // Invoke calls RecoverFromStun after 'duration' seconds (scheduled, not blocking)
        Invoke(nameof(RecoverFromStun), duration);
    }

    // Called automatically by Invoke after the stun duration expires
    private void RecoverFromStun()
    {
        isStunned = false;
        animator.SetBool("IsStunned", false);
    }
}
