using UnityEngine;
using System;
using System.Collections.Generic;

namespace CardGame
{
    [System.Serializable]
    public class PlayerStats
    {
        #region Events
        public event Action<int> OnChipsChanged;
        public event Action<int> OnBetsChanged;
        public event Action<int> OnItemAdded;
        public event Action<int> OnItemRemoved;
        #endregion

        #region Properties
        public int ownedChips;
        public int totalBets;
        private List<int> _ownedItems = new List<int>();

        // Add public property for OwnedItems
        public List<int> OwnedItems
        {
            get { return _ownedItems; }
        }
        #endregion

        public PlayerStats(int startingChips)
        {
            ownedChips = startingChips;
            OnChipsChanged?.Invoke(startingChips);
        }

        #region Chip Management
        public void AddChips(int value)
        {
            if (value <= 0) return;
            ownedChips += value;
            OnChipsChanged?.Invoke(ownedChips);
        }

        public void RemoveChips(int value)
        {
            if (value <= 0) return;
            ownedChips = Mathf.Max(0, ownedChips - value);
            OnChipsChanged?.Invoke(ownedChips);
        }
        #endregion

        #region Betting
        public void AddBet(int value)
        {
            if (value <= 0) return;
            if (value > ownedChips) return;

            totalBets += value;
            ownedChips -= value;
            OnBetsChanged?.Invoke(totalBets);
            OnChipsChanged?.Invoke(ownedChips);
        }

        public void ResetBets()
        {
            totalBets = 0;
            OnBetsChanged?.Invoke(totalBets);
        }
        #endregion

        #region Item Management
        public void AddItem(int itemId)
        {
            _ownedItems.Add(itemId);
            OnItemAdded?.Invoke(itemId);
        }

        public bool UseItem(int itemId)
        {
            if (!_ownedItems.Contains(itemId)) return false;
            _ownedItems.Remove(itemId);
            OnItemRemoved?.Invoke(itemId);
            return true;
        }

        public bool HasItem(int itemId)
        {
            return _ownedItems.Contains(itemId);
        }
        #endregion

        public bool CanAfford(int amount) => ownedChips >= amount;
    }
}