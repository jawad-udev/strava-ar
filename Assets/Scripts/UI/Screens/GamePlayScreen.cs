using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Newtonsoft.Json;
using Unity.Android.Gradle.Manifest;

public class GamePlayScreen : GameMonoBehaviour
{
    public Button pauseButton, profileButton;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI heartRateTxt, lapsTxt;
    public Button fetchActivitiesBtn;
    public GameObject activitiesPanel;
    
    [Header("Athlete Info")]
    public TextMeshProUGUI athleteNameTxt;
    public TextMeshProUGUI athleteUsernameTxt;
    public TextMeshProUGUI athleteLocationTxt;
    public TextMeshProUGUI athleteFollowersTxt;

    [Header("Athlete Stats")]
    public TextMeshProUGUI athleteStatsTxt;

    [Header("User Heart Rate Zones")]
    public TextMeshProUGUI heartRateZonesTxt;

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
            FetchAndDisplayAthlete();
            FetchAndDisplayAthleteStats();
            FetchAndDisplayAthleteHeartRateZones();
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

    private void FetchAndDisplayAthlete()
    {
        Services.UserService.FetchAthleteProfile(
            athlete =>
            {
                athleteNameTxt.text = $"{athlete.firstname} {athlete.lastname}";
                athleteUsernameTxt.text = $"@{athlete.username}";
                athleteLocationTxt.text = $"{athlete.city}, {athlete.country}";
                athleteFollowersTxt.text = $"Followers: {athlete.follower_count}";

                PlayerPrefs.SetInt("athlete_id", (int)athlete.id);
            },
            error =>
            {
                athleteNameTxt.text = "Failed to load athlete info";
                Debug.LogError("FetchAthlete error: " + error);
            });
    }

    private void FetchAndDisplayAthleteStats()
    {
        long athleteId = PlayerPrefs.GetInt("athlete_id", 0);
        if (athleteId == 0)
        {
            athleteStatsTxt.text = "Athlete ID missing";
            return;
        }

        Services.UserService.FetchAthleteStats(athleteId,
            stats =>
            {
                athleteStatsTxt.text =
                    $"Biggest Ride: {stats.biggest_ride_distance / 1000f:F1} km\n" +
                    $"Biggest Climb: {stats.biggest_climb_elevation_gain:F0} m\n" +
                    $"Recent Rides: {stats.recent_ride_totals.count}, {stats.recent_ride_totals.distance / 1000f:F1} km\n" +
                    $"Year To Date: {stats.ytd_ride_totals.count}, {stats.ytd_ride_totals.distance / 1000f:F1} km\n" +
                    $"All Time Rides: {stats.all_ride_totals.count}, {stats.all_ride_totals.distance / 1000f:F1} km";
            },
            error =>
            {
                athleteStatsTxt.text = "Failed to load athlete stats";
                Debug.LogError("FetchAthleteStats error: " + error);
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

    private void FetchAndDisplayAthleteHeartRateZones()
    {
        Services.UserService.FetchAthleteHeartRateZones(
            zones =>
            {
                if (zones?.heart_rate != null && zones.heart_rate.Count > 0)
                {
                    heartRateZonesTxt.text = "Heart Rate Zones:\n";
                    int zoneNumber = 1;
                    foreach (var zone in zones.heart_rate)
                    {
                        heartRateZonesTxt.text +=
                            $"Zone {zoneNumber}: {zone.min} - {zone.max} bpm\n";
                        zoneNumber++;
                    }
                }
                else
                {
                    heartRateZonesTxt.text = "No heart rate zones data";
                }
            },
            error =>
            {
                heartRateZonesTxt.text = "Failed to load heart rate zones";
                Debug.LogError("FetchUserHeartRateZones error: " + error);
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
            if (texts.Length < 1)
            {
                Debug.LogError("Prefab must have 1 TextMeshProUGUI components.");
                continue;
            }

            TimeSpan time = TimeSpan.FromSeconds(activity.moving_time);
            texts[0].text = $"Name: {activity.name}\n" +
                            $"Distance: {activity.distance / 1000f:F1}km\n" +
                            $"Time: {time.Hours}h {time.Minutes}m\n" +
                            $"Elevation: {activity.total_elevation_gain:F0}m";

            heartRateTxt.text = "Loading heart rate...";

            Button btn = item.GetComponent<Button>();
            btn.onClick.AddListener(() => OnActivitySelected(activity, heartRateTxt));
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

                // Add Lap Display if any
                if (detail.laps != null && detail.laps.Count > 0)
                {
                    string lapSummary = "\nLaps:\n";
                    foreach (var lap in detail.laps)
                    {
                        TimeSpan lapTime = TimeSpan.FromSeconds(lap.elapsedTime);
                        lapSummary += $"- Lap {lap.lapIndex}: {lap.distance / 1000f:F2}km, {lapTime.Minutes}m {lapTime.Seconds}s\n";
                    }

                    //heartRateText.text += lapSummary;
                    lapsTxt.text = lapSummary;
                }
                else
                {
                    lapsTxt.text = "\nNo lap data found.";
                }

                // Optional: Load AR Scene
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
