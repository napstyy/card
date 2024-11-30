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
    [SerializeField] Button startGameButton;

    private PlayerStats player;

    private void Start()
    {
        GameManager.Instance.OnGameStateChanged += GameStateChangedHandler;
        player = GameManager.Instance.PlayerStats;
        player.OnChipsChanged += UpdateChipsText;
        player.OnBetsChanged += UpdateBetsText;
        startGameButton.onClick.AddListener(() => GameManager.Instance.SetGameState(GameManager.GameState.Betting));
        UpdateChipsText(player.ownedChips);
        UpdateBetsText(player.totalBets);
    }

    private void GameStateChangedHandler(GameManager.GameState state)
    {
        switch(state)
        {
            case GameManager.GameState.Preparation:
                betsButtons.SetActive(false);
                actionButtons.SetActive(false);
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
            break;
            case GameManager.GameState.RoundEnd:

            break;
        }
    }

    private void Update() {

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
