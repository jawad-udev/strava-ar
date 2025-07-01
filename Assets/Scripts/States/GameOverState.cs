using UnityEngine;
using System.Collections;

public class GameOverState : _StatesBase
{

	#region implemented abstract members of _StatesBase
	public override void OnActivate()
	{
		Debug.Log("Game Over State OnActive");

		if (!Services.GameService.isGameActive)
			return;

		Services.GameService.isGameActive = false;

		if (Services.GameService.gameStatus == GameStatus.WON)
		{
			Services.UIService.ActivateUIPopups(Popups.WIN);
			Services.AudioService.PlayWinSound();
		}
		else if (Services.GameService.gameStatus == GameStatus.LOST)
		{
			if (Services.GameService.gameMode == GameMode.SinglePlayer)
			{
				Services.UIService.ActivateUIPopups(Popups.FAIL);
				Services.AudioService.PlayLoseSound();
			}
			else
			{
				Services.UIService.ActivateUIPopups(Popups.LOSE);
				Services.AudioService.PlayLoseSound();
			}
		}

		Services.BackLogService.RemoveLastScreens(1);
		Services.PlayerService.SetHighScore(Services.ScoreService.currentScore);
		Services.PlayerService.SetNumberOfGames(1);
	}

	public override void OnDeactivate()
	{
		Debug.Log("Game Over State OnDeactivate");
		Services.GameService.gameStatus = GameStatus.TOSTART;
		Services.GameService.DestryoGameplayManager();
	}

	public override void OnUpdate()
	{
	}
	#endregion

}