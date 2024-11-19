using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using DG.Tweening;

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
        bool isAnimating;
        
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
                    if (isAnimating) return;

                    blackjackController.selectedHands = this;
                    if (selectedCardIndex == index)
                    {
                        // Deselect the card
                        selectedCardIndex = -1;
                        isAnimating = true;
                        cardObject.transform.DOLocalMoveY(cardObject.transform.localPosition.y - 0.5f, 0.5f).OnComplete(() => isAnimating = false);;// Move back to original position
                    }
                    else
                    {
                        // Move previously selected card back to original position
                        if (selectedCardIndex != -1)
                        {
                            Transform previousCard = transform.GetChild(selectedCardIndex);
                            isAnimating = true;
                            previousCard.DOLocalMoveY(previousCard.localPosition.y - 0.5f, 0.5f).OnComplete(() => isAnimating = false);;
                        }

                        // Select the new card and move it up
                        selectedCardIndex = index;
                        isAnimating = true;
                        cardObject.transform.DOLocalMoveY(cardObject.transform.localPosition.y + 0.5f, 0.5f).OnComplete(() => isAnimating = false);; // Move up
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
            if (selectedCardIndex > -1 && selectedCardIndex < cards.Count)
            {
                replacedCard = cards[selectedCardIndex];
                cards[selectedCardIndex] = card;
                transform.GetChild(selectedCardIndex).GetComponent<DisplayCard>().Instantiate(card);
                UpdateHands();
                return true;
            }
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
            if (isAnimating) return;
            if (selectedCardIndex != -1 && selectedCardIndex < transform.childCount)
            {
                Transform selectedCard = transform.GetChild(selectedCardIndex);
                if (moveBack) {
                    isAnimating = true;
                    selectedCard.DOLocalMoveY(selectedCard.localPosition.y - 0.5f, 0.5f); // Move back to original position
                }
                selectedCardIndex = -1;
            }
        }
    }
}