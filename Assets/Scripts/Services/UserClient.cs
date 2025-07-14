using System;
using System.Collections.Generic;
using UnityEngine;

public class UserClient : GameMonoBehaviour
{
    public void Login(string code, Action onSuccess, Action<string> onError)
    {
        StravaClient.ExchangeCodeForToken(code,
            () =>
            {
                Debug.Log("User login successful.");
                onSuccess?.Invoke();
            },
            error =>
            {
                Debug.LogError("User login failed: " + error);
                onError?.Invoke(error);
            });
    }

    public void FetchUserActivities(Action<List<StravaActivity>> onSuccess, Action<string> onError)
    {
        StravaClient.FetchActivities(
            activities =>
            {
                Debug.Log($"UserClient received {activities.Count} activities.");
                onSuccess?.Invoke(activities);
            },
            error =>
            {
                Debug.LogError("FetchUserActivities failed: " + error);
                onError?.Invoke(error);
            });
    }

    public void FetchActivityDetail(long activityId, Action<StravaActivityDetail> onSuccess, Action<string> onError)
    {
        StravaClient.FetchActivityDetail(activityId, onSuccess, onError);
    }

    public void FetchActivityLaps(long activityId, Action<List<StravaLap>> onSuccess, Action<string> onError)
    {
        StravaClient.FetchActivityLaps(activityId, onSuccess, onError);
    }

    public void FetchActivityZones(long activityId, Action<List<StravaZone>> onSuccess, Action<string> onError)
    {
        StravaClient.FetchActivityZones(activityId, onSuccess, onError);
    }

    public void FetchActivityPhotos(long activityId, Action<List<StravaPhoto>> onSuccess, Action<string> onError)
    {
        StravaClient.FetchActivityPhotos(activityId, onSuccess, onError);
    }

    public bool IsUserAuthenticated()
    {
        return PlayerPrefs.HasKey("strava_access_token");
    }

    public void RefreshSession(Action onSuccess, Action<string> onError)
    {
        StravaClient.RefreshToken(onSuccess, onError);
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey("strava_access_token");
        PlayerPrefs.DeleteKey("strava_refresh_token");
        PlayerPrefs.DeleteKey("strava_token_expiry");
        Debug.Log("User logged out.");
    }
}
