using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Configuration")]
    public int playerIndex = 0;        // 0 = Popeye, 1 = Bluto
    public float moveSpeed = 5f;       // Movement speed

    [Header("Components")]
    private Rigidbody rb;              // Player Rigidbody

    [Header("State")]
    private float moveInput;           // -1 = left, 0 = idle, 1 = right

    void Start()
    {
        // Get the Rigidbody component attached to this GameObject
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Read input based on player index
        if (playerIndex == 0)
        {
            // Popeye — WASD
            moveInput = Input.GetAxisRaw("Horizontal");
        }
        else
        {
            // Bluto — Arrow keys
            moveInput = Input.GetAxisRaw("Horizontal2");
        }
    }

    void FixedUpdate()
    {
        // Apply horizontal movement to Rigidbody
        Vector3 movement = new Vector3(moveInput * moveSpeed, rb.linearVelocity.y, 0f);
        rb.linearVelocity = movement;
    }
}