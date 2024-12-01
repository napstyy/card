using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionUIManager : MonoBehaviour
{
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject background;
    public GameObject backButton;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Dropdown displayModeDropdown; // Change this line
    [SerializeField] private TMP_Dropdown resolutionDropdown; // Change this line

    private Resolution[] resolutions;

    void Start()
    {
        backButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(Back);
        masterSlider.GetComponent<UnityEngine.UI.Slider>().onValueChanged.AddListener(SetMasterVolume);
        musicSlider.GetComponent<UnityEngine.UI.Slider>().onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.GetComponent<UnityEngine.UI.Slider>().onValueChanged.AddListener(SetSFXVolume);
        displayModeDropdown.onValueChanged.AddListener(SetDisplayMode);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);

        // Ensure these methods are only called once
        if (resolutionDropdown.options.Count == 0)
        {
            PopulateResolutionDropdown();
        }

        if (displayModeDropdown.options.Count == 0)
        {
            PopulateDisplayModeDropdown();
        }
    }

    public void OpenOptions()
    {
        // Open the options menu
        optionsMenu.SetActive(true);
        background.SetActive(true);
    }

    void Back()
    {
        // Close the options menu
        optionsMenu.SetActive(false);
        background.SetActive(false);
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

    void SetDisplayMode(int displayMode)
    {
        // Set the display mode
        switch (displayMode)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
        }
    }

    void SetResolution(int resolutionIndex)
    {
        // Set the resolution
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
    }

    // void PopulateResolutionDropdown()
    // {
    //     // Populate the resolution dropdown
    //     resolutions = Screen.resolutions;
    //     resolutionDropdown.ClearOptions();

    //     List<string> options = new List<string>();
    //     int currentResolutionIndex = 0;

    //     for (int i = 0; i < resolutions.Length; i++)
    //     {
    //         string option = resolutions[i].width + " x " + resolutions[i].height + " @" + resolutions[i].refreshRate.ToString() + " Hz";
    //         options.Add(option);

    //         if (resolutions[i].width == Screen.currentResolution.width &&
    //             resolutions[i].height == Screen.currentResolution.height)
    //         {
    //             currentResolutionIndex = i;
    //         }
    //     }

    //     resolutionDropdown.AddOptions(options);
    //     resolutionDropdown.value = currentResolutionIndex;
    //     resolutionDropdown.RefreshShownValue();
    // }

    void PopulateResolutionDropdown()
{
    // Populate the resolution dropdown
    resolutions = Screen.resolutions;
    resolutionDropdown.ClearOptions();

    List<string> options = new List<string>();
    HashSet<string> addedResolutions = new HashSet<string>();
    int currentResolutionIndex = 0;

    for (int i = 0; i < resolutions.Length; i++)
    {
        string resolutionString = resolutions[i].width + " x " + resolutions[i].height;
        if (!addedResolutions.Contains(resolutionString))
        {
            addedResolutions.Add(resolutionString);
            string option = resolutionString;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
    }

    resolutionDropdown.AddOptions(options);
    resolutionDropdown.value = currentResolutionIndex;
    resolutionDropdown.RefreshShownValue();
}

    void PopulateDisplayModeDropdown()
    {
        // Populate the display mode dropdown
        displayModeDropdown.ClearOptions();

        List<string> options = new List<string>
        {
            "Fullscreen",
            "Windowed",
            "Exclusive Fullscreen"
        };

        displayModeDropdown.AddOptions(options);
        displayModeDropdown.value = (int)Screen.fullScreenMode;
        displayModeDropdown.RefreshShownValue();
    }
}
