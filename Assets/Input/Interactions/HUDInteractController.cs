using UnityEngine;
using TMPro;

public class HUDInteractController : MonoBehaviour
{
    public static HUDInteractController Instance;

    [Header("UI References")]
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private TMP_Text objectNameText;
    [SerializeField] private TMP_Text actionText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (interactionPanel != null)
            interactionPanel.SetActive(false);

        Debug.Log("HUD initialized");
    }

    public void EnableInteractionText(string key, string objectName, string action)
    {
        if (interactionPanel == null)
        {
            Debug.Log("HUD interaction panel missing");
            return;
        }

        if (keyText == null || objectNameText == null || actionText == null)
        {
            Debug.Log("HUD text references missing");
            return;
        }

        keyText.text = key;
        objectNameText.text = objectName;
        actionText.text = action;

        interactionPanel.SetActive(true);
    }

    public void DisableInteractionText()
    {
        if (interactionPanel == null) return;

        interactionPanel.SetActive(false);
    }
}