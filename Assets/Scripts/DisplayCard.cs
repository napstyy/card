using UnityEngine;

public class DisplayCard : MonoBehaviour {
    static int order;
    SpriteRenderer m_spriteRenderer;

    public void Instantiate(Card card)
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_spriteRenderer.sprite = CardSpriteReference.Instance.GetCardSprite(card.rank, card.suit);
        m_spriteRenderer.sortingOrder = order++;
    }
}