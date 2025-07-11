using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class LoginScreen : GameMonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI statusText;
    public TMP_InputField codeInputField;
    public Button loginBtn, submitCodeBtn;
    public Button backButton;



    private void Start()
    {
        loginBtn.onClick.AddListener(OnClickLogin);
        submitCodeBtn.onClick.AddListener(OnClickSubmitCode);
        if (Services.UserService.IsUserAuthenticated())
        {
            LoginSuccesss();
        }
    }

    private void OnClickLogin()
    {
        Application.OpenURL(StravaClient.GetLoginUrl());
        SetStatus("Awaiting Strava authentication...");
    }

    private void OnClickSubmitCode()
    {
        string code = codeInputField.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            SetStatus("Please enter a valid code");
            return;
        }

        SetStatus("Authenticating...");
        SetUIInteractable(false);

        Services.UserService.Login(code,
            onSuccess: () =>
            {
                SetStatus("Authentication successful!");
                SetUIInteractable(true);
                LoginSuccesss();
            },
            onError: error =>
            {
                SetStatus($"Login Error: {error}");
                Debug.LogError(error);
                SetUIInteractable(true);
            });
    }



    private void SetStatus(string message)
    {
        if (statusText) statusText.text = message;
    }

    private void SetUIInteractable(bool interactable)
    {
        loginBtn.interactable = interactable;
        submitCodeBtn.interactable = interactable;
        codeInputField.interactable = interactable;
    }

    private void LoginSuccesss()
    {
        Services.UIService.ActivateUIScreen(Screens.PLAY);
    }
}
