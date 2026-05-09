// HeartItem.cs
// A heart collectible thrown by Olive that floats down toward the ground with a gentle side-to-side flutter.
// When Popeye (Player1) touches it, the GameManager increments the heart counter toward the 24-heart win condition.
// If a heart falls below Y = -10 without being collected, it destroys itself to avoid clutter.
using UnityEngine;
using System;

public class HeartItem : MonoBehaviour
{
    // Vertical fall speed in units per second
    public float fallSpeed = 1f;

    // How fast the heart oscillates left and right (radians per second, passed to Mathf.Sin)
    public float flutterSpeed = 2f;

    // Maximum horizontal offset from the starting X position (amplitude of the side-to-side swing)
    public float flutterMagnitude = 0.5f;

    // Stores the X position at spawn time — the flutter oscillates around this fixed point
    private float startX;

    // Prevents double-collection if OnTriggerEnter fires multiple times in a single frame
    private bool isCollected = false;

    // Static event fired when Popeye successfully collects this heart
    // Subscribers: GameManager.AddHeart(), AudioManager (plays ding), UIManager (updates counter)
    public static event Action OnHeartCollected;

    // ─── UNITY LOOP ──────────────────────────────────────────────────────────

    // Cache the starting X position before the first Update runs
    private void Start() => startX = transform.position.x;

    private void Update()
    {
        // Mathf.Sin returns a value oscillating between -1 and +1 over time
        // Multiplied by flutterMagnitude to set the maximum horizontal offset
        float newX = startX + Mathf.Sin(Time.time * flutterSpeed) * flutterMagnitude;

        // Move the heart: flutter horizontally + fall vertically
        // fallSpeed * Time.deltaTime converts units/sec → units/frame
        transform.position = new Vector3(
            newX,
            transform.position.y - fallSpeed * Time.deltaTime,
            transform.position.z
        );

        // Auto-destroy if the heart falls below the visible stage (Y < -10)
        // This prevents an infinite build-up of uncollected hearts in memory
        if (transform.position.y < -10f) Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // If it was already collected this frame, ignore further collisions
        if (isCollected) return;

        // Only Popeye (Player1) can collect hearts — Bluto cannot
        if (other.CompareTag("Player1"))
        {
            isCollected = true;              // Lock to prevent duplicate collection
            GameManager.Instance.AddHeart(); // Increment heart counter; triggers win if count reaches 24
            OnHeartCollected?.Invoke();      // Notify AudioManager, UIManager, etc.
            Destroy(gameObject);             // Remove the heart from the scene after collection
        }
    }
}