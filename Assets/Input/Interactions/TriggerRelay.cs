using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ColliderEvent : UnityEvent<Collider> {}

public class TriggerRelay : MonoBehaviour
{
    [SerializeField]public ColliderEvent onTriggerEnter;

    void OnTriggerEnter(Collider other)
    {
       
        if (!other.CompareTag("Player")) return;
        
        onTriggerEnter.Invoke(other);
    }
}