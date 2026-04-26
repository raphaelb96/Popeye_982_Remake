// Billboard.cs
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    [Tooltip("הזן 90 או -90 בציר ה-Y אם הדמויות עדיין עומדות על הצד")]
    public Vector3 offsetRotation = Vector3.zero;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        // מעתיק את זווית המצלמה במלואה ומוסיף את תיקון הסטייה אם נדרש
        transform.rotation = mainCamera.transform.rotation * Quaternion.Euler(offsetRotation);
    }
}