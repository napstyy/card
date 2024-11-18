using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleUIController : MonoBehaviour {
    [SerializeField] Button[] betsButtons;
    [SerializeField] TextMeshProUGUI chipsText;

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
}