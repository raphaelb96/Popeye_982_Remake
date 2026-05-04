// SeaHagController.cs
// Controls the Sea Hag NPC: she appears off-screen at random intervals,
// fires a magic projectile across the stage, then hides again.
// She alternates randomly between the left and right spawn points each attack.
// The projectile stuns both players on contact (it's an environmental hazard, not targeted).
using UnityEngine;
using System.Collections;

public class SeaHagController : MonoBehaviour
{
    // ─── REFERENCES ──────────────────────────────────────────────────────────
    // The projectile prefab to instantiate when the Sea Hag attacks (assign in Inspector)
    public GameObject magicProjectilePrefab;

    // Off-screen position on the left side of the stage (X = -16, Y = 6, Z = 0)
    public Transform leftSpawnPoint;

    // Off-screen position on the right side of the stage (X = 25, Y = 6, Z = 0)
    public Transform rightSpawnPoint;

    // ─── TIMING ──────────────────────────────────────────────────────────────
    // Minimum seconds between attacks — controls the lowest attack frequency
    public float minSpawnTime = 5f;

    // Maximum seconds between attacks — creates unpredictable timing
    public float maxSpawnTime = 12f;

    // Gate: Sea Hag only acts after GameManager.OnGameStart fires
    private bool canAct = false;

    // ─── EVENT SUBSCRIPTIONS ─────────────────────────────────────────────────

    private void OnEnable()  => GameManager.OnGameStart += StartSpawning;
    private void OnDisable() => GameManager.OnGameStart -= StartSpawning;

    // ─── LOGIC ───────────────────────────────────────────────────────────────

    // Called when OnGameStart fires — enables the Sea Hag and starts the attack loop
    private void StartSpawning()
    {
        canAct = true;
        StartCoroutine(SpawnRoutine()); // Begin the infinite random-wait → attack loop
    }

    // Coroutine: waits a random duration then attacks, then repeats forever
    private IEnumerator SpawnRoutine()
    {
        while (canAct) // Runs until the game ends or this object is disabled
        {
            // Wait a random number of seconds between min and max before the next attack
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime); // Pause here without blocking the main thread

            SpawnAndAttack(); // Execute the attack after the wait
        }
    }

    // Randomly choose a side, teleport to that spawn point, flip to face inward, fire a projectile
    private void SpawnAndAttack()
    {
        // Random.Range(0, 2) returns either 0 or 1 — used as a 50/50 coin flip
        bool isLeft = Random.Range(0, 2) == 0;

        // Pick the spawn point and the direction the projectile should travel
        Transform spawnPoint = isLeft ? leftSpawnPoint : rightSpawnPoint;
        float direction      = isLeft ? 1f : -1f; // Left side → projectile goes right (+1), and vice versa

        // Snap the Sea Hag's position to the chosen spawn point (off-screen)
        transform.position = spawnPoint.position;

        // Flip the sprite to face inward: positive scale = facing right, negative = facing left
        transform.localScale = new Vector3(direction, 1, 1);

        // Instantiate the projectile at the Sea Hag's position and give it its travel direction
        GameObject projectile = Instantiate(magicProjectilePrefab, transform.position, Quaternion.identity);
        projectile.GetComponent<SeaHagProjectile>().Initialize(direction);
    }
}
