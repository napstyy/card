using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public GameObject startButton;
    public GameObject optionsButton;
    public GameObject exitButton;

    [SerializeField] private GameObject optionsMenu;

    private const string gameScene = "Blackjack_UI_2";

    void Start()
    {
        startButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(StartGame);
        optionsButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OpenOptions);
        exitButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(ExitGame);
    }

    void StartGame()
    {
        // Load the game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameScene);
    }

    void OpenOptions()
    {
        // Open the options menu
        optionsMenu.SetActive(true);
    }

    void ExitGame()
    {
        // Exit the application if it is a build, or stop play mode if in the editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
