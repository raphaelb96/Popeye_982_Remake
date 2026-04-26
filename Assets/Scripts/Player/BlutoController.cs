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
    public Collider meleeHitbox; // שייך לכאן את הקוליידר של האגרוף מאובייקט הבן

    private Rigidbody rb;
    private CapsuleCollider col;
    private Animator animator;

    [Header("State")]
    public bool isGrounded;
    public bool isClimbing;
    private bool isStunned;
    private bool canMove = false;
    private Vector3 originalScale;

    // Events
    public static event Action OnHeavyPunch;
    public static event Action OnBottleThrow;
    public static event Action OnBottleCountChanged;
    public static event Action OnBlutoStunned;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();

        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        rb.useGravity = true;
        originalScale = transform.localScale;

        if (meleeHitbox != null) meleeHitbox.enabled = false;
    }

    private void OnEnable() => GameManager.OnGameStart += EnableMovement;
    private void OnDisable() => GameManager.OnGameStart -= EnableMovement;

    private void EnableMovement()
    {
        canMove = true;
        OnBottleCountChanged?.Invoke(); // עדכון ראשוני ל-UI
    }

    private void Update()
    {
        if (isStunned || !canMove) return;

        CheckGrounded();
        HandleMovement();
        HandleJumpAndDrop();
        HandleAction();
    }

    private void HandleMovement()
    {
        float moveX = InputManager.Instance.BlutoMove.x;

        if (isClimbing)
        {
            float moveY = InputManager.Instance.BlutoMove.y;
            // תנועה חופשית ב-4 כיוונים על הסולם
            rb.linearVelocity = new Vector3(moveX * moveSpeed, moveY * moveSpeed, 0);
        }
        else
        {
            rb.linearVelocity = new Vector3(moveX * moveSpeed, rb.linearVelocity.y, 0);
            animator.SetFloat("Speed", Mathf.Abs(moveX));

            if (moveX != 0)
            {
                transform.localScale = new Vector3(Mathf.Sign(moveX) * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
            }
        }
    }

    private void HandleJumpAndDrop()
    {
        // קפיצה
        if (isGrounded && InputManager.Instance.BlutoJumpDown && !isClimbing)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, 0);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // ירידה מפלטפורמה
        if (isGrounded && InputManager.Instance.BlutoDropDown)
        {
            Vector3 rayStart = new Vector3(col.bounds.center.x, col.bounds.min.y + 0.1f, col.bounds.center.z);
            RaycastHit hit;
            if (Physics.Raycast(rayStart, Vector3.down, out hit, 1.0f))
            {
                OneWayPlatform3D platform = hit.collider.GetComponent<OneWayPlatform3D>();
                if (platform != null) platform.FallThrough(col);
            }
        }
    }

    private void HandleAction()
    {
        if (InputManager.Instance.BlutoPunchDown)
        {
            animator.SetTrigger("Punch");
            OnHeavyPunch?.Invoke();
        }
        else if (InputManager.Instance.BlutoThrowDown && currentBottles > 0)
        {
            animator.SetTrigger("Throw");
        }
    }

    // --- Animation Events ---

    public void SpawnBottleProjectile()
    {
        if (currentBottles <= 0) return;

        currentBottles--;
        OnBottleCountChanged?.Invoke();
        OnBottleThrow?.Invoke();

        GameObject bottle = Instantiate(bottlePrefab, throwPoint.position, Quaternion.identity);
        float direction = Mathf.Sign(transform.localScale.x);
        bottle.GetComponent<BottleItem>().InitializeProjectile(direction);
    }

    public void EnableHitbox()
    {
        if (meleeHitbox != null) meleeHitbox.enabled = true;
    }

    public void DisableHitbox()
    {
        if (meleeHitbox != null) meleeHitbox.enabled = false;
    }

    // --- Logic ---

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
        }
    }

    public void ApplySpinachStun()
    {
        ApplyStun(10f);
        OnBlutoStunned?.Invoke();
    }

    public void ApplyStun(float duration)
    {
        if (isStunned) return;
        isStunned = true;
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