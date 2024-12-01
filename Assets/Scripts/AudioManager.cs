using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    public AudioMixer audioMixer;

    [SerializeField] private AudioClip buttonClick;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(AudioClip musicClip, ulong delay = 0)
    {
        musicSource.clip = musicClip;
        musicSource.Play(delay);
    }

    public void PlaySFX(AudioClip sfxClip)
    {
        sfxSource.PlayOneShot(sfxClip);
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", volume);
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClick);
    }
}
