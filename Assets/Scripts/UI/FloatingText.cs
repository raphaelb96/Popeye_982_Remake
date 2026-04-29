// FloatingText.cs
// A short-lived UI text effect that floats upward and then destroys itself.
// Used for comic-style hit feedback: "POW!", "BAM!", "KA-POW!" etc.
// Spawned by VFXManager.SpawnFloatingText() at a world position above the hit point.
//
// Requires a TextMeshPro component on the same GameObject (or a child) to display the text.
// The prefab should be set up as a World Space Canvas or a 3D TextMeshPro object
// so it appears in the game world rather than on the screen overlay.
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    // How many seconds before the text object destroys itself (default: 1 second)
    public float destroyTime = 1f;

    // Upward movement speed in world units per second
    public float floatSpeed = 2f;

    // Reference to the TextMeshPro component that renders the text string
    // Cached in Awake to avoid repeated GetComponent calls in Update
    private TextMeshPro tmpText;

    // ─── UNITY LIFECYCLE ─────────────────────────────────────────────────────

    private void Awake()
    {
        // Cache the TextMeshPro component — can be on this GameObject or a child
        tmpText = GetComponentInChildren<TextMeshPro>();
    }

    private void Start()
    {
        // Schedule self-destruction after destroyTime seconds
        // Destroy() with a delay is cleaner than tracking a timer manually
        Destroy(gameObject, destroyTime);
    }

    private void Update()
    {
        // Move upward every frame at floatSpeed units/second
        // Vector3.up = (0, 1, 0) — straight up in world space
        // * Time.deltaTime makes the movement frame-rate independent
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
    }

    // ─── PUBLIC API ──────────────────────────────────────────────────────────

    // Called by VFXManager.SpawnFloatingText() immediately after Instantiate()
    // Sets the displayed text string (e.g., "POW!", "BAM!")
    public void SetText(string text)
    {
        if (tmpText != null) tmpText.text = text;
    }
}
