using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CardGame
{

    public enum RuleCategory
    {
        SuitsType,
        RanksType
    }

    [System.Serializable]
    public struct SecretRule
    {
        public int id;
        public RuleCategory category;
        public Card.Ranks triggeredRank;
        public Card.Suits triggeredSuit; 
    }

    public class BlackjackController : MonoBehaviour
    {
        public static BlackjackController Instance { get; private set; }

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public enum RoundState
        {
            Start,
            End
        }
        public GameObject playerHandsPrefab;
        public bool isDoubleDown;
        public bool isSplited;
        public List<Hands> playerHands;
        public Hands dealerHands;
        public Hands selectedHands;

        [Header("Secret Rules")]
        public SecretRule[] rules;

        RoundState roundState;

        List<Card> deck;
        List<Card> removedCards;

        SimpleUIController simpleUIController;

        [Header("Audio Clips")]
        [SerializeField] public AudioClip carddrop;

        // Start is called before the first frame update
        void Start()
        {
            rules = new SecretRule[4];
            removedCards = new List<Card>();
            simpleUIController = FindAnyObjectByType<SimpleUIController>();
            simpleUIController.ShowBetsButtons();
            simpleUIController.HideGameButtons();
            simpleUIController.HidePlayerPointsText();
            simpleUIController.DisableStartButton();
            roundState = RoundState.End;
        }

        public void StartOfRound()
        {
            if(playerHands.Count == 0) return;
            isSplited = false;
            isDoubleDown = false;
            playerHands[0].InitializeHands();
            dealerHands.InitializeHands();
            if(deck == null || deck.Count < 104)deck = InitializeDeck(4);
            playerHands[0].AddCardToHands(DrawCard());
            dealerHands.AddCardToHands(DrawCard());
            playerHands[0].AddCardToHands(DrawCard());
            dealerHands.AddCardToHands(DrawCard());

            simpleUIController.HideBetsButtons();
            simpleUIController.ShowGameButtons();
            simpleUIController.ShowPlayerPointsText();
            roundState = RoundState.Start;
        }

        List<Card> InitializeDeck(int numberOfDecks = 1)
        {
            List<Card> deck = new List<Card>();
            while (numberOfDecks-- > 0)
            {
                for (int i = 2; i < 15; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        deck.Add(new Card((Card.Ranks)i, (Card.Suits)j));
                    }
                }
            }

            return deck;
        }

        Card DrawCard()
        {
            int rnd = UnityEngine.Random.Range(0, deck.Count);
            Card card = deck[rnd];
            deck.Remove(card);
            removedCards.Add(card);
            return card;
        }

        int GetCardPoint(Card card)
        {
            int point;
            switch (card.rank)
            {
                case Card.Ranks.Two:
                case Card.Ranks.Three:
                case Card.Ranks.Four:
                case Card.Ranks.Five:
                case Card.Ranks.Six:
                case Card.Ranks.Seven:
                case Card.Ranks.Eight:
                case Card.Ranks.Nine:
                case Card.Ranks.Ten:
                    point = (int)card.rank; // The card value corresponds to its rank (2-10)
                    break;

                case Card.Ranks.Jack:
                case Card.Ranks.Queen:
                case Card.Ranks.King:
                    point = 10; // Face cards (Jack, Queen, King) are worth 10 points
                    break;

                case Card.Ranks.Ace:
                    point = 11; // Choose 11 initially for Ace. Adjust later if necessary based on the hand's total score.
                    break;

                default:
                    throw new System.Exception("Unknown card rank!");
            }

            return point;
        }

        #region Rules' Effect

        void IncrementPointsForSameSuit(Card referenceCard, Hands hands)
        {
            foreach (var card in hands.cards)
            {
                if (card.suit == referenceCard.suit)
                {
                    hands.AddExtraPoints(1);
                }
            }
        }

        void DecrementPointsForSameSuit(Card referenceCard, Hands hands)
        {
            foreach (var card in hands.cards)
            {
                if (card.suit == referenceCard.suit)
                {
                    hands.ReduceExtraPoints(1);
                }
            }
        }
        void AddExtraPoints(int extraPoints, Hands hands)
        {
            hands.AddExtraPoints(extraPoints);
        }

        void KeepDrawingUntilDifferentSuit(Hands hands)
        {
            while (deck.Count > 0)
            {
                Card lastCard = hands.cards[hands.cards.Count-1];
                Card card = DrawCard();
                hands.AddCardToHands(card);
                if(lastCard.suit != card.suit)
                {
                    break;
                }
            }
        }

        void RemoveCardsGreaterThan(int value, Hands hands)
        {
            for(int i = hands.cards.Count-1; i >= 0; i--)
            {
                if((int)hands.cards[i].rank < value)
                {
                    hands.DropCard(i);
                }
            }
        }

        void RemoveCardsSmallerThan(int value, Hands hands)
        {
            for(int i = hands.cards.Count-1; i >= 0; i--)
            {
                if((int)hands.cards[i].rank > value)
                {
                    hands.DropCard(i);
                }
            }
        }

        //射龍門
        void GoalCheck(Hands hands)
        {
            if (hands.cards.Count == 3)
            {
                int middleRankIndex = (int)hands.cards[1].rank;
                int leftRankIndex = (int)hands.cards[0].rank;
                int rightRankIndex = (int)hands.cards[2].rank;

                if(middleRankIndex == leftRankIndex || middleRankIndex == rightRankIndex)
                {
                    Stand();
                }
                else if(middleRankIndex > leftRankIndex || middleRankIndex < rightRankIndex)
                {
                    FindAnyObjectByType<Test>().UpdateChips(FindAnyObjectByType<Test>().ownedChips + hands.chips);
                }
                else
                {
                    hands.DropCard(1);
                }
            }
        }

        #endregion

        void TriggerSecretRules(Card card, Hands hands)
        {
            for(int i = 0; i < rules.Length; i++)
            {
                SecretRule rule = rules[i];
                if(rule.category == RuleCategory.SuitsType && card.suit == rule.triggeredSuit)
                {
                    switch(rule.id)
                    {
                        case 0:
                            IncrementPointsForSameSuit(card, hands);
                        break;
                        case 1:
                            DecrementPointsForSameSuit(card, hands);
                        break;
                        case 2:
                            KeepDrawingUntilDifferentSuit(hands);
                        break;
                        case 3:
                            GoalCheck(hands);
                        break;
                        case 4:
                            Card tempCard = DrawCard();
                            if(GetCardPoint(tempCard) > GetCardPoint(card)){
                                hands.AddCardToHands(tempCard);
                            }
                            else
                            {
                                dealerHands.AddCardToHands(tempCard);
                            }
                        break;
                        case 5:
                            AddExtraPoints(GetCardPoint(card)%2 == 0?-1:1,hands);
                        break;
                    }
                }
                else if(rule.category == RuleCategory.RanksType && card.rank == rule.triggeredRank)
                {
                    switch(rule.id)
                    {
                        case 0:
                            AddExtraPoints(1,hands);
                        break;
                        case 2:
                            RemoveCardsGreaterThan(GetCardPoint(card),hands);
                        break;
                        case 3:
                            RemoveCardsSmallerThan(GetCardPoint(card),hands);
                        break;
                        case 5:
                            List<Card> tempCard = dealerHands.cards;
                            dealerHands.SetHands(hands.cards);
                            hands.SetHands(tempCard);
                        break;
                        case 6:
                            Card copiedCard = new Card(card.rank, card.suit);
                            dealerHands.AddCardToHands(copiedCard);
                        break;
                    }
                }
            }
        }

        public void Stand()
        {
            if(roundState == RoundState.End)return;
            foreach(Hands hands in playerHands)
            {
                hands.ResetSelectedCard();
            }
            int dealerPoints = CountPoints(dealerHands);
            dealerHands.ShowHands();
            while (dealerPoints < 17)
            {
                dealerHands.AddCardToHands(DrawCard());
                dealerPoints = CountPoints(dealerHands);
            }
            foreach(Hands hands in playerHands)
            {
                int playerPoints = CountPoints(hands);

                if(playerPoints <= 21)
                {
                    if(dealerPoints > 21 || playerPoints > dealerPoints)
                    {
                        FindAnyObjectByType<Test>().UpdateChips(FindAnyObjectByType<Test>().ownedChips + hands.chips * (playerPoints == 21? 3:2));
                    }
                    else if(playerPoints == dealerPoints && dealerHands.cards.Count < 5)
                    {
                        FindAnyObjectByType<Test>().UpdateChips(FindAnyObjectByType<Test>().ownedChips + hands.chips);
                    }
                }
                hands.chips = 0;
                string gameResult = playerPoints > 21 ? "Dealer Win!" 
                    : dealerPoints > 21 ? "Player Win!" 
                    : playerPoints > dealerPoints ? "Player Win!" 
                    : playerPoints < dealerPoints ? "Dealer Win!" 
                    : "Tie";
                Debug.Log(gameResult + $" Dealer: {dealerPoints} | Player: {playerPoints}");   
            }
            roundState = RoundState.End;
        }

        public void Replace()
        {
            if(roundState == RoundState.End)return;
            if (selectedHands?.ReplaceCard(DrawCard(), out Card replacedCard) ?? false)
            {
                removedCards.Add(replacedCard);
                TriggerSecretRules(replacedCard,selectedHands);
            }
            foreach(Hands hands in playerHands)
            {
                hands.ResetSelectedCard(false);
            }
            AudioManager.Instance.PlaySFX(carddrop);
        }

        public void Hit(Hands hands)
        {
            if (CountPoints(hands) > 21 || roundState == RoundState.End) return;
            Card card = DrawCard();
            hands.AddCardToHands(card);
            TriggerSecretRules(card, hands);
            bool allBusted = playerHands.All(tempHands => CountPoints(tempHands) > 21);
            if (allBusted) {
                Debug.Log("Player Busted");
                Stand();
            }
        }

        public void Bet(int chips)
        {
            if(chips > FindAnyObjectByType<Test>().ownedChips)return;
            playerHands[0].chips += chips;
            FindAnyObjectByType<Test>().UpdateChips(FindAnyObjectByType<Test>().ownedChips - chips);
            simpleUIController.UpdateBetText(playerHands[0].chips);
            if (playerHands[0].chips > 0 && roundState == RoundState.End) simpleUIController.EnableStartButton();
        }

        public void DoubleDown()
        {
            if(FindAnyObjectByType<Test>().ownedChips < playerHands[0].chips || roundState == RoundState.End || isSplited)return;
            isDoubleDown = true;
            playerHands[0].ResetSelectedCard();
            FindAnyObjectByType<Test>().UpdateChips(FindAnyObjectByType<Test>().ownedChips - playerHands[0].chips);
            playerHands[0].chips *= 2;
            playerHands[0].AddCardToHands(DrawCard());
            int playerPoints = CountPoints(playerHands[0]);
            if (playerPoints > 21) Debug.Log("Player Busted");
            Stand();
        }

        public void Split()
        {
            if(FindAnyObjectByType<Test>().ownedChips < playerHands[0].chips || roundState == RoundState.End || !playerHands[0].IsPair() || isSplited)return;
            isSplited = true;
            playerHands.Add(playerHands[0].Split());
            FindAnyObjectByType<Test>().UpdateChips(FindAnyObjectByType<Test>().ownedChips - playerHands[0].chips);
            foreach(Hands hands in playerHands)
            {
                hands.AddCardToHands(DrawCard());
            }
        }

        public int CountPoints(Hands hands)
        {
            if(hands == null) return 999;
            int points = 0;
            int aceNumber = 0;
            foreach (Card card in hands.cards)
            {
                points += GetCardPoint(card);
                aceNumber += card.rank == Card.Ranks.Ace ? 1 : 0;
            }

            while (points > 21 && aceNumber > 0)
            {
                aceNumber--;
                points -= 10;
            }

            return points + hands.extraPoints;
        }

        public void GetRandomRules()
        {
            rules[0] = new SecretRule{
                id = UnityEngine.Random.Range(0,6),
                category = RuleCategory.SuitsType,
                triggeredSuit = Card.Suits.Spades
            };
            rules[1] = new SecretRule{
                id = UnityEngine.Random.Range(0,6),
                category = RuleCategory.SuitsType,
                triggeredSuit = Card.Suits.Hearts
            };
            rules[2] = new SecretRule{
                id = UnityEngine.Random.Range(0,6),
                category = RuleCategory.SuitsType,
                triggeredSuit = Card.Suits.Diamonds
            };
            rules[3] = new SecretRule{
                id = UnityEngine.Random.Range(0,6),
                category = RuleCategory.SuitsType,
                triggeredSuit = Card.Suits.Clubs
            };
            // rules[1] = new SecretRule{
            //     id = UnityEngine.Random.Range(0,6),
            //     category = RuleCategory.RanksType,
            //     triggeredRank = (Card.Ranks)UnityEngine.Random.Range(2,15)
            // };
        }

        public void Restart()
        {
            while(playerHands.Count > 1)
            {
                Hands hands = playerHands[playerHands.Count-1];
                playerHands.Remove(hands);
                Destroy(hands.gameObject);
            }
            playerHands[0].InitializeHands();
            dealerHands.InitializeHands();
            simpleUIController.ShowBetsButtons();
            simpleUIController.HideGameButtons();
            simpleUIController.HidePlayerPointsText();
            simpleUIController.DisableStartButton();
        }
    }
}