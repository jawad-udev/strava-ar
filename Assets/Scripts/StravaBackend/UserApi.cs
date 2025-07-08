using System;
using UnityEngine;
using Backend;
using System.Collections.Generic;

public static class UserAPI
{
    private static readonly string BASE_URL = "https://www.strava.com/api/v3/"; 

    public static void ExchangeStravaCode(string code, Action<StravaTokenResponse> callback, Action<string> onError)
    {
        if (string.IsNullOrEmpty(code))
        {
            onError?.Invoke("Code is null or empty.");
            return;
        }

        // Wrap code into a serializable model for correct JSON conversion
        var requestBody = JsonUtility.ToJson(new StravaCodeModel { code = code });

        var request = new RequestMessage
        {
            _requestPath = $"{BASE_URL}/strava/token",
            _requestType = RequestMessage.RequestType.POST,
            _headers = RequestMessage._defaultHeaders,
            _body = requestBody
        };

        // Create and assign a RequestDispatcher on a new GameObject
        var dispatcherGO = new GameObject("StravaTokenRequestDispatcher");
        var dispatcher = dispatcherGO.AddComponent<RequestDispatcher>();

        dispatcher.Request<StravaTokenResponse>(request, response =>
        {
            if (response != null && !string.IsNullOrEmpty(response.access_token))
            {
                callback?.Invoke(response);
            }
            else
            {
                onError?.Invoke("Failed to get a valid token response.");
            }
        });
    }

    public static void FetchStravaActivities(string accessToken, Action<List<StravaActivity>> callback, Action<string> onError)
    {
        var request = new RequestMessage
        {
            _requestPath = $"{BASE_URL}/athlete/activities",
            _requestType = RequestMessage.RequestType.GET,
            _headers = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {accessToken}" }
            }
        };

        var dispatcherGO = new GameObject("StravaActivitiesDispatcher");
        var dispatcher = dispatcherGO.AddComponent<RequestDispatcher>();

        dispatcher.Request<List<StravaActivity>>(request, response =>
        {
            if (response != null)
                callback?.Invoke(response);
            else
                onError?.Invoke("Failed to parse Strava activities");
        });
    }

    [Serializable]
    private class StravaCodeModel
    {
        public string code;
    }
}
