// LoginScreen.cs
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class LoginScreen : GameMonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI statusText;
    public TMP_InputField codeInputField;
    public Button loginBtn, submitCodeBtn, fetchActivitiesBtn;
    public GameObject  activitiesPanel;

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
       /*  backButton.onClick.AddListener(() => 
        {
            activitiesPanel.SetActive(false);
        }); */

        // Auto-show activities if already authenticated
        if (PlayerPrefs.HasKey("strava_access_token"))
        {
           // activitiesPanel.SetActive(true);
            FetchAndDisplayActivities();
        }
    }

    public void OnClickLogin()
    {
        Application.OpenURL(StravaClient.Instance.GetLoginUrl());
        SetStatus("Awaiting Strava authentication...");
    }

    public void OnClickSubmitCode()
    {
        string code = codeInputField.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            SetStatus("Please enter a valid code");
            return;
        }

        SetStatus("Authenticating...");
        SetUIInteractable(false);

        StravaClient.Instance.ExchangeCodeForToken(code,
            jsonResponse => {
                var token = JsonUtility.FromJson<StravaTokenResponse>(jsonResponse);
                PlayerPrefs.SetString("strava_access_token", token.access_token);
                PlayerPrefs.SetString("strava_refresh_token", token.refresh_token);
                PlayerPrefs.SetInt("strava_token_expiry", 
                    (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond) + token.expires_in);
                
                SetStatus("Authentication successful!");
//                activitiesPanel.SetActive(true);
                SetUIInteractable(true);
            },
            error => {
                SetStatus($"Error: {error}");
                Debug.LogError(error);
                SetUIInteractable(true);
            });
    }

    public void FetchAndDisplayActivities()
    {
        SetStatus("Loading activities...");
        SetUIInteractable(false);

        StravaClient.Instance.FetchActivities(
            activities => {
                currentActivities = activities;
                DisplayActivities(activities);
                SetStatus($"Loaded {activities.Count} activities");
                SetUIInteractable(true);
            },
            error => {
                SetStatus($"Error: {error}");
                Debug.LogError(error);
                SetUIInteractable(true);
            });
    }

    private void DisplayActivities(List<StravaActivity> activities)
    {
        // Clear existing items
        foreach (Transform child in activitiesParent)
            Destroy(child.gameObject);

        // Create new items
        foreach (var activity in activities)
        {
            GameObject item = Instantiate(activityItemPrefab, activitiesParent);
            var text = item.GetComponentInChildren<TextMeshProUGUI>();
            TimeSpan time = TimeSpan.FromSeconds(activity.moving_time);
            
            text.text = $"{activity.name}\n" +
                         $"Distance: {activity.distance/1000:F1}km\n" +
                         $"Time: {time.Hours}h {time.Minutes}m\n" +
                         $"Elevation: {activity.total_elevation_gain:F0}m";

            Button btn = item.GetComponent<Button>();
            btn.onClick.AddListener(() => OnActivitySelected(activity));
        }
    }

    private void OnActivitySelected(StravaActivity activity)
    {
        // Save selected activity for AR scene
        PlayerPrefs.SetString("selected_activity", JsonUtility.ToJson(activity));
        PlayerPrefs.SetString("selected_polyline", activity.map.summary_polyline);
        
        Debug.Log($"Selected activity: {activity.name}");
        // Load your AR scene here
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