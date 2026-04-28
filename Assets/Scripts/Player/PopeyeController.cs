using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(Animator))]
public class PopeyeController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 8.5f;

    [Header("Combat Settings")]
    public Collider meleeHitbox; // שייך לכאן את הקוליידר של האגרוף מאובייקט הבן

    private Rigidbody rb;
    private CapsuleCollider col;
    private Animator animator;

    [Header("State")]
    public bool isGrounded;
    public bool isClimbing;
    public bool isStunned;
    public bool isInvincible = false;
    private bool canMove = false;
    private Vector3 originalScale;

    // אירועים (Events)
    public static event Action OnPunch;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();

        // הגדרות פיזיקה בסיסיות למניעת נפילה מהמסלול
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        rb.useGravity = true;

        originalScale = transform.localScale;

        // כיבוי ראשוני של ה-Hitbox
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
    }

    private void HandleMovement()
    {
        float moveX = InputManager.Instance.PopeyeMove.x;

        if (isClimbing)
        {
            float moveY = InputManager.Instance.PopeyeMove.y;
            // תנועה חופשית ב-4 כיוונים על הסולם
            rb.linearVelocity = new Vector3(moveX * moveSpeed, moveY * moveSpeed, 0);
        }
        else
        {
            // תנועה רגילה על הקרקע
            rb.linearVelocity = new Vector3(moveX * moveSpeed, rb.linearVelocity.y, 0);
            animator.SetFloat("Speed", Mathf.Abs(moveX));

            if (moveX != 0)
            {
                // שמירה על ה-Scale המקורי ושינוי הכיוון בלבד
                transform.localScale = new Vector3(Mathf.Sign(moveX) * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
            }
        }
    }

    private void HandleJumpAndDrop()
    {
        // קפיצה - רק אם על הקרקע ולא מטפס
        if (isGrounded && InputManager.Instance.PopeyeJumpDown && !isClimbing)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, 0); // איפוס מהירות אנכית
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // ירידה דרך פלטפורמה (One-Way)
        if (isGrounded && InputManager.Instance.PopeyeDropDown)
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
        if (InputManager.Instance.PopeyePunchDown)
        {
            animator.SetTrigger("Punch");
            OnPunch?.Invoke();
        }
    }

    // --- מערכת נזק ורתיעה (Knockback) ---
    public void TakeDamage(int amount, Vector3 knockbackVector)
    {
        if (isInvincible || isStunned) return;

        // עדכון GameManager
        GameManager.Instance.TakeDamage();

        // הפעלת כוח פיזי (Knockback)
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(knockbackVector, ForceMode.Impulse);

        // כניסה להלם קצר
        ApplyStun(0.5f);
        animator.SetTrigger("Hit");
    }

    // --- Animation Events (קריאות מהאנימטור) ---
    public void EnableHitbox()
    {
        if (meleeHitbox != null) meleeHitbox.enabled = true;
    }

    public void DisableHitbox()
    {
        if (meleeHitbox != null) meleeHitbox.enabled = false;
    }

    // --- בדיקות לוגיות ---
    private void CheckGrounded()
    {
        // בדיקה מתחת לרגליים בעזרת גבולות הקוליידר
        float rayLength = 0.3f;
        Vector3 rayStart = new Vector3(col.bounds.center.x, col.bounds.min.y + 0.05f, col.bounds.center.z);

        // התעלמות משכבת השחקנים (Player Layer)
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
        animator.SetBool("IsStunned", true);
        Invoke(nameof(RecoverFromStun), duration);
    }

    private void RecoverFromStun()
    {
        isStunned = false;
        animator.SetBool("IsStunned", false);
    }

    // --- מצב תרד (Spinach Mode) ---
    private void ActivateSpinachMode()
    {
        isInvincible = true;
        moveSpeed *= 2f;
        // אפשר להוסיף כאן שינוי צבע או אפקט ויזואלי
        Invoke(nameof(DeactivateSpinachMode), 10f);
    }

    private void DeactivateSpinachMode()
    {
        isInvincible = false;
        moveSpeed /= 2f;
    }
}