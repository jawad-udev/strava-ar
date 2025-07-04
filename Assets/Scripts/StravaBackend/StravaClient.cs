using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class StravaClient : MonoBehaviour
{
    private const string clientId = "166612";
    private const string clientSecret = "dff62ecbb731ba53b61c0436b9334af6348c93f2";
    private const string redirectUri = "http://localhost"; // For Editor testing only
    private const string baseUrl = "https://www.strava.com/api/v3/";

    public static StravaClient Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Build Strava OAuth login URL
    public string GetLoginUrl()
    {
        string scope = "read,activity:read_all";
        return $"https://www.strava.com/oauth/authorize?client_id={clientId}" +
               $"&response_type=code&redirect_uri={redirectUri}&scope={scope}&approval_prompt=auto";
    }

    // Exchange authorization code for access token
    public void ExchangeCodeForToken(string code, Action<StravaTokenResponse> onSuccess, Action<string> onError)
    {
        StartCoroutine(TokenExchangeCoroutine(code, onSuccess, onError));
    }

    private IEnumerator TokenExchangeCoroutine(string code, Action<StravaTokenResponse> onSuccess, Action<string> onError)
    {
        WWWForm form = new WWWForm();
        form.AddField("client_id", clientId);
        form.AddField("client_secret", clientSecret);
        form.AddField("code", code);
        form.AddField("grant_type", "authorization_code");

        using (UnityWebRequest req = UnityWebRequest.Post("https://www.strava.com/oauth/token", form))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var token = JsonUtility.FromJson<StravaTokenResponse>(req.downloadHandler.text);
                PlayerPrefs.SetString("strava_access_token", token.access_token);
                PlayerPrefs.SetString("strava_refresh_token", token.refresh_token);
                onSuccess?.Invoke(token);
            }
            else
            {
                onError?.Invoke("Token exchange failed: " + req.error);
            }
        }
    }

    // Fetch user's activities from Strava API
    public void FetchActivities(Action<List<StravaActivity>> onSuccess, Action<string> onError)
    {
        StartCoroutine(FetchActivitiesCoroutine(onSuccess, onError));
    }

    private IEnumerator FetchActivitiesCoroutine(Action<List<StravaActivity>> onSuccess, Action<string> onError)
    {
        string accessToken = PlayerPrefs.GetString("strava_access_token", "");
        if (string.IsNullOrEmpty(accessToken))
        {
            onError?.Invoke("Access token missing.");
            yield break;
        }

        UnityWebRequest req = UnityWebRequest.Get(baseUrl + "athlete/activities");
        req.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            try
            {
                // Wrap response for JsonUtility parsing
                string wrappedJson = "{\"list\":" + req.downloadHandler.text + "}";
                var wrapper = JsonUtility.FromJson<StravaActivityListWrapper>(wrappedJson);
                onSuccess?.Invoke(wrapper.list);
            }
            catch (Exception ex)
            {
                onError?.Invoke("Failed parsing JSON: " + ex.Message);
            }
        }
        else
        {
            onError?.Invoke("API error: " + req.error);
        }
    }
}
