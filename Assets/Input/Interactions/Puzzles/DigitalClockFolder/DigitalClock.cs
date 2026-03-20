using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DigitalClock : MonoBehaviour
{
    public TextMeshPro clockText;

    private int hours = 12;
    private int minutes = 0;

    [SerializeField]public bool isInteracting = false;
    [SerializeField]public objectZoom oZ;

    private float repeatDelay = 0.2f;
    private float timer;

    void Start()
    {
        UpdateClock();
    }

    void Update()
    {   
        if (oZ.isInPuzzle) isInteracting = true;
        if (!oZ.isInPuzzle) isInteracting = false;
        
        // Only allow control if player is interacting
        if (!isInteracting) return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            if (Keyboard.current.upArrowKey.isPressed)
            {
                ChangeHour(1);
                timer = repeatDelay;
            }
            else if (Keyboard.current.downArrowKey.isPressed)
            {
                ChangeHour(-1);
                timer = repeatDelay;
            }
            else if (Keyboard.current.rightArrowKey.isPressed)
            {
                ChangeMinute(5);
                timer = repeatDelay;
            }
            else if (Keyboard.current.leftArrowKey.isPressed)
            {
                ChangeMinute(-5);
                timer = repeatDelay;
            }
        }
    }

    public void StartInteraction()
    {
        isInteracting = true;
    }

    public void StopInteraction()
    {
        isInteracting = false;
    }

    void ChangeHour(int value)
    {
        hours += value;

        if (hours > 23) hours = 0;
        if (hours < 0) hours = 23;

        UpdateClock();
    }

    void ChangeMinute(int value)
    {
        minutes += value;

        while (minutes >= 60)
        {
            minutes -= 60;
            ChangeHour(1);
        }

        while (minutes < 0)
        {
            minutes += 60;
            ChangeHour(-1);
        }

        UpdateClock();
    }

    void UpdateClock()
    {
        clockText.text = $"{hours:00}:{minutes:00}";

        if(hours == 1 && minutes == 30)//change this to desired answer
        {
            print("Puzzle Done!");
           
        }else{
            // can add other things
        }
    }
}