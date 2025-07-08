using UnityEngine;
using System.Collections;

public class MenuState : _StatesBase
{

	#region implemented abstract members of GameState

	public override void OnActivate()
	{
		Debug.Log("Menu State OnActive");
		Services.BackLogService.DisableAndremoveAllScreens();
		Services.UIService.ActivateUIScreen(Screens.LOGIN);
		Services.GameService.gameStatus = GameStatus.TOSTART;
		Services.CameraService.ResetCameraSize();
		Services.AudioService.PlayGameMusic();

		Services.SceneService.LoadMainScene();

	}

	public override void OnDeactivate()
	{
		Debug.Log("Menu State OnDeactivate");
		//Services.AudioService.StopGameMusic();
	}

	public override void OnUpdate()
	{

	}

	#endregion
}