// MeleeHitbox.cs
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class MeleeHitbox : MonoBehaviour
{
    public string targetTag; 
    public int damageAmount = 1;
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
            if (targetTag == "Player1") GameManager.Instance.TakeDamage();
            else if (targetTag == "Player2" && transform.parent.GetComponent<PopeyeController>()) 
            {
                other.GetComponent<BlutoController>().ApplySpinachStun();
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
        else if (other.GetComponent<LadderTrigger>() && transform.parent.GetComponent<PopeyeController>())
        {
            other.GetComponent<LadderTrigger>().DisableLadder();
        }
    }
}