using TMPro;
using UnityEngine;
using System.Collections;

public class RadioPuzzleHandler : MonoBehaviour
{
    [SerializeField] public TextMeshPro frequencyText;
    [SerializeField] public KnobRotate knobValue;
    [SerializeField] public objectZoom objZoom;

    public float minTargetFrequency = 88.0f;
    public float maxTargetFrequency = 108.0f;
    public float minOminousFrequency = 88.0f;
    public float maxOminousFrequency = 108.0f;
    public float submitDelay = 2f;
    public float ominousPushForce = 5f;
    public float verticalPushForce = 2f;

    private float targetFrequency;
    private float ominousFrequency;
    private float submitTimer = 0f;
    private float lastFrequency;

    void Start()
    {
        ResetPuzzle();
    }

    void Update()
    {
        if (objZoom.isInPuzzle && knobValue.dragging && !knobValue.submitted)
        {
            UpdateFrequency();
            if (Mathf.Abs(knobValue.frequency - lastFrequency) > 0.001f)
            {
                submitTimer = submitDelay;
                lastFrequency = knobValue.frequency;
            }

            submitTimer -= Time.deltaTime;
            if (submitTimer <= 0f)
            {
                SubmitFrequency();
                submitTimer = 0f;
            }
        }
    }

    void UpdateFrequency()
    {
        if (knobValue.frequency == 0f)
            knobValue.frequency = knobValue.minFrequency;
        frequencyText.text = knobValue.frequency.ToString("F1") + " MHz";
    }

    public void SubmitFrequency()
    {
        if (Mathf.Abs(knobValue.frequency - ominousFrequency) < 0.1f)
        {
            Debug.Log($"Ominous frequency triggered! {ominousFrequency:F1} MHz");
            StartCoroutine(ExitAndPushPlayer());
            return;
        }

        if (Mathf.Abs(knobValue.frequency - targetFrequency) < 0.1f)
        {
            Debug.Log($"Puzzle solved! Correct frequency was {targetFrequency:F1} MHz");
            knobValue.submitted = true;
        }
        else
        {
            Debug.Log($"Incorrect frequency. Target: {targetFrequency:F1} MHz, Ominous: {ominousFrequency:F1} MHz");
        }
    }

    IEnumerator ExitAndPushPlayer()
    {
        objZoom.ExitPuzzle();
        yield return new WaitForFixedUpdate();

        Rigidbody playerRb = objZoom.playerController?.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            if (playerRb.isKinematic)
                playerRb.isKinematic = false;

            Vector3 pushDirection = objZoom.transform.forward.normalized;
            pushDirection.y = verticalPushForce / ominousPushForce;
            playerRb.AddForce(pushDirection * ominousPushForce, ForceMode.Impulse);
            Debug.Log("Player pushed by ominous frequency!");
        }

        ResetPuzzle();
    }

    public void ResetPuzzle()
    {
        knobValue.ResetKnob();
        UpdateFrequency();
        targetFrequency = Random.Range(minTargetFrequency, maxTargetFrequency);
        do
        {
            ominousFrequency = Random.Range(minOminousFrequency, maxOminousFrequency);
        } while (Mathf.Abs(ominousFrequency - targetFrequency) < 0.1f);
        submitTimer = submitDelay;
        lastFrequency = knobValue.frequency;
        Debug.Log($"New target frequency: {targetFrequency:F1} MHz, Ominous frequency: {ominousFrequency:F1} MHz");
    }
}