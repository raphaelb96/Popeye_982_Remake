// BlutoController.cs
using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(Animator))]
public class BlutoController : MonoBehaviour
{
    public float moveSpeed = 4f; 
    public float jumpForce = 7f;
    public int maxBottles = 5;
    public int currentBottles = 5;
    
    public Transform throwPoint;
    public GameObject bottlePrefab;

    private Rigidbody rb;
    private CapsuleCollider col;
    private Animator animator;

    private bool isGrounded;
    private bool isClimbing;
    private bool isStunned;
    private bool canMove = false;

    public static event Action OnHeavyPunch;
    public static event Action OnBottleThrow;
    public static event Action OnBlutoStunned; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
    }

    private void OnEnable() => GameManager.OnGameStart += EnableMovement;
    private void OnDisable() => GameManager.OnGameStart -= EnableMovement;
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
        float moveX = InputManager.Instance.BlutoMove.x;

        if (isClimbing)
        {
            float moveY = InputManager.Instance.BlutoMove.y;
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
        if (isGrounded && InputManager.Instance.BlutoJumpDown && !isClimbing)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        if (isGrounded && InputManager.Instance.BlutoDropDown)
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
        if (InputManager.Instance.BlutoPunchDown)
        {
            animator.SetTrigger("Punch");
            OnHeavyPunch?.Invoke();
        }
        else if (InputManager.Instance.BlutoThrowDown && currentBottles > 0)
        {
            animator.SetTrigger("Throw");
            currentBottles--;
            OnBottleThrow?.Invoke();
        }
    }

    public void SpawnBottleProjectile()
    {
        GameObject bottle = Instantiate(bottlePrefab, throwPoint.position, Quaternion.identity);
        bottle.GetComponent<BottleItem>().InitializeProjectile(Mathf.Sign(transform.localScale.x));
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

    public void AddBottle()
    {
        if (currentBottles < maxBottles) currentBottles++;
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