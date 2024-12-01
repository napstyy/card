using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

namespace CardGame
{
    /// <summary>
    /// Manages a hand of cards in the game, handling layout, animations, and interactions.
    /// Can represent either a player's or dealer's hand.
    /// </summary>
    public class Hands : MonoBehaviour
    {
        #region Events and Delegates
        protected static event Action UpdateSelectedCards;
        public event Action<int> OnHandsUpdate;  // Required by HandsUIController
        #endregion

        #region Public Fields and Properties
        [Header("Position Settings")]
        [SerializeField] private float cardSpacing = 1.2f;
        [SerializeField] private float dealerYPosition = 0.8f;
        [SerializeField] private float playerYPosition = -0.3f;
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private float selectionOffset = 0.5f;

        [Header("Audio")]
        [SerializeField] private AudioClip cardSlide;

        // Public fields required by BlackjackController
        public List<Card> cards { get; private set; }
        public Role playerRole = Role.Player;
        public int chips;
        public int extraPoints;
        public bool hideCards;
        #endregion

        #region Private Fields
        private List<DisplayCard> displayCards;
        private int selectedCardIndex = -1;
        private bool isAnimating;
        private UIController uiController;
        private bool hideFirstCard;
        private Vector3 basePosition;
        private Sequence currentAnimation;
        #endregion

        #region Enums
        public enum Role
        {
            Dealer,
            Player
        }
        #endregion

        #region Unity Lifecycle

        public List<Card> cards { get; private set; }
        public Role playerRole = Role.Player;
        public float spacing = 1f;
        public int chips;
        public int extraPoints { get; private set; }
        public bool hideCards;

        private List<DisplayCard> _cardObjectsList;
        private int _selectedCardIndex = -1;
        private bool _hideFirstCard;
        private bool _isAnimating;
        private UIController _uiController;
        private Action _updateSelectedCardsHandler;

        [SerializeField] private AudioClip cardSlide1;
        [SerializeField] private AudioClip cardSlide2;

        private void Start()
        {
            Initialize();
            SetupEventHandlers();
            basePosition = transform.position;
        }

        private void OnDestroy()
        {
            CleanupEventHandlers();
            currentAnimation?.Kill();
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            cards = new List<Card>();
            displayCards = new List<DisplayCard>();
            uiController = FindAnyObjectByType<UIController>();
        }

        private void SetupEventHandlers()
        {
            UpdateSelectedCards += () =>
            {
                if (BlackjackController.Instance.selectedHands != this && selectedCardIndex >= 0)
                {
                    DeselectCard(displayCards[selectedCardIndex].gameObject);
                }
            };
        }

        private void CleanupEventHandlers()
        {
            UpdateSelectedCards = null;
            foreach (var displayCard in displayCards)
            {
                if (displayCard != null)
                {
                    displayCard.RemoveEvent();
                }
            }
        }

        public void InitializeHands()
        {
            foreach (DisplayCard displayCard in displayCards)
            {
                if (displayCard != null && displayCard.gameObject != null)
                {
                    ObjectPool.Instance.ReturnObject(CardSpriteReference.Instance.cardPrefab, displayCard.gameObject);
                }
            }

            extraPoints = 0;
            hideFirstCard = playerRole == Role.Dealer;
            selectedCardIndex = -1;
            cards.Clear();
            displayCards.Clear();
            OnHandsUpdate?.Invoke(-1);
        }
        #endregion

        #region Card Management
        public void ResetExtraPoints()
        {
            extraPoints = 0;
            UpdateHandsLayout(true);
            OnHandsUpdate?.Invoke(BlackjackController.Instance.CountPoints(this));
        }

        public void AddExtraPoints(int value)
        {
            extraPoints += value;
            UpdateHandsLayout(true);
            OnHandsUpdate?.Invoke(BlackjackController.Instance.CountPoints(this));
        }

