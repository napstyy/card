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
    // [SerializeField] Button splitButton;

    [Header("Bet and Chips")]
    [SerializeField] TextMeshProUGUI chipsText;
    [SerializeField] TextMeshProUGUI betsText;
    [SerializeField] TextMeshProUGUI burstLimitText;
    [SerializeField] TextMeshProUGUI replaceText;
    [SerializeField] GameObject betsButtons;
    [SerializeField] Button confirmBetsButton;
    [SerializeField] Button resetBetsButton;

    [Header("Shop")]
    [SerializeField] GameObject shopWindow;

    [Header("Others")]
    [SerializeField] GameObject shopHeader;
    [SerializeField] GameObject normalHeader;
    [SerializeField] Button confirmButton;
    [SerializeField] TextMeshProUGUI targetMoneyText;
    [SerializeField] TextMeshProUGUI remainingRoundsText;

    [Header("Options")]
    [SerializeField] Button optionsButton;
    [SerializeField] OptionUIManager optionsMenu;


    private PlayerStats player;

    private void Start()
    {
        GameManager.Instance.OnGameStateChanged += GameStateChangedHandler;
        BlackjackController.Instance.OnRoundStateChanged += RoundStateChangedHandler;
        targetMoneyText.SetText($"{GameManager.Instance.TargetMoney}");
        optionsMenu = FindObjectOfType<OptionUIManager>();
        GameObject canvas = optionsMenu.transform.root.gameObject;
        Canvas canvasComponent = canvas.GetComponent<Canvas>();
        if (canvasComponent != null && canvasComponent.renderMode == RenderMode.ScreenSpaceCamera)
        {
            canvasComponent.worldCamera = Camera.main;
        }
        else
        {
            canvasComponent.renderMode = RenderMode.ScreenSpaceCamera;
            canvasComponent.worldCamera = Camera.main;
        }
        confirmButton.onClick.AddListener(() => GameManager.Instance.CompleteRound());
        optionsButton.onClick.AddListener(() => optionsMenu.OpenOptions());
    }

    private void Update() {
        remainingRoundsText.SetText($"{GameManager.Instance.CurrentRound}/{GameManager.Instance.MaxRounds}");
        burstLimitText.SetText(BlackjackController.Instance.RoundBurstLimit.ToString());
        replaceText.SetText(GameManager.Instance.Swaps.ToString());
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
                shopHeader.SetActive(false);
                normalHeader.SetActive(true);
                confirmButton.gameObject.SetActive(false);
                shopWindow.SetActive(false);
            break;
            case GameManager.GameState.Betting:
                betsButtons.SetActive(true);
                shopHeader.SetActive(false);
                normalHeader.SetActive(true);
                actionButtons.SetActive(false);
                shopWindow.SetActive(false);
            break;
            case GameManager.GameState.Playing:
                betsButtons.SetActive(false);
                actionButtons.SetActive(true);
            break;
            case GameManager.GameState.Shopping:
                shopHeader.SetActive(true);
                normalHeader.SetActive(false);
                betsButtons.SetActive(false);
                actionButtons.SetActive(false);
                confirmButton.gameObject.SetActive(false);
                shopWindow.SetActive(true);
            break;
            case GameManager.GameState.RoundEnd:

            break;
        }
    }

    void UpdateChipsText(int newValue)
    {
        Debug.Log("Chips Changed");
        chipsText.SetText("<sprite name=\"chips\"> "+newValue.ToString());
    }

    void UpdateBetsText(int newValue)
    {
        Debug.Log("Bets Changed");
        betsText.SetText(newValue.ToString());
    }

    public void BindTextWithPlayer(PlayerStats playerStats)
    {
        player = playerStats;
        player.OnChipsChanged += UpdateChipsText;
        player.OnBetsChanged += UpdateBetsText;
        UpdateChipsText(player.ownedChips);
        UpdateBetsText(player.totalBets);
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
        // splitButton.gameObject.SetActive(true);
    }

    public void DisableSplitButton()
    {
        // splitButton.gameObject.SetActive(false);
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
