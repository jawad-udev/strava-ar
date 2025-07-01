using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UniRx;

public class ProfilePopup : GameMonoBehaviour
{
    public Button closeButton;
    public TextMeshProUGUI usernameText, levelText, totalScoreText, highScoreText, timeSpentText, gamePlayedText, coinsText;

    private void Awake()
    {
        closeButton.onClick.AsObservable().Subscribe(x => OnClickCloseButton());
    }

    private void Start()
    {
        Services.PlayerService._player.playerName.AsObservable().SubscribeToText(usernameText);
        Services.PlayerService._player.timeSpent.AsObservable().Subscribe(x => timeSpentText.SetText(Mathf.FloorToInt(x / 60) + " mins"));
        Services.PlayerService._player.level.AsObservable().SubscribeToText(levelText);
        Services.PlayerService._player.highScore.AsObservable().SubscribeToText(highScoreText);
        Services.PlayerService._player.numberOfGames.AsObservable().SubscribeToText(gamePlayedText);
        Services.PlayerService._player.coins.AsObservable().SubscribeToText(coinsText);
        Services.PlayerService._player.totalScore.AsObservable().SubscribeToText(totalScoreText);
    }

    void OnClickCloseButton()
    {
        Services.BackLogService.CloseLastUI();
        Services.AudioService.PlayUIClick();
    }
}
