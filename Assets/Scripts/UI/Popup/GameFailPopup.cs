using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UniRx;

public class GameFailPopup : GameMonoBehaviour
{
    public Button restartButton, homeButton;
    public TextMeshProUGUI coinText;

    void Awake()
    {
        restartButton.onClick.AsObservable().Subscribe(x => OnClickRestartButton());
        homeButton.onClick.AsObservable().Subscribe(x => OnClickHomeButton());
    }

    private void OnEnable()
    {
        DeductCoin();
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

    void DeductCoin()
    {
        int coinsToDeduct = Random.Range(-500,-100);
        coinText.SetText(coinsToDeduct.ToString());
        Services.PlayerService.SetCoins(coinsToDeduct);
    }
}
