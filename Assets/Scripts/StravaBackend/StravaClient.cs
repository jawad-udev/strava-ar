#define USE_EDITOR_REDIRECT

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public static class StravaClient
{
    //  public const string clientId = "166612";
    //  public const string clientSecret = "dff62ecbb731ba53b61c0436b9334af6348c93f2";
    public const string clientId = "165849";
    public const string clientSecret = "e275c5b5ea57bba3a71d53e1793eacb649f482e5";

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
        string fullUrl = baseUrl + endpoint;

        using UnityWebRequest req = UnityWebRequest.Get(fullUrl);
        req.SetRequestHeader("Authorization", $"Bearer {accessToken}");

        yield return req.SendWebRequest();

        if (req.responseCode == 401)
        {
            Debug.LogWarning(" Access token unauthorized (401). Attempting refresh...");
            RefreshToken(() =>
            {
                CoroutineRunner.Instance.StartCoroutine(FetchCoroutine(endpoint, onSuccess, onError));
            }, error =>
            {
                onError?.Invoke("Token refresh failed: " + error);
            });
            yield break;
        }

        if (req.result == UnityWebRequest.Result.Success)
        {
            string json = req.downloadHandler.text;

            if (string.IsNullOrEmpty(json))
            {
                onError?.Invoke("Empty response from server.");
                yield break;
            }

            try
            {
                T data = JsonConvert.DeserializeObject<T>(json);
                if (data == null)
                {
                    onError?.Invoke("Parsed data is null. Model mismatch?");
                    yield break;
                }

                onSuccess?.Invoke(data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSON parse error: {ex.Message}\nResponse:\n{json}");
                onError?.Invoke("Parse error: " + ex.Message);
            }
        }
        else
        {
            onError?.Invoke($"Fetch failed: {req.error} (HTTP {req.responseCode})");
        }
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
    public static void FetchActivityStreams(long activityId, Action<StravaStreamResponse> onSuccess, Action<string> onError) =>
        Get($"activities/{activityId}/streams?keys=time,distance,latlng,altitude,velocity_smooth,heartrate&key_by_type=true",
            onSuccess, onError);

    public static void CreateNewRunActivity()
    {

    }

}
