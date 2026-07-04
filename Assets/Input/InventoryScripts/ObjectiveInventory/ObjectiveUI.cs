using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveUI : MonoBehaviour
{
    public Image[] slotIcons;
    public TextMeshProUGUI[] slotAmounts;
    public Image[] slotBackgrounds;

    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    [Header("Item Name Popup")]
    public TextMeshProUGUI itemNameText;
    public float fadeDuration = 0.5f;
    public float displayDuration = 2.0f;

    private Coroutine fadeCoroutine;
    private ObjectiveItemData lastDisplayedItem = null;

    private void Start()
    {
        // Ensure the text starts completely invisible when the game begins
        if (itemNameText != null)
        {
            Color c = itemNameText.color;
            c.a = 0f;
            itemNameText.color = c;
        }
    }

    public void Refresh(List<ObjectiveInventorySlot> slots, int selectedSlot)
    {
        // 1. Update the slots visually
        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (i < slots.Count && !slots[i].IsEmpty())
            {
                slotIcons[i].enabled = true;
                slotIcons[i].sprite = slots[i].item.icon;

                // Keep your custom logic for hiding amounts if 1 or less
                if (slots[i].amount > 1)
                {
                    slotAmounts[i].text = slots[i].amount.ToString();
                }
                else
                {
                    slotAmounts[i].text = "";
                }
            }
            else
            {
                slotIcons[i].enabled = false;
                slotAmounts[i].text = "";
            }

            if (slotBackgrounds != null && i < slotBackgrounds.Length)
            {
                slotBackgrounds[i].color = (i == selectedSlot) ? selectedColor : normalColor;
            }
        }

        // 2. Handle the Item Name Popup
        ObjectiveItemData currentItem = null;
        if (selectedSlot >= 0 && selectedSlot < slots.Count && !slots[selectedSlot].IsEmpty())
        {
            currentItem = slots[selectedSlot].item;
        }

        // Only trigger the popup if the selected item actually changed
        if (currentItem != lastDisplayedItem)
        {
            lastDisplayedItem = currentItem;

            if (currentItem != null)
            {
                // --- THE CHANGE IS HERE ---
                // If it has a book type assigned, hide the true name and just say "Book"
                if (currentItem.bookType != LibraryBookType.None)
                {
                    TriggerItemNamePopup("Book");
                }
                else
                {
                    // Otherwise, display its normal name (for keys, tools, etc.)
                    TriggerItemNamePopup(currentItem.itemName);
                }
            }
            else
            {
                HideItemNamePopup(); // Instantly hide if we selected an empty slot
            }
        }
    }

    private void TriggerItemNamePopup(string itemName)
    {
        if (itemNameText == null) return;

        itemNameText.text = itemName;

        // Stop any ongoing fade animation before starting a new one
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeInOutRoutine());
    }

    private void HideItemNamePopup()
    {
        if (itemNameText == null) return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        // Set alpha to 0 instantly
        Color c = itemNameText.color;
        c.a = 0f;
        itemNameText.color = c;
    }

    private IEnumerator FadeInOutRoutine()
    {
        Color c = itemNameText.color;

        // --- FADE IN ---
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, time / fadeDuration);
            itemNameText.color = c;
            yield return null; // Wait for the next frame
        }
        c.a = 1f;
        itemNameText.color = c;

        // --- WAIT ---
        yield return new WaitForSeconds(displayDuration);

        // --- FADE OUT ---
        time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, time / fadeDuration);
            itemNameText.color = c;
            yield return null;
        }
        c.a = 0f;
        itemNameText.color = c;
    }
}