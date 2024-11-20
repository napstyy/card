using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleUIController : MonoBehaviour {
    [SerializeField] Button[] betsButtons;
    [SerializeField] Button[] gameButtons;
    [SerializeField] TextMeshProUGUI chipsText;
    [SerializeField] TextMeshProUGUI betText;
    [SerializeField] TextMeshProUGUI playerPointsText;
    [SerializeField] Button startButton;
    [SerializeField] Button replaceButton;

    void Awake()
    {
        FindAnyObjectByType<Test>().OnChipsUpdate += UpdateChipsText;
    }

    void UpdateChipsText(int newValue)
    {
        chipsText.SetText(newValue.ToString());
    }

    public void ShowBetsButtons()
    {
        foreach(Button button in betsButtons)
        {
            button.gameObject.SetActive(true);
        }
    }

    public void HideBetsButtons()
    {
        foreach(Button button in betsButtons)
        {
            button.gameObject.SetActive(false);
        }
    }

    public void ShowGameButtons()
    {
        foreach(Button button in gameButtons)
        {
            button.gameObject.SetActive(true);
        }
    }

    public void HideGameButtons()
    {
        foreach(Button button in gameButtons)
        {
            button.gameObject.SetActive(false);
        }
    }

    public void UpdateBetText(int newValue)
    {
        betText.SetText(newValue.ToString());
    }

    // public void UpdatePlayerPointsText(int newValue)
    // {
    //     playerPointsText.SetText(newValue.ToString());
    // }

    public void ShowPlayerPointsText()
    {
        playerPointsText.gameObject.SetActive(true);
    }

    public void HidePlayerPointsText()
    {
        playerPointsText.gameObject.SetActive(false);
    }

    public void EnableStartButton()
    {
        startButton.interactable = true;
    }

    public void DisableStartButton()
    {
        startButton.interactable = false;
    }

    public void EnableReplaceButton()
    {
        replaceButton.interactable = true;
    }

    public void DisableReplaceButton()
    {
        replaceButton.interactable = false;
    }
}