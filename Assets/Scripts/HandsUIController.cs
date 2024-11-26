using CardGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HandsUIController : MonoBehaviour 
{
    [SerializeField] Button hitButton;
    [SerializeField] TextMeshProUGUI pointsText;

    Hands hands;

    private void Start() {
        hands = transform.root.GetComponent<Hands>();
        hitButton.onClick.AddListener(()=>BlackjackController.Instance.Hit(hands));
        BlackjackController.Instance.RoundStateChanged += HitButtonHandler;
        GetComponent<Canvas>().worldCamera = Camera.main;
        hands.OnHandsUpdate += (int points) => {
            pointsText.SetText(points.ToString());
        };
    }

    private void OnDisable() {
        BlackjackController.Instance.RoundStateChanged -= HitButtonHandler;
    }
    
    private void HitButtonHandler(BlackjackController.RoundState state)
    {
        if(state == BlackjackController.RoundState.Start)
            EnableHitButton();
        else
            DisableHitButton();
    }

    private void EnableHitButton()
    {
        hitButton.gameObject.SetActive(true);
    }    

    private void DisableHitButton()
    {
        hitButton.gameObject.SetActive(false);
    }
}