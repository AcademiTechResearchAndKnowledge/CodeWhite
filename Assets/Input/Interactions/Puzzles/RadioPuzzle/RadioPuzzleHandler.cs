using TMPro;
using UnityEngine;

public class RadioPuzzleHandler : MonoBehaviour
{
    [SerializeField] public TextMeshPro frequencyText;
    [SerializeField] public KnobRotate knobValue;
    [SerializeField] public GameObject submitButtonUI;
    [SerializeField] public objectZoom objZoom;


    public float targetFrequency = 100f; // correct answer

    void Start()
    {
        UpdateFrequency();
    }

    void Update()
    {
        if (knobValue.dragging)
            UpdateFrequency();

        submitButtonUI.SetActive(objZoom.isInPuzzle);
       
    }

    void UpdateFrequency()
    {
        if (knobValue.frequency == 0f)
            knobValue.frequency = knobValue.minFrequency;

        frequencyText.text = knobValue.frequency.ToString("F1") + " MHz";
    }

    public void SubmitFrequency()
    {


        if (Mathf.Abs(knobValue.frequency - targetFrequency) < 0.1f)
        {
            
            Debug.Log("Puzzle solved!");
            knobValue.submitted = true;
        }
        else
        {

            Debug.Log("Incorrect frequency.");
        }
    }

    public void ResetPuzzle()
    {
        knobValue.ResetKnob();
        UpdateFrequency();
    }
}