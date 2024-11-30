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
        GetComponent<Canvas>().worldCamera = Camera.main;
        hands.OnHandsUpdate += (int points) => {
            pointsText.SetText(points == -1? "":points.ToString());
        };
    }

    private void Update() {
        if(GameManager.Instance.CurrentState != GameManager.GameState.Playing && hitButton.gameObject.activeSelf)
        {
            DisableHitButton();
        }
        else if(GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            EnableHitButton();
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
}