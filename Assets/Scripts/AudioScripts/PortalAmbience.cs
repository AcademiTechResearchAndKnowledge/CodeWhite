using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PortalAmbience : MonoBehaviour
{
    [SerializeField] private AudioClip ambienceClip;
    [SerializeField] private float volume = 0.6f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private bool playOnStart = true;

    private AudioSource src;

    void Awake()
    {
        src = GetComponent<AudioSource>();

        src.clip = ambienceClip;
        src.loop = true;
        src.spatialBlend = 1f;     // 3D
        src.volume = volume;
        src.rolloffMode = AudioRolloffMode.Logarithmic;
        src.minDistance = minDistance;
        src.maxDistance = maxDistance;

        if (playOnStart && ambienceClip != null)
            src.Play();
    }

    public void StartAmbience()
    {
        if (src != null && ambienceClip != null && !src.isPlaying)
            src.Play();
    }

    public void StopAmbience()
    {
        if (src != null && src.isPlaying)
            src.Stop();
    }
}