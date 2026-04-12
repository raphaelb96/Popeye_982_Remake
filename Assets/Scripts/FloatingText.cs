// FloatingText.cs
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public float destroyTime = 1f;
    public float floatSpeed = 2f;

    private void Start() => Destroy(gameObject, destroyTime);
    private void Update() => transform.position += Vector3.up * floatSpeed * Time.deltaTime;
}