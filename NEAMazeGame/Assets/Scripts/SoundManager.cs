using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{

    [SerializeField] Slider sfxSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] TextMeshProUGUI sfxVolumeIndicator;
    [SerializeField] TextMeshProUGUI musicVolumeIndicator;

    [SerializeField] AudioMixer mixer;

    // Start is called before the first frame update
    void Start()
    {
        // PlayerPrefs is a method of saving values onto the computer,
        // so it can be saved even after the application closes.
        // We can use this to save user settings.

        if(!PlayerPrefs.HasKey("SFXVolume")){ // checks if user has already altered sfx volume before
            PlayerPrefs.SetFloat("SFXVolume", 1);
        }
        float newSFXVolume = PlayerPrefs.GetFloat("SFXVolume");
        sfxSlider.value = newSFXVolume; // initialises slider position to either max if first time,
                                        // or the user setting
        sfxVolumeIndicator.text = "" + Mathf.RoundToInt(newSFXVolume * 100) + "%"; // displays as %

        if(!PlayerPrefs.HasKey("MusicVolume")){ // repeat with music volume
            PlayerPrefs.SetFloat("MusicVolume", 1);
        }
        float newMusicVolume = PlayerPrefs.GetFloat("MusicVolume");
        musicSlider.value = newMusicVolume;
        musicVolumeIndicator.text = "" + Mathf.RoundToInt(newMusicVolume * 100) + "%";
    }

    public void changeSFXVolume(){
        // Set sfx volume to slider value (converted from decibels)
        mixer.SetFloat("SFXVolume", Mathf.Log10(sfxSlider.value) * 20);
        // Saves the user preference under the 'SFXVolume' tag
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        sfxVolumeIndicator.text = "" + Mathf.RoundToInt(sfxSlider.value * 100) + "%";
    }

    public void changeMusicVolume(){
        mixer.SetFloat("MusicVolume", Mathf.Log10(musicSlider.value) * 20);
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        musicVolumeIndicator.text = "" + Mathf.RoundToInt(musicSlider.value * 100) + "%";
    }

}
