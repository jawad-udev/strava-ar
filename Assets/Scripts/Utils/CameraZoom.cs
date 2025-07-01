using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraZoom : MonoBehaviour
{
    Action zoomCallback;
    public Camera cam;

    public void ZoomIn(Action zoomListener = null)
    {
        zoomCallback = zoomListener;

        if (cam.orthographicSize > Services.CameraService._zoomInLimit)
        {
            StartTween(Services.CameraService._zoomOutLimit, Services.CameraService._zoomInLimit);
        }
    }

    public void ZoomOut(Action zoomListener = null)
    {
        zoomCallback = zoomListener;

        if (cam.orthographicSize < Services.CameraService._zoomOutLimit)
            StartTween(Services.CameraService._zoomInLimit, Services.CameraService._zoomOutLimit);
    }

    void StartTween(float initialValue, float finalValue)
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", initialValue, "to", finalValue, "time", Services.CameraService.zoomSpeed,
            "easetype", Services.CameraService.easeType, "onupdatetarget", gameObject, "onupdate", "OnUpdateValue", "oncomplete", "OnTweenComplete"));
    }

    void OnUpdateValue(float newValue)
    {
        cam.orthographicSize = newValue;
    }

    void OnTweenComplete()
    {
        zoomCallback?.Invoke();
    }
}
