using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

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
        public Role role = Role.Player;
        public float spacing = 1;
        public int chips;

        List<DisplayCard> cardObjectsList;

        int selectedCardIndex;
        bool hideFirstCard;
        bool isAnimating;

        BlackjackController blackjackController;
        SimpleUIController simpleUIController;

        void Start()
        {
            cardObjectsList = new List<DisplayCard>();
            cards = new List<Card>();
            blackjackController = FindAnyObjectByType<BlackjackController>();
            simpleUIController = FindAnyObjectByType<SimpleUIController>();
            simpleUIController.DisableReplaceButton();
        }

        public void InitializeHands()
        {
            foreach(DisplayCard displayCard in cardObjectsList)
            {
                ObjectPool.instance.ReturnObject(CardSpriteReference.Instance.cardPrefab, displayCard.gameObject);
            }
            hideFirstCard = role == Role.Dealer;
            selectedCardIndex = -1;
            cards?.Clear();
            cardObjectsList?.Clear();
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
                        cardObject.transform.DOLocalMoveY(cardObject.transform.localPosition.y - 0.5f, 0.5f).OnComplete(() =>
                        {
                            isAnimating = false;
                            simpleUIController.DisableReplaceButton();
                        }); // Move back to original position
                    }
                    else
                    {
                        // Move previously selected card back to original position
                        if (selectedCardIndex != -1)
                        {
                            Transform previousCard = cardObjectsList[selectedCardIndex].transform;
                            isAnimating = true;
                            previousCard.DOLocalMoveY(previousCard.localPosition.y - 0.5f, 0.5f).OnComplete(() => isAnimating = false); ;
                        }

                        // Select the new card and move it up
                        selectedCardIndex = index;
                        isAnimating = true;
                        cardObject.transform.DOLocalMoveY(cardObject.transform.localPosition.y + 0.5f, 0.5f).OnComplete(() => 
                        {
                            isAnimating = false;
                            simpleUIController.EnableReplaceButton();
                        }); // Move up
                    }
                };
            }
            displayCard.Instantiate(card);
            cards.Add(card);
            cardObjectsList.Add(displayCard);
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
                cardObjectsList[selectedCardIndex].Instantiate(card);
                UpdateHands();
                selectedCardIndex = -1;
                simpleUIController.DisableReplaceButton();
                return true;
            }
            return false;
        }

        public void DropCard(int index)
        {
            if(index > cards.Count-1)return;
            cards.RemoveAt(index);
            GameObject removeCard = cardObjectsList[index].gameObject;
            removeCard.transform.SetParent(null);
            Destroy(removeCard);
            UpdateHands();
        }

        public Hands Split()
        {
            GameObject splitHands = new GameObject("Split Hands");
            Hands newHands = splitHands.AddComponent<Hands>(); 
            splitHands.transform.position = transform.position - new Vector3(2.5f,0);
            newHands.InitializeHands();
            newHands.AddCardToHands(cards[0]);
            newHands.chips = this.chips;
            DropCard(0);
            return newHands;
        }

        public bool IsPair() => cards[0].rank == cards[1].rank && cards.Count == 2;

        void UpdateHands()
        {
            for (int i = 0; i < cardObjectsList.Count; i++)
            {
                DisplayCard card = cardObjectsList[i];
                Transform child = card.transform;
                SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
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
            if (selectedCardIndex != -1 && selectedCardIndex < cardObjectsList.Count)
            {
                Transform selectedCard = cardObjectsList[selectedCardIndex].transform;
                if (moveBack)
                {
                    isAnimating = true;
                    selectedCard.DOLocalMoveY(selectedCard.localPosition.y - 0.5f, 0.5f).OnComplete(() => 
                    {
                        isAnimating = false;
                        simpleUIController.DisableReplaceButton();
                    }); // Move back to original position
                }
                selectedCardIndex = -1;
            }
        }
    }
}