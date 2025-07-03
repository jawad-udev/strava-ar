using UnityEngine;
using TMPro;

public class StravaLogin : GameMonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI statusText;
    public TMP_InputField codeInputField;

    public void OnLoginButtonPressed()
    {
        string url = StravaClient.Instance.GetLoginUrl();
        Application.OpenURL(url);
        ShowStatus(" Opening Strava login page...");
    }

    public void OnSubmitCodePressed()
    {
        string code = codeInputField.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            ShowStatus(" Please enter a code.");
            return;
        }

        ShowStatus(" Exchanging code for token...");
        StravaClient.Instance.ExchangeCodeForToken(code,
            onSuccess: _ => ShowStatus(" Login successful!"),
            onError: err => ShowStatus(" Login failed: " + err));
    }

    private void ShowStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}
