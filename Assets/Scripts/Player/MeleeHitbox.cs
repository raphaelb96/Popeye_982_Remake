// MeleeHitbox.cs
// A reusable punch hitbox attached as a child of any character with melee attacks.
// The hitbox starts disabled and is only activated during the punch animation frames
// via Animation Events (EnableHitbox at frame 5, DisableHitbox at frame 10).
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class MeleeHitbox : MonoBehaviour
{
    public string targetTag;
    public int damageAmount = 1;

    [Header("Impact Settings")]
    public float knockbackForce = 16f; // Doubled from 8f. Adjust this in the Inspector.
    public bool canDestroyProjectiles = true;

    private BoxCollider hitCollider;

    private void Awake()
    {
        hitCollider = GetComponent<BoxCollider>();
        hitCollider.isTrigger = true;
        hitCollider.enabled = false;
    }

    public void EnableHitbox() => hitCollider.enabled = true;
    public void DisableHitbox() => hitCollider.enabled = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            VFXManager vfx = FindFirstObjectByType<VFXManager>();
            if (vfx != null) vfx.SpawnHitSprite(other.transform.position);

            if (targetTag == "Player1")
            {
                PopeyeController popeye = other.GetComponent<PopeyeController>();
                if (popeye != null)
                {
                    Vector3 knockbackDir = (other.transform.position - transform.parent.position).normalized;
                    knockbackDir.y = 0.3f; // Adds a slight upward pop to the knockback
                    popeye.TakeDamage(damageAmount, knockbackDir.normalized * knockbackForce);
                }
            }
            else if (targetTag == "Player2" && transform.parent.GetComponent<PopeyeController>())
            {
                PopeyeController popeye = transform.parent.GetComponent<PopeyeController>();
                BlutoController bluto = other.GetComponent<BlutoController>();

                if (popeye.isInvincible)
                {
                    // Spinach active — stun Bluto for the extended duration
                    bluto.ApplyStun(bluto.spinachStunDuration);

                    // Apply knockback. Bluto's mass is 5, so the impulse must scale to affect him properly
                    Rigidbody blutoRb = other.GetComponent<Rigidbody>();
                    if (blutoRb != null)
                    {
                        Vector3 knockbackDir = (other.transform.position - transform.parent.position).normalized;
                        knockbackDir.y = 0.5f;
                        blutoRb.AddForce(knockbackDir.normalized * (knockbackForce * 2.5f), ForceMode.Impulse);
                    }
                }
                else
                {
                    // Normal punch — standard stun, no knockback
                    bluto.ApplyStun(bluto.normalStunDuration);
                }
            }
        }
        else if (canDestroyProjectiles && other.GetComponent<BottleItem>())
        {
            other.GetComponent<BottleItem>().Smash();
        }
        else if (canDestroyProjectiles && other.GetComponent<SeaHagProjectile>())
        {
            other.GetComponent<SeaHagProjectile>().Smash();
        }
        else if (other.GetComponent<SpinachItem>() && transform.parent.GetComponent<BlutoController>())
        {
            other.GetComponent<SpinachItem>().DestroyByBluto();
        }
        else if (other.GetComponent<HeartItem>() && transform.parent.GetComponent<BlutoController>())
        {
            Destroy(other.gameObject);
        }
        else if (other.GetComponent<LadderTrigger>() && transform.parent.GetComponent<PopeyeController>())
        {
            other.GetComponent<LadderTrigger>().DisableLadder();
        }
    }
}