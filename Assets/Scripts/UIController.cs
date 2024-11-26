using System;
using System.Collections;
using System.Collections.Generic;
using CardGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Blackjack Common Actions")]
    [SerializeField] GameObject actionButtons;
    [SerializeField] Button doubleDownButton;
    [SerializeField] Button replaceButton;
    [SerializeField] Button standButton;
    [SerializeField] Button splitButton;

    [Header("Bet and Chips")]
    [SerializeField] TextMeshProUGUI chipsText;
    [SerializeField] TextMeshProUGUI betsText;
    [SerializeField] GameObject betsButtons;
    [SerializeField] Button startButton;

    [Header("Others")]
    [SerializeField] GameObject confirmButton;

    private Player player;

    private void Start()
    {
        BlackjackController.Instance.RoundStateChanged += RoundStateChanged;
        player = FindAnyObjectByType<Player>();
        player.OnChipsUpdate += UpdateChipsText;
        player.OnBetsChanged += UpdateBetsText;
        UpdateChipsText(player.ownedChips);
        UpdateBetsText(player.totalBets);
    }

    private void Update() {
        if (BlackjackController.Instance.roundState == BlackjackController.RoundState.Start)
        {
            if(BlackjackController.Instance.isSplit || BlackjackController.Instance.isDoubleDown)
            {
                DisableDoubleDownButton();
                DisableSplitButton();
            }
            else
            {   
                EnableDoubleDownButton();
                if(BlackjackController.Instance.AllowSplit)
                {
                    EnableSplitButton();
                }
            }
        }
        else
        {
            startButton.interactable = player.totalBets > 0;
        }
    }

    private void RoundStateChanged(BlackjackController.RoundState state)
    {
        if(state == BlackjackController.RoundState.Start)
        {
            actionButtons.SetActive(true);
            confirmButton.SetActive(false);
            DisableBets();
        }
        else
        {
            actionButtons.SetActive(false);
            confirmButton.SetActive(true);
            EnableBets();
        }
    }

    void UpdateChipsText(int newValue)
    {
        chipsText.SetText(newValue.ToString());
    }

    void UpdateBetsText(int newValue)
    {
        betsText.SetText(newValue.ToString());
    }

    public void EnableDoubleDownButton()
    {
        doubleDownButton.gameObject.SetActive(true);
    }

    public void DisableDoubleDownButton()
    {
        doubleDownButton.gameObject.SetActive(false);
    }

    public void EnableSplitButton()
    {
        splitButton.gameObject.SetActive(true);
    }

    public void DisableSplitButton()
    {
        splitButton.gameObject.SetActive(false);
    }

    public void EnableReplaceButton()
    {
        replaceButton.interactable = true;
    }

    public void DisableReplaceButton()
    {
        replaceButton.interactable = false;
    }

    public void EnableBets()
    {
        betsButtons.SetActive(true);
        startButton.gameObject.SetActive(true);
    }

    public void DisableBets()
    {
        betsButtons.SetActive(false);
        startButton.gameObject.SetActive(false);
    }
}
