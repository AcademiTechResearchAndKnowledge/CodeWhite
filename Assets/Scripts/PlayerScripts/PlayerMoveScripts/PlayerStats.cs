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

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip exhaustionClip;
    [SerializeField] private float exhaustionRecoveryThreshold = 25f;

    // We use this to prevent the sound from restarting every frame while at 0 stamina
    private bool isExhausted = false;

    public float Anxiety => anxiety;
    public float MaxAnxiety => maxAnxiety;
    public float Stamina => stamina;
    public float SpeedStat => speedStat;
    public float MaxStamina => maxStamina;

    private void Update()
    {
        HandleExhaustionAudio();
    }

    private void HandleExhaustionAudio()
    {
        if (audioSource == null || exhaustionClip == null) return;

        if (stamina <= 0f && !isExhausted)
        {
            isExhausted = true;
            audioSource.clip = exhaustionClip;
            audioSource.loop = true;
            audioSource.Play();
        }
        else if (stamina >= exhaustionRecoveryThreshold && isExhausted)
        {
            isExhausted = false;
            audioSource.Stop();
        }
    }

    public void ResetAnxiety()
    {
        anxiety = 0f;
    }

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