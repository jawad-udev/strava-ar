using UniRx;
using System;
using UnityEngine;

public class Player
{
    public ReactiveProperty<string> playerName;
    public ReactiveProperty<int> highScore;
    public ReactiveProperty<int> totalScore;
    public ReactiveProperty<float> timeSpent;
    public ReactiveProperty<int> numberOfGames;
    public ReactiveProperty<int> level;
    public ReactiveProperty<int> coins;
    public ReactiveProperty<bool> isTutorialSeen;

    public Player(string _playerName, int _highScore, int _totalScore, float _timeSpent, int _numberOfGames, int _level, int _coins)
    {
        this.playerName = new ReactiveProperty<string>(_playerName);
        this.highScore = new ReactiveProperty<int>(_highScore);
        this.totalScore = new ReactiveProperty<int>(_totalScore);
        this.timeSpent = new ReactiveProperty<float>(_timeSpent);
        this.numberOfGames = new ReactiveProperty<int>(_numberOfGames);
        this.level = new ReactiveProperty<int>(_level);
        this.coins = new ReactiveProperty<int>(_coins);
        isTutorialSeen = new ReactiveProperty<bool>(false);
    }
}
