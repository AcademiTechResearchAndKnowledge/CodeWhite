using UnityEngine;

public class TableHideTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger is the player
        if (other.CompareTag("Player"))
        {
            TableHideState tableState = other.GetComponent<TableHideState>();
            if (tableState != null)
            {
                tableState.isUnderTable = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // When the player leaves the trigger area, they are no longer under the table
        if (other.CompareTag("Player"))
        {
            TableHideState tableState = other.GetComponent<TableHideState>();
            if (tableState != null)
            {
                tableState.isUnderTable = false;
            }
        }
    }
}