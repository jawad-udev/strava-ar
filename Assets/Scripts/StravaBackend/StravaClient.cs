#define USE_EDITOR_REDIRECT

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

    // ---------------- AUTH ----------------
    public static string GetLoginUrl() =>
        $"https://www.strava.com/oauth/authorize" +
        $"?client_id={clientId}&response_type=code" +
        $"&redirect_uri={redirectUri}&scope=read,activity:read_all&approval_prompt=auto";

    public static void ExchangeCodeForToken(string code, Action onSuccess, Action<string> onError) =>
        CoroutineRunner.Instance.StartCoroutine(TokenCoroutine(code, onSuccess, onError));

    private static IEnumerator TokenCoroutine(string code, Action onSuccess, Action<string> onError)
    {
        WWWForm form = new WWWForm();
        form.AddField("client_id", clientId);
        form.AddField("client_secret", clientSecret);
        form.AddField("code", code);
        form.AddField("grant_type", "authorization_code");

        using UnityWebRequest req = UnityWebRequest.Post("https://www.strava.com/oauth/token", form);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var token = JsonConvert.DeserializeObject<StravaTokenResponse>(req.downloadHandler.text);
            SaveTokenData(token);
            onSuccess?.Invoke();
        }
        else onError?.Invoke("Token Error: " + req.error);
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

        using UnityWebRequest req = UnityWebRequest.Post("https://www.strava.com/oauth/token", form);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var token = JsonConvert.DeserializeObject<StravaTokenResponse>(req.downloadHandler.text);
            SaveTokenData(token);
            onSuccess?.Invoke();
        }
        else onError?.Invoke("Refresh Error: " + req.error);
    }

    private static void SaveTokenData(StravaTokenResponse token)
    {
        PlayerPrefs.SetString("strava_access_token", token.access_token);
        PlayerPrefs.SetString("strava_refresh_token", token.refresh_token);
        int expiry = (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond) + token.expires_in;
        PlayerPrefs.SetInt("strava_token_expiry", expiry);
    }

    private static bool IsTokenExpired()
    {
        int expiry = PlayerPrefs.GetInt("strava_token_expiry", 0);
        int now = (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond);
        return now >= expiry;
    }

    // ---------------- GENERIC FETCH ----------------
    private static void Get<T>(string endpoint, Action<T> onSuccess, Action<string> onError)
    {
        void Run() => CoroutineRunner.Instance.StartCoroutine(FetchCoroutine(endpoint, onSuccess, onError));
        if (IsTokenExpired()) RefreshToken(Run, onError);
        else Run();
    }

    private static IEnumerator FetchCoroutine<T>(string endpoint, Action<T> onSuccess, Action<string> onError)
    {
        string accessToken = PlayerPrefs.GetString("strava_access_token", "");
        using UnityWebRequest req = UnityWebRequest.Get(baseUrl + endpoint);
        req.SetRequestHeader("Authorization", $"Bearer {accessToken}");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            try
            {
                T data = JsonConvert.DeserializeObject<T>(req.downloadHandler.text);
                onSuccess?.Invoke(data);
            }
            catch (Exception ex) { onError?.Invoke("Parse error: " + ex.Message); }
        }
        else onError?.Invoke("Fetch failed: " + req.error);
    }

    // ---------------- ENDPOINT WRAPPERS ----------------
    public static void FetchActivities(Action<List<StravaActivity>> onSuccess, Action<string> onError) =>
        Get("athlete/activities?per_page=50", onSuccess, onError);

    public static void FetchActivityDetail(long activityId, Action<StravaActivityDetail> onSuccess, Action<string> onError) =>
        Get($"activities/{activityId}", onSuccess, onError);

    public static void FetchActivityLaps(long activityId, Action<List<StravaLap>> onSuccess, Action<string> onError) =>
        Get($"activities/{activityId}/laps", onSuccess, onError);

    public static void FetchActivityZones(long activityId, Action<List<StravaZone>> onSuccess, Action<string> onError) =>
        Get($"activities/{activityId}/zones", onSuccess, onError);

    public static void FetchActivityPhotos(long activityId, Action<List<StravaPhoto>> onSuccess, Action<string> onError) =>
        Get($"activities/{activityId}/photos", onSuccess, onError);

    public static void FetchAthlete(Action<StravaAthlete> onSuccess, Action<string> onError) =>
        Get("athlete", onSuccess, onError);

    public static void FetchAthleteStats(long athleteId, Action<StravaStats> onSuccess, Action<string> onError) =>
        Get($"athletes/{athleteId}/stats", onSuccess, onError);

    public static void FetchAthleteHeartRateZones(Action<StravaUserZones> onSuccess, Action<string> onError) =>
        Get("athlete/zones", onSuccess, onError);
}
