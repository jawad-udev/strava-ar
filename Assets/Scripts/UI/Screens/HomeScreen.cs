using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class HomeScreen : GameMonoBehaviour
{
    public Button playButton;
    public Button profileButton;
    public Button settingsButton;
    public TextMeshProUGUI coinText, usernameText, levelText;

    public Button testButton;

    private void Awake()
    {
        playButton.onClick.AsObservable().Subscribe(x => OnClickPlayButton());
        profileButton.onClick.AsObservable().Subscribe(x => OnClickProfileButton());
        settingsButton.onClick.AsObservable().Subscribe(x => OnClickSettingsButton());
        testButton.onClick.AsObservable().Subscribe(x => Test());
    }

    private void Start()
    {
        Services.PlayerService._player.playerName.AsObservable().SubscribeToText(usernameText);
        Services.PlayerService._player.level.AsObservable().Subscribe(x => levelText.SetText("Level " + x));
        Services.PlayerService._player.coins.AsObservable().SubscribeToText(coinText);
    }

    private void OnClickProfileButton()
    {
        Services.UIService.ActivateUIPopups(Popups.PROFILE);
        Services.AudioService.PlayUIClick();
    }

    private void OnClickSettingsButton()
    {
        Services.UIService.ActivateUIPopups(Popups.SETTINGS);
        Services.AudioService.PlayUIClick();
    }

    private void OnClickPlayButton()
    {
        Services.UIService.CommonPopup.OpenPopup("Game Mode", "Please select game mode to start game.",
            "Singleplayer", "Multiplayer", OnClickSinglePlayerButton, OnClickMultiplePlayerButton);
        Services.AudioService.PlayUIClick();
    }

    private void OnClickSinglePlayerButton()
    {
        Services.GameService.gameMode = GameMode.SinglePlayer;
        StartGame();
    }

    private void OnClickMultiplePlayerButton()
    {
        Services.GameService.gameMode = GameMode.MultiPlayer;
        StartGame();
    }

    private void StartGame()
    {
        Services.GameService.SetState<GamePlayState>();
    }

    private void Test()
    {
        Services.PlayerService.SetCoins(Random.Range(5,11));
        Services.PlayerService.SetHighScore(Random.Range(500, 1100));
        Services.PlayerService.SetTotalScore(Random.Range(90, 700000));
        Services.PlayerService.SetTimeSpent(Random.Range(500f, 1100f));
        Services.PlayerService.SetPlayerLevel(Random.Range(5, 100));
    }
}