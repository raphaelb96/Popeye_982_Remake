// BlutoController.cs
// Controls all of Bluto's gameplay behaviour.
using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(Animator))]
public class BlutoController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float jumpForce = 8f;

    [Header("Inventory")]
    public int maxBottles = 5;
    public int currentBottles = 5;

    [Header("References")]
    public Transform throwPoint;
    public GameObject bottlePrefab;
    public Collider meleeHitbox;

    private Rigidbody rb;
    private CapsuleCollider col;
    private Animator animator;

    [Header("State")]
    public bool isGrounded;
    public bool isClimbing;
    private bool isStunned;
    private float punchTimer = 0f;
    private bool canMove = false;
    private float facingDirection = 1f;

    [Header("Stun Durations")]
    public float normalStunDuration = 1f;
    public float spinachStunDuration = 5f;
    public float punchCooldown = 1f;

    // ─── AUDIO EVENTS ────────────────────────────────────────────────────────
    public static event Action OnHeavyPunch;
    public static event Action OnThrowBottle;
    public static event Action OnBottleCountChanged;
    public static event Action OnBlutoStunned;
    public static event Action OnJump;
    public static event Action OnHit;
    public static event Action OnBottleCollected;
    public static event Action OnWalk;

    private float walkTimer = 0f;
    private float walkInterval = 0.35f;

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

    private void OnEnable() => GameManager.OnGameStart += EnableMovement;
    private void OnDisable() => GameManager.OnGameStart -= EnableMovement;

    private void EnableMovement()
    {
        canMove = true;
        OnBottleCountChanged?.Invoke();
    }

    private void Update()
    {
        if (isStunned || !canMove) return;

        CheckGrounded();
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
        float moveX = InputManager.Instance.BlutoMove.x;

        if (isClimbing)
        {
            float moveY = InputManager.Instance.BlutoMove.y;
            rb.linearVelocity = new Vector3(moveX * moveSpeed, moveY * moveSpeed, 0);
        }
        else
        {
            rb.linearVelocity = new Vector3(moveX * moveSpeed, rb.linearVelocity.y, 0);
            animator.SetFloat("Speed", Mathf.Abs(moveX));

            if (moveX < 0)
            {
                transform.rotation = leftRotation;
                facingDirection = -1f;
                if (visualTransform != null)
                {
                    visualTransform.localScale = new Vector3(-Mathf.Abs(baseVisualScale.x), baseVisualScale.y, baseVisualScale.z);
                }
            }
            else if (moveX > 0)
            {
                transform.rotation = rightRotation;
                facingDirection = 1f;
                if (visualTransform != null)
                {
                    visualTransform.localScale = new Vector3(Mathf.Abs(baseVisualScale.x), baseVisualScale.y, baseVisualScale.z);
                }
            }
        }
    }

    private void HandleJumpAndDrop()
    {
        if (isGrounded && InputManager.Instance.BlutoJumpDown && !isClimbing)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, 0);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            OnJump?.Invoke();
        }

        if (isGrounded && InputManager.Instance.BlutoDropDown)
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
        if (InputManager.Instance.BlutoPunchDown && punchTimer <= 0f)
        {
            animator.SetTrigger("Punch");
            OnHeavyPunch?.Invoke();
            punchTimer = punchCooldown;
        }
        else if (InputManager.Instance.BlutoThrowDown && currentBottles > 0)
        {
            animator.SetTrigger("Throw");
        }
    }

    public void SpawnBottleProjectile()
    {
        if (currentBottles <= 0) return;

        currentBottles--;
        OnBottleCountChanged?.Invoke();
        OnThrowBottle?.Invoke();

        GameObject bottle = Instantiate(bottlePrefab, throwPoint.position, Quaternion.identity);
        bottle.GetComponent<BottleItem>().InitializeProjectile(facingDirection);
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

    public void AddBottle()
    {
        if (currentBottles < maxBottles)
        {
            currentBottles++;
            OnBottleCountChanged?.Invoke();
            OnBottleCollected?.Invoke();
        }
    }

    public void ApplyStun(float duration)
    {
        if (isStunned) return;
        isStunned = true;
        OnHit?.Invoke();
        OnBlutoStunned?.Invoke();
        animator.SetBool("IsStunned", true);
        rb.linearVelocity = Vector3.zero;
        Invoke(nameof(RecoverFromStun), duration);
    }

    private void RecoverFromStun()
    {
        isStunned = false;
        animator.SetBool("IsStunned", false);
    }
}