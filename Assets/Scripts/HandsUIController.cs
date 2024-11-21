using CardGame;
using UnityEngine;
using UnityEngine.UI;

public class HandsUIController : MonoBehaviour 
{
    [SerializeField] Button replaceButton;
    [SerializeField] Button hitButton;

    Hands hands;

    private void Start() {
        hands = transform.root.GetComponent<Hands>();
        replaceButton.onClick.AddListener(()=>BlackjackController.Instance.Replace());
        hitButton.onClick.AddListener(()=>BlackjackController.Instance.Hit(hands));
    }    
}