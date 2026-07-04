using UnityEngine;
using UnityEngine.Playables;

public class MainMenuController : MonoBehaviour
{
    public PlayableDirector menuTimeline;
    public GameObject uiCanvas; // Drag your UI Canvas here

    // The Signal Emitter in the Timeline will call this method
    public void PauseTimelineAndShowMenu()
    {
        menuTimeline.Pause();
        uiCanvas.SetActive(true); // Turn on the interactive buttons
    }
}