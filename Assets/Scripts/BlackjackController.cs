using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BlackjackController : MonoBehaviour
{
    public int chips;
    public bool isDoubleDown;
    public GameObject cardPrefab;
    public TextMeshProUGUI playerPointsText;
    public Hands playerHands;
    public Hands dealerHands;
    public Hands selectedHands;

    List<Card> deck;
    List<Card> removedCards;

    // Start is called before the first frame update
    void Start()
    {
        removedCards = new List<Card>();
        StartOfRound();
    }

    void StartOfRound()
    {
        playerHands.InitializeHands();
        dealerHands.InitializeHands();
        deck = InitializeDeck();
        playerHands.AddCardToHands(DrawCard());
        dealerHands.AddCardToHands(DrawCard());
        playerHands.AddCardToHands(DrawCard());
        dealerHands.AddCardToHands(DrawCard());

        int playerPoints = CountPoints(playerHands.cards);
        playerPointsText.SetText(playerPoints.ToString());
    }

    List<Card> InitializeDeck(int numberOfDecks = 1)
    {
        List<Card> deck = new List<Card>();
        while(numberOfDecks-- > 0){
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
        int dealerPoints = CountPoints(dealerHands.cards);
        dealerHands.ShowHands();
        while(dealerPoints < 17)
        {
            dealerHands.AddCardToHands(DrawCard());
            dealerPoints = CountPoints(dealerHands.cards);
        }
        int playerPoints = CountPoints(playerHands.cards);
        string gameResult = playerPoints < dealerPoints? "Dealer Win!":playerPoints > dealerPoints? "Player Win!":"Tie";
        Debug.Log(gameResult+$" Dealer: {dealerPoints} | Player: {playerPoints}");
    }

    public void Replace()
    {
        List<Card> cards = selectedHands.cards;
        if(selectedHands.ReplaceCard(DrawCard(), out Card replacedCard))
        {
            removedCards.Add(replacedCard);
            int playerPoints = CountPoints(cards);
            playerPointsText.SetText(playerPoints.ToString());
        }
    }

    public void Hit()
    {
        if(CountPoints(playerHands.cards) > 21) return;
        playerHands.AddCardToHands(DrawCard());
        int playerPoints = CountPoints(playerHands.cards);
        playerPointsText.SetText(playerPoints.ToString());
        if(playerPoints > 21) Debug.Log("Player Busted");
    }

    public void Bet(int chips)
    {
        this.chips += chips;
    }

    public void DoubleDown()
    {
        this.chips *= 2;
        isDoubleDown = true;
        playerHands.AddCardToHands(DrawCard());
        int playerPoints = CountPoints(playerHands.cards);
        playerPointsText.SetText(playerPoints.ToString());
        if(playerPoints > 21) Debug.Log("Player Busted");
        Stand();
    }

    public int CountPoints(List<Card> hands)
    {
        int points = 0;
        int aceNumber = 0;
        foreach(Card card in hands)
        {
            points += GetCardPoint(card);
            aceNumber += card.rank == Card.Ranks.Ace? 1:0; 
        }

        while(points > 21 && aceNumber > 0)
        {
            aceNumber--;
            points -= 10;
        }

        return points;
    }

    public void Restart()
    {
        playerHands.InitializeHands();
        dealerHands.InitializeHands();
        StartOfRound();
    }
}
