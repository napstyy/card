using UnityEngine;

public class HandsContainer : MonoBehaviour {
    public bool hideFirstCard;
    public float spacing = 1;

    public void UpdateHands()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
            DisplayCard card = child.GetComponent<DisplayCard>();
            child.localPosition = new Vector3(i-transform.childCount/2,0,0) * spacing;
            spriteRenderer.sortingOrder = i;
            if(hideFirstCard && i == 0)
                card.HideCard();
            else
                card.ShowCard();
        }
    }

    public void ClearHands()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}