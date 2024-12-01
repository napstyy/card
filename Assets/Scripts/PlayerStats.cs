using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

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
            if (totalBets > ownedChips) return;

            totalBets += value;
            OnBetsChanged?.Invoke(totalBets);
        }

        public void PayBet()
        {
            if (totalBets == 0 || totalBets > ownedChips) return;
            RemoveChips(totalBets);
        }

        public void ResetBets()
        {
            totalBets = 0;
            OnBetsChanged?.Invoke(totalBets);
        }
        #endregion

        #region Item Management

        // Add this method to your PlayerStats class
        public int GetItemCount(int itemId)
        {
            return _ownedItems.Count(x => x == itemId);
        }

        // Add this method to check for multiple items
        public bool HasItem(int itemId)
        {
            return _ownedItems.Contains(itemId);
        }

        public void AddItem(int itemId)
        {
            _ownedItems.Add(itemId);
            OnItemAdded?.Invoke(itemId);
        }

        public bool UseItem()
        {
            if (_ownedItems.Count == 0) return false;
            int itemId = FirstItemID();
            _ownedItems.RemoveAt(0);
            OnItemRemoved?.Invoke(itemId);
            return true;
        }
        public bool CanAfford(int amount) => ownedChips >= amount;
        public int FirstItemID() => _ownedItems.Count > 0?_ownedItems[0]:-1;
        #endregion
    }
}