        public void ReduceExtraPoints(int value)
        {
            extraPoints -= value;
            UpdateHandsLayout(true);
            OnHandsUpdate?.Invoke(BlackjackController.Instance.CountPoints(this));
        }
        public void AddCardToHands(Card card)
        {
            if (card == null) return;

            GameObject cardObject = ObjectPool.Instance.GetObject(CardSpriteReference.Instance.cardPrefab, transform);
            DisplayCard displayCard = cardObject.GetComponent<DisplayCard>();

            // Setup card
            displayCard.Instantiate(card);
            if (playerRole == Role.Player)
            {
                int newCardIndex = cards.Count;
                displayCard.OnCardClicked += () => HandleCardClick(cardObject, newCardIndex);
            }

            // Set initial sorting order
            SpriteRenderer spriteRenderer = cardObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = (playerRole == Role.Dealer ? 100 : 200) + (cards.Count * 2);

            // Add to collections
            cards.Add(card);
            displayCards.Add(displayCard);

            // Animate card entry
            AnimateCardEntry(cardObject, displayCard);
        }

        private void AnimateCardEntry(GameObject cardObject, DisplayCard displayCard)
        {
            float yPosition = playerRole == Role.Dealer ? dealerYPosition : playerYPosition;
            float totalWidth = (cards.Count - 1) * cardSpacing;

            // Starting position (off-screen right)
            Vector3 startPosition = basePosition + new Vector3(10f, yPosition, 0);
            cardObject.transform.position = startPosition;

            // Target position
            Vector3 targetPosition = basePosition + new Vector3(
                -totalWidth / 2f + ((cards.Count - 1) * cardSpacing),
                yPosition,
                0
            );

            // Create and store animation sequence
            currentAnimation?.Kill();
            currentAnimation = DOTween.Sequence()
                .Append(cardObject.transform.DOMove(targetPosition, animationDuration).SetEase(Ease.OutQuad))
                .OnComplete(() =>
                {
                    UpdateHandsLayout(false);
                    if (hideFirstCard && cards.Count == 1 || hideCards)
                    {
                        displayCard.HideCard();
                    }
                    else
                    {
                        displayCard.ShowCard();
                    }
                    OnHandsUpdate?.Invoke(BlackjackController.Instance.CountPoints(this));
                });
        }

        public bool ReplaceCard(Card newCard, out Card replacedCard)
        {
            replacedCard = null;
            if (selectedCardIndex < 0 || selectedCardIndex >= cards.Count || newCard == null)
                return false;

            replacedCard = cards[selectedCardIndex];
            cards[selectedCardIndex] = newCard;
            displayCards[selectedCardIndex].Instantiate(newCard);

            UpdateHandsLayout(true);
            ResetSelectedCard();
            return true;
        }

        public void ShowHands()
        {
            hideFirstCard = false;
            foreach (var card in displayCards)
            {
                if (card != null)
                {
                    card.ShowCard();
                }
            }
            UpdateHandsLayout(false);
        }

        public bool IsPair()
        {
            return cards.Count == 2 && cards[0].rank == cards[1].rank;
        }
        #endregion

        #region Card Layout and Animation
        private void UpdateHandsLayout(bool animate = true)
        {
            if (displayCards == null || displayCards.Count == 0) return;

            currentAnimation?.Kill();
            float totalWidth = (displayCards.Count - 1) * cardSpacing;
            float yPosition = playerRole == Role.Dealer ? dealerYPosition : playerYPosition;

            currentAnimation = DOTween.Sequence();

            for (int i = 0; i < displayCards.Count; i++)
            {
                DisplayCard card = displayCards[i];
                if (card == null) continue;

                Vector3 targetPosition = new Vector3(
                    -totalWidth / 2f + (i * cardSpacing),
                    yPosition + (i == selectedCardIndex ? selectionOffset : 0),
                    0
                );

                if (animate)
                {
                    currentAnimation.Join(card.transform.DOLocalMove(targetPosition, animationDuration)
                        .SetEase(Ease.OutQuad));
                }
                else
                {
                    card.transform.localPosition = targetPosition;
                }

                // Update sorting order
                SpriteRenderer spriteRenderer = card.GetComponent<SpriteRenderer>();
                spriteRenderer.sortingOrder = (playerRole == Role.Dealer ? 100 : 200) + (i * 2);

                // Update visibility
                if (hideFirstCard && i == 0 || hideCards)
                {
                    card.HideCard();
                }
                else
                {
                    card.ShowCard();
                }
            }

            OnHandsUpdate?.Invoke(BlackjackController.Instance.CountPoints(this));
        }
        #endregion

