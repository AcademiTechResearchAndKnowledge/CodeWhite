using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class PersistentUIBinder : MonoBehaviour
{
    [Header("Dialogue References to Push")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public Image expressionImage;
    public TextMeshProUGUI tipsText;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.dialoguePanel = dialoguePanel;
            DialogueManager.Instance.dialogueText = dialogueText;
            DialogueManager.Instance.nameText = nameText;
            DialogueManager.Instance.expressionImage = expressionImage;
            DialogueManager.Instance.tipsText = tipsText;
        }
    }
}