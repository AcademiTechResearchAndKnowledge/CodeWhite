using TMPro;
using UnityEngine;

public class RadioPuzzleHandler : MonoBehaviour
{   
    [SerializeField]public TextMeshPro frequencyText;
    [SerializeField]public KnobRotate knobValue;
    void Start()
    {
        UpdateFrequency();
    }
    void Update()
    {
        if (knobValue.dragging)
        {
          UpdateFrequency();
        }
        
    }
    void UpdateFrequency()
    {
        //print(knobValue.frequency.ToString("F1") + " MHz");
        if(knobValue.frequency == 0.0)
        {
            knobValue.frequency = knobValue.minFrequency;
        }

        frequencyText.text = knobValue.frequency.ToString("F1") + " MHz";

        if(knobValue.frequency.ToString("F1") + " MHz" == "100.0 MHz")
        {
            print("done!");
        }

 
    }
}
