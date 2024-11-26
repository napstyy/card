using System;
using UnityEngine;
using CardGame;

public class Player : MonoBehaviour
{
    [Header("Starting Values")]
    public int startingChips = 10000;

    // Events for UI updates
    public event Action<int> OnChipsUpdate;
    public event Action<int> OnBetsChanged;

    // Current values
    public int ownedChips { get; private set; }
    public int totalBets { get; private set; }

    // Reference to stats
    private PlayerStats playerStats;
    private GameManager gameManager;

    private void Start()
    {
        // Find GameManager
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("[Player] GameManager not found!");
            return;
        }

        // Subscribe to game state changes
        gameManager.OnGameStateChanged += HandleGameStateChange;

        // Remove initial chip update as it will be handled by InitializeWithStats
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged -= HandleGameStateChange;
        }
    }

    public void InitializeWithStats(PlayerStats stats)
    {
        playerStats = stats;

        // Subscribe to PlayerStats events
        playerStats.OnChipsChanged += HandleChipsChanged;
        playerStats.OnBetsChanged += HandleBetsChanged;

        // Sync initial values
        UpdateChips(playerStats.ownedChips);
        UpdateBets(playerStats.totalBets);
    }

    public void HandleChipsChanged(int newValue)
    {
        UpdateChips(newValue);
    }

    public void HandleBetsChanged(int newValue)
    {
        UpdateBets(newValue);
    }

    private void HandleGameStateChange(GameManager.GameState newState)
    {
        switch (newState)
        {
            case GameManager.GameState.RoundEnd:
                // Reset bets at round end
                UpdateBets(0);
                break;
        }
    }

    public void UpdateChips(int newValue)
    {
        ownedChips = newValue;
        OnChipsUpdate?.Invoke(newValue);
    }

    public void UpdateBets(int newValue)  // Change from private to public
    {
        totalBets = newValue;
        OnBetsChanged?.Invoke(newValue);
    }

    public void AddBets(int value)
    {
        if (value <= 0) return;

        if (playerStats != null)
        {
            playerStats.AddBet(value);
        }
        else
        {
            // Fallback if stats not initialized
            UpdateBets(totalBets + value);  // Use UpdateBets instead of direct assignment
        }
    }

    public void ResetBets()
    {
        if (playerStats != null)
        {
            playerStats.ResetBets();
        }
        else
        {
            UpdateBets(0);
        }
    }

    public void AddChips(int value)
    {
        if (value <= 0) return;

        if (playerStats != null)
        {
            playerStats.AddChips(value);
        }
        else
        {
            // Fallback if stats not initialized
            UpdateChips(ownedChips + value);
        }
    }

    public void RemoveChips(int value)
    {
        if (value <= 0) return;

        if (playerStats != null)
        {
            playerStats.RemoveChips(value);
        }
        else
        {
            // Fallback if stats not initialized
            UpdateChips(ownedChips - value);
        }
    }

    // New methods for item management
    public bool UseItem(int itemId)
    {
        return playerStats?.UseItem(itemId) ?? false;
    }

    public bool HasItem(int itemId)
    {
        return playerStats?.HasItem(itemId) ?? false;  // Fix incorrect method call
    }

    public void AddItem(int itemId)
    {
        playerStats?.OwnedItems.Add(itemId);
    }
}