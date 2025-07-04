using System;
using UnityEngine;
using Backend;

public static class UserAPI
{
    private static string BASE_URL = "https://yourapi.com/api/user";

    public static void ExchangeStravaCode(string code, Action<StravaTokenResponse> callback, Action<string> onError)
    {
        var form = new WWWForm();
        form.AddField("code", code);

        var request = new RequestMessage
        {
            _requestPath = $"{BASE_URL}/strava/token", // Your backend endpoint for token exchange
            _requestType = RequestMessage.RequestType.POST,
            _headers = RequestMessage._defaultHeaders,
            _body = JsonUtility.ToJson(new { code = code }) // or send form data as needed
        };

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
                onError?.Invoke("Failed to get token response");
            }
        });
    }

    
}
