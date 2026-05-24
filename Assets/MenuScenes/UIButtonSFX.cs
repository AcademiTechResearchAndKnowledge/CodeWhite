using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class UIButtonSFX : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    [Header("Sound Effects")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    private Selectable selectable;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
    }

    // Triggered when the mouse enters the UI element's rect area
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Only play if the button is interactable
        if (selectable.interactable && hoverSound != null)
        {
            UIAudioManager.Instance.PlaySound(hoverSound);
        }
    }

    // Triggered the exact moment the mouse clicks down
    public void OnPointerDown(PointerEventData eventData)
    {
        if (selectable.interactable && clickSound != null)
        {
            UIAudioManager.Instance.PlaySound(clickSound);
        }
    }
}