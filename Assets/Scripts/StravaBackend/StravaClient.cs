// StravaClient.cs
#define USE_EDITOR_REDIRECT // Comment this out for Android build

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public static class StravaClient
{
    public const string clientId = "166612";
    public const string clientSecret = "dff62ecbb731ba53b61c0436b9334af6348c93f2";

#if USE_EDITOR_REDIRECT
    private const string redirectUri = "http://localhost/exchange_token";
#else
    private const string redirectUri = "myapp://strava.auth";
#endif

    private const string baseUrl = "https://www.strava.com/api/v3/";

    public static string GetLoginUrl()
    {
        return $"https://www.strava.com/oauth/authorize" +
               $"?client_id={clientId}" +
               $"&response_type=code" +
               $"&redirect_uri={redirectUri}" +
               $"&scope=read,activity:read_all" +
               $"&approval_prompt=auto";
    }

    public static void ExchangeCodeForToken(string code, Action onSuccess, Action<string> onError)
    {
        CoroutineRunner.Instance.StartCoroutine(TokenCoroutine(code, onSuccess, onError));
    }

    private static IEnumerator TokenCoroutine(string code, Action onSuccess, Action<string> onError)
    {
        WWWForm form = new WWWForm();
        form.AddField("client_id", clientId);
        form.AddField("client_secret", clientSecret);
        form.AddField("code", code);
        form.AddField("grant_type", "authorization_code");

        using (UnityWebRequest request = UnityWebRequest.Post("https://www.strava.com/oauth/token", form))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var json = request.downloadHandler.text;
                var token = JsonConvert.DeserializeObject<StravaTokenResponse>(json);
                SaveTokenData(token);
                Debug.Log("Token Success");
                onSuccess?.Invoke();
            }
            else
            {
                Debug.LogError("Token Error: " + request.error);
                onError?.Invoke("Token Error: " + request.error);
            }
        }
    }

    private static void SaveTokenData(StravaTokenResponse token)
    {
        PlayerPrefs.SetString("strava_access_token", token.access_token);
        PlayerPrefs.SetString("strava_refresh_token", token.refresh_token);
        int expiry = (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond) + token.expires_in;
        PlayerPrefs.SetInt("strava_token_expiry", expiry);
    }

    public static void RefreshToken(Action onSuccess, Action<string> onError)
    {
        string refreshToken = PlayerPrefs.GetString("strava_refresh_token", "");
        if (string.IsNullOrEmpty(refreshToken))
        {
            onError?.Invoke("No refresh token found.");
            return;
        }

        CoroutineRunner.Instance.StartCoroutine(RefreshTokenCoroutine(refreshToken, onSuccess, onError));
    }

    private static IEnumerator RefreshTokenCoroutine(string refreshToken, Action onSuccess, Action<string> onError)
    {
        WWWForm form = new WWWForm();
        form.AddField("client_id", clientId);
        form.AddField("client_secret", clientSecret);
        form.AddField("grant_type", "refresh_token");
        form.AddField("refresh_token", refreshToken);

        using (UnityWebRequest request = UnityWebRequest.Post("https://www.strava.com/oauth/token", form))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var token = JsonConvert.DeserializeObject<StravaTokenResponse>(request.downloadHandler.text);
                SaveTokenData(token);
                onSuccess?.Invoke();
            }
            else
            {
                onError?.Invoke("Refresh Error: " + request.error);
            }
        }
    }

    private static bool IsTokenExpired()
    {
        int expiryTime = PlayerPrefs.GetInt("strava_token_expiry", 0);
        int currentTime = (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond);
        return currentTime >= expiryTime;
    }

    public static void FetchActivities(Action<List<StravaActivity>> onSuccess, Action<string> onError)
    {
        if (IsTokenExpired())
        {
            Debug.Log("Token expired. Refreshing...");
            RefreshToken(
                () => CoroutineRunner.Instance.StartCoroutine(FetchActivitiesCoroutine(onSuccess, onError)),
                error => onError?.Invoke("Token Refresh Failed: " + error)
            );
        }
        else
        {
            CoroutineRunner.Instance.StartCoroutine(FetchActivitiesCoroutine(onSuccess, onError));
        }
    }

    private static IEnumerator FetchActivitiesCoroutine(Action<List<StravaActivity>> onSuccess, Action<string> onError)
    {
        string accessToken = PlayerPrefs.GetString("strava_access_token", "");
        UnityWebRequest req = UnityWebRequest.Get($"{baseUrl}athlete/activities?per_page=50");
        req.SetRequestHeader("Authorization", $"Bearer {accessToken}");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string json = req.downloadHandler.text;

            if (string.IsNullOrEmpty(json) || json == "[]")
            {
                Debug.LogWarning("No activities found.");
                onSuccess?.Invoke(new List<StravaActivity>());
                yield break;
            }

            try
            {
                var activities = JsonConvert.DeserializeObject<List<StravaActivity>>(json);
                onSuccess?.Invoke(activities);
            }
            catch (Exception ex)
            {
                Debug.LogError("Activity Parse Error: " + ex.Message);
                onError?.Invoke("Failed to parse activities.");
            }
        }
        else
        {
            onError?.Invoke("Fetch Error: " + req.error);
        }
    }

    public static void FetchActivityDetail(long activityId, Action<StravaActivityDetail> onSuccess, Action<string> onError)
    {
        CoroutineRunner.Instance.StartCoroutine(FetchActivityDetailCoroutine(activityId, onSuccess, onError));
    }

    private static IEnumerator FetchActivityDetailCoroutine(long activityId, Action<StravaActivityDetail> onSuccess, Action<string> onError)
    {
        string accessToken = PlayerPrefs.GetString("strava_access_token", "");

        UnityWebRequest req = UnityWebRequest.Get($"{baseUrl}activities/{activityId}");
        req.SetRequestHeader("Authorization", $"Bearer {accessToken}");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            try
            {
                var activityDetail = JsonConvert.DeserializeObject<StravaActivityDetail>(req.downloadHandler.text);
                onSuccess?.Invoke(activityDetail);
            }
            catch (Exception ex)
            {
                Debug.LogError("Detail Parse Error: " + ex.Message);
                onError?.Invoke("Failed to parse activity details.");
            }
        }
        else
        {
            Debug.LogError("Fetch detail failed: " + req.error);
            onError?.Invoke("Activity detail fetch failed.");
        }
    }
}
