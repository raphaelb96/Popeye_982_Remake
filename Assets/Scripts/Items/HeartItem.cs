// HeartItem.cs
using UnityEngine;
using System;

public class HeartItem : MonoBehaviour
{
    public float fallSpeed = 1f;
    public float flutterSpeed = 2f;
    public float flutterMagnitude = 0.5f;
    private float startX;

    public static event Action OnHeartCollected;

    private void Start() => startX = transform.position.x;

    private void Update()
    {
        float newX = startX + Mathf.Sin(Time.time * flutterSpeed) * flutterMagnitude;
        transform.position = new Vector3(newX, transform.position.y - fallSpeed * Time.deltaTime, transform.position.z);

        if (transform.position.y < -10f) Destroy(gameObject); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1")) 
        {
            GameManager.Instance.AddHeart();
            OnHeartCollected?.Invoke();
            Destroy(gameObject);
        }
    }
}