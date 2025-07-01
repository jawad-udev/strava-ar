using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UniRx;

public class PausePopup : GameMonoBehaviour
{
    public Button resumeButton, restartButton, exitButton;

    void Awake()
    {
        resumeButton.onClick.AsObservable().Subscribe(x => OnClickResumeButton());
        restartButton.onClick.AsObservable().Subscribe(x => OnClickRestartButton());
        exitButton.onClick.AsObservable().Subscribe(x => OnClickExitButton());
    }

    void OnClickResumeButton()
    {
        Services.GameService.SetState<GamePlayState>();
        Services.AudioService.PlayUIClick();
    }

    void OnClickRestartButton()
    {
        Services.GameService.DestryoGameplayManager();
        Services.GameService.gameStatus = GameStatus.TOSTART;
        Services.GameService.SetState<GamePlayState>();
        Services.AudioService.PlayUIClick();
    }

    void OnClickExitButton()
    {
        Services.GameService.SetState<MenuState>();
        Services.AudioService.PlayUIClick();
    }
}
