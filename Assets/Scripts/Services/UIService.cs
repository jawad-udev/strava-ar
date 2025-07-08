using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIService
{
    #region UI Screens

    public void ActivateUIScreen(Screens screenType)
    {
        if (screenType.Equals(Screens.SPLASH))
        {
            SplashScreen.Show();
        }
        else if (screenType.Equals(Screens.HOME))
        {
            HomeScreen.Show();
        }
        else if (screenType.Equals(Screens.PLAY))
        {
            GamePlayScreen.Show();
        }
        else if (screenType.Equals(Screens.LOGIN))
        {
            LoginScreen.Show();
        }
    }

    public void ActivateUIPopups(Popups popupType)
    {
        if (popupType.Equals(Popups.PROFILE))
        {
            ProfilePopup.Show(BacklogType.KeepPreviousScreen);
        }
        else if (popupType.Equals(Popups.SETTINGS))
        {
            SettingsPopup.Show(BacklogType.KeepPreviousScreen);
        }
        else if (popupType.Equals(Popups.PAUSE))
        {
            PausePopup.Show(BacklogType.KeepPreviousScreen);
        }
        else if (popupType.Equals(Popups.WIN))
        {
            GameWinPopup.Show(BacklogType.KeepPreviousScreen);
        }
        else if (popupType.Equals(Popups.LOSE))
        {
            GameLosePopup.Show(BacklogType.KeepPreviousScreen);
        }
        else if (popupType.Equals(Popups.FAIL))
        {
            GameFailPopup.Show(BacklogType.KeepPreviousScreen);
        }
    }

    [Inject]
    private SplashScreen _splashScreen;
    public SplashScreen SplashScreen
    {
        get
        {
            return _splashScreen;
        }
    }

    [Inject]
    private HomeScreen _homeScreen;
    public HomeScreen HomeScreen
    {
        get
        {
            return _homeScreen;
        }
    }

    [Inject]
    private GamePlayScreen _gamePlayScreen;
    public GamePlayScreen GamePlayScreen
    {
        get
        {
            return _gamePlayScreen;
        }
    }

    [Inject]
    private LoginScreen _loginScreen;
    public LoginScreen LoginScreen
    {
        get
        {
            return _loginScreen;
        }
    }

    #endregion

    #region UI Popups

    [Inject]
    private ProfilePopup _profilePopup;
    public ProfilePopup ProfilePopup
    {
        get
        {
            return _profilePopup;
        }
    }

    [Inject]
    private SettingsPopup _settingsPopup;
    public SettingsPopup SettingsPopup
    {
        get
        {
            return _settingsPopup;
        }
    }

    [Inject]
    private CommonPopup _commonPopup;
    public CommonPopup CommonPopup
    {
        get
        {
            return _commonPopup;
        }
    }

    [Inject]
    private PausePopup _pausePopup;
    public PausePopup PausePopup
    {
        get
        {
            return _pausePopup;
        }
    }

    [Inject]
    private GameWinPopup _gameWinPopup;
    public GameWinPopup GameWinPopup
    {
        get
        {
            return _gameWinPopup;
        }
    }

    [Inject]
    private GameFailPopup _gameFailPopup;
    public GameFailPopup GameFailPopup
    {
        get
        {
            return _gameFailPopup;
        }
    }

    [Inject]
    private GameLosePopup _gameLosePopup;
    public GameLosePopup GameLosePopup
    {
        get
        {
            return _gameLosePopup;
        }
    }
    #endregion
}