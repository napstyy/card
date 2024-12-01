using CardGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandsUIController : MonoBehaviour
{
    [SerializeField] Button hitButton;
    [SerializeField] Image background;
    [SerializeField] TextMeshProUGUI pointsText;
    [SerializeField] GameObject pointsTextHolder;


    Hands hands;

    private void Start()
    {
        hands = transform.root.GetComponent<Hands>();
        hitButton.onClick.AddListener(() => BlackjackController.Instance.Hit(hands));
        GetComponent<Canvas>().worldCamera = Camera.main;
        hands.OnHandsUpdate += (int points, int extraPoints) =>
        {
            
            pointsText.SetText(points == -1 ? "" : points.ToString());
            // Change text color based on extraPoints value
            if (extraPoints > 0)
            {
                // Set text color to green if extraPoints is positive
                pointsText.color = Color.green; 
            }
            else if (extraPoints < 0)
            {
                // Set text color to red if extraPoints is negative
                pointsText.color = new Color(1f, 0.4f, 0f);;
            }
            else
            {
                // Set text color to a neutral color if extraPoints is zero
                pointsText.color = Color.white; 
            }
        };
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing && hitButton.gameObject.activeSelf)
        {
            DisableHitButton();
            DisableBackground();
            pointsTextHolder.SetActive(false);
        }
        else if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            EnableHitButton();
            EnableBackground();
            pointsTextHolder.SetActive(true);
        }
    }

    private void EnableHitButton()
    {
        hitButton.gameObject.SetActive(true);
    }

    private void DisableHitButton()
    {
        hitButton.gameObject.SetActive(false);
    }

    private void EnableBackground()
    {
        background?.gameObject.SetActive(true);
    }

    private void DisableBackground()
    {
        background?.gameObject.SetActive(false);
    }
}