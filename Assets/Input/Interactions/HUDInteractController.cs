using UnityEngine;
using TMPro;
using UnityEditor.PackageManager;

public class HUDInteractController : MonoBehaviour
{
    public static HUDInteractController Instance;

    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] TMP_Text interactionText;

    public void EnableInteractionText(string text)
        {
            interactionText.text = text + " (F)";
            interactionText.gameObject.SetActive(true);
        }

    public void DisableInteractionText()
        {
            interactionText.gameObject.SetActive(false);
        }
}
