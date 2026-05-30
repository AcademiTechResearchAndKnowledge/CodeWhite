#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class SpriteMaterialFixer : ScriptableWizard
{
    [Tooltip("Drag your perfectly working 3D shadow material here from the Project window")]
    public Material cleanMasterMaterial;

    [MenuItem("Tools/PGDX/Sanitize Sprite Materials")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<SpriteMaterialFixer>(
            "Fix Sprite Materials",
            "Clean 108 Sprites"
        );
    }

    void OnWizardCreate()
    {
        if (cleanMasterMaterial == null)
        {
            Debug.LogError("Assign the clean 3D shadow material first!");
            return;
        }

        int fixedCount = 0;

        foreach (GameObject obj in Selection.gameObjects)
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                Undo.RecordObject(sr, "Fixed Sprite Material");

                sr.sharedMaterial = cleanMasterMaterial;
                sr.color = Color.white;

                fixedCount++;
            }
        }

        Debug.Log($"Successfully sanitized {fixedCount} sprites.");
    }
}
#endif