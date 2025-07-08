using System;
using System.Collections.Generic;
using UnityEngine;

public class UserService:MonoBehaviour
{
    public void LoginWithStravaCode(string code, Action<StravaTokenResponse> onSuccess, Action<string> onError)
    {
        UserAPI.ExchangeStravaCode(code, tokenResponse =>
        {
            PlayerPrefs.SetString("strava_access_token", tokenResponse.access_token);
            PlayerPrefs.SetString("strava_refresh_token", tokenResponse.refresh_token);

            onSuccess?.Invoke(tokenResponse);

        }, err => onError?.Invoke(err));
    }

    public void FetchStravaActivities(Action<List<StravaActivity>> onSuccess, Action<string> onError)
    {
        string accessToken = PlayerPrefs.GetString("strava_access_token", "");
        if (string.IsNullOrEmpty(accessToken))
        {
            onError?.Invoke("Missing access token.");
            return;
        }

        UserAPI.FetchStravaActivities(accessToken, onSuccess, onError);
    }
}
