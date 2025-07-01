using UnityEngine;
using System.Collections;

public class GamePauseState : _StatesBase
{

	#region implemented abstract members of GameState

	public override void OnActivate()
	{
		Debug.Log("Game Pause State OnActive");

		Services.UIService.ActivateUIPopups(Popups.PAUSE);
		Services.GameService.gameStatus = GameStatus.PAUSED;
	}

	public override void OnDeactivate()
	{
		Debug.Log("Game Pause State OnDeactivate");
	}

	public override void OnUpdate()
	{
	}

	#endregion
}