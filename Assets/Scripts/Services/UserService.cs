using System;
using System.Collections.Generic;
using UnityEngine;

public class UserService : MonoBehaviour
{
    public void LoginWithStravaCode(string code, Action<StravaTokenResponse> onSuccess, Action<string> onError)
    {
        UserAPI.ExchangeStravaCode(code, tokenResponse =>
        {
            // Save tokens if needed
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

        var request = new Backend.RequestMessage
        {
            _requestPath = "https://www.strava.com/api/v3/athlete/activities",
            _requestType = Backend.RequestMessage.RequestType.GET,
            _headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + accessToken }
            }
        };

        var dispatcherGO = new GameObject("StravaActivitiesDispatcher");
        var dispatcher = dispatcherGO.AddComponent<Backend.RequestDispatcher>();

        dispatcher.Request<StravaActivityListWrapper>(request, response =>
        {
            if (response != null && response.list != null)
                onSuccess?.Invoke(response.list);
            else
                onError?.Invoke("Failed to parse activities");
        });
    }

    
}
