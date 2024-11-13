using System.Collections.Generic;
using UnityEngine;

public class BlackjackController : MonoBehaviour
{
    public enum Position
    {
        Player,
        Dealer
    }

    public int chips;
    public bool isDoubleDown;
    public GameObject cardPrefab;
    public Transform playerHandsPivot;
    public Transform dealerHandsPivot;

    List<Card> deck;
    List<Card> removedCards;

    List<Card> playerHands;
    List<Card> dealerHands;

    // Start is called before the first frame update
    void Start()
    {
        removedCards = new List<Card>();
        playerHands = new List<Card>();
        dealerHands = new List<Card>();
        deck = InitializeDeck();
        StartOfRound();
    }

    void StartOfRound()
    {
        playerHands.Clear();
        dealerHands.Clear();
        playerHands.Add(DrawCard(Position.Player));
        dealerHands.Add(DrawCard(Position.Dealer));
        playerHands.Add(DrawCard(Position.Player));
        dealerHands.Add(DrawCard(Position.Dealer));
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

    Card DrawCard(Position position)
    {
        int rnd = Random.Range(0, deck.Count);
        Card card = deck[rnd];
        deck.Remove(card);
        removedCards.Add(card);
        int handsCount = position == Position.Dealer? dealerHands.Count:playerHands.Count;
        Transform targetPivot = position == Position.Dealer? dealerHandsPivot : playerHandsPivot;
        GameObject cardObject = Instantiate(cardPrefab, targetPivot);
        cardObject.transform.position += new Vector3(1,0,0) * handsCount;
        targetPivot.position -= new Vector3(0.5f,0,0);
        cardObject.GetComponent<DisplayCard>().Instantiate(card);
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
        int dealerPoints = CountPoints(dealerHands);
        while(dealerPoints < 17)
        {
            dealerHands.Add(DrawCard(Position.Dealer));
            dealerPoints = CountPoints(dealerHands);
        }
    }

    public void Replace(int index)
    {
        if(index > 0 && index < playerHands.Count)
        {
            removedCards.Add(playerHands[index]);
            playerHands[index] = DrawCard(Position.Player);
        }
    }

    public void Hit()
    {
        if(CountPoints(playerHands) > 21) return;
        playerHands.Add(DrawCard(Position.Player));
    }

    public void Bet(int chips)
    {
        this.chips += chips;
    }

    public void DoubleDown()
    {
        this.chips *= 2;
        isDoubleDown = true;
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
}
