// SpinachItem.cs
using UnityEngine;
using System;

public class SpinachItem : MonoBehaviour
{
    public static event Action OnSpinachEaten;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1")) 
        {
            OnSpinachEaten?.Invoke();
            Destroy(gameObject);
        }
    }

    public void DestroyByBluto()
    {
        Destroy(gameObject);
    }
}