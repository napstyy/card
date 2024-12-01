using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scroller : MonoBehaviour
{
    [SerializeField] private RawImage img;
    [SerializeField] private float x, y;

    void Update()
    {
        img.uvRect = new Rect(img.uvRect.x + x * Time.deltaTime, img.uvRect.y + y * Time.deltaTime, img.uvRect.width, img.uvRect.height);
    }
}
