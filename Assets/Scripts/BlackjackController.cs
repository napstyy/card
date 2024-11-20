using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CardGame
{
    public class BlackjackController : MonoBehaviour
    {
        public enum RoundState
        {
            Start,
            End
        }

        public bool isDoubleDown;
        public bool isSplited;
        public List<Hands> playerHands;
        public Hands dealerHands;
        public Hands selectedHands;

        RoundState roundState;

        List<Card> deck;
        List<Card> removedCards;

        SimpleUIController simpleUIController;

        [Header("Audio Clips")]
        [SerializeField] public AudioClip carddrop;

        // Start is called before the first frame update
        void Start()
        {
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
            deck = InitializeDeck(4);
            // playerHands[0].AddCardToHands(DrawCard());
            dealerHands.AddCardToHands(DrawCard());
            // playerHands[0].AddCardToHands(DrawCard());
            dealerHands.AddCardToHands(DrawCard());

            // Debug Usage
            playerHands[0].AddCardToHands(new Card(Card.Ranks.Ace, Card.Suits.Spades));
            playerHands[0].AddCardToHands(new Card(Card.Ranks.Ace, Card.Suits.Spades));

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
            int rnd = Random.Range(0, deck.Count);
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

        public void Stand()
        {
            if(roundState == RoundState.End)return;
            foreach(Hands hands in playerHands)
            {
                hands.ResetSelectedCard();
            }
            int dealerPoints = CountPoints(dealerHands.cards);
            dealerHands.ShowHands();
            while (dealerPoints < 17)
            {
                dealerHands.AddCardToHands(DrawCard());
                dealerPoints = CountPoints(dealerHands.cards);
            }
            foreach(Hands hands in playerHands)
            {
                int playerPoints = CountPoints(hands.cards);

                if(playerPoints <= 21)
                {
                    if(dealerPoints > 21 || playerPoints > dealerPoints)
                    {
                        FindAnyObjectByType<Test>().UpdateChips(FindAnyObjectByType<Test>().ownedChips + hands.chips * 2);
                    }
                    else if(playerPoints == dealerPoints)
                    {
                        FindAnyObjectByType<Test>().UpdateChips(FindAnyObjectByType<Test>().ownedChips + hands.chips);
                    }
                }
                hands.chips = 0;
                string gameResult = dealerPoints > 21 ? "Player Win!" : playerPoints < dealerPoints || playerPoints > 21? "Dealer Win!" : playerPoints > dealerPoints ? "Player Win!" : "Tie";
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
            }
            foreach(Hands hands in playerHands)
            {
                hands.ResetSelectedCard(false);
            }
            AudioManager.Instance.PlaySFX(carddrop);
        }

        public void Hit(Hands hands)
        {
            if (CountPoints(hands?.cards) > 21 || roundState == RoundState.End) return;
            hands.AddCardToHands(DrawCard());
            int playerPoints = CountPoints(hands.cards);
            if (playerPoints > 21) {
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
            int playerPoints = CountPoints(playerHands[0].cards);
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

        public int CountPoints(List<Card> hands)
        {
            int points = 0;
            int aceNumber = 0;
            foreach (Card card in hands)
            {
                points += GetCardPoint(card);
                aceNumber += card.rank == Card.Ranks.Ace ? 1 : 0;
            }

            while (points > 21 && aceNumber > 0)
            {
                aceNumber--;
                points -= 10;
            }

            return points;
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