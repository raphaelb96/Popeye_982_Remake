// PopeyeController.cs
// Controls all of Popeye's gameplay behaviour.
using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(Animator))]
public class PopeyeController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 8.5f;

    [Header("Combat Settings")]
    public Collider meleeHitbox;

    private Rigidbody rb;
    private CapsuleCollider col;
    private Animator animator;

    [Header("State")]
    public bool isGrounded;
    public bool isClimbing;
    public bool isStunned;
    public bool isInvincible = false;
    private bool canMove = false;

    [Header("Stun Durations")]
    public float bottleStunDuration = 2f;

    // ─── AUDIO EVENTS ────────────────────────────────────────────────────────
    public static event Action OnPunch;
    public static event Action OnJump;
    public static event Action OnHit;
    public static event Action OnWalk;

    private float walkTimer = 0f;
    private float walkInterval = 0.3f;

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

        // Find the specific child that holds the visuals and capture its scale
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

        CheckGrounded();
        HandleMovement();
        HandleJumpAndDrop();
        HandleAction();

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
                transform.rotation = leftRotation; // Swing the physics hitboxes left
                if (visualTransform != null)
                {
                    // Scale the visual child negatively to mirror the shader/texture
                    visualTransform.localScale = new Vector3(-Mathf.Abs(baseVisualScale.x), baseVisualScale.y, baseVisualScale.z);
                }
            }
            else if (moveX > 0)
            {
                transform.rotation = rightRotation; // Swing the physics hitboxes right
                if (visualTransform != null)
                {
                    // Restore the visual child scale to face right
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
        if (InputManager.Instance.PopeyePunchDown)
        {
            animator.SetTrigger("Punch");
            OnPunch?.Invoke();
        }
    }

    public void TakeDamage(int amount, Vector3 knockbackVector)
    {
        if (isInvincible || isStunned) return;

        GameManager.Instance.TakeDamage();

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(knockbackVector, ForceMode.Impulse);

        ApplyStun(0.5f);
        animator.SetTrigger("Hit");
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
        animator.SetBool("IsStunned", true);
        Invoke(nameof(RecoverFromStun), duration);
    }

    private void RecoverFromStun()
    {
        isStunned = false;
        animator.SetBool("IsStunned", false);
    }

    private void ActivateSpinachMode()
    {
        if (isInvincible) return;
        isInvincible = true;
        moveSpeed *= 2f;
        Invoke(nameof(DeactivateSpinachMode), 10f);
    }

    private void DeactivateSpinachMode()
    {
        isInvincible = false;
        moveSpeed /= 2f;
    }
}