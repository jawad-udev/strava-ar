using UnityEngine;
using TMPro;

public class StravaLogin : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI statusText;
    public TMP_InputField codeInputField;

    // Called by "Login" button to open Strava login URL
    public void OnClickLogin()
    {
        string url = StravaClient.Instance.GetLoginUrl();
        Application.OpenURL(url);
        SetStatus("Opening Strava login page...");
    }

    // Called by "Submit Code" button after user pastes auth code
    public void OnClickSubmitCode()
    {
        string code = codeInputField.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            SetStatus("Please enter a valid code.");
            return;
        }

        SetStatus("Authenticating...");

        StravaClient.Instance.ExchangeCodeForToken(code, 
            onSuccess: (json) =>
            {
                SetStatus("Strava login successful!");
                Debug.Log("Token Exchange Response: " + json);
            },
            onError: (error) =>
            {
                SetStatus("Login failed: " + error);
                Debug.LogError("Token Error: " + error);
            });
    }

    private void SetStatus(string msg)
    {
        if (statusText != null)
            statusText.text = msg;
    }
}
