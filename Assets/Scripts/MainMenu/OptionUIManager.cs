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
    [SerializeField] private TMP_Dropdown displayModeDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown refreshRateDropdown;

    private Resolution[] resolutions;
    private int currentRefreshRateIndex;
    private int totalNumberOfRefreshRates;

    void Start()
    {
        backButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(Back);
        masterSlider.GetComponent<UnityEngine.UI.Slider>().onValueChanged.AddListener(SetMasterVolume);
        musicSlider.GetComponent<UnityEngine.UI.Slider>().onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.GetComponent<UnityEngine.UI.Slider>().onValueChanged.AddListener(SetSFXVolume);
        displayModeDropdown.onValueChanged.AddListener(SetDisplayMode);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        refreshRateDropdown.onValueChanged.AddListener(SetRefreshRate); // Add this line
        resolutions = Screen.resolutions;

        if (refreshRateDropdown.options.Count == 0)
        {
            PopulateRefreshRateDropdown();
            totalNumberOfRefreshRates = refreshRateDropdown.options.Count;
        }

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
        optionsMenu.SetActive(true);
        background.SetActive(true);
    }

    void Back()
    {
        AudioManager.Instance.PlayButtonClick();
        optionsMenu.SetActive(false);
        background.SetActive(false);
    }

    void SetMasterVolume(float volume)
    {
        AudioManager.Instance.SetMasterVolume(volume);
    }

    void SetMusicVolume(float volume)
    {
        AudioManager.Instance.SetMusicVolume(volume);
    }

    void SetSFXVolume(float volume)
    {
        AudioManager.Instance.SetSFXVolume(volume);
    }

    void SetDisplayMode(int displayMode)
    {
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
        Resolution resolution = resolutions[resolutionIndex * totalNumberOfRefreshRates + currentRefreshRateIndex];
        RefreshRate refreshRate = resolution.refreshRateRatio;
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, refreshRate);
        PopulateRefreshRateDropdown(); // Refresh the refresh rate dropdown
    }

    void SetRefreshRate(int refreshRateIndex)
    {
        RefreshRate refreshRate = resolutions[currentRefreshRateIndex].refreshRateRatio;
        Resolution currentResolution = Screen.currentResolution;
        Screen.SetResolution(currentResolution.width, currentResolution.height, Screen.fullScreenMode, refreshRate);
    }

    void PopulateResolutionDropdown()
    {
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
                    currentResolutionIndex = i * totalNumberOfRefreshRates;
                }
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex / totalNumberOfRefreshRates;
        resolutionDropdown.RefreshShownValue();
    }

    void PopulateRefreshRateDropdown()
    {
        refreshRateDropdown.ClearOptions();

        List<string> options = new List<string>();
        currentRefreshRateIndex = 0;
        Resolution currentResolution = Screen.currentResolution;

        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == currentResolution.width && resolutions[i].height == currentResolution.height)
            {
                int refreshRate = (int)resolutions[i].refreshRateRatio.numerator / (int)resolutions[i].refreshRateRatio.denominator;
                string option = refreshRate + " Hz";
                if (!options.Contains(option))
                {
                    options.Add(option);
                    if (refreshRate == (int)currentResolution.refreshRateRatio.numerator / (int)currentResolution.refreshRateRatio.denominator)
                    {
                        currentRefreshRateIndex = options.Count - 1;
                    }
                }
            }
        }

        refreshRateDropdown.AddOptions(options);
        refreshRateDropdown.value = currentRefreshRateIndex;
        refreshRateDropdown.RefreshShownValue();
    }

    void PopulateDisplayModeDropdown()
    {
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
