// LadderTrigger.cs
using UnityEngine;
using System.Collections;

public class LadderTrigger : MonoBehaviour
{
    private bool isDisabled = false;

    private void OnTriggerStay(Collider other)
    {
        if (isDisabled) return;

        if (other.CompareTag("Player1"))
        {
            HandleClimbing(other.GetComponent<PopeyeController>());
        }
        else if (other.CompareTag("Player2"))
        {
            HandleClimbing(other.GetComponent<BlutoController>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // ניתוק מיידי מהסולם ברגע שיוצאים מגבולות הטריגר (כולל יציאה מהצדדים)
        if (other.CompareTag("Player1"))
        {
            other.GetComponent<PopeyeController>().SetClimbing(false);
        }
        else if (other.CompareTag("Player2"))
        {
            other.GetComponent<BlutoController>().SetClimbing(false);
        }
    }

    private void HandleClimbing(MonoBehaviour controller)
    {
        // האחיזה הראשונית דורשת קלט אנכי כדי למנוע הידבקות לסולם בזמן הליכה חולפת
        if (Mathf.Abs(InputManager.Instance.PopeyeMove.y) > 0.1f && controller is PopeyeController p) p.SetClimbing(true);
        if (Mathf.Abs(InputManager.Instance.BlutoMove.y) > 0.1f && controller is BlutoController b) b.SetClimbing(true);
    }

    public void DisableLadder()
    {
        if (!isDisabled) StartCoroutine(DisableRoutine());
    }

    private IEnumerator DisableRoutine()
    {
        isDisabled = true;
        yield return new WaitForSeconds(3f);
        isDisabled = false;
    }
}