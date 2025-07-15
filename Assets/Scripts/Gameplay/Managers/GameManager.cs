using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UniRx;

public class GameManager : MonoBehaviour
{
    #region Variables
    public Camera gameplayCamera;
    CameraShake cameraShake;
    CameraZoom cameraZoom;

    public IReactiveProperty<int> totalLifes { get; private set; }
    #endregion

    private void Awake()
    {
        //Init();

        /* this.PerformActionWithDelay(5f, ()=> {
            Services.GameService.SetState<MenuState>();
        }); */
    }

    public void Init()
    {
        totalLifes = new ReactiveProperty<int>(5);
        Services.CameraService.AssignPlayerCamera(this);
        gameplayCamera.orthographicSize = Services.CameraService._zoomOutLimit;
        cameraShake = gameplayCamera.GetComponent<CameraShake>();
        cameraZoom = gameplayCamera.GetComponent<CameraZoom>();
        ZoomIn();

        totalLifes.Subscribe(x => {
            if (x == 0)
                Services.GameService.OnGameFinish(false);
        });
    }


    public void ShakeCamera()
    {
        cameraShake.ShakeCamera(0.03f);
    }

    public void ShakeCameraInfinite()
    {
        cameraShake.ShakeCameraInfinite(0.03f);
    }

    public void StopShaking()
    {
        cameraShake.StopShaking();
    }

    public void ZoomIn(Action zoomListener = null)
    {
        cameraZoom.ZoomIn(zoomListener);
    }

    public void ZoomOut(Action zoomListener = null)
    {
        cameraZoom.ZoomOut(zoomListener);
    }
}