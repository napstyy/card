using UnityEngine;

namespace CardGame
{
    public class CardSpriteReference : MonoBehaviour
    {
        [SerializeField] Sprite[] spades;
        [SerializeField] Sprite[] hearts;
        [SerializeField] Sprite[] diamonds;
        [SerializeField] Sprite[] clubs;

        [SerializeField] Sprite cardBack;

        public static CardSpriteReference Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public Sprite GetCardSprite(Card.Ranks ranks, Card.Suits suits)
        {
            Sprite sprite;
            switch (suits)
            {
                case Card.Suits.Diamonds:
                    sprite = diamonds[(int)ranks - 2];
                    break;
                case Card.Suits.Clubs:
                    sprite = clubs[(int)ranks - 2];
                    break;
                case Card.Suits.Hearts:
                    sprite = hearts[(int)ranks - 2];
                    break;
                case Card.Suits.Spades:
                    sprite = spades[(int)ranks - 2];
                    break;
                default:
                    sprite = null;
                    break;
            }

            return sprite;
        }

        public Sprite GetCardBack()
        {
            return cardBack;
        }
    }
}