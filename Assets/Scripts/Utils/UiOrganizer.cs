using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.Linq; // using LINQ to GameObject
using Zenject;

public interface IUiCanvas
{
    void ParentThisToCanvas(Transform toBeMadeChild, UIType type);
    Canvas GetCanvas();
}

public class UiOrganizer : IUiCanvas {
    private readonly Canvas mainCanvas;

    public Canvas GetCanvas()
    {
        return mainCanvas;
    }

    public GameObject GetScreenParent()
    {
        return mainCanvas.gameObject.Child("Screens"); ;
    }

    public GameObject GetPopupParent()
    {
        return mainCanvas.gameObject.Child("Popups"); ;
    }

    public Transform GetMainCanvasRoot()
    {
        return mainCanvas.transform;
    }

    public void ParentThisToCanvas(Transform toBeMadeChild, UIType type)
    {
        if(type == UIType.Screen)
            toBeMadeChild.SetParent(GetScreenParent().transform, false);
        if (type == UIType.Popup)
            toBeMadeChild.SetParent(GetPopupParent().transform, false);
    }

    public UiOrganizer(Canvas mainCanvas)
    {
        this.mainCanvas = mainCanvas;
    }

    public void Initialize()
    {
        
    }
}