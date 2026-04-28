// DeathZone.cs
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1"))
        {
            GameManager.Instance.TakeDamage();
            other.transform.position = new Vector3(0, 0, 0); 
        }
        else if (other.CompareTag("Player2"))
        {
            other.transform.position = new Vector3(0, 0, 0);
        }
        else if (other.GetComponent<BottleItem>() || other.GetComponent<HeartItem>() || other.GetComponent<SeaHagProjectile>())
        {
            Destroy(other.gameObject);
        }
    }
}