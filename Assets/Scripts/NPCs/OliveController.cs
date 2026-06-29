// OliveController.cs
// Controls Olive's patrol movement along the top platform and manages all item spawning.
using UnityEngine;
using System;

public class OliveController : MonoBehaviour
{
    // ─── MOVEMENT ────────────────────────────────────────────────────────────
    public float moveSpeed = 2f;
    public float leftBound = -8f;
    public float rightBound = 8f;
    private int direction = 1;

    [Header("Patrol Timers")]
    public float minWalkTime = 2f;
    public float maxWalkTime = 5f;
    public float minIdleTime = 1.5f;
    public float maxIdleTime = 3f;

    private bool isWalking = true;
    private float stateTimer = 0f;

    [Header("Heart Spawn Timer")]
    public float minHeartTime = 1.5f;
    public float maxHeartTime = 2.5f;

    // ─── SPAWN PREFABS ───────────────────────────────────────────────────────
    public GameObject heartPrefab;
    public GameObject bottlePrefab;
    public GameObject spinachPrefab;

    // ─── SPAWN POINTS ────────────────────────────────────────────────────────
    public Transform[] spinachSpawnPoints;
    public Transform[] bottleSpawnPoints;

    //─── ANIMATORS ────────────────────────────────────────────────────────
    public Animator oliveAnimator;

    // ─── STATE ───────────────────────────────────────────────────────────────
    private int heartsThrown = 0;
    private float heartTimer = 0f;
    private float nextHeartTime = 2f;
    private bool canAct = false;

    // ─── AUDIO EVENTS ────────────────────────────────────────────────────────
    public static event Action OnThrowHeart;
    public static event Action OnWalk;

    private float walkTimer = 0f;
    private float walkInterval = 0.4f;

    // ─── VISUAL CACHE ────────────────────────────────────────────────────────
    private Transform visualTransform;
    private Vector3 baseVisualScale;
    private Quaternion rightRotation;
    private Quaternion leftRotation;

    // ─── UNITY LIFECYCLE ─────────────────────────────────────────────────────

    private void Awake()
    {
        rightRotation = transform.rotation;
        leftRotation = transform.rotation * Quaternion.Euler(0, 180, 0);

        Billboard billboard = GetComponentInChildren<Billboard>();
        if (billboard != null)
        {
            visualTransform = billboard.transform;
            baseVisualScale = visualTransform.localScale;
        }
    }

    private void OnEnable()
    {
        GameManager.OnGameStart += StartPatrol;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= StartPatrol;
    }

    private void StartPatrol()
    {
        canAct = true;
        isWalking = true;
        stateTimer = UnityEngine.Random.Range(minWalkTime, maxWalkTime);
    }

    private void Update()
    {
        if (!canAct) return;

        HandlePatrolState();

        if (isWalking)
        {
            MoveOlive();
        }

        HandleSpawning();
    }

    private void HandlePatrolState()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            isWalking = !isWalking;

            if (isWalking)
                stateTimer = UnityEngine.Random.Range(minWalkTime, maxWalkTime);
            else
                stateTimer = UnityEngine.Random.Range(minIdleTime, maxIdleTime);
        }
    }

    // ─── MOVEMENT ────────────────────────────────────────────────────────────

    private void MoveOlive()
    {
        transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime, Space.World);

        walkTimer -= Time.deltaTime;
        if (walkTimer <= 0f)
        {
            OnWalk?.Invoke();
            walkTimer = walkInterval;
        }

        if (transform.position.x > rightBound) direction = -1;
        else if (transform.position.x < leftBound) direction = 1;

        if (direction < 0)
        {
            transform.rotation = leftRotation;
            if (visualTransform != null)
            {
                visualTransform.localScale = new Vector3(-Mathf.Abs(baseVisualScale.x), baseVisualScale.y, baseVisualScale.z);
            }
        }
        else
        {
            transform.rotation = rightRotation;
            if (visualTransform != null)
            {
                visualTransform.localScale = new Vector3(Mathf.Abs(baseVisualScale.x), baseVisualScale.y, baseVisualScale.z);
            }
        }
    }

    // ─── SPAWNING ────────────────────────────────────────────────────────────

    private void HandleSpawning()
    {
        heartTimer += Time.deltaTime;

        if (heartTimer >= nextHeartTime)
        {
            heartTimer = 0f;
            nextHeartTime = UnityEngine.Random.Range(minHeartTime, maxHeartTime);
            SpawnHeart();
        }
    }

    private void SpawnHeart()
    {
        oliveAnimator.SetTrigger("Heart");

        Instantiate(heartPrefab, transform.position, Quaternion.identity);
        OnThrowHeart?.Invoke();
        heartsThrown++;

        if (heartsThrown % 2 == 0) SpawnBottle();
        if (heartsThrown % 5 == 0) SpawnSpinach();
    }

    private void SpawnBottle()
    {
        Transform spawnPoint = bottleSpawnPoints[UnityEngine.Random.Range(0, bottleSpawnPoints.Length)];
        GameObject bottle = Instantiate(bottlePrefab, spawnPoint.position, Quaternion.identity);
        bottle.GetComponent<BottleItem>().InitializePickup();
    }

    private void SpawnSpinach()
    {
        Transform spawnPoint = spinachSpawnPoints[UnityEngine.Random.Range(0, spinachSpawnPoints.Length)];
        Instantiate(spinachPrefab, spawnPoint.position, Quaternion.identity);
    }
}