// OliveController.cs
// Controls Olive's patrol movement along the top platform and manages all item spawning.
// Olive patrols left and right between two bounds, flipping her sprite based on direction.
// She throws a heart every 2 seconds. Every 2nd heart also spawns a bottle collectible.
// Every 5th heart also spawns a spinach can. This creates a predictable escalating rhythm.
using UnityEngine;

public class OliveController : MonoBehaviour
{
    // ─── MOVEMENT ────────────────────────────────────────────────────────────
    public float moveSpeed = 2f;     // Patrol speed in units per second
    public float leftBound = -8f;    // X position where Olive turns around (left wall)
    public float rightBound = 8f;    // X position where Olive turns around (right wall)
    private int direction = 1;       // Current patrol direction: +1 = right, -1 = left

    // ─── SPAWN PREFABS ───────────────────────────────────────────────────────
    // Assign these in the Inspector — all created by Michael
    public GameObject heartPrefab;    // The heart collectible Popeye picks up
    public GameObject bottlePrefab;   // The bottle collectible Bluto picks up
    public GameObject spinachPrefab;  // The spinach can Popeye picks up for the buff

    // ─── SPAWN POINTS ────────────────────────────────────────────────────────
    // Arrays of Transforms — OliveController picks one at random when spawning
    public Transform[] spinachSpawnPoints;  // Fixed positions on platforms for spinach
    public Transform[] bottleSpawnPoints;   // Fixed positions on platforms for bottles

    // ─── STATE ───────────────────────────────────────────────────────────────
    private int heartsThrown = 0;   // Running count of hearts Olive has thrown this round
    private float heartTimer = 0f;  // Accumulates delta time — heart spawns when it reaches 2f
    private bool canAct = false;    // Gate: Olive only acts after GameManager.OnGameStart fires

    // Stores the scale from the Inspector so flipping only changes the X sign, not the size
    private Vector3 originalScale;

    // ─── UNITY LIFECYCLE ─────────────────────────────────────────────────────

    private void Awake()
    {
        // Cache the original scale set in the Inspector before anything modifies it
        originalScale = transform.localScale;
    }

    // Subscribe to OnGameStart with a lambda — enables Olive after the 3s countdown
    private void OnEnable() => GameManager.OnGameStart += () => canAct = true;

    private void Update()
    {
        if (!canAct) return; // Do nothing during the startup countdown

        MoveOlive();
        HandleSpawning();
    }

    // ─── MOVEMENT ────────────────────────────────────────────────────────────

    private void MoveOlive()
    {
        // Move Olive horizontally at moveSpeed units per second
        transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime);

        // Reverse direction when hitting either patrol bound
        if      (transform.position.x > rightBound) direction = -1;
        else if (transform.position.x < leftBound)  direction =  1;

        // Flip the sprite by setting localScale.x to ±|originalScale.x|
        // Using Abs() ensures the flip only changes the sign, not the magnitude
        transform.localScale = new Vector3(
            Mathf.Abs(originalScale.x) * direction,
            originalScale.y,
            originalScale.z
        );
    }

    // ─── SPAWNING ────────────────────────────────────────────────────────────

    private void HandleSpawning()
    {
        // Accumulate time since last heart spawn
        heartTimer += Time.deltaTime;

        // Spawn a heart every 2 seconds
        if (heartTimer >= 2f)
        {
            heartTimer = 0f; // Reset the timer
            SpawnHeart();
        }
    }

    private void SpawnHeart()
    {
        // Spawn the heart at Olive's current position — it will fall with its own script
        Instantiate(heartPrefab, transform.position, Quaternion.identity);
        heartsThrown++;

        // Every 2nd heart: also spawn a bottle collectible at a random bottle spawn point
        if (heartsThrown % 2 == 0) SpawnBottle();

        // Every 5th heart: also spawn a spinach can at a random spinach spawn point
        if (heartsThrown % 5 == 0) SpawnSpinach();
    }

    private void SpawnBottle()
    {
        // Pick a random spawn point from the array
        Transform spawnPoint = bottleSpawnPoints[Random.Range(0, bottleSpawnPoints.Length)];

        // Instantiate the bottle and configure it as a floor pickup (not a projectile)
        GameObject bottle = Instantiate(bottlePrefab, spawnPoint.position, Quaternion.identity);
        bottle.GetComponent<BottleItem>().InitializePickup();
    }

    private void SpawnSpinach()
    {
        // Pick a random spawn point from the array and place the spinach there
        Transform spawnPoint = spinachSpawnPoints[Random.Range(0, spinachSpawnPoints.Length)];
        Instantiate(spinachPrefab, spawnPoint.position, Quaternion.identity);
    }
}
