using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class BGMusicDontDestroy : MonoBehaviour
{

    [SerializeField] AudioMixer mixer;

    // Called before game starts, used for initialising
    void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("BGMenuMusic");
        if(objs.Length > 1){ // destroy other instances with the tag if they exist
                             // (returning to the menu will try to create another instance, this avoids that)
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject); // dont destroy this object for seamless music between scenes
    }

    void Start(){
        mixer.SetFloat("SFXVolume", Mathf.Log10(PlayerPrefs.GetFloat("SFXVolume")) * 20);
        mixer.SetFloat("MusicVolume", Mathf.Log10(PlayerPrefs.GetFloat("MusicVolume")) * 20);
    }

}


