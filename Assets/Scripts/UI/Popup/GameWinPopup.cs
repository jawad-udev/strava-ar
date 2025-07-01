using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UniRx;

public class GameWinPopup : GameMonoBehaviour
{
    public Button restartButton, homeButton;
    public TextMeshProUGUI coinText, scoreText, timeText, ShapesPlacedText, titleText;

    void Awake()
    {
        restartButton.onClick.AsObservable().Subscribe(x => OnClickRestartButton());
        homeButton.onClick.AsObservable().Subscribe(x => OnClickHomeButton());
    }

    private void OnEnable()
    {
        GiftCoin();
        UpdateGameTime();
        UpdateGameScore();
        UpdateGameShapes();
        SetTitleText();
    }

    void OnClickRestartButton()
    {
        Services.GameService.gameStatus = GameStatus.TOSTART;
        Services.GameService.DestryoGameplayManager();
        Services.GameService.SetState<GamePlayState>();
        Services.AudioService.PlayUIClick();
        this.Hide();
    }

    void OnClickHomeButton()
    {
        Services.GameService.SetState<MenuState>();
        Services.AudioService.PlayUIClick();
        this.Hide();
    }

    void GiftCoin()
    {
        int coins = Random.Range(100, 500);
        coinText.SetText("+" + coins.ToString());
        Services.PlayerService.SetCoins(coins);
    }

    void UpdateGameTime()
    {
        float time = Services.GameService.GetGameTime();
        timeText.SetText(Mathf.FloorToInt(time).ToString() + " Secs");
    }

    void UpdateGameShapes()
    {
    }

    void UpdateGameScore()
    {
        int score = 0;

        scoreText.SetText(score.ToString());
    }

    void SetTitleText()
    {
        if(Services.GameService.gameMode == GameMode.SinglePlayer)
        {
            titleText.SetText("TOWER COMPLETED");
        }
        else if (Services.GameService.gameMode == GameMode.MultiPlayer)
        {
            titleText.SetText("YOU WON");
        }
    }
}
