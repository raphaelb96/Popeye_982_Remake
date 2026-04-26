// BottleItem.cs
using UnityEngine;
using System;

public class BottleItem : MonoBehaviour
{
    public float projectileSpeed = 8f;
    private bool isProjectile = false;
    private float moveDirection;

    public static event Action OnBottleSmashed;

    public void InitializePickup() => isProjectile = false;

    public void InitializeProjectile(float dir)
    {
        isProjectile = true;
        moveDirection = dir;
    }

    private void Update()
    {
        if (isProjectile)
        {
            transform.Translate(Vector3.right * moveDirection * projectileSpeed * Time.deltaTime);
            if (Mathf.Abs(transform.position.x) > 15f) Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isProjectile && other.CompareTag("Player2"))
        {
            other.GetComponent<BlutoController>().AddBottle();
            Destroy(gameObject);
        }
        else if (isProjectile && other.CompareTag("Player1"))
        {
            // שיתוק לחצי שנייה וללא ירידת חיים
            other.GetComponent<PopeyeController>().ApplyStun(0.5f);
            OnBottleSmashed?.Invoke();
            Destroy(gameObject);
        }
    }

    public void Smash()
    {
        OnBottleSmashed?.Invoke();
        Destroy(gameObject);
    }
}