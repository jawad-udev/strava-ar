using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Newtonsoft.Json;

public class GamePlayScreen : GameMonoBehaviour
{
    public Button pauseButton, profileButton;
    public TextMeshProUGUI statusText;
    public Button fetchActivitiesBtn;
    public GameObject activitiesPanel;

    [Header("Activities")]
    public Transform activitiesParent;
    public GameObject activityItemPrefab;
    private List<StravaActivity> currentActivities = new List<StravaActivity>();

    private void Awake()
    {
        pauseButton.onClick.AsObservable().Subscribe(_ => OnClickPauseButton());
        profileButton.onClick.AsObservable().Subscribe(_ => OnClickProfileButton());
        fetchActivitiesBtn.onClick.AddListener(FetchAndDisplayActivities);
    }

    void Start()
    {
        if (Services.UserService.IsUserAuthenticated())
        {
            FetchAndDisplayActivities();
        }
    }

    public void OnClickProfileButton()
    {
        Services.UIService.ActivateUIPopups(Popups.PROFILE);
        Services.AudioService.PlayUIClick();
    }

    public void OnClickPauseButton()
    {
        Services.GameService.SetState<GamePauseState>();
        Services.AudioService.PlayUIClick();
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
            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length < 2)
            {
                Debug.LogError("Prefab must have 2 TextMeshProUGUI components.");
                continue;
            }

            TimeSpan time = TimeSpan.FromSeconds(activity.moving_time);
            texts[0].text = $"{activity.name}\n" +
                            $"Distance: {activity.distance / 1000f:F1}km\n" +
                            $"Time: {time.Hours}h {time.Minutes}m\n" +
                            $"Elevation: {activity.total_elevation_gain:F0}m";

            texts[1].text = "Loading heart rate...";

            Button btn = item.GetComponent<Button>();
            btn.onClick.AddListener(() => OnActivitySelected(activity, texts[1]));
        }
    }

    private void OnActivitySelected(StravaActivity activity, TextMeshProUGUI heartRateText)
    {
        Services.UserService.FetchActivityDetail(activity.id,
            detail =>
            {
                PlayerPrefs.SetString("selected_activity", JsonConvert.SerializeObject(detail));
                PlayerPrefs.SetString("selected_polyline", activity.map?.summary_polyline ?? "");

                heartRateText.text = $"Avg HR: {detail.average_heartrate:F0} bpm\n" +
                                     $"Max HR: {detail.max_heartrate:F0} bpm";

                Debug.Log($"Selected activity with HR: {detail.name}");
                // SceneManager.LoadScene("ARScene");
            },
            error =>
            {
                heartRateText.text = "HR Load Failed";
                Debug.LogError($"Failed to load activity detail: {error}");
            });
    }

    private void SetUIInteractable(bool interactable)
    {
        fetchActivitiesBtn.interactable = interactable;
    }

    private void SetStatus(string message)
    {
        if (statusText) statusText.text = message;
    }
}
