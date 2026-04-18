// PopeyeController.cs
using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(Animator))]
public class PopeyeController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    
    private Rigidbody rb;
    private CapsuleCollider col;
    private Animator animator;

    private bool isGrounded;
    private bool isClimbing;
    private bool isStunned;
    private bool canMove = false;
    
    public bool isInvincible = false;
    private float originalSpeed;

    public static event Action OnPunch;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        
        originalSpeed = moveSpeed;
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
            rb.linearVelocity = new Vector3(0, moveY * moveSpeed, 0);
        }
        else
        {
            rb.linearVelocity = new Vector3(moveX * moveSpeed, rb.linearVelocity.y, 0);
            animator.SetFloat("Speed", Mathf.Abs(moveX));
            if (moveX != 0) transform.localScale = new Vector3(Mathf.Sign(moveX), 1, 1);
        }
    }

    private void HandleJumpAndDrop()
    {
        CheckGrounded();

        if (isGrounded && InputManager.Instance.PopeyeJumpDown && !isClimbing)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        if (isGrounded && InputManager.Instance.PopeyeDropDown)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f))
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

    private void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.2f);
    }

    public void SetClimbing(bool state)
    {
        isClimbing = state;
        rb.useGravity = !state;
        if (state) rb.linearVelocity = Vector3.zero;
    }

    public void ApplyStun(float duration)
    {
        if (isStunned || isInvincible) return; 
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

    private void ActivateSpinachMode()
    {
        isInvincible = true;
        moveSpeed = originalSpeed * 2f;
        Invoke(nameof(DeactivateSpinachMode), 10f);
    }

    private void DeactivateSpinachMode()
    {
        isInvincible = false;
        moveSpeed = originalSpeed;
    }
}