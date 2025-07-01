using UnityEngine;
using System.Collections;

public class SplashState : _StatesBase
{

	#region implemented abstract members of GameState

	public override void OnActivate()
	{
		Debug.Log("Splash State OnActive");

		Services.UIService.ActivateUIScreen(Screens.SPLASH);
		Services.GameService.gameStatus = GameStatus.TOSTART;

	}

	public override void OnDeactivate()
	{
		Debug.Log("Splash State OnDeactivate");
	}

	public override void OnUpdate()
	{

	}

	#endregion
}