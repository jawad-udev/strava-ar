using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UniRx;

public class SettingsPopup : GameMonoBehaviour
{
    public Button musicButton, soundButton, vibrationButton, closeButton, clearData, showDebugButton;

    void Awake()
    {
        //musicButton.onClick.AsObservable().Subscribe(x => OnClickMusicButton());
        //soundButton.onClick.AsObservable().Subscribe(x => OnClickSoundButton());
        //vibrationButton.onClick.AsObservable().Subscribe(x => OnClickVibrationButton());
        closeButton.onClick.AsObservable().Subscribe(x => OnClickCloseButton());
        clearData.onClick.AsObservable().Subscribe(x => OnClickClearDataButton());
        showDebugButton.onClick.AsObservable().Subscribe(x => OnClickShowDebugButton());
    }

    void OnClickMusicButton()
    {
        Services.AudioService.PlayUIClick();
    }

    void OnClickSoundButton()
    {
        Services.AudioService.PlayUIClick();
    }

    void OnClickVibrationButton()
    {
        Services.AudioService.PlayUIClick();
    }

    void OnClickCloseButton()
    {
        Services.BackLogService.CloseLastUI();
        Services.AudioService.PlayUIClick();
    }

    void OnClickClearDataButton()
    {
        Services.instance.clearPrefs = true;
        clearData.interactable = false;
        Services.AudioService.PlayUIClick();
    }

    void OnClickShowDebugButton()
    {
        // Services.DebugConsole.SetActive(true);
        Services.AudioService.PlayUIClick();
    }
}
