// Billboard.cs
// Makes any 3D GameObject always face the camera, regardless of the camera's rotation.
// This gives the 2.5D visual effect: sprites on flat quads always appear front-facing
// even when the camera is slightly angled. Attach this to every character and item.
//
// LateUpdate is used (not Update) to ensure the camera has finished moving
// before we copy its rotation — prevents one-frame jitter artifacts.
using UnityEngine;

public class Billboard : MonoBehaviour
{
    // Cache the main camera reference to avoid Camera.main lookup every frame (expensive)
    private Camera mainCamera;

    // Optional rotation correction applied on top of the camera rotation.
    // Use this if the sprite appears tilted (e.g., enter Y = 90 or Y = -90 in the Inspector
    // to fix characters that stand sideways after Billboard is applied).
    [Tooltip("Enter Y = 90 or Y = -90 if characters appear sideways after applying Billboard")]
    public Vector3 offsetRotation = Vector3.zero;

    // ─── UNITY LIFECYCLE ─────────────────────────────────────────────────────

    private void Start()
    {
        // Cache once at startup — Camera.main searches the scene every call if not cached
        mainCamera = Camera.main;
    }

    // LateUpdate runs after all Update() calls have finished for this frame
    // This guarantees the camera has already moved before we copy its rotation
    private void LateUpdate()
    {
        // Copy the camera's exact world rotation, then apply the optional offset correction
        // Quaternion * Quaternion = combine two rotations sequentially
        // Quaternion.Euler(offsetRotation) converts the Vector3 offset to a rotation
        transform.rotation = mainCamera.transform.rotation * Quaternion.Euler(offsetRotation);
    }
}
