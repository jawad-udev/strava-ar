#define USE_EDITOR_REDIRECT // Comment this out when testing on Android

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class StravaClient : MonoBehaviour
{
    public const string clientId = "166612";
    public const string clientSecret = "dff62ecbb731ba53b61c0436b9334af6348c93f2";

#if USE_EDITOR_REDIRECT
    private const string redirectUri = "http://localhost/exchange_token"; // For Unity Editor Testing
#else
    private const string redirectUri = "myapp://strava.auth"; // For Android deep link
#endif

    private const string baseUrl = "https://www.strava.com/api/v3/";

    public static StravaClient Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public string GetLoginUrl()
    {
        string scope = "read,activity:read_all";
        return $"https://www.strava.com/oauth/authorize" +
               $"?client_id={clientId}" +
               $"&response_type=code" +
               $"&redirect_uri={redirectUri}" +
               $"&scope={scope}" +
               $"&approval_prompt=auto";
    }

    public void ExchangeCodeForToken(string code, Action<string> onSuccess, Action<string> onError)
    {
        StartCoroutine(TokenCoroutine(code, onSuccess, onError));
    }

    private IEnumerator TokenCoroutine(string code, Action<string> onSuccess, Action<string> onError)
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
                Debug.Log("Token Success: " + request.downloadHandler.text);
                var token = JsonUtility.FromJson<StravaTokenResponse>(request.downloadHandler.text);
                SaveTokenData(token);
                onSuccess?.Invoke(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Token Error: " + request.error);
                onError?.Invoke($"Token Error: {request.error}");
            }
        }
    }

    public void RefreshToken(Action onSuccess, Action<string> onError)
    {
        string refreshToken = PlayerPrefs.GetString("strava_refresh_token");
        if (string.IsNullOrEmpty(refreshToken))
        {
            onError?.Invoke("No refresh token found.");
            return;
        }

        StartCoroutine(RefreshTokenCoroutine(refreshToken, onSuccess, onError));
    }

    private IEnumerator RefreshTokenCoroutine(string refreshToken, Action onSuccess, Action<string> onError)
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
                Debug.Log("Token Refresh Success: " + request.downloadHandler.text);
                var token = JsonUtility.FromJson<StravaTokenResponse>(request.downloadHandler.text);
                SaveTokenData(token);
                onSuccess?.Invoke();
            }
            else
            {
                Debug.LogError("Refresh Error: " + request.error);
                onError?.Invoke($"Refresh Error: {request.error}");
            }
        }
    }

    private void SaveTokenData(StravaTokenResponse token)
    {
        PlayerPrefs.SetString("strava_access_token", token.access_token);
        PlayerPrefs.SetString("strava_refresh_token", token.refresh_token);
        int expiry = (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond) + token.expires_in;
        PlayerPrefs.SetInt("strava_token_expiry", expiry);
    }

    private bool IsTokenExpired()
    {
        int expiryTime = PlayerPrefs.GetInt("strava_token_expiry", 0);
        int currentTime = (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond);
        return currentTime >= expiryTime;
    }

    public void FetchActivities(Action<List<StravaActivity>> onSuccess, Action<string> onError)
    {
        if (IsTokenExpired())
        {
            Debug.Log("Token expired. Refreshing...");
            RefreshToken(
                () => StartCoroutine(FetchActivitiesCoroutine(onSuccess, onError)),
                error => onError?.Invoke($"Token Refresh Failed: {error}")
            );
        }
        else
        {
            StartCoroutine(FetchActivitiesCoroutine(onSuccess, onError));
        }
    }

    private IEnumerator FetchActivitiesCoroutine(Action<List<StravaActivity>> onSuccess, Action<string> onError)
    {
        string accessToken = PlayerPrefs.GetString("strava_access_token");
        UnityWebRequest req = UnityWebRequest.Get($"{baseUrl}athlete/activities?per_page=50");
        req.SetRequestHeader("Authorization", $"Bearer {accessToken}");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string rawJson = req.downloadHandler.text;

            if (string.IsNullOrEmpty(rawJson) || rawJson == "[]")
            {
                Debug.LogWarning("No activities found.");
                onSuccess?.Invoke(new List<StravaActivity>());
                yield break;
            }

            try
            {
                // Wrap raw JSON array for JsonUtility
                string wrappedJson = $"{{\"activities\":{rawJson}}}";
                var wrapper = JsonUtility.FromJson<StravaActivityWrapper>(wrappedJson);

                if (wrapper?.activities != null)
                {
                    onSuccess?.Invoke(wrapper.activities);
                }
                else
                {
                    onError?.Invoke("Parsed wrapper or activities is null");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Activity Parse Error: " + ex.Message);
                onError?.Invoke("Failed to parse activities.");
            }
        }
        else
        {
            onError?.Invoke($"Activity Fetch Error: {req.error}");
        }
    }

}
