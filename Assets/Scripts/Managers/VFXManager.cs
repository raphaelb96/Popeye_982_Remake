// VFXManager.cs
using UnityEngine;
using System.Collections;

public class VFXManager : MonoBehaviour
{
    public Camera mainCamera;

    private void OnEnable()
    {
        BlutoController.OnHeavyPunch += TriggerCameraShake;
    }

    private void OnDisable()
    {
        BlutoController.OnHeavyPunch -= TriggerCameraShake;
    }

    private void TriggerCameraShake()
    {
        StartCoroutine(ShakeRoutine(0.2f, 0.3f));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        Vector3 originalPos = mainCamera.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            mainCamera.transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mainCamera.transform.localPosition = originalPos;
    }
}