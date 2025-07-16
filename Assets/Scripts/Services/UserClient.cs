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

    public void RefreshSession(Action onSuccess, Action<string> onError)
    {
        StravaClient.RefreshToken(onSuccess, onError);
    }

    public bool IsUserAuthenticated()
    {
        return PlayerPrefs.HasKey("strava_access_token");
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey("strava_access_token");
        PlayerPrefs.DeleteKey("strava_refresh_token");
        PlayerPrefs.DeleteKey("strava_token_expiry");
        Debug.Log("User logged out.");
    }

    // ---------------- ACTIVITY DATA ----------------
    public void FetchUserActivities(Action<List<StravaActivity>> onSuccess, Action<string> onError)
    {
        StravaClient.FetchActivities(onSuccess, onError);
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

    // ---------------- ATHLETE INFO ----------------
    public void FetchAthleteProfile(Action<StravaAthlete> onSuccess, Action<string> onError)
    {
        StravaClient.FetchAthlete(onSuccess, onError);
    }

    public void FetchAthleteStats(long athleteId, Action<StravaStats> onSuccess, Action<string> onError)
    {
        StravaClient.FetchAthleteStats(athleteId, onSuccess, onError);
    }

    public void FetchAthleteHeartRateZones(Action<StravaUserZones> onSuccess, Action<string> onError)
    {
        StravaClient.FetchAthleteHeartRateZones(onSuccess, onError);
    }
}
