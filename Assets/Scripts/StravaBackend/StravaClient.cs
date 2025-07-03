using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class StravaClient : SingletonMonobehaviour<StravaClient>
{
    public string clientId = "166612";
    public string clientSecret = "dff62ecbb731ba53b61c0436b9334af6348c93f2";

    private string baseUrl = "https://www.strava.com/api/v3/";
    public string AccessToken => PlayerPrefs.GetString("strava_access_token", "");

    public static StravaClient Instance;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public string GetLoginUrl()
    {
        string redirectUri = "http://localhost"; // Or your mobile scheme
        string scope = "read,activity:read_all";

        return $"https://www.strava.com/oauth/authorize" +
            $"?client_id={clientId}" +
            $"&response_type=code" +
            $"&redirect_uri={redirectUri}" +
            $"&scope={scope}" +
            $"&approval_prompt=auto";
    }

    //  Exchange Code for Access Token
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
                var json = request.downloadHandler.text;
                Debug.Log(" Token Response: " + json);
                var tokenResponse = JsonUtility.FromJson<StravaTokenResponse>(json);

                PlayerPrefs.SetString("strava_access_token", tokenResponse.access_token);
                PlayerPrefs.SetString("strava_refresh_token", tokenResponse.refresh_token);
                onSuccess?.Invoke(json);
            }
            else
            {
                Debug.LogError(" Token Error: " + request.error);
                onError?.Invoke(request.error);
            }
        }
    }

    //  Fetch User Activities
    public void FetchActivities(Action<List<StravaActivity>> onSuccess, Action<string> onError)
    {
        StartCoroutine(GetActivitiesCoroutine(onSuccess, onError));
    }

    private IEnumerator GetActivitiesCoroutine(Action<List<StravaActivity>> onSuccess, Action<string> onError)
    {
        string url = baseUrl + "athlete/activities";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + AccessToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                string wrapped = "{\"list\":" + request.downloadHandler.text + "}";
                StravaActivityList activityList = JsonUtility.FromJson<StravaActivityList>(wrapped);
                onSuccess?.Invoke(activityList.list);
            }
            catch (Exception e)
            {
                onError?.Invoke("JSON parse error: " + e.Message);
            }
        }
        else
        {
            onError?.Invoke("API error: " + request.error);
        }
    }
}

[Serializable]
public class StravaTokenResponse
{
    public string token_type;
    public string access_token;
    public string refresh_token;
    public int expires_at;
    public int expires_in;
    public Athlete athlete;
}

[Serializable]
public class Athlete
{
    public string username;
    public string firstname;
    public string lastname;
}

[Serializable]
public class StravaActivity
{
    public long id;
    public string name;
    public float distance;
    public int moving_time;
    public string start_date;
    public StravaMap map;
}

[Serializable]
public class StravaMap
{
    public string summary_polyline;
}

[Serializable]
public class StravaActivityList
{
    public List<StravaActivity> list;
}