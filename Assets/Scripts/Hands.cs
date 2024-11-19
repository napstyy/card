using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace CardGame
{
    public class Hands : MonoBehaviour
    {
        public enum Role
        {
            Dealer,
            Player
        }

        public List<Card> cards { get; private set; }
        public Role role;
        public float spacing = 1;
        int selectedCardIndex;
        bool hideFirstCard;
        BlackjackController blackjackController;

        void Start()
        {
            blackjackController = FindAnyObjectByType<BlackjackController>();
        }

        public void InitializeHands()
        {
            while (transform.childCount > 0)
            {
                var child = transform.GetChild(transform.childCount - 1);
                ObjectPool.instance.ReturnObject(CardSpriteReference.Instance.cardPrefab, child.gameObject);
            }
            hideFirstCard = role == Role.Dealer;
            selectedCardIndex = -1;
            cards?.Clear();
            cards = new List<Card>();
        }

        public void AddCardToHands(Card card)
        {
            GameObject cardObject = ObjectPool.instance.GetObject(CardSpriteReference.Instance.cardPrefab, transform);
            DisplayCard displayCard = cardObject.GetComponent<DisplayCard>();
            if (role == Role.Player)
            {
                int index = cards.Count;
                displayCard.OnCardClicked += () =>
                {
                    // blackjackController.selectedHands = this;
                    // selectedCardIndex = index == selectedCardIndex ? -1 : index;
                    blackjackController.selectedHands = this;
                    if (selectedCardIndex == index)
                    {
                        // Deselect the card
                        selectedCardIndex = -1;
                        cardObject.transform.localPosition -= new Vector3(0, 0.5f, 0); // Move back to original position
                    }
                    else
                    {
                        // Move previously selected card back to original position
                        if (selectedCardIndex != -1)
                        {
                            Transform previousCard = transform.GetChild(selectedCardIndex);
                            previousCard.localPosition -= new Vector3(0, 0.5f, 0);
                        }

                        // Select the new card and move it up
                        selectedCardIndex = index;
                        cardObject.transform.localPosition += new Vector3(0, 0.5f, 0); // Move up
                    }
                };
            }
            displayCard.Instantiate(card);
            cards.Add(card);
            UpdateHands();
        }

        public void ShowHands()
        {
            hideFirstCard = false;
            UpdateHands();
        }

        public bool ReplaceCard(Card card, out Card replacedCard)
        {
            replacedCard = null;
            Debug.Log("selectedCardIndex: " + selectedCardIndex);
            if (selectedCardIndex > -1 && selectedCardIndex < cards.Count)
            {
                Debug.Log("Replacing card");
                replacedCard = cards[selectedCardIndex];
                cards[selectedCardIndex] = card;
                transform.GetChild(selectedCardIndex).GetComponent<DisplayCard>().Instantiate(card);
                UpdateHands();
                return true;
            }
            Debug.Log("No card selected");
            return false;
        }

        void UpdateHands()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
                DisplayCard card = child.GetComponent<DisplayCard>();
                child.localPosition = new Vector3(i - transform.childCount / 2, 0, 0) * spacing;
                spriteRenderer.sortingOrder = i;
                if (hideFirstCard && i == 0)
                    card.HideCard();
                else
                    card.ShowCard();
            }
        }

        public void ResetSelectedCard(bool moveBack = true)
        {
            if (selectedCardIndex != -1 && selectedCardIndex < transform.childCount)
            {
                Transform selectedCard = transform.GetChild(selectedCardIndex);
                if (moveBack) selectedCard.localPosition -= new Vector3(0, 0.5f, 0); // Move back to original position
                selectedCardIndex = -1;
            }
        }
    }
}