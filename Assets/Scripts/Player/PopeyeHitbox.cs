using UnityEngine;

public class PopeyeHitbox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // זיהוי בקבוק (גם באוויר וגם על הרצפה) ושבירה שלו
        BottleItem bottle = other.GetComponent<BottleItem>();
        if (bottle != null)
        {
            bottle.Smash();
        }
        
        // הכנה עתידית לפגיעה בבלוטו
        /*
        else if (other.CompareTag("Player2"))
        {
            BlutoController bluto = other.GetComponent<BlutoController>();
            if (bluto != null) bluto.ApplyStun(1f);
        }
        */
    }
}