using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public GameObject startButton;
    public GameObject optionsButton;
    public GameObject exitButton;

    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject background;

    private const string gameScene = "GameScene";

    void Start()
    {
        startButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(StartGame);
        optionsButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OpenOptions);
        exitButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(ExitGame);
    }

    void StartGame()
    {
        AudioManager.Instance.PlayButtonClick();
        // Load the game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameScene);
    }

    void OpenOptions()
    {
        AudioManager.Instance.PlayButtonClick();
        // Open the options menu
        optionsMenu.SetActive(true);
        background.SetActive(true);
    }

    void ExitGame()
    {
        AudioManager.Instance.PlayButtonClick();
        // Exit the application if it is a build, or stop play mode if in the editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
