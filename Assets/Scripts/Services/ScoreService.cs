using UnityEngine;
using System.Collections;

public class ScoreService
{
    public int currentScore = 0;

    public void OnScore(GameplayOwner owner, int scoreIncreaseAmount)
    {
        if (owner == GameplayOwner.Player1)
        {
            currentScore += scoreIncreaseAmount;
            CheckHighScore();
            Services.PlayerService.SetTotalScore(Services.PlayerService.GetTotalScore() + scoreIncreaseAmount);
        }
        else if (owner == GameplayOwner.Player2)
        {
            // Player-2 score can be handle here
        }
    }

    public void CheckHighScore()
    {
        if (Services.PlayerService.GetHighScore() < currentScore)
        {
            Services.PlayerService.SetHighScore(currentScore);
        }
    }

    public void ResetScore()
    {
        currentScore = 0;
    }
}