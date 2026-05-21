using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer mainMixer;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Gameplay")]
    public Slider sensitivitySlider;

    void Start()
    {
        // Load saved values, or default to 1 (max volume/default sensitivity) if none exist
        float savedMaster = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 1f);
        float savedSens = PlayerPrefs.GetFloat("MouseSensitivity", 100f);

        // Update sliders to match saved values
        masterSlider.value = savedMaster;
        musicSlider.value = savedMusic;
        sfxSlider.value = savedSFX;
        sensitivitySlider.value = savedSens;

        // Apply audio settings immediately
        SetMasterVolume(savedMaster);
        SetMusicVolume(savedMusic);
        SetSFXVolume(savedSFX);
    }

    // --- Assign these methods to the "On Value Changed" event on your UI Sliders ---

    public void SetMasterVolume(float volume)
    {
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        mainMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        mainMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        // Save it to PlayerPrefs for future levels/sessions
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);

        // Find the player and update them immediately if they exist in the current scene
        PlayerLook playerLook = FindFirstObjectByType<PlayerLook>();
        if (playerLook != null)
        {
            playerLook.mouseSensitivity = sensitivity;
        }
    }
}