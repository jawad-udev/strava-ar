using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class LoginScreen : GameMonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI statusText;
    public TMP_InputField codeInputField;
    public Button loginBtn, submitCodeBtn, fetchActivitiesBtn;
    public GameObject activitiesPanel;

    [Header("Activities")]
    public Transform activitiesParent;
    public GameObject activityItemPrefab;
    public Button backButton;

    private List<StravaActivity> currentActivities = new List<StravaActivity>();

    private void Start()
    {
        loginBtn.onClick.AddListener(OnClickLogin);
        submitCodeBtn.onClick.AddListener(OnClickSubmitCode);
        fetchActivitiesBtn.onClick.AddListener(FetchAndDisplayActivities);
        //backButton.onClick.AddListener(() => activitiesPanel.SetActive(false));

        if (Services.UserService.IsUserAuthenticated())
        {
            FetchAndDisplayActivities();
        }
    }

    private void OnClickLogin()
    {
        Application.OpenURL(StravaClient.GetLoginUrl());
        SetStatus("Awaiting Strava authentication...");
    }

    private void OnClickSubmitCode()
    {
        string code = codeInputField.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            SetStatus("Please enter a valid code");
            return;
        }

        SetStatus("Authenticating...");
        SetUIInteractable(false);

        Services.UserService.Login(code,
            onSuccess: () =>
            {
                SetStatus("Authentication successful!");
                activitiesPanel.SetActive(true);
                SetUIInteractable(true);
            },
            onError: error =>
            {
                SetStatus($"Login Error: {error}");
                Debug.LogError(error);
                SetUIInteractable(true);
            });
    }

    private void FetchAndDisplayActivities()
    {
        SetStatus("Loading activities...");
        SetUIInteractable(false);

        Services.UserService.FetchUserActivities(
            onSuccess: activities =>
            {
                currentActivities = activities;
                DisplayActivities(activities);
                SetStatus($"Loaded {activities.Count} activities");
                SetUIInteractable(true);
            },
            onError: error =>
            {
                SetStatus($"Fetch Error: {error}");
                Debug.LogError(error);
                SetUIInteractable(true);
            });
    }

    private void DisplayActivities(List<StravaActivity> activities)
    {
        foreach (Transform child in activitiesParent)
            Destroy(child.gameObject);

        foreach (var activity in activities)
        {
            GameObject item = Instantiate(activityItemPrefab, activitiesParent);
            var text = item.GetComponentInChildren<TextMeshProUGUI>();
            TimeSpan time = TimeSpan.FromSeconds(activity.moving_time);

            text.text = $"{activity.name}\n" +
                        $"Distance: {activity.distance / 1000f:F1}km\n" +
                        $"Time: {time.Hours}h {time.Minutes}m\n" +
                        $"Elevation: {activity.total_elevation_gain:F0}m";

            Button btn = item.GetComponent<Button>();
            btn.onClick.AddListener(() => OnActivitySelected(activity));
        }
    }

    private void OnActivitySelected(StravaActivity activity)
    {
        string json = JsonConvert.SerializeObject(activity);
        PlayerPrefs.SetString("selected_activity", json);
        PlayerPrefs.SetString("selected_polyline", activity.map?.summary_polyline ?? "");

        Debug.Log($"Selected activity: {activity.name}");
        // SceneManager.LoadScene("ARScene");
    }

    private void SetStatus(string message)
    {
        if (statusText) statusText.text = message;
    }

    private void SetUIInteractable(bool interactable)
    {
        loginBtn.interactable = interactable;
        submitCodeBtn.interactable = interactable;
        fetchActivitiesBtn.interactable = interactable;
        codeInputField.interactable = interactable;
    }
}
