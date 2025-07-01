using UnityEngine;
using System;
using System.Collections.Generic;

public class GameService : MonoBehaviour
{
	public GameMode gameMode;
	public GameStatus gameStatus;
	public bool isGameActive;
	public ColorService colorService;

	public GameManager gameManager;
	public float gameTime;

	public void Start()
	{
		isGameActive = false;
		SetState<SplashState>();
	}

	private _StatesBase currentState;
	public _StatesBase State
	{
		get { return currentState; }
	}

	//Changes the current game state
	public void SetState(System.Type newStateType)
	{
		if (currentState != null)
		{
			currentState.OnDeactivate();
		}

		currentState = GetComponentInChildren(newStateType) as _StatesBase;
		if (currentState != null)
		{
			currentState.OnActivate();
		}
	}

	public void SetState<T>()
	{
		if (currentState != null)
		{
			currentState.OnDeactivate();
		}

		currentState = GetComponentInChildren(typeof(T)) as _StatesBase;
		if (currentState != null)
		{
			currentState.OnActivate();
		}
	}

	void Update()
	{
		if (currentState != null)
		{
			currentState.OnUpdate();
		}
	}

	#region GamePlay Managers

	public void StartGame()
    {
		Services.SceneService.LoadGameScene();
		ResetGameTime();
        Services.AudioService.RestartGameMusic();
	}

	//Incase of spawning game object in the same scene
	public GameManager SpawnGamePlay(GameManager _gameplayManager)
	{
		GameManager gameplayManager = Instantiate(_gameplayManager);
		gameplayManager.gameObject.name = "GameManager";
		Services.CameraService.AssignPlayerCamera(gameplayManager);
		gameplayManager.Init();

		return gameplayManager;
	}

	public GameManager GetPlayerManager(GameplayOwner gameplayOwner)
	{
		return gameManager;
	}

	public void DestryoGameplayManager()
	{
		if(Services.GameService.gameManager.gameObject)
			DestroyImmediate(Services.GameService.gameManager.gameObject);
	}

	public void SetGameTime(float time)
    {
		gameTime += time;
    }

	public float GetGameTime() {
		return gameTime;
	}

    private void ResetGameTime()
    {
		gameTime = 0;
    }
    #endregion


	#region Game Finisher

	public void OnGameFinish(bool isWin)
	{
		if (isWin)
		{
			gameStatus = GameStatus.WON;

			Services.CameraService.ZoomOut(gameManager, () =>
			{
				Extensions.PerformActionWithDelay(this, 2f, SetState<GameOverState>);
			});
		}

		if (!isWin)
		{
			gameStatus = GameStatus.LOST;

			SetState<GameOverState>();
		}
	}

    #endregion
}