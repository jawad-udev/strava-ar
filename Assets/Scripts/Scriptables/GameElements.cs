using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameElements", menuName = "ScriptableObjects/GameElements", order = 1)]
public class GameElements : ScriptableObject
{
    [Header("Canvas")]
    public Canvas globalCanvasPrefab;

    [Header("Services")]
    public Services servicesPrefab;
    public GameService gameService;
    public AudioService audioService;
    public SceneService sceneService;

    [Header("UI Screens")]
    public SplashScreen splashScreen;
    public HomeScreen homeScreen;
    public GamePlayScreen gamePlayScreen;

    [Header("UI Popups")]
    public ProfilePopup profilePopup;
    public SettingsPopup settingsPopup;
    public CommonPopup commonPopup;
    public PausePopup pausePopup;
    public GameWinPopup gameWinPopup;
    public GameLosePopup gameLosePopup;
    public GameFailPopup gameFailPopup;

}