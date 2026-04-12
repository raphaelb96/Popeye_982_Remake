// SeaHagProjectile.cs 
using UnityEngine;

public class SeaHagProjectile : MonoBehaviour
{
    public float speed = 6f;
    private float moveDirection;

    public void Initialize(float dir)
    {
        moveDirection = dir;
    }

    private void Update()
    {
        transform.Translate(Vector3.right * moveDirection * speed * Time.deltaTime);
        if (Mathf.Abs(transform.position.x) > 15f) Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1"))
        {
            other.GetComponent<PopeyeController>().ApplyStun(1f);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Player2"))
        {
            other.GetComponent<BlutoController>().ApplyStun(1f);
            Destroy(gameObject);
        }
    }

    public void Smash()
    {
        Destroy(gameObject);
    }
}