using UnityEngine;
using System;
using System.Collections.Generic;

namespace CardGame
{
    // Defines all possible item types
    public enum ItemType
    {
        PeekNextCards = 0,    // Crystal Ball - See next two cards
        RevealDealerCard = 1, // X-Ray Specs - See dealer's hidden card
        PreventBurst = 2,     // Safety Net - Prevent one burst
        AllInBonus = 3        // High Roller - All-in with 10x multiplier
    }

    // Represents a single item's data
    [System.Serializable]
    public class ItemData
    {
        public ItemType type;
        public string name;
        public string description;
        public Sprite icon;
        public int cost;
        public bool isUnlocked;
        public bool isOneTimeUse;

        public ItemData(ItemType type, string name, string description, int cost, bool isOneTimeUse = true)
        {
            this.type = type;
            this.name = name;
            this.description = description;
            this.cost = cost;
            this.isOneTimeUse = isOneTimeUse;
            this.isUnlocked = false;
        }
    }

    // Main Item Manager class
    public class ItemManager : MonoBehaviour
    {
        #region Singleton
        public static ItemManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeItemDatabase();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        #region Variables
        [Header("References")]
        [SerializeField] private Sprite[] itemIcons;

        private Dictionary<ItemType, ItemData> itemDatabase;
        private BlackjackController blackjackController;
        private GameManager gameManager;

        // Events for UI updates
        public event Action<ItemType> OnItemUsed;
        public event Action<ItemType> OnItemPurchased;
        #endregion

        #region Initialization
        private void Start()
        {
            blackjackController = BlackjackController.Instance;
            gameManager = GameManager.Instance;
        }

        private void InitializeItemDatabase()
        {
            itemDatabase = new Dictionary<ItemType, ItemData>
            {
                {
                    ItemType.PeekNextCards,
                    new ItemData(
                        ItemType.PeekNextCards,
                        "Crystal Ball",
                        "Reveals the next two cards you would draw",
                        5000
                    )
                },
                {
                    ItemType.RevealDealerCard,
                    new ItemData(
                        ItemType.RevealDealerCard,
                        "X-Ray Specs",
                        "Reveals the dealer's hidden card",
                        3000
                    )
                },
                {
                    ItemType.PreventBurst,
                    new ItemData(
                        ItemType.PreventBurst,
                        "Safety Net",
                        "Prevents one burst and returns your bet",
                        7000
                    )
                },
                {
                    ItemType.AllInBonus,
                    new ItemData(
                        ItemType.AllInBonus,
                        "High Roller",
                        "Go all-in with 10x potential return",
                        10000
                    )
                }
            };

            // Assign icons if available
            if (itemIcons != null && itemIcons.Length >= itemDatabase.Count)
            {
                int index = 0;
                foreach (var item in itemDatabase.Values)
                {
                    item.icon = itemIcons[index++];
                }
            }
        }
        #endregion

        #region Public Methods
        public ItemData GetItemData(ItemType type)
        {
            return itemDatabase.TryGetValue(type, out ItemData data) ? data : null;
        }

        public bool CanUseItem(ItemType type)
        {
            if (!gameManager.PlayerStats.HasItem((int)type))
                return false;

            // Additional checks based on game state
            switch (type)
            {
                case ItemType.PeekNextCards:
                    return gameManager.CurrentState == GameManager.GameState.Playing;
                case ItemType.RevealDealerCard:
                    return gameManager.CurrentState == GameManager.GameState.Playing;
                case ItemType.PreventBurst:
                    return gameManager.CurrentState == GameManager.GameState.Playing;
                case ItemType.AllInBonus:
                    return gameManager.CurrentState == GameManager.GameState.Betting;
                default:
                    return false;
            }
        }

        public bool UseItem(ItemType type)
        {
            if (!CanUseItem(type))
                return false;

            bool success = ExecuteItemEffect(type);
            if (success)
            {
                gameManager.PlayerStats.UseItem((int)type);
                OnItemUsed?.Invoke(type);
            }
            return success;
        }

        public bool PurchaseItem(ItemType type)
        {
            ItemData item = GetItemData(type);
            if (item == null || !gameManager.PlayerStats.CanAfford(item.cost))
                return false;

            gameManager.PlayerStats.RemoveChips(item.cost);
            gameManager.PlayerStats.AddItem((int)type);
            OnItemPurchased?.Invoke(type);
            return true;
        }
        #endregion

        #region Private Methods
        private bool ExecuteItemEffect(ItemType type)
        {
            switch (type)
            {
                case ItemType.PeekNextCards:
                    return ExecutePeekNextCards();
                case ItemType.RevealDealerCard:
                    return ExecuteRevealDealerCard();
                case ItemType.PreventBurst:
                    return ExecutePreventBurst();
                case ItemType.AllInBonus:
                    return ExecuteAllInBonus();
                default:
                    return false;
            }
        }

        private bool ExecutePeekNextCards()
        {
            Card[] nextCards = blackjackController.deck.PeekNextCards(2);
            if (nextCards == null)
                return false;

            // TODO: Show UI with card preview
            Debug.Log($"Next cards: {nextCards[0].rank} of {nextCards[0].suit}, {nextCards[1].rank} of {nextCards[1].suit}");
            return true;
        }

        private bool ExecuteRevealDealerCard()
        {
            blackjackController.dealerHands.ShowHands();
            return true;
        }

        private bool ExecutePreventBurst()
        {
            blackjackController.EnableBurstPrevention();
            return true;
        }

        private bool ExecuteAllInBonus()
        {
            int currentChips = gameManager.PlayerStats.ownedChips;
            if (currentChips <= 0)
                return false;

            gameManager.PlayerStats.AddBet(currentChips);
            blackjackController.SetBonusMultiplier(10);
            return true;
        }
        #endregion
    }
}