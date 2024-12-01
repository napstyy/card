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
    [SerializeField] Button confirmBetsButton;
    [SerializeField] Button resetBetsButton;

    [Header("Others")]
    [SerializeField] Button confirmButton;
    [SerializeField] Button startGameButton;

    [Header("Options")]
    [SerializeField] Button optionsButton;
    [SerializeField] OptionUIManager optionsMenu;

    private PlayerStats player;

    private void Start()
    {
        GameManager.Instance.OnGameStateChanged += GameStateChangedHandler;
        BlackjackController.Instance.OnRoundStateChanged += RoundStateChangedHandler;
        player = GameManager.Instance.PlayerStats;
        optionsMenu = FindObjectOfType<OptionUIManager>();
        player.OnChipsChanged += UpdateChipsText;
        player.OnBetsChanged += UpdateBetsText;
        confirmButton.onClick.AddListener(() => GameManager.Instance.CompleteRound());
        startGameButton.onClick.AddListener(() => GameManager.Instance.SetGameState(GameManager.GameState.Betting));
        optionsButton.onClick.AddListener(() => optionsMenu.OpenOptions());
        UpdateChipsText(player.ownedChips);
        UpdateBetsText(player.totalBets);
    }

    private void RoundStateChangedHandler(BlackjackController.RoundState state)
    {
        if(GameManager.Instance.CurrentState == GameManager.GameState.Playing)
            switch(state)
            {
                case BlackjackController.RoundState.Start:
                    confirmButton.gameObject.SetActive(false);
                break;
                case BlackjackController.RoundState.End:
                    confirmButton.gameObject.SetActive(true);
                break;
            }
    }

    private void GameStateChangedHandler(GameManager.GameState state)
    {
        switch(state)
        {
            case GameManager.GameState.Preparation:
                betsButtons.SetActive(false);
                actionButtons.SetActive(false);
                confirmButton.gameObject.SetActive(false);
            break;
            case GameManager.GameState.Betting:
                betsButtons.SetActive(true);
                actionButtons.SetActive(false);
            break;
            case GameManager.GameState.Playing:
                betsButtons.SetActive(false);
                actionButtons.SetActive(true);
            break;
            case GameManager.GameState.Shopping:
                betsButtons.SetActive(false);
                actionButtons.SetActive(false);
                confirmButton.gameObject.SetActive(false);
            break;
            case GameManager.GameState.RoundEnd:

            break;
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
        confirmBetsButton.gameObject.SetActive(true);
        resetBetsButton.gameObject.SetActive(true);
    }

    public void DisableBets()
    {
        betsButtons.SetActive(false);
        confirmBetsButton.gameObject.SetActive(false);
        resetBetsButton.gameObject.SetActive(false);
    }
}
