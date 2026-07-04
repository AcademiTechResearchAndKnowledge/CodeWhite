using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class JumpScareSequence : MonoBehaviour
{

    public RawImage scareImage;
    public Texture2D[] frames;

    public float frameRate = 30.0f;
    public int repeat = 1;
    public Animator camera_shake;

    private AudioSource audioSource;
    private bool hasPlayed = false;

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Play()
    {
        if (!hasPlayed)
        {
            hasPlayed = true;
            StartCoroutine(PlaySequence());
        }
    }

    private IEnumerator PlaySequence()
    {
        scareImage.gameObject.SetActive(true);
        scareImage.transform.SetAsLastSibling();
        float delay = 1.0f / frameRate;
        audioSource.Play();
        camera_shake.SetTrigger("Shake");
        for (int i = 0; i < repeat; i++)
        {
            foreach (Texture2D frame in frames)
            {
                scareImage.texture = frame;
                yield return new WaitForSeconds(delay);
            }
        }

        hasPlayed = false;
        scareImage.gameObject.SetActive(false);
    }
}