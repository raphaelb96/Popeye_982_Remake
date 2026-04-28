// OliveController.cs
using UnityEngine;

public class OliveController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float leftBound = -8f;
    public float rightBound = 8f;
    private int direction = 1;

    public GameObject heartPrefab;
    public GameObject bottlePrefab;
    public GameObject spinachPrefab;

    public Transform[] spinachSpawnPoints;
    public Transform[] bottleSpawnPoints;

    private int heartsThrown = 0;
    private float heartTimer = 0f;
    private bool canAct = false;

    private Vector3 originalScale; // הוספת משתנה לשמירת הגודל המקורי

    private void Awake()
    {
        originalScale = transform.localScale; // שמירת הגודל מה-Inspector
    }

    private void OnEnable() => GameManager.OnGameStart += () => canAct = true;

    private void Update()
    {
        if (!canAct) return;

        MoveOlive();
        HandleSpawning();
    }

    private void MoveOlive()
    {
        transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime);
        if (transform.position.x > rightBound) direction = -1;
        else if (transform.position.x < leftBound) direction = 1;

        // שימוש בגודל המקורי המוחלט כפול הכיוון, ושמירה על Y ו-Z מקוריים
        transform.localScale = new Vector3(Mathf.Abs(originalScale.x) * direction, originalScale.y, originalScale.z);
    }

    private void HandleSpawning()
    {
        heartTimer += Time.deltaTime;
        if (heartTimer >= 2f)
        {
            heartTimer = 0f;
            SpawnHeart();
        }
    }

    private void SpawnHeart()
    {
        Instantiate(heartPrefab, transform.position, Quaternion.identity);
        heartsThrown++;

        if (heartsThrown % 2 == 0) SpawnBottle();
        if (heartsThrown % 5 == 0) SpawnSpinach();
    }

    private void SpawnBottle()
    {
        Transform spawnPoint = bottleSpawnPoints[Random.Range(0, bottleSpawnPoints.Length)];
        GameObject bottle = Instantiate(bottlePrefab, spawnPoint.position, Quaternion.identity);
        bottle.GetComponent<BottleItem>().InitializePickup();
    }

    private void SpawnSpinach()
    {
        Transform spawnPoint = spinachSpawnPoints[Random.Range(0, spinachSpawnPoints.Length)];
        Instantiate(spinachPrefab, spawnPoint.position, Quaternion.identity);
    }
}