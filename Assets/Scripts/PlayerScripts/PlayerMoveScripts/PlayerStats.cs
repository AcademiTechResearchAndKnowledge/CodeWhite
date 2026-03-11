using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float anxiety = 0f;
    [SerializeField] private float maxAnxiety = 100f;
    [SerializeField] private float stamina = 100f;
    [SerializeField] private float speedStat = 100f;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 20f;
    [SerializeField] private float staminaRegenRate = 12f;

    public float Anxiety => anxiety;
    public float MaxAnxiety => maxAnxiety;
    public float Stamina => stamina;
    public float SpeedStat => speedStat;
    public float MaxStamina => maxStamina;

    public void AddStat(StatType type, float amount)
    {
        switch (type)
        {
            case StatType.ANX:
                anxiety = Mathf.Clamp(anxiety + amount, 0f, maxAnxiety);
                break;

            case StatType.STA:
                stamina = Mathf.Clamp(stamina + amount, 0f, maxStamina);
                break;

            case StatType.SPD:
                speedStat += amount;
                break;

            case StatType.None:
            default:
                Debug.LogWarning("No valid stat type selected.");
                break;
        }
    }

    public void SubtractStat(StatType type, float amount)
    {
        switch (type)
        {
            case StatType.ANX:
                anxiety = Mathf.Clamp(anxiety - amount, 0f, maxAnxiety);
                break;

            case StatType.STA:
                stamina = Mathf.Clamp(stamina - amount, 0f, maxStamina);
                break;

            case StatType.SPD:
                speedStat -= amount;
                break;

            case StatType.None:
            default:
                Debug.LogWarning("No valid stat type selected.");
                break;
        }
    }

    public float GetStat(StatType type)
    {
        switch (type)
        {
            case StatType.ANX: return anxiety;
            case StatType.STA: return stamina;
            case StatType.SPD: return speedStat;
            default:
                Debug.LogWarning("No valid stat type selected.");
                return -1f;
        }
    }

    public void DrainStamina(float amount)
    {
        stamina = Mathf.Clamp(stamina - amount, 0f, maxStamina);
    }

    public void RegenerateStamina(float amount)
    {
        stamina = Mathf.Clamp(stamina + amount, 0f, maxStamina);
    }

    public bool HasStamina()
    {
        return stamina > 0f;
    }

    public bool IsMaxAnxiety()
    {
        return anxiety >= maxAnxiety;
    }

    public float GetDrainRate()
    {
        return staminaDrainRate;
    }

    public float GetRegenRate()
    {
        return staminaRegenRate;
    }
}