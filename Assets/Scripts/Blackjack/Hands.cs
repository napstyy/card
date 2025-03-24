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
        // Make this public so other hands can access it
        public static event Action OnCardSelectionChanged;
        public event Action<int,int> OnHandsUpdate;
        #endregion

        #region Public Fields and Properties
        [Header("Position Settings")]
        [SerializeField] private float cardSpacing = 1.2f;
        [SerializeField] private float dealerYPosition = 0.8f;
        [SerializeField] private float playerYPosition = -0.3f;
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private float selectionOffset = 0.5f;

        [Header("Audio")]
        [SerializeField] private AudioClip cardSlide1;
        [SerializeField] private AudioClip cardSlide2;

        // Public fields required by BlackjackController
        public List<Card> cards { get; private set; }
        public Role playerRole = Role.Player;
        public int chips;
        public int extraPoints;
        public bool hideCards;

        public Card[] CardsArray => cards.ToArray();
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

        private List<DisplayCard> _cardObjectsList;
        private int _selectedCardIndex = -1;
        private bool _hideFirstCard;
        private bool _isAnimating;
        private UIController _uiController;

        #region Unity Lifecycle
        private void Start()
        {
            Initialize();
            SetupEventHandlers();
            basePosition = transform.position;
        }

        private void OnDestroy()
        {
            // Unsubscribe from the global card selection event
            OnCardSelectionChanged -= HandleGlobalCardSelection;
            CleanupEventHandlers();
            currentAnimation?.Kill();
        }
        private void HandleGlobalCardSelection()
        {
            // If this hand isn't the currently selected one, deselect any selected card
            if (BlackjackController.Instance.selectedHands != this && selectedCardIndex >= 0)
            {
                DeselectCard(displayCards[selectedCardIndex].gameObject, false); // false means don't trigger global event
            }
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
            // Subscribe to the global card selection event
            OnCardSelectionChanged += HandleGlobalCardSelection;
        }


        private void CleanupEventHandlers()
        {
            OnCardSelectionChanged -= HandleGlobalCardSelection;  // Change from UpdateSelectedCards
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
            if(cards == null) Initialize();
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
            OnHandsUpdate?.Invoke(-1,extraPoints);
        }
        #endregion

        #region Card Management
        public void ResetExtraPoints()
        {
            extraPoints = 0;
            UpdateHandsLayout(true);
            OnHandsUpdate?.Invoke(BlackjackController.Instance.CountPoints(this),extraPoints);
        }

        public void AddExtraPoints(int value)
        {
            extraPoints += value;
            UpdateHandsLayout(true);
            OnHandsUpdate?.Invoke(BlackjackController.Instance.CountPoints(this),extraPoints);
        }

        public void ReduceExtraPoints(int value)
        {
            extraPoints -= value;
            UpdateHandsLayout(true);
            OnHandsUpdate?.Invoke(BlackjackController.Instance.CountPoints(this),extraPoints);
        }
        public void AddCardToHands(Card card)
        {
            if (card == null) return;

            GameObject cardObject = ObjectPool.Instance.GetObject(CardSpriteReference.Instance.cardPrefab, transform);
            DisplayCard displayCard = cardObject.GetComponent<DisplayCard>();

            // Setup card
            displayCard.Instantiate(card);

            // Only add click handlers to player cards
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

            DebugPrintCardsArray();
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
                    OnHandsUpdate?.Invoke(BlackjackController.Instance.CountPoints(this),extraPoints);
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

            OnHandsUpdate?.Invoke(BlackjackController.Instance.CountPoints(this),extraPoints);
        }
        #endregion

        #region Card Interaction
        private void HandleCardClick(GameObject cardObject, int index)
        {
            // Only allow card selection during Playing state and when not animating
            if (isAnimating || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
                return;

            // Don't allow dealer's cards to be selected
            if (playerRole == Role.Dealer)
                return;

            // Don't allow selection if it's the dealer's turn
            if (BlackjackController.Instance.roundState == BlackjackController.RoundState.End)
                return;

            if (selectedCardIndex == index)
            {
                AudioManager.Instance.PlaySFX(cardSlide2);
                DeselectCard(cardObject, true);
            }
            else
            {
                AudioManager.Instance.PlaySFX(cardSlide1);
                SelectCard(cardObject, index);
            }
        }

        private void SelectCard(GameObject cardObject, int index)
        {
            // Step 1: Handle any previously selected card first
            bool isSameCard = (BlackjackController.Instance.selectedHands == this && selectedCardIndex == index);
            if (isSameCard)
            {
                // If clicking the same card, just deselect it
                DeselectCard(cardObject, true);
                return;
            }

            // Step 2: Create a sequence for smooth animation
            Sequence sequence = DOTween.Sequence();

            // Step 3: If there's any card selected anywhere, lower it first
            if (BlackjackController.Instance.selectedHands != null)
            {
                if (BlackjackController.Instance.selectedHands != this)
                {
                    // Different hand - tell it to deselect its card without triggering events
                    sequence.AppendCallback(() =>
                    {
                        BlackjackController.Instance.selectedHands.ResetSelectedCard(true);
                    });
                }
                else if (selectedCardIndex >= 0)
                {
                    // Same hand, different card - animate current card down first
                    sequence.Append(displayCards[selectedCardIndex].transform
                        .DOLocalMoveY(playerYPosition, animationDuration)
                        .SetEase(Ease.InOutQuad));
                }
            }

            // Step 4: Update selection state
            BlackjackController.Instance.selectedHands = this;
            selectedCardIndex = index;

            // Step 5: Animate new card up
            sequence.Append(cardObject.transform
                .DOLocalMoveY(playerYPosition + selectionOffset, animationDuration)
                .SetEase(Ease.OutQuad));

            // Step 6: Play sequence and update UI
            currentAnimation?.Kill();
            currentAnimation = sequence;
            sequence.Play();

            // Step 7: Update UI elements
            if (GameManager.Instance.Progress.HasSwapsRemaining())
            {
                uiController.EnableReplaceButton();
            }

            // Step 8: Notify other hands after animation is complete
            sequence.OnComplete(() =>
            {
                OnCardSelectionChanged?.Invoke();
            });
        }

        private void DeselectCard(GameObject cardObject, bool triggerGlobalEvent)
        {
            if (cardObject == null) return;

            // Create deselection animation
            Sequence sequence = DOTween.Sequence();
            sequence.Append(cardObject.transform
                .DOLocalMoveY(playerYPosition, animationDuration)
                .SetEase(Ease.InQuad));

            // Update state after animation
            sequence.OnComplete(() =>
            {
                selectedCardIndex = -1;
                if (BlackjackController.Instance.selectedHands == this)
                {
                    BlackjackController.Instance.selectedHands = null;
                }

                if (triggerGlobalEvent)
                {
                    OnCardSelectionChanged?.Invoke();
                }

                uiController.DisableReplaceButton();
            });

            currentAnimation?.Kill();
            currentAnimation = sequence;
            sequence.Play();
        }

        public void ResetSelectedCard(bool moveBack = true)
        {
            if (selectedCardIndex >= 0 && selectedCardIndex < displayCards.Count)
            {
                if (moveBack)
                {
                    DeselectCard(displayCards[selectedCardIndex].gameObject, true);
                }
                else
                {
                    selectedCardIndex = -1;
                    if (BlackjackController.Instance.selectedHands == this)
                    {
                        BlackjackController.Instance.selectedHands = null;
                    }
                }
            }

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

        public void DebugPrintCardsArray()
        {
            // we bring the newly added array here
            Card[] cardArray = CardsArray;

            // make a string thats just the card rank so we can clearly read the enemy cards from the debug
            string output = string.Join(", ", cardArray.Select(card => card.rank.ToString()).ToArray());
            Debug.Log("Cards Array: " + output);
        }

        #endregion
    }
}