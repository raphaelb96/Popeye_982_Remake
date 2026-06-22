// VFXManager.cs
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    [Header("Screen Shake")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.15f;

    [Header("Floating Text")]
    public GameObject floatingTextPrefab;

    [Header("Hit Sprites")]
    public GameObject popeyeHitSpritePrefab;   // POW
    public GameObject blutoHitSpritePrefab;    // BAM
    public float hitSpriteLifetime = 0.4f;

    public void SpawnFloatingText(Vector3 position, string text)
    {
        if (floatingTextPrefab != null)
        {
            GameObject floatingText = Instantiate(floatingTextPrefab, position, Quaternion.identity);
            // Assuming your floating text script has an initialization method, it would be called here
        }
    }

    public void SpawnHitSprite(Vector3 position, GameObject spritePrefab)
    {
        if (spritePrefab == null) return;

        // Spawn it slightly offset up and toward the camera so it does not clip into the character
        Vector3 spawnPos = position + new Vector3(0, 1.5f, -1f);
        GameObject hitEffect = Instantiate(spritePrefab, spawnPos, Quaternion.identity);

        // Destroy the sprite after the designated lifetime
        Destroy(hitEffect, hitSpriteLifetime);
    }
}