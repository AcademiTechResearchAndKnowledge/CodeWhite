using UnityEngine;
using UnityEditor;

public class SpriteMaterialFixer : ScriptableWizard
{
    [Tooltip("Drag your perfectly working 3D shadow material here from the Project window")]
    public Material cleanMasterMaterial;

    [MenuItem("Tools/PGDX/Sanitize Sprite Materials")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<SpriteMaterialFixer>("Fix Sprite Materials", "Clean 108 Sprites");
    }

    void OnWizardCreate()
    {
        if (cleanMasterMaterial == null)
        {
            Debug.LogError("Whoops! Assign the clean 3D shadow material first!");
            return;
        }

        int fixedCount = 0;

        // Loop through everything you currently have selected in the Hierarchy
        foreach (GameObject obj in Selection.gameObjects)
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                // Register an undo state just in case
                Undo.RecordObject(sr, "Fixed Sprite Material");

                // CRITICAL: Use sharedMaterial to prevent Unity from creating broken (Instance) clones
                sr.sharedMaterial = cleanMasterMaterial;

                // Reset vertex color to pure white (removes any hidden alpha tint causing glow)
                sr.color = Color.white;

                fixedCount++;
            }
        }

        Debug.Log($"Successfully sanitized {fixedCount} sprites. Get back to level design!");
    }
}