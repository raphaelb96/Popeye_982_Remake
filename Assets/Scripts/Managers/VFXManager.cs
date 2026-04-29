// VFXManager.cs
// Handles all visual effects in the game:
//   1. Camera shake — triggered by Bluto's heavy punch landing
//   2. Floating text — POW! / BAM! comic-style text that floats up from a world position
//
// To spawn floating text from another script:
//   VFXManager.Instance is NOT used here — call it directly via GetComponent or a serialized reference.
//   Better: add a static Instance and call VFXManager.Instance.SpawnFloatingText(pos, "POW!");
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    [Header("Camera Shake")]
    // How long the shake lasts in seconds
    public float shakeDuration = 0.2f;

    // How far the camera is displaced from its rest position each frame during shake
    public float shakeMagnitude = 0.15f;

    [Header("Floating Text")]
    // Drag the FloatingText prefab here in the Inspector
    // The prefab must have a FloatingText component and a TextMeshPro child
    public GameObject floatingTextPrefab;

    // ─── PRIVATE STATE ───────────────────────────────────────────────────────
    private Camera mainCamera;

    // The camera's resting position — shake displaces from this and returns to it
    private Vector3 originalCameraPos;

    // Countdown timer for the shake — decremented each frame, shake stops when it hits 0
    private float shakeTimer = 0f;

    // ─── UNITY LIFECYCLE ─────────────────────────────────────────────────────

    private void Awake()
    {
        mainCamera = Camera.main; // Cache the main camera reference once
        if (mainCamera != null)
            originalCameraPos = mainCamera.transform.localPosition; // Store its rest position
    }

    private void OnEnable()
    {
        // Subscribe to Bluto's heavy punch event — shake the camera when it fires
        BlutoController.OnHeavyPunch += TriggerShake;
    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent memory leaks if this object is disabled
        BlutoController.OnHeavyPunch -= TriggerShake;
    }

    private void Update()
    {
        if (shakeTimer > 0f)
        {
            // Random.insideUnitCircle returns a random point inside a unit circle (radius 1)
            // Multiplied by shakeMagnitude to set the maximum displacement distance
            // Cast to Vector3 (Z = 0) since camera moves in screen space (X and Y only)
            mainCamera.transform.localPosition = originalCameraPos + (Vector3)Random.insideUnitCircle * shakeMagnitude;

            // Count down the shake timer — shake lasts shakeDuration seconds total
            shakeTimer -= Time.deltaTime;
        }
        else if (mainCamera != null)
        {
            // Snap back to the original position when the shake is over (no lerp, instant)
            mainCamera.transform.localPosition = originalCameraPos;
        }
    }

    // ─── EVENT HANDLERS ──────────────────────────────────────────────────────

    // Resets the shake timer to start a new shake (or extend an ongoing one)
    // Called automatically by the BlutoController.OnHeavyPunch event
    private void TriggerShake()
    {
        shakeTimer = shakeDuration;
    }

    // ─── PUBLIC API ──────────────────────────────────────────────────────────

    // Spawns a floating text effect at a world position (e.g., above the hit character)
    // text: the string to display — typically "POW!", "BAM!", or "KA-POW!"
    // Call this from MeleeHitbox or PopeyeController after a successful hit
    public void SpawnFloatingText(Vector3 worldPosition, string text)
    {
        if (floatingTextPrefab == null) return; // Safety check — assign prefab in Inspector

        // Spawn slightly above the hit position so the text appears over the character's head
        GameObject fx = Instantiate(floatingTextPrefab, worldPosition + Vector3.up * 0.5f, Quaternion.identity);

        // Pass the text string to the FloatingText component so it can display it
        FloatingText ft = fx.GetComponent<FloatingText>();
        if (ft != null) ft.SetText(text);
    }
}
