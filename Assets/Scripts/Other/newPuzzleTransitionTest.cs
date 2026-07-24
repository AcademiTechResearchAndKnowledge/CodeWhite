using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EyeTransition : MonoBehaviour
{
    public Image Bg;
    public Image eyeTransitionImage;
    public Sprite[] eyeSprites;
    public AudioSource transitionSound;
    public AudioClip[] transitionClips;
    public float perSpriteTime = 0.5f;  
    public float holdDuration = 0.3f;    
    private void Start()
    {
        Bg.gameObject.SetActive(false);
        eyeTransitionImage.gameObject.SetActive(false);
    }

    public void TriggerEyeTransition()
    {
        StartCoroutine(EyeTransitionRoutine());
    }

    private IEnumerator EyeTransitionRoutine()
    {
        Bg.gameObject.SetActive(true);
        eyeTransitionImage.gameObject.SetActive(true);

        if (transitionSound != null)
            transitionSound.Play();


        for (int i = 0; i < eyeSprites.Length; i++)
        {
            eyeTransitionImage.sprite = eyeSprites[i];

            if (transitionClips != null && transitionClips.Length > i && transitionSound != null)
            {
                transitionSound.clip = transitionClips[i];
                transitionSound.Play();
            }

            yield return new WaitForSeconds(perSpriteTime);
        }

        yield return new WaitForSeconds(holdDuration);

        Bg.gameObject.SetActive(false);
        eyeTransitionImage.gameObject.SetActive(false);
    }
}