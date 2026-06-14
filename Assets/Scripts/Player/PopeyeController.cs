// PopeyeController.cs
// Controls all of Popeye's gameplay behaviour.
using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(Animator))]
public class PopeyeController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 8.5f;

    [Header("Combat Settings")]
    public Collider meleeHitbox;
    public float punchCooldown = 0.5f;   // FIX: minimum delay between punches

    private Rigidbody rb;
    private CapsuleCollider col;
    private Animator animator;

    [Header("State")]
    public bool isGrounded;
    public bool isClimbing;
    public bool isStunned;
    public bool isInvincible = false;
    private bool canMove = false;
    private float punchTimer = 0f;

    [Header("Stun Durations")]
    public float bottleStunDuration = 2f;

    // ─── AUDIO EVENTS ────────────────────────────────────────────────────────
    public static event Action OnPunch;
    public static event Action OnJump;
    public static event Action OnHit;
    public static event Action OnWalk;

    private float walkTimer = 0f;
    private float walkInterval = 0.3f;

    // Timer to prevent instant ground-detection immediately after jumping
    private float groundCheckCooldown = 0f;

    // ─── VISUAL & ROTATION CACHE ─────────────────────────────────────────────
    private Transform visualTransform;
    private Vector3 baseVisualScale;
    private Quaternion rightRotation;
    private Quaternion leftRotation;

    // ─── UNITY LIFECYCLE ─────────────────────────────────────────────────────

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();

        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        rb.useGravity = true;

        rightRotation = transform.rotation;
        leftRotation = transform.rotation * Quaternion.Euler(0, 180, 0);

        Billboard billboard = GetComponentInChildren<Billboard>();
        if (billboard != null)
        {
            visualTransform = billboard.transform;
            baseVisualScale = visualTransform.localScale;
        }

        if (meleeHitbox != null) meleeHitbox.enabled = false;
    }

    private void OnEnable()
    {
        GameManager.OnGameStart += EnableMovement;
        SpinachItem.OnSpinachEaten += ActivateSpinachMode;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= EnableMovement;
        SpinachItem.OnSpinachEaten -= ActivateSpinachMode;
    }

    private void EnableMovement() => canMove = true;

    private void Update()
    {
        if (isStunned || !canMove) return;

        // Manage ground detection delay after jumping
        if (groundCheckCooldown > 0f)
        {
            groundCheckCooldown -= Time.deltaTime;
        }
        else
        {
            CheckGrounded();
        }

        HandleMovement();
        HandleJumpAndDrop();
        HandleAction();
        if (punchTimer > 0f) punchTimer -= Time.deltaTime;

        if (isGrounded && rb.linearVelocity.magnitude > 0.1f && !isClimbing)
        {
            walkTimer -= Time.deltaTime;
            if (walkTimer <= 0f)
            {
                OnWalk?.Invoke();
                walkTimer = walkInterval;
            }
        }
        else
        {
            walkTimer = 0f;
        }
    }

    private void HandleMovement()
    {
        float moveX = InputManager.Instance.PopeyeMove.x;

        if (isClimbing)
        {
            float moveY = InputManager.Instance.PopeyeMove.y;
            rb.linearVelocity = new Vector3(moveX * moveSpeed, moveY * moveSpeed, 0);
        }
        else
        {
            rb.linearVelocity = new Vector3(moveX * moveSpeed, rb.linearVelocity.y, 0);
            animator.SetFloat("Speed", Mathf.Abs(moveX));

            if (moveX < 0)
            {
                transform.rotation = leftRotation;
                if (visualTransform != null)
                {
                    visualTransform.localScale = new Vector3(-Mathf.Abs(baseVisualScale.x), baseVisualScale.y, baseVisualScale.z);
                }
            }
            else if (moveX > 0)
            {
                transform.rotation = rightRotation;
                if (visualTransform != null)
                {
                    visualTransform.localScale = new Vector3(Mathf.Abs(baseVisualScale.x), baseVisualScale.y, baseVisualScale.z);
                }
            }
        }
    }

    private void HandleJumpAndDrop()
    {
        if (isGrounded && InputManager.Instance.PopeyeJumpDown && !isClimbing)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, 0);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            // Suspend physics checks to let the collider clear the floor
            isGrounded = false;
            animator.SetBool("IsGrounded", false);
            groundCheckCooldown = 0.1f;

            animator.SetTrigger("Jump");
            OnJump?.Invoke();
        }

        if (isGrounded && InputManager.Instance.PopeyeDropDown)
        {
            Vector3 rayStart = new Vector3(col.bounds.center.x, col.bounds.min.y + 0.1f, col.bounds.center.z);
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 1.0f))
            {
                OneWayPlatform3D platform = hit.collider.GetComponent<OneWayPlatform3D>();
                if (platform != null) platform.FallThrough(col);
            }
        }
    }

    private void HandleAction()
    {
        if (InputManager.Instance.PopeyePunchDown && punchTimer <= 0f)
        {
            animator.SetTrigger("Punch");
            OnPunch?.Invoke();
            punchTimer = punchCooldown;
        }
    }

    public void TakeDamage(int amount, Vector3 knockbackVector)
    {
        if (isInvincible || isStunned) return;

        GameManager.Instance.TakeDamage();

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(knockbackVector, ForceMode.Impulse);

        ApplyStun(0.5f);
    }

    public void EnableHitbox()
    {
        if (meleeHitbox != null) meleeHitbox.enabled = true;
    }

    public void DisableHitbox()
    {
        if (meleeHitbox != null) meleeHitbox.enabled = false;
    }

    private void CheckGrounded()
    {
        float rayLength = 0.3f;
        Vector3 rayStart = new Vector3(col.bounds.center.x, col.bounds.min.y + 0.05f, col.bounds.center.z);
        int layerMask = ~LayerMask.GetMask("Player");

        isGrounded = Physics.Raycast(rayStart, Vector3.down, rayLength, layerMask);
        animator.SetBool("IsGrounded", isGrounded);
    }

    public void SetClimbing(bool state)
    {
        isClimbing = state;
        rb.useGravity = !state;
        if (state) rb.linearVelocity = Vector3.zero;
        animator.SetBool("IsClimbing", state);
    }

    public void ApplyStun(float duration)
    {
        if (isStunned || isInvincible) return;
        isStunned = true;
        OnHit?.Invoke();
        animator.SetTrigger("Hit");
        animator.SetBool("IsStunned", true);
        StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        // Keep the stun animation until Popeye is back on the ground
        while (!isGrounded) { CheckGrounded(); yield return null; }
        isStunned = false;
        animator.SetBool("IsStunned", false);
    }

    private void ActivateSpinachMode()
    {
        if (isInvincible) return;
        isInvincible = true;

        // Halt momentum and block input for the eating animation
        rb.linearVelocity = Vector3.zero;
        canMove = false;
        animator.SetTrigger("EatSpinach");

        // Hold the freeze for 1 second, then apply the buff
        Invoke(nameof(FinishEatingSpinach), 1f);
    }

    private void FinishEatingSpinach()
    {
        canMove = true;
        moveSpeed *= 2f;

        // 9 seconds remaining on the active buff
        Invoke(nameof(DeactivateSpinachMode), 9f);
    }

    private void DeactivateSpinachMode()
    {
        isInvincible = false;
        moveSpeed /= 2f;
    }
}