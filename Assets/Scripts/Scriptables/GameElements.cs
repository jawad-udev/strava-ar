using UnityEngine;

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
    public UserService userService;

    [Header("UI Screens")]
    public SplashScreen splashScreen;
    public HomeScreen homeScreen;
    public GamePlayScreen gamePlayScreen;
    public StravaLogin loginScreen;

    [Header("UI Popups")]
    public ProfilePopup profilePopup;
    public SettingsPopup settingsPopup;
    public CommonPopup commonPopup;
    public PausePopup pausePopup;
    public GameWinPopup gameWinPopup;
    public GameLosePopup gameLosePopup;
    public GameFailPopup gameFailPopup;

}