using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace CardGame
{
    public class Hands : MonoBehaviour
    {
        protected static event Action UpdateSelectedCards;
        public event Action<int> OnHandsUpdate;

        public enum Role
        {
            Dealer,
            Player
        }

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

        [SerializeField] private AudioClip cardSlide;

        private void Start()
        {
            _uiController = FindAnyObjectByType<UIController>();
            _updateSelectedCardsHandler = () =>
            {
                if (BlackjackController.Instance.selectedHands != this && _selectedCardIndex >= 0)
                {
                    Transform previousCard = _cardObjectsList[_selectedCardIndex].transform;
                    _isAnimating = true;
                    previousCard.DOLocalMoveY(previousCard.localPosition.y - 0.5f, 0.5f).OnComplete(() =>
                    {
                        _isAnimating = false;
                        _selectedCardIndex = -1;
                    });
                }
            };

            UpdateSelectedCards += _updateSelectedCardsHandler;
        }

        private void OnDestroy()
        {
            UpdateSelectedCards -= _updateSelectedCardsHandler;
        }

        public void InitializeHands()
        {
            if (_cardObjectsList == null) _cardObjectsList = new List<DisplayCard>();
            if (cards == null) cards = new List<Card>();

            foreach (DisplayCard displayCard in _cardObjectsList)
            {
                ObjectPool.Instance.ReturnObject(CardSpriteReference.Instance.cardPrefab, displayCard.gameObject);
            }

            extraPoints = 0;
            _hideFirstCard = playerRole == Role.Dealer;
            _selectedCardIndex = -1;
            cards.Clear();
            _cardObjectsList.Clear();
            OnHandsUpdate?.Invoke(-1);
        }

        public void AddCardToHands(Card card)
        {
            GameObject cardObject = ObjectPool.Instance.GetObject(CardSpriteReference.Instance.cardPrefab, transform);
            DisplayCard displayCard = cardObject.GetComponent<DisplayCard>();
            int index = cards.Count;

            // Calculate start and end positions
            Vector3 startPos = transform.position + Vector3.right * 10f; // Starting position off-screen
            Vector3 targetPos = transform.position + new Vector3((index - cards.Count / 2f) * spacing, 0f, 0f);

            // Start the draw animation
            if (CardAnimationSystem.Instance != null)
            {
                CardAnimationSystem.Instance.AnimateCardDraw(
                    cardObject,
                    startPos,
                    targetPos,
                    playerRole != Role.Dealer || index != 0
                );
            }

            if (playerRole == Role.Player)
            {
                displayCard.OnCardClicked += () => HandleCardClick(cardObject, index);
            }

            displayCard.Instantiate(card);
            cards.Add(card);
            _cardObjectsList.Add(displayCard);
            UpdateHands();
        }
        private void HandleCardClick(GameObject cardObject, int index)
        {
            if (_isAnimating) return;
            AudioManager.Instance.PlaySFX(cardSlide);

            if (_selectedCardIndex == index)
            {
                DeselectCard(cardObject);
            }
            else
            {
                SelectCard(cardObject, index);
            }

            UpdateSelectedCards?.Invoke();
        }

        private void DeselectCard(GameObject cardObject)
        {
            _selectedCardIndex = -1;
            _isAnimating = true;
            cardObject.transform.DOLocalMoveY(cardObject.transform.localPosition.y - 0.5f, 0.5f).OnComplete(() =>
            {
                _isAnimating = false;
                _uiController.DisableReplaceButton();
            });
        }

        private void SelectCard(GameObject cardObject, int index)
        {
            if (_selectedCardIndex >= 0 && BlackjackController.Instance.selectedHands == this)
            {
                Transform previousCard = _cardObjectsList[_selectedCardIndex].transform;
                _isAnimating = true;
                previousCard.DOLocalMoveY(previousCard.localPosition.y - 0.5f, 0.5f).OnComplete(() => _isAnimating = false);
            }

            BlackjackController.Instance.selectedHands = this;
            _selectedCardIndex = index;
            _isAnimating = true;
            cardObject.transform.DOLocalMoveY(cardObject.transform.localPosition.y + 0.5f, 0.5f).OnComplete(() =>
            {
                _isAnimating = false;
                _uiController.EnableReplaceButton();
            });
        }

        public void ShowHands()
        {
            _hideFirstCard = false;
            UpdateHands();
        }

        public bool ReplaceCard(Card card, out Card replacedCard)
        {
            replacedCard = null;
            if (_selectedCardIndex >= 0 && _selectedCardIndex < cards.Count)
            {
                replacedCard = cards[_selectedCardIndex];
                cards[_selectedCardIndex] = card;
                _cardObjectsList[_selectedCardIndex].Instantiate(card);
                UpdateHands();
                _selectedCardIndex = -1;
                _uiController.DisableReplaceButton();
                return true;
            }
            return false;
        }

        public void DropCard(int index)
        {
            if (index < 0 || index >= cards.Count) return;

            GameObject removeCard = _cardObjectsList[index].gameObject;
            cards.RemoveAt(index);
            _cardObjectsList.RemoveAt(index);
            removeCard.transform.SetParent(null);
            ObjectPool.Instance.ReturnObject(CardSpriteReference.Instance.cardPrefab, removeCard);
            for (int i = 0; i < cards.Count; i++)
            {
                if (playerRole == Role.Player)
                {
                    DisplayCard displayCard = _cardObjectsList[i];
                    displayCard.RemoveEvent();
                    displayCard.OnCardClicked += () => HandleCardClick(displayCard.gameObject, i);
                }
            }
            UpdateHands();
        }

        public Hands Split()
        {
            GameObject splitHandsObj = Instantiate(BlackjackController.Instance.playerHandsPrefab);
            Hands newHands = splitHandsObj.GetComponent<Hands>();
            splitHandsObj.transform.position = transform.position - new Vector3(2.5f, 0);

            newHands.InitializeHands();
            newHands.AddCardToHands(cards[1]);
            newHands.chips = chips;
            DropCard(1);

            return newHands;
        }

        public bool IsPair()
        {
            return cards.Count == 2 && cards[0].rank == cards[1].rank;
        }

        private void UpdateHands()
        {
            for (int i = 0; i < _cardObjectsList.Count; i++)
            {
                DisplayCard card = _cardObjectsList[i];
                Transform child = card.transform;
                SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();

                child.localPosition = new Vector3((i - _cardObjectsList.Count / 2f) * spacing, 0f, 0f);
                spriteRenderer.sortingOrder = i * 2;

                if (_hideFirstCard && i == 0 || hideCards)
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

        public void ResetSelectedCard(bool moveBack = true)
        {
            if (_isAnimating || _selectedCardIndex < 0 || _selectedCardIndex >= _cardObjectsList.Count) return;

            Transform selectedCard = _cardObjectsList[_selectedCardIndex].transform;
            if (moveBack)
            {
                _isAnimating = true;
                selectedCard.DOLocalMoveY(selectedCard.localPosition.y - 0.5f, 0.5f).OnComplete(() =>
                {
                    _isAnimating = false;
                    _uiController.DisableReplaceButton();
                });
            }
            _selectedCardIndex = -1;
        }

        public void SetHands(List<Card> cards)
        {
            this.cards = cards;
            UpdateHands();
        }

        public void ResetExtraPoints()
        {
            extraPoints = 0;
            UpdateHands();
        }

        public void AddExtraPoints(int value)
        {
            extraPoints += value;
            UpdateHands();
        }

        public void ReduceExtraPoints(int value)
        {
            extraPoints -= value;
            UpdateHands();
        }
    }
}
