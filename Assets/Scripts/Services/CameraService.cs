using UnityEngine;
using System;
using System.Collections;

public class CameraService
{
    public float _zoomOutLimit = 50f;
    public float _zoomInLimit = 30f;
    public float zoomSpeed = 1f;
    public iTween.EaseType easeType = iTween.EaseType.easeInOutSine;

    public Camera AssignPlayerCamera(GameplayOwner owner)
    {
        Services.GameService.GetPlayerManager(owner).gameplayCamera = Camera.main;
        Vector3 camPosition = Services.GameService.GetPlayerManager(owner).transform.position;
        Camera.main.transform.position = new Vector3(camPosition.x, camPosition.y, -10);
        return Camera.main;
    }

    public Camera AssignPlayerCamera(GameManager gameplayManager)
    {
        gameplayManager.gameplayCamera = Camera.main;
        Vector3 camPosition = gameplayManager.transform.position;
        Camera.main.transform.position = new Vector3(camPosition.x, camPosition.y, -10);
        Camera.main.transform.rotation = Quaternion.identity;

        return Camera.main;
    }

    public void ShakeCamera(GameManager gameplay)
    {
        gameplay.ShakeCamera();
    }

    public void ShakeCamera(GameplayOwner gameplayOwner)
    {
        Services.GameService.GetPlayerManager(gameplayOwner).ShakeCamera();
    }

    public void ZoomIn(GameManager gameplay, Action zoomListener = null)
    {
        gameplay.ZoomIn(zoomListener);
    }

    public void ZoomOut(GameManager gameplay, Action zoomListener = null)
    {
        gameplay.ZoomOut(zoomListener);
    }

    public Vector3 GetCameraTopPosition(Camera cam)
    {
        ScreenUtility screenUtility = cam.GetComponent<ScreenUtility>();

        return new Vector3(0, screenUtility.Top, 0);
    }

    public Vector3 GetCameraTopPosition(GameplayOwner gameplayOwner)
    {
        ScreenUtility screenUtility = Services.GameService.GetPlayerManager(gameplayOwner).gameplayCamera.GetComponent<ScreenUtility>();

        return new Vector3(0, screenUtility.Top, 0);
    }

    public float GetCameraHeight(Camera cam)
    {
        ScreenUtility screenUtility = cam.GetComponent<ScreenUtility>();

        return screenUtility.Height;
    }

    public bool IsInCamView(GameplayOwner gameplayOwner, Vector3 pos)
    {
        ScreenUtility screenUtility = Services.GameService.GetPlayerManager(gameplayOwner).gameplayCamera.GetComponent<ScreenUtility>();

        if (screenUtility.Right > pos.x && screenUtility.Left < pos.x && screenUtility.Top > pos.y && screenUtility.Bottom < pos.y)
            return true;

        return false;

    }

    public void ResetCameraSize()
    {
        if (Services.GameService.gameManager)
            Services.GameService.gameManager.gameplayCamera.orthographicSize = _zoomOutLimit;
    }
}