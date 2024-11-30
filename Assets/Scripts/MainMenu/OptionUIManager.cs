using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionUIManager : MonoBehaviour
{
    public GameObject optionsMenu;
    public GameObject backButton;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    void Start()
    {
        backButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(Back);
        masterSlider.GetComponent<UnityEngine.UI.Slider>().onValueChanged.AddListener(SetMasterVolume);
        musicSlider.GetComponent<UnityEngine.UI.Slider>().onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.GetComponent<UnityEngine.UI.Slider>().onValueChanged.AddListener(SetSFXVolume);
    }

    void Back()
    {
        // Close the options menu
        Debug.Log("Back button clicked");
        optionsMenu.SetActive(false);
    }

    void SetMasterVolume(float volume)
    {
        // Set the master volume
        AudioManager.Instance.SetMasterVolume(volume);
    }

    void SetMusicVolume(float volume)
    {
        // Set the music volume
        AudioManager.Instance.SetMusicVolume(volume);
    }

    void SetSFXVolume(float volume)
    {
        // Set the SFX volume
        AudioManager.Instance.SetSFXVolume(volume);
    }
}
