// ScreenWrapTrigger.cs
using UnityEngine;

public class ScreenWrapTrigger : MonoBehaviour
{
    public Transform teleportDestination;
    private float cooldown = 0f;

    private void Update()
    {
        if (cooldown > 0) cooldown -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (cooldown <= 0 && (other.CompareTag("Player1") || other.CompareTag("Player2") || other.CompareTag("Item")))
        {
            Vector3 targetPos = teleportDestination.position;
            other.transform.position = new Vector3(targetPos.x, other.transform.position.y, other.transform.position.z);
            
            ScreenWrapTrigger destTrigger = teleportDestination.GetComponent<ScreenWrapTrigger>();
            if (destTrigger != null) destTrigger.cooldown = 0.5f;
        }
    }
}