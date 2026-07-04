using UnityEngine;
using System.Collections.Generic;

public class TipsDatabase : MonoBehaviour
{
    public static TipsDatabase Instance;

    [Header("All Tips")]
    public TipData[] tips;

    [Header("Default Tip (optional)")]
    public TipData defaultTip;

    private Dictionary<string, TipData> tipDict;

    private void Awake()
    {
        Instance = this;

        tipDict = new Dictionary<string, TipData>();

        Debug.Log("TIPS COUNT: " + tips.Length);

        foreach (var tip in tips)
        {
            if (tip == null)
            {
                Debug.LogWarning("NULL TIP FOUND");
                continue;
            }

            if (string.IsNullOrEmpty(tip.tipID))
            {
                Debug.LogWarning("TIP HAS EMPTY ID: " + tip.name);
                continue;
            }

            if (!tipDict.ContainsKey(tip.tipID))
            {
                tipDict.Add(tip.tipID, tip);
                Debug.Log("ADDED TIP: " + tip.tipID);
            }
            else
            {
                Debug.LogWarning("DUPLICATE TIP ID: " + tip.tipID);
            }
        }
    }

    private void Start()
    {
        var tip = GetRandomTip();

        if (tip == null)
            return;

        Debug.Log("TIP: " + tip.tipText);

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.SetTipText(tip);
        }
    }

    void ApplyTipSafe()
    {
        if (defaultTip == null)
        {
            Debug.LogWarning("No defaultTip assigned in TipsDatabase.");
            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("DialogueManager.Instance not found.");
            return;
        }

        Debug.Log("TIP SET: " + defaultTip.tipText);
    }

    public TipData GetTip(string id)
    {
        if (tipDict.TryGetValue(id, out TipData tip))
            return tip;

        Debug.LogWarning("Tip not found: " + id);
        return null;
    }

    public TipData GetRandomTip()
    {
        if (tips == null || tips.Length == 0)
            return null;

        return tips[Random.Range(0, tips.Length)];
    }
}