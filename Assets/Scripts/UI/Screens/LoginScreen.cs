using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LoginScreen : GameMonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI statusText;
    public TMP_InputField codeInputField;
    public Button loginBtn, submitCodeBtn, fetchActivitiesBtn;

    [Header("Activities")]
    public Transform activitiesParent;
    public GameObject activityItemPrefab; // A prefab with TMP_Text to show activity name

    private void Awake()
    {
        loginBtn.onClick.AsObservable().Subscribe(_ => OnClickLogin());
        submitCodeBtn.onClick.AsObservable().Subscribe(_ => OnClickSubmitCode());
        fetchActivitiesBtn.onClick.AsObservable().Subscribe(_ => FetchAndDisplayActivities());
    }

    public void OnClickLogin()
    {
        string url = StravaClient.Instance.GetLoginUrl();
        Application.OpenURL(url);
        SetStatus("Opening Strava login page...");
    }

    public void OnClickSubmitCode()
    {
        string code = codeInputField.text.Trim();

        if (string.IsNullOrEmpty(code))
        {
            SetStatus("Please enter a valid code.");
            return;
        }

        SetStatus("Authenticating...");

        StravaClient.Instance.ExchangeCodeForToken(code,
            onSuccessJson =>
            {
                var token = JsonUtility.FromJson<StravaTokenResponse>(onSuccessJson);
                if (!string.IsNullOrEmpty(token.access_token))
                {
                    SetStatus("Strava login successful!");
                    Debug.Log("Strava Token: " + token.access_token);
                }
                else
                {
                    SetStatus("Login failed: Invalid token received.");
                    Debug.LogError("Invalid token data: " + onSuccessJson);
                }
            },
            onError =>
            {
                SetStatus("Login failed: " + onError);
                Debug.LogError("Strava Login Error: " + onError);
            });
    }

    public void FetchAndDisplayActivities()
    {
        SetStatus("Fetching activities...");

        StravaClient.Instance.FetchActivities(
            onSuccess: (activities) =>
            {
                SetStatus($"Fetched {activities.Count} activities.");
                foreach (var activity in activities)
                {
                    Debug.Log($"Activity Name: {activity.name}");
                }
                DisplayActivities(activities);
            },
            onError: (err) =>
            {
                SetStatus("Failed to fetch activities: " + err);
                Debug.LogError("Activity Fetch Error: " + err);
            });
    }

    private void DisplayActivities(List<StravaActivity> activities)
    {
        // Clear old items
        foreach (Transform child in activitiesParent)
        {
            Destroy(child.gameObject);
        }

        // Create UI elements for each activity
        foreach (var activity in activities)
        {
            GameObject go = Instantiate(activityItemPrefab, activitiesParent);
            var text = go.GetComponentInChildren<TextMeshProUGUI>();
            text.text = $"🚴 {activity.name} - {activity.distance}m";
        }
    }

    private void SetStatus(string msg)
    {
        if (statusText != null)
            statusText.text = msg;
    }
}
