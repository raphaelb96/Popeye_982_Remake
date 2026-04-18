// OneWayPlatform3D.cs
using UnityEngine;
using System.Collections;

public class OneWayPlatform3D : MonoBehaviour
{
    private Collider platformCollider;

    private void Awake()
    {
        platformCollider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Player2"))
        {
            float characterBottom = collision.collider.bounds.min.y;
            float platformTop = platformCollider.bounds.max.y;

            if (characterBottom < platformTop - 0.1f) 
            {
                Physics.IgnoreCollision(collision.collider, platformCollider, true);
                StartCoroutine(RestoreCollision(collision.collider));
            }
        }
    }

    private IEnumerator RestoreCollision(Collider playerCollider)
    {
        yield return new WaitUntil(() => 
            playerCollider == null || 
            playerCollider.bounds.min.y > platformCollider.bounds.max.y || 
            playerCollider.transform.position.y < transform.position.y - 2f
        );
        
        if (playerCollider != null)
        {
            Physics.IgnoreCollision(playerCollider, platformCollider, false);
        }
    }

    public void FallThrough(Collider playerCollider)
    {
        StartCoroutine(FallRoutine(playerCollider));
    }

    private IEnumerator FallRoutine(Collider playerCollider)
    {
        Physics.IgnoreCollision(playerCollider, platformCollider, true);
        yield return new WaitForSeconds(0.5f); 
        if (playerCollider != null)
        {
            Physics.IgnoreCollision(playerCollider, platformCollider, false);
        }
    }
}