        #region Card Interaction
        private void HandleCardClick(GameObject cardObject, int index)
        {
            if (isAnimating || GameManager.Instance.CurrentState != GameManager.GameState.Preparation)
                return;

            AudioManager.Instance.PlaySFX(cardSlide);
            if (_isAnimating) return;

            if (selectedCardIndex == index)
            {
                AudioManager.Instance.PlaySFX(cardSlide2);
                DeselectCard(cardObject);
            }
            else
            {
                AudioManager.Instance.PlaySFX(cardSlide1);
                SelectCard(cardObject, index);
            }

            UpdateSelectedCards?.Invoke();
        }

        private void SelectCard(GameObject cardObject, int index)
        {
            if (selectedCardIndex >= 0 && BlackjackController.Instance.selectedHands == this)
            {
                DeselectCard(displayCards[selectedCardIndex].gameObject);
            }

            BlackjackController.Instance.selectedHands = this;
            selectedCardIndex = index;

            // Animate card selection
            currentAnimation?.Kill();
            currentAnimation = DOTween.Sequence()
                .Append(cardObject.transform.DOMove(
                    cardObject.transform.position + Vector3.up * selectionOffset,
                    animationDuration
                ).SetEase(Ease.OutQuad));

            uiController.EnableReplaceButton();
        }

        private void DeselectCard(GameObject cardObject)
        {
            selectedCardIndex = -1;
            UpdateHandsLayout(true);
            uiController.DisableReplaceButton();
        }

        public void ResetSelectedCard(bool moveBack = true)
        {
            if (selectedCardIndex < 0 || selectedCardIndex >= displayCards.Count)
                return;

            if (moveBack)
            {
                DeselectCard(displayCards[selectedCardIndex].gameObject);
            }

            selectedCardIndex = -1;
        }
        #endregion

        #region Split Functionality
        public Hands Split()
        {
            if (!IsPair()) return null;

            GameObject splitHandsObj = Instantiate(BlackjackController.Instance.playerHandsPrefab);
            Hands newHands = splitHandsObj.GetComponent<Hands>();
            splitHandsObj.transform.position = transform.position - new Vector3(2.5f, 0);

            newHands.InitializeHands();
            newHands.AddCardToHands(cards[1]);
            newHands.chips = chips;
            DropCard(1);

            return newHands;
        }

        private void DropCard(int index)
        {
            if (index < 0 || index >= cards.Count) return;

            GameObject removeCard = displayCards[index].gameObject;
            cards.RemoveAt(index);
            displayCards.RemoveAt(index);

            if (removeCard != null)
            {
                ObjectPool.Instance.ReturnObject(CardSpriteReference.Instance.cardPrefab, removeCard);
            }

            // Update event handlers for remaining cards
            for (int i = 0; i < cards.Count; i++)
            {
                var displayCard = displayCards[i];
                if (playerRole == Role.Player)
                {
                    int capturedIndex = i;
                    displayCard.RemoveEvent();
                    displayCard.OnCardClicked += () => HandleCardClick(displayCard.gameObject, capturedIndex);
                }
            }

            UpdateHandsLayout(true);
        }
        #endregion
    }
}