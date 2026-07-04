using UnityEngine;

public class CursorManager : MonoBehaviour
{
    // Assign your cursor texture in the Inspector
    public Texture2D cursorTexture;
       public Vector2 hotspot = Vector2.zero; 
    public CursorMode cursorMode = CursorMode.Auto; // Use hardware rendering if supported

    void Start()
    {
        // Set the custom cursor when the game starts
        Cursor.SetCursor(cursorTexture, hotspot, cursorMode);
        
        // Ensure the cursor is visible and unlocked
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // You can add a method to change the cursor dynamically
    public void ChangeCursor(Texture2D newTexture, Vector2 newHotspot)
    {
        Cursor.SetCursor(newTexture, newHotspot, cursorMode);
    }
}
