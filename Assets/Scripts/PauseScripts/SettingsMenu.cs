using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer mainMixer;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Audio Text Displays")]
    public TMP_Text masterText;
    public TMP_Text musicText;
    public TMP_Text sfxText;

    [Header("Audio Feedback")]
    public AudioSource uiAudioSource;
    public AudioClip sliderTickSound;
    private float lastTickTime;
    private float tickCooldown = 0.1f; // How fast the tick can repeat (0.1 = 10 times per sec)
    private bool isInitialized = false; // Prevents sounds from playing on Start()

    [Header("Gameplay")]
    public Slider sensitivitySlider;
    public TMP_Text sensitivityText;

    void Start()
    {
        float savedMaster = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 1f);
        float savedSens = PlayerPrefs.GetFloat("MouseSensitivity", 100f);

        masterSlider.value = savedMaster;
        musicSlider.value = savedMusic;
        sfxSlider.value = savedSFX;
        sensitivitySlider.value = savedSens;

        SetMasterVolume(savedMaster);
        SetMusicVolume(savedMusic);
        SetSFXVolume(savedSFX);

        sensitivityText.text = savedSens.ToString("0.##");

        // --- ADD THIS BLOCK RIGHT HERE ---
        if (uiAudioSource != null)
        {
            uiAudioSource.ignoreListenerPause = true;
        }
        // ---------------------------------

        // Initialization is done, allow sounds to play from now on
        isInitialized = true;
    }

    public void SetMasterVolume(float volume)
    {
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
        UpdateVolumeText(masterText, volume);
        PlayTickSound();
    }

    public void SetMusicVolume(float volume)
    {
        mainMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
        UpdateVolumeText(musicText, volume);
        PlayTickSound();
    }

    public void SetSFXVolume(float volume)
    {
        mainMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
        UpdateVolumeText(sfxText, volume);
        PlayTickSound();
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        sensitivityText.text = sensitivity.ToString("0.##");

        PlayerLook playerLook = FindFirstObjectByType<PlayerLook>();
        if (playerLook != null)
        {
            playerLook.mouseSensitivity = sensitivity;
        }

        // Also play a tick when adjusting sensitivity!
        PlayTickSound();
    }

    private void UpdateVolumeText(TMP_Text textElement, float volume)
    {
        int percentage = Mathf.RoundToInt(volume * 100f);
        textElement.text = percentage.ToString() + "%";
    }

    // --- New Audio Feedback Method ---
    // --- New Audio Feedback Method ---
    private void PlayTickSound()
    {
        // Don't play if the game just started, or if we are missing references
        if (!isInitialized || uiAudioSource == null || sliderTickSound == null) return;

        // Check if enough time has passed since the last tick 
        if (Time.unscaledTime - lastTickTime > tickCooldown)
        {
            // FIX: Assign the clip and use Play() instead of PlayOneShot()
            // This cuts off the previous tick instantly so they never overlap and max out the engine
            uiAudioSource.clip = sliderTickSound;
            uiAudioSource.Play();

            lastTickTime = Time.unscaledTime;
        }
    }
}