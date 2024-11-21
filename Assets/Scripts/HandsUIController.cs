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
            pointsText.SetText(points.ToString());
        };
    }    
}