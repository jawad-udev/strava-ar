using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class GamePlayScreen : GameMonoBehaviour
{
    public Button pauseButton, profileButton;

    private void Awake()
    {
        pauseButton.onClick.AsObservable().Subscribe(x => OnClickPauseButton()) ;
        profileButton.onClick.AsObservable().Subscribe(x => OnClickProfileButton());
    }

    public void OnClickProfileButton()
    {
        Services.UIService.ActivateUIPopups(Popups.PROFILE);
        Services.AudioService.PlayUIClick();
    }

    public void OnClickPauseButton()
    {
        Services.GameService.SetState<GamePauseState>();
        Services.AudioService.PlayUIClick();
    }
}