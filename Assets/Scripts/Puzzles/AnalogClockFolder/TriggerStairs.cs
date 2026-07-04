using UnityEngine;

public class StairTrigger : MonoBehaviour
{
    public enum StairType { Oro, Plata, Mata }
    public StairType stairType;
    public Transform entity_Spawnpoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        switch (stairType)
        {
            case StairType.Oro:
                StairsEffectManager.Instance.TriggerOro();
                break;

            case StairType.Plata:
                StairsEffectManager.Instance.TriggerPlata();
                break;

            case StairType.Mata:
                StairsEffectManager.Instance.TriggerMata(entity_Spawnpoint.position);
                break;
        }
    }
}