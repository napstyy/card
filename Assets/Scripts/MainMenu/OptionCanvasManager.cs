using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionCanvasManager : MonoBehaviour
{
    void Start()
    {
        OptionUIManager optionsMenu = FindObjectOfType<OptionUIManager>();
        GameObject canvas = optionsMenu.transform.root.gameObject;
        Canvas canvasComponent = canvas.GetComponent<Canvas>();
        if (canvasComponent != null && canvasComponent.renderMode == RenderMode.ScreenSpaceCamera)
        {
            canvasComponent.worldCamera = Camera.main;
        }
        else
        {
            canvasComponent.renderMode = RenderMode.ScreenSpaceCamera;
            canvasComponent.worldCamera = Camera.main;
        }
    }

}
