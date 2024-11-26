using UnityEngine;
using System;
using System.Collections.Generic;

namespace CardGame
{
    [System.Serializable]
    public class GameProgress
    {
        public readonly int MaxRounds;
        public readonly int TargetMoney;
        public readonly int SwapsPerRound;

        public int CurrentRound { get; private set; }
        public int CurrentSwapsRemaining { get; private set; }

        public event Action<int> OnRoundChanged;
        public event Action<int> OnSwapsChanged;

        public GameProgress(int maxRounds, int targetMoney, int swapsPerRound)
        {
            MaxRounds = maxRounds;
            TargetMoney = targetMoney;
            SwapsPerRound = swapsPerRound;
            CurrentRound = 1;
            ResetSwaps();
        }

        public void AdvanceRound()
        {
            CurrentRound++;
            OnRoundChanged?.Invoke(CurrentRound);
            ResetSwaps();
        }

        public void UseSwap()
        {
            if (CurrentSwapsRemaining > 0)
            {
                CurrentSwapsRemaining--;
                OnSwapsChanged?.Invoke(CurrentSwapsRemaining);
            }
        }

        public void ResetSwaps()
        {
            CurrentSwapsRemaining = SwapsPerRound;
            OnSwapsChanged?.Invoke(CurrentSwapsRemaining);
        }

        public bool IsLastRound() => CurrentRound >= MaxRounds;
        public bool HasSwapsRemaining() => CurrentSwapsRemaining > 0;
    }

    public class GameManager : MonoBehaviour
    {
        #region Singleton
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (playerComponent != null)
            {
                // Unsubscribe from events
                Stats.OnChipsChanged -= playerComponent.HandleChipsChanged;
                Stats.OnBetsChanged -= playerComponent.HandleBetsChanged;
            }
        }
        #endregion

        #region Settings
        [Header("Game Settings")]
        [SerializeField] private int targetMoney = 1000000;
        [SerializeField] private int maxRounds = 6;
        [SerializeField] private int swapsPerRound = 3;
        [SerializeField] private int startingChips = 10000;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;
        #endregion

        #region State Management
        public enum GameState
        {
            Preparation,
            Betting,
            Playing,
            Shopping,
            RoundEnd,
            GameOver
        }

        public GameState CurrentState { get; private set; }
        public bool IsGameActive { get; private set; }

        // Core components
        private BlackjackController blackjackController;
        private Player playerComponent;

        // Game data
        public PlayerStats Stats { get; private set; }
        public GameProgress Progress { get; private set; }

        public int CurrentRound => Progress.CurrentRound;

        // Events
        public event Action<GameState> OnGameStateChanged;
        public event Action<bool> OnGameOver;
        #endregion

        #region Initialization
        private void Start()
        {
            InitializeReferences();
            StartNewGame();
        }

        private void InitializeReferences()
        {
            blackjackController = FindObjectOfType<BlackjackController>();
            playerComponent = FindObjectOfType<Player>();

            if (!blackjackController || !playerComponent)
            {
                LogError("Required components not found!");
                enabled = false;
                return;
            }

            // Use startingChips from inspector instead of Player component
            Stats = new PlayerStats(startingChips);  // Changed from playerComponent.startingChips

            // Initialize player with stats
            playerComponent.InitializeWithStats(Stats);
            SyncPlayerWithStats();

            // Connect game events
            OnGameStateChanged += HandleGameStateChange;
            Progress.OnRoundChanged += HandleRoundChange;
            Progress.OnSwapsChanged += HandleSwapsChange;

            LogDebug("Game components initialized successfully");
        }

        private void HandleGameStateChange(GameState newState)
        {
            switch (newState)
            {
                case GameState.Preparation:
                    // Reset for new round
                    Stats.ResetBets();
                    Progress.ResetSwaps();
                    break;

                case GameState.RoundEnd:
                    // Handle end of round cleanup
                    if (Stats.ownedChips >= Progress.TargetMoney)
                    {
                        EndGame(true);
                    }
                    break;
            }
        }

        private void HandleRoundChange(int newRound)
        {
            LogDebug($"Round changed to {newRound}");
            // Add any round change specific logic
        }

        private void HandleSwapsChange(int remainingSwaps)
        {
            LogDebug($"Swaps remaining: {remainingSwaps}");
            // Add any swap change specific logic
        }

