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

    public void Refresh(List<ObjectiveInventorySlot> slots, int selectedSlot)
    {
        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (i < slots.Count && !slots[i].IsEmpty())
            {
                slotIcons[i].enabled = true;
                slotIcons[i].sprite = slots[i].item.icon;

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
    }
}