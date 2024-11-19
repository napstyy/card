using System;
using UnityEngine;

namespace CardGame
{
    public class DisplayCard : MonoBehaviour
    {

        public event Action OnCardClicked;
        SpriteRenderer m_spriteRenderer;
        Card.Ranks rank;
        Card.Suits suit;
        int posIndex;

        public void Instantiate(Card card)
        {
            m_spriteRenderer = GetComponent<SpriteRenderer>();
            rank = card.rank;
            suit = card.suit;
        }

        public void HideCard()
        {
            m_spriteRenderer.sprite = CardSpriteReference.Instance.GetCardBack();
        }

        public void ShowCard()
        {
            m_spriteRenderer.sprite = CardSpriteReference.Instance.GetCardSprite(rank, suit);
        }

        // void Update()
        // {
        //     // Detect if the left mouse button is clicked
        //     if (Input.GetMouseButtonDown(0))
        //     {
        //         // Convert mouse position to world space
        //         Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //         // Perform a 2D raycast at the mouse position
        //         RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        //         // Check if the raycast hit this object
        //         if (hit.collider != null && hit.collider.gameObject == gameObject)
        //         {
        //             Debug.Log($"{gameObject.name} was clicked!");
        //             // Add your custom event logic here (e.g., destroying the object, changing color, etc.)
        //             OnCardClicked?.Invoke();
        //         }
        //     }
        // }

        void Update()
        {
            // Detect if the left mouse button is clicked
            if (Input.GetMouseButtonDown(0))
            {
                // Convert mouse position to world space
                Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                // Perform a 2D raycast at the mouse position
                RaycastHit2D[] hits = Physics2D.RaycastAll(worldPoint, Vector2.zero);

                // Check if the raycast hit this object
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider != null && hit.collider.gameObject == gameObject)
                    {
                        // Check if this object has the highest sorting order
                        SpriteRenderer hitRenderer = hit.collider.GetComponent<SpriteRenderer>();
                        bool isTopMost = true;
                        foreach (RaycastHit2D otherHit in hits)
                        {
                            if (otherHit.collider != null && otherHit.collider != hit.collider)
                            {
                                SpriteRenderer otherRenderer = otherHit.collider.GetComponent<SpriteRenderer>();
                                if (otherRenderer != null && otherRenderer.sortingOrder > hitRenderer.sortingOrder)
                                {
                                    isTopMost = false;
                                    break;
                                }
                            }
                        }

                        if (isTopMost)
                        {
                            Debug.Log($"{gameObject.name} was clicked!");
                            // Add your custom event logic here (e.g., destroying the object, changing color, etc.)
                            OnCardClicked?.Invoke();
                            break;
                        }
                    }
                }
            }
        }

        private void OnDisable()
        {
            OnCardClicked = null;
        }
    }
}