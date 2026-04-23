using UnityEngine;
using TMPro;

public class HUDInteractController : MonoBehaviour
{
    public static HUDInteractController Instance;

    private void Awake()
    {
        Instance = this;
        DisableInteractionText();
    }

    [Header("UI References")]
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private TMP_Text objectNameText;
    [SerializeField] private TMP_Text actionText;

    public void EnableInteractionText(string key, string objectName, string action)
    {
        keyText.text = key;
        objectNameText.text = objectName;
        actionText.text = action;

        interactionPanel.SetActive(true);
    }

    public void DisableInteractionText()
    {
        interactionPanel.SetActive(false);
    }
}