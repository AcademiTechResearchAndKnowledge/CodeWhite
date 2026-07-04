using UnityEngine;

public class TableHideState : MonoBehaviour
{
    [Header("Table State")]
    [Tooltip("True if the player is currently standing/crouching inside a table's trigger collider.")]
    public bool isUnderTable = false;
}