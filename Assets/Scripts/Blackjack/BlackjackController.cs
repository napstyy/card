using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CardGame
{
    #region Enums and Structs

    public enum RuleEffect
    {
        HoldEffect,
        DrawEffect,
        DiscardEffect
    }

    [Serializable]
    public struct SecretRule
    {
        public const int HoldEffectNum = 7;
        public const int DrawEffectNum = 3;
        public const int DiscardEffectNum = 4;

        public int id;
        public RuleEffect effectType;
        public Card.Ranks? triggeredRank;
        public Card.Suits? triggeredSuit;
    }

    #endregion

    public class BlackjackController : MonoBehaviour
    {
        #region Singleton Implementation

        public static BlackjackController Instance { get; private set; }

        private void Awake()
        {
            // Implement singleton pattern
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Enums

        public enum RoundState
        {
            Start,
            End
        }

        #endregion

        #region Event
            public event Action<RoundState> RoundStateChanged;
        #endregion

        #region Public Variables

        [Header("Game Objects")]
        public GameObject playerHandsPrefab;

        [Header("Game State")]
        public bool isDoubleDown;
        public bool isSplit;
        public List<Hands> playerHands;
        public Hands dealerHands;
        public Hands selectedHands;
        public RoundState roundState{get; private set;}
        public bool AllowSplit{get{return playerHands[0].IsPair() && !isSplit && !isDoubleDown;}}

        [Header("Secret Rules")]
        public SecretRule[] rules;

        [Header("Audio Clips")]
        public AudioClip cardDrop;

        #endregion

        #region Private Variables

        private Deck deck;
        private Player player;
        private int bustLimit = 21;
        private bool reducePointsByHalf;

        #endregion

        #region Unity Methods

        private void Start()
        {
            rules = new SecretRule[4];
            roundState = RoundState.End;
            deck = new Deck(4);
            player = FindAnyObjectByType<Player>();
            RoundStateChanged?.Invoke(roundState);
        }

        #endregion

        #region Initialization Methods
        public void StartOfRound()
        {
            if (playerHands.Count == 0) return;

            isSplit = false;
            isDoubleDown = false;

            playerHands[0].InitializeHands();
            dealerHands.InitializeHands();

            if (deck?.Count < 104)
            {
                deck.Initialize();
            }

            DealInitialCards();
            roundState = RoundState.Start;
            RoundStateChanged?.Invoke(roundState);
        }

        private void DealInitialCards()
        {
            // Deal two cards to player and dealer
            playerHands[0].AddCardToHands(deck.DrawCard());
            dealerHands.AddCardToHands(deck.DrawCard());
            playerHands[0].AddCardToHands(deck.DrawCard());
            dealerHands.AddCardToHands(deck.DrawCard());
        }

        #endregion

        #region Card Drawing Methods

        #endregion

        #region Game Logic Methods

        private int GetCardPoint(Card card)
        {
            return card.rank switch
            {
                Card.Ranks.Two or Card.Ranks.Three or Card.Ranks.Four or
                Card.Ranks.Five or Card.Ranks.Six or Card.Ranks.Seven or
                Card.Ranks.Eight or Card.Ranks.Nine or Card.Ranks.Ten => (int)card.rank,

                Card.Ranks.Jack or Card.Ranks.Queen or Card.Ranks.King => 10,

                Card.Ranks.Ace => 11,

                _ => throw new Exception("Unknown card rank!")
            };
        }

        private void TriggerHoldEffect(Hands hands, out float bonus)
        {
            bonus = 1;
            hands.ResetExtraPoints();

            foreach (var secretRule in rules)
            {
                if (secretRule.effectType != RuleEffect.HoldEffect) continue;

                foreach (var card in hands.cards)
                {
                    if (!card.isSecretCard) continue;

                    switch (secretRule.id)
                    {
                        case 0:
                            bustLimit++;
                            break;
                        case 1:
                            AddRightSideCardPoints(hands, card);
                            break;
                        case 2:
                            if (isDoubleDown) bonus *= 2;
                            break;
                        case 3:
                            AdjustPointsBySuit(hands, card, true);
                            break;
                        case 4:
                            AdjustPointsBySuit(hands, card, false);
                            break;
                        case 5:
                            bonus *= CountPoints(hands) > 17 ? 2 : 0.5f;
                            break;
                        case 6:
                            bonus *= CountPoints(hands) < 17 ? 2 : 0.5f;
                            break;
                    }
                }
            }
        }

        private void AddRightSideCardPoints(Hands hands, Card card)
        {
            int index = hands.cards.IndexOf(card);
            if (index + 1 < hands.cards.Count)
            {
                var rightSideCard = hands.cards[index + 1];
                hands.AddExtraPoints(GetCardPoint(rightSideCard));
            }
        }

        private void AdjustPointsBySuit(Hands hands, Card card, bool add)
        {
            int count = hands.cards.Count(c => c.suit == card.suit);
            if (add)
                hands.AddExtraPoints(count);
            else
                hands.ReduceExtraPoints(count);
        }

        private void TriggerDrawEffect(Card card, Hands hands)
        {
            foreach (var secretRule in rules)
            {
                if (secretRule.effectType != RuleEffect.DrawEffect) continue;

                switch (secretRule.id)
                {
                    case 0:
                        dealerHands.ShowHands();
                        break;
                    case 1:
                        Hit(hands);
                        break;
                    case 2:
                        SwapCardWithDealer(hands, card);
                        break;
                }
            }
        }

        private void SwapCardWithDealer(Hands hands, Card card)
        {
            dealerHands.ReplaceCard(card, out var replacedCard);
            hands.ReplaceCard(replacedCard, out _);
        }

        private void TriggerDiscardEffect(Card card, Hands hands)
        {
            foreach (var secretRule in rules)
            {
                switch (secretRule.id)
                {
                    case 0:
                        player.AddChips(hands.chips / 2);
                        break;
                    case 1:
                        bustLimit += 2;
                        break;
                    case 2:
                        reducePointsByHalf = true;
                        break;
                    case 3:
                        dealerHands.AddCardToHands(card);
                        break;
                }
            }
        }

        public void Stand()
        {
            if (roundState == RoundState.End) return;

            foreach (var hands in playerHands)
            {
                hands.ResetSelectedCard();
            }

            dealerHands.ShowHands();
            DealerPlay();

            foreach (var hands in playerHands)
            {
                ResolveHands(hands);
            }
            
            player.totalBets = 0;
            roundState = RoundState.End;
            RoundStateChanged?.Invoke(roundState);
        }

        private void DealerPlay()
        {
            int dealerPoints = CountPoints(dealerHands);

            while (dealerPoints < 17)
            {
                dealerHands.AddCardToHands(deck.DrawCard());
                dealerPoints = CountPoints(dealerHands);
            }
        }

        private void ResolveHands(Hands hands)
        {
            TriggerHoldEffect(hands, out float bonus);
            int playerPoints = CountPoints(hands);
            int dealerPoints = CountPoints(dealerHands);

            if (playerPoints <= 21)
            {
                if (dealerPoints > 21 || playerPoints > dealerPoints)
                {
                    int multiplier = (playerPoints == 21 && hands.cards.Count == 2) ? 3 : 2;
                    player.AddChips(hands.chips * multiplier * (int)bonus);
                }
                else if (playerPoints == dealerPoints && dealerHands.cards.Count < 5)
                {
                    player.AddChips(hands.chips);
                }
            }

            hands.chips = 0;
            DisplayGameResult(playerPoints, dealerPoints);
        }

        private void DisplayGameResult(int playerPoints, int dealerPoints)
        {
            string gameResult = playerPoints > 21 ? "Dealer Wins!"
                : dealerPoints > 21 ? "Player Wins!"
                : playerPoints > dealerPoints ? "Player Wins!"
                : playerPoints < dealerPoints ? "Dealer Wins!"
                : "Tie";

            Debug.Log($"{gameResult} Dealer: {dealerPoints} | Player: {playerPoints}");
        }

        public void Replace()
        {
            if (roundState == RoundState.End) return;

            Card newCard = deck.SpecialDrawCard();
            if (selectedHands?.ReplaceCard(newCard, out Card replacedCard) ?? false)
            {
                TriggerDrawEffect(newCard, selectedHands);
                deck.AddRemovedCard(replacedCard);
                TriggerDiscardEffect(replacedCard, selectedHands);
            }

            foreach (var hands in playerHands)
            {
                hands.ResetSelectedCard(false);
            }

            AudioManager.Instance.PlaySFX(cardDrop);
        }

        public void Hit(Hands hands)
        {
            if (CountPoints(hands) > 21 || roundState == RoundState.End) return;

            Card newCard = deck.DrawCard();
            TriggerDrawEffect(newCard, hands);
            hands.AddCardToHands(newCard);

            if (playerHands.All(h => CountPoints(h) > 21))
            {
                Debug.Log("Player Busted");
                Stand();
            }
        }

        public void Bet(int chips)
        {
            if (chips > player.ownedChips) return;

            playerHands[0].chips += chips;
            player.AddBets(chips);
            player.RemoveChips(chips);
        }

        public void DoubleDown()
        {
            if (player.ownedChips < playerHands[0].chips || roundState == RoundState.End || isSplit) return;

            isDoubleDown = true;
            playerHands[0].ResetSelectedCard();
            player.RemoveChips(playerHands[0].chips);
            player.AddBets(playerHands[0].chips);
            playerHands[0].chips *= 2;
            playerHands[0].AddCardToHands(deck.DrawCard());

            if (CountPoints(playerHands[0]) > 21)
            {
                Debug.Log("Player Busted");
            }

            Stand();
        }

        public void Split()
        {
            if (player.ownedChips < playerHands[0].chips || roundState == RoundState.End || !playerHands[0].IsPair() || isSplit) return;

            isSplit = true;
            Hands newHands = playerHands[0].Split();
            playerHands.Add(newHands);
            player.RemoveChips(playerHands[0].chips);
            player.AddBets(playerHands[0].chips);

            foreach (var hands in playerHands)
            {
                hands.AddCardToHands(deck.DrawCard());
            }
        }

        public int CountPoints(Hands hands)
        {
            if (hands == null) return 0;

            int points = hands.cards.Sum(card => GetCardPoint(card));
            int aceCount = hands.cards.Count(card => card.rank == Card.Ranks.Ace);

            while (points > 21 && aceCount > 0)
            {
                points -= 10;
                aceCount--;
            }

            return points + hands.extraPoints;
        }

        #endregion

        #region Utility Methods

        public void GetRandomRules()
        {
            rules[0] = new SecretRule
            {
                effectType = RuleEffect.HoldEffect,
                id = UnityEngine.Random.Range(0, SecretRule.HoldEffectNum)
            };
            rules[1] = new SecretRule
            {
                effectType = RuleEffect.DiscardEffect,
                id = UnityEngine.Random.Range(0, SecretRule.DiscardEffectNum)
            };
            rules[2] = new SecretRule
            {
                effectType = RuleEffect.DrawEffect,
                id = UnityEngine.Random.Range(0, SecretRule.DrawEffectNum)
            };

            RuleEffect randomEffect = (RuleEffect)UnityEngine.Random.Range(0, 3);
            rules[3] = new SecretRule
            {
                effectType = randomEffect,
                id = UnityEngine.Random.Range(0,
                    randomEffect == RuleEffect.HoldEffect ? SecretRule.HoldEffectNum :
                    randomEffect == RuleEffect.DiscardEffect ? SecretRule.DiscardEffectNum :
                    SecretRule.DrawEffectNum),
                triggeredRank = (Card.Ranks)UnityEngine.Random.Range((int)Card.Ranks.Two, (int)Card.Ranks.Ace + 1)
            };
        }

        public void Restart()
        {
            while (playerHands.Count > 1)
            {
                Hands hands = playerHands.Last();
                playerHands.Remove(hands);
                Destroy(hands.gameObject);
            }

            playerHands[0].InitializeHands();
            dealerHands.InitializeHands();
        }

        #endregion
    }
}