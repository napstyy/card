using System;
using System.Collections.Generic;
using System.Linq;

namespace CardGame
{
    [Serializable]
    public class Deck
    {
        private List<Card> remainCards;
        private List<Card> removedCards;

        public Deck(int numOfDecks)
        {
            remainCards = new List<Card>();
            removedCards = new List<Card>();
            while (numOfDecks-- > 0)
            {
                foreach (Card.Ranks rank in Enum.GetValues(typeof(Card.Ranks)))
                {
                    foreach (Card.Suits suit in Enum.GetValues(typeof(Card.Suits)))
                    {
                        remainCards.Add(new Card(rank, suit, numOfDecks == 0));
                    }
                }
            }
        }

        private Queue<Card> previewedCards = new Queue<Card>();

        // Add these methods to your Deck class
        public Card[] PeekNextCards(int count)
        {
            if (count <= 0 || count > remainCards.Count)
                return null;

            // Clear any previously peeked cards
            previewedCards.Clear();

            Card[] peekedCards = new Card[count];
            List<Card> tempCards = new List<Card>(remainCards);

            for (int i = 0; i < count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, tempCards.Count);
                peekedCards[i] = tempCards[randomIndex];
                previewedCards.Enqueue(tempCards[randomIndex]); // Store for later drawing
                tempCards.RemoveAt(randomIndex);
            }

            return peekedCards;
        }

        // Optional: Add a method to peek at a specific position
        public Card PeekCard(int position)
        {
            if (position < 0 || position >= remainCards.Count)
                return null;

            return remainCards[position];
        }

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

        public Card DrawCard()
        {
            if (previewedCards.Count > 0)
            {
                Card previewedCard = previewedCards.Dequeue();
                remainCards.Remove(previewedCard);
                removedCards.Add(previewedCard);
                return previewedCard;
            }

            int randomIndex = UnityEngine.Random.Range(0, remainCards.Count);
            if(removedCards.Count == 0)
            {
                Initialize();
            }
            Card drawnCard = remainCards[randomIndex];
            remainCards.RemoveAt(randomIndex);
            removedCards.Add(drawnCard);
            return drawnCard;
        }

        public Card SpecialDrawCard()
        {
            // Filter cards with point >= 5
            var eligibleCards = remainCards.Where(card => GetCardPoint(card) >= 5).ToList();
            var specialCards = eligibleCards.Where(card => card.isSecretCard).ToList();
            var normalCards = eligibleCards.Except(specialCards).ToList();

            Card chosenCard;
            if (specialCards.Count > 0 && UnityEngine.Random.Range(0, 100) < 70)
            {
                chosenCard = specialCards[UnityEngine.Random.Range(0, specialCards.Count)];
            }
            else if (normalCards.Count > 0)
            {
                chosenCard = normalCards[UnityEngine.Random.Range(0, normalCards.Count)];
            }
            else
            {
                chosenCard = remainCards[UnityEngine.Random.Range(0, remainCards.Count)];
            }

            remainCards.Remove(chosenCard);
            removedCards.Add(chosenCard);
            return chosenCard;
        }

        public void AddRemovedCard(Card card) => removedCards.Add(card);
        public void Initialize()
        {
            remainCards.AddRange(removedCards);
            removedCards.Clear();
        }

        public int Count { get { return remainCards.Count; } }
    }
}