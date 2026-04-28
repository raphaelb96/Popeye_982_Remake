// OneWayPlatform3D.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OneWayPlatform3D : MonoBehaviour
{
    private Collider platformCollider;
    private Collider player1Collider;
    private Collider player2Collider;

    // מונע מהפונקציה הרציפה להפריע לירידה היזומה כשלוחצים למטה
    private HashSet<Collider> fallingPlayers = new HashSet<Collider>();

    private void Awake()
    {
        platformCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        GameObject p1 = GameObject.FindGameObjectWithTag("Player1");
        if (p1 != null) player1Collider = p1.GetComponent<Collider>();

        GameObject p2 = GameObject.FindGameObjectWithTag("Player2");
        if (p2 != null) player2Collider = p2.GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        HandlePlayerCollision(player1Collider);
        HandlePlayerCollision(player2Collider);
    }

    private void HandlePlayerCollision(Collider playerCol)
    {
        if (playerCol == null || fallingPlayers.Contains(playerCol)) return;

        // אם תחתית השחקן (הרגליים) נמצאת מתחת למפלס העליון של הפלטפורמה
        if (playerCol.bounds.min.y < platformCollider.bounds.max.y - 0.2f)
        {
            Physics.IgnoreCollision(playerCol, platformCollider, true);
        }
        else
        {
            // השחקן עבר לחלוטין מעל הפלטפורמה
            Physics.IgnoreCollision(playerCol, platformCollider, false);
        }
    }

    public void FallThrough(Collider playerCollider)
    {
        if (!fallingPlayers.Contains(playerCollider))
        {
            StartCoroutine(FallRoutine(playerCollider));
        }
    }

    private IEnumerator FallRoutine(Collider playerCollider)
    {
        fallingPlayers.Add(playerCollider);
        Physics.IgnoreCollision(playerCollider, platformCollider, true);

        // זמן מעבר כדי ליפול מבעד לפלטפורמה מבלי להיתקע
        yield return new WaitForSeconds(0.4f);

        fallingPlayers.Remove(playerCollider);
    }
}