        // Add this method to handle shop purchases
        public bool TryPurchaseItem(int itemId, int cost)
        {
            if (!Stats.CanAfford(cost))
            {
                LogDebug("Cannot afford item");
                return false;
            }

            Stats.RemoveChips(cost);
            Stats.OwnedItems.Add(itemId);
            LogDebug($"Purchased item {itemId} for {cost} chips");
            return true;
        }


        private void InitializeGame()
        {
            Progress = new GameProgress(maxRounds, targetMoney, swapsPerRound);
            IsGameActive = true;
            // Remove Stats initialization from here as it's done in InitializeReferences
        }

        private void StartNewGame()
        {
            InitializeGame();
            StartNewRound();
        }

        private void SyncPlayerWithStats()
        {
            // Connect PlayerStats events to Player component
            Stats.OnChipsChanged += (chips) => playerComponent.UpdateChips(chips);
            Stats.OnBetsChanged += (bets) => playerComponent.AddBets(bets);
        }
        #endregion

        #region Game Flow Control
        public void StartNewRound()
        {
            if (!IsGameActive || Progress.IsLastRound())
            {
                EndGame(Stats.ownedChips >= Progress.TargetMoney);
                return;
            }

            Progress.ResetSwaps();
            SetGameState(GameState.Preparation);
            blackjackController.GetRandomRules();

            LogDebug($"Starting Round {Progress.CurrentRound}");
        }

        public void SetGameState(GameState newState)
        {
            if (CurrentState == newState) return;

            LogDebug($"State changing from {CurrentState} to {newState}");
            CurrentState = newState;
            OnGameStateChanged?.Invoke(newState);

            HandleStateTransition(newState);
        }

        private void HandleStateTransition(GameState newState)
        {
            switch (newState)
            {
                case GameState.Preparation:
                    HandlePreparationPhase();
                    break;
                case GameState.Betting:
                    HandleBettingPhase();
                    break;
                case GameState.Playing:
                    HandlePlayingPhase();
                    break;
                case GameState.Shopping:
                    HandleShoppingPhase();
                    break;
                case GameState.RoundEnd:
                    HandleRoundEndPhase();
                    break;
                case GameState.GameOver:
                    HandleGameOverPhase();
                    break;
            }
        }
        #endregion

        #region Phase Handlers
        private void HandlePreparationPhase()
        {
            blackjackController.Restart();
            // Additional preparation logic
        }

        private void HandleBettingPhase()
        {
            LogDebug("Entering betting phase");
            // Betting phase initialization if needed
        }

        private void HandlePlayingPhase()
        {
            blackjackController.StartOfRound();
        }

        private void HandleShoppingPhase()
        {
            LogDebug("Entering shopping phase");
            // Shopping phase initialization
        }

        private void HandleRoundEndPhase()
        {
            if (Stats.ownedChips >= Progress.TargetMoney)
            {
                EndGame(true);
                return;
            }

            Progress.AdvanceRound();
            SetGameState(GameState.Shopping);
        }

        private void HandleGameOverPhase()
        {
            IsGameActive = false;
            LogDebug($"Game Over - Player {(Stats.ownedChips >= Progress.TargetMoney ? "Won" : "Lost")}");
        }
        #endregion

        #region Public Methods
        public bool TryUseCardSwap()
        {
            if (CurrentState != GameState.Preparation || !Progress.HasSwapsRemaining())
                return false;

            Progress.UseSwap();
            return true;
        }

        public void CompletePreparation()
        {
            if (CurrentState == GameState.Preparation)
            {
                SetGameState(GameState.Betting);
            }
        }

        public void CompleteBetting()
        {
            if (CurrentState == GameState.Betting && Stats.totalBets > 0)
            {
                SetGameState(GameState.Playing);
            }
        }

        public void CompleteRound(bool playerWon = false, int winAmount = 0)
        {
            if (playerWon)
            {
                Stats.AddChips(winAmount);
            }

            Stats.ResetBets();

            if (Stats.ownedChips >= Progress.TargetMoney || Progress.IsLastRound())
            {
                EndGame(Stats.ownedChips >= Progress.TargetMoney);
            }
            else
            {
                SetGameState(GameState.Shopping);
            }
        }

        public void CompleteShoppingPhase()
        {
            if (CurrentState == GameState.Shopping)
            {
                StartNewRound();
            }
        }

        private void EndGame(bool playerWon)
        {
            SetGameState(GameState.GameOver);
            OnGameOver?.Invoke(playerWon);
        }
        #endregion

        #region Utility Methods
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[GameManager] {message}");
            }
        }

        private void LogError(string message)
        {
            Debug.LogError($"[GameManager] {message}");
        }
        #endregion
    }
}