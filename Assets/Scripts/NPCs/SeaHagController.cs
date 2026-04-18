// SeaHagController.cs 
using UnityEngine;
using System.Collections;

public class SeaHagController : MonoBehaviour
{
    public GameObject magicProjectilePrefab;
    public Transform leftSpawnPoint;
    public Transform rightSpawnPoint;
    
    public float minSpawnTime = 5f;
    public float maxSpawnTime = 12f;
    
    private bool canAct = false;

    private void OnEnable() => GameManager.OnGameStart += StartSpawning;
    private void OnDisable() => GameManager.OnGameStart -= StartSpawning;

    private void StartSpawning()
    {
        canAct = true;
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (canAct)
        {
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);
            SpawnAndAttack();
        }
    }

    private void SpawnAndAttack()
    {
        bool isLeft = Random.Range(0, 2) == 0;
        Transform spawnPoint = isLeft ? leftSpawnPoint : rightSpawnPoint;
        float direction = isLeft ? 1f : -1f;

        transform.position = spawnPoint.position;
        transform.localScale = new Vector3(direction, 1, 1);

        GameObject projectile = Instantiate(magicProjectilePrefab, transform.position, Quaternion.identity);
        projectile.GetComponent<SeaHagProjectile>().Initialize(direction);
    }
}