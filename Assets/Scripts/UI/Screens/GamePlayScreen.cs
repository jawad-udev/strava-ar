using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class GamePlayScreen : GameMonoBehaviour
{
    public Button fetchActivitesBtn, fetchProfileInfoBtn, fetchAthleteStatsBtn, fetchHeartZonesBtn;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI heartRateTxt, lapsTxt;
    public Button fetchActivitiesBtn;
    public GameObject activitiesPanel;

    [Header("Athlete Info")]
    public Transform athleteprefabParent;
    public ARProfileCard athleteProfilePrefab;

    [Header("Athlete Stats")]
    public TextMeshProUGUI athleteStatsTxt;

    [Header("User Heart Rate Zones")]
    public TextMeshProUGUI heartRateZonesTxt;

    [Header("Activities")]
    public Transform activitiesParent;
    public GameObject activityItemPrefab, athleteStatsPrefab;
    private List<StravaActivity> currentActivities = new List<StravaActivity>();

    private void Awake()
    {
        fetchActivitiesBtn.onClick.AddListener(FetchAndDisplayActivities);
        fetchProfileInfoBtn.onClick.AddListener(FetchAndDisplayAthleteProfile);
        fetchAthleteStatsBtn.onClick.AddListener(FetchAndDisplayAthleteStats);
        fetchHeartZonesBtn.onClick.AddListener(FetchAndDisplayAthleteHeartRateZones);
    }

    public void StartRunningActivity()
    {
        Services.GameService.ghostRunnerWithStrava.StartRun();
    }

    public void StopRunningActivity()
    {
        Services.GameService.ghostRunnerWithStrava.EndRun();
    }

    void Start()
    {
        if (Services.UserService.IsUserAuthenticated())
        {
            FetchAndDisplayActivities();
        }
    }


    private void FetchAndDisplayAthleteProfile()
    {
        Services.UserService.FetchAthleteProfile(
            athlete =>
            {
                // Instantiate ARProfileCard in the scene
                if (athleteProfilePrefab != null)
                {
                    ARProfileCard card = Instantiate(athleteProfilePrefab, athleteprefabParent);
                    card.SetupAthleteInfo(athlete);
                }
                else
                {
                    Debug.LogWarning("athleteProfilePrefab not assigned!");
                }

                PlayerPrefs.SetInt("athlete_id", (int)athlete.id);
            },
            error =>
            {
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
                GameObject item = Instantiate(athleteStatsPrefab, athleteprefabParent);
                var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
                texts[1].text =
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
            texts[1].text = $"Name: {activity.name}\n" +
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

                // LAP display
                if (detail.laps != null && detail.laps.Count > 0)
                {
                    string lapSummary = "\nLaps:\n";
                    foreach (var lap in detail.laps)
                    {
                        TimeSpan lapTime = TimeSpan.FromSeconds(lap.elapsedTime);
                        lapSummary += $"- Lap {lap.lapIndex}: {lap.distance / 1000f:F2}km, {lapTime.Minutes}m {lapTime.Seconds}s\n";
                    }
                    lapsTxt.text = lapSummary;
                }
                else
                {
                    lapsTxt.text = "\nNo lap data found.";
                }

                FetchAndSpawnGhost(activity.id);
            },
            error =>
            {
                heartRateText.text = "HR Load Failed";
                Debug.LogError($"Failed to load activity detail: {error}");
            });
    }

    public void FetchAndSpawnGhost(long activityId)
    {
        Services.UserService.FetchActivityStreams(
            activityId,
            stream =>
            {
                try
                {
                    // --- Validate streams ---
                    if (stream == null) { Debug.LogError("Stream object is null."); return; }
                    if (stream.latlng?.data == null || stream.latlng.data.Count < 2)
                    { Debug.LogError("latlng stream is null/too short."); return; }
                    if (stream.time?.data == null || stream.time.data.Count < 2)
                    { Debug.LogError("time stream is null/too short."); return; }

                    var latlngRaw = stream.latlng.data;   // List<List<float>> [lat, lon]
                    var timeRaw = stream.time.data;     // List<float> seconds since activity start

                    if (latlngRaw.Count != timeRaw.Count)
                    {
                        Debug.LogError($"Stream mismatch: latlng ({latlngRaw.Count}) vs time ({timeRaw.Count})");
                        return;
                    }

                    // --- Convert to List<Vector2>(lat, lon) ---
                    var latlngList = new List<Vector2>(latlngRaw.Count);
                    for (int i = 0; i < latlngRaw.Count; i++)
                    {
                        var pair = latlngRaw[i];
                        if (pair != null && pair.Count == 2)
                            latlngList.Add(new Vector2(pair[0], pair[1])); // [lat, lon]
                        else
                            Debug.LogWarning($"Skipped malformed latlng at index {i}");
                    }

                    // --- Optional: trim initial stationary jitter (privacy zone / warmup) ---
                    TrimLeadingStationary(latlngList, timeRaw, minMeters: 8f, windowSec: 6f);

                    // --- Init ghost runner (no elevation needed) ---
                    var ghostRunner = Services.GameService.ghostRunner;
                    if (ghostRunner == null) { Debug.LogError("GhostRunnerManager is null."); return; }

                    ghostRunner.Init(latlngList, timeRaw);

                    // Choose ONE mode
                    ghostRunner.leadMeters = 4f;  // simple, responsive
                    ghostRunner.leadSeconds = 0f;  // set 3–6 and leadMeters=0 for “pace replay” feel

                    // Don’t place here—let your placement controller do:
                    //   ghostRunner.AttachToAnchor(anchorTransform);
                    //   ghostRunner.DrapeRouteToGround(groundMask);

                    // Start feeding GPS now (it will still work pre/post anchor)
                    Debug.Log("Starting GPS feed to ghost runner...");
                    StartCoroutine(PipeGpsToGhost(ghostRunner));

                    Debug.Log($"✅ Ghost prepared with {latlngList.Count} points / {timeRaw.Count} timestamps. Tap a plane to place.");
                }
                catch (Exception ex)
                {
                    Debug.LogError("Exception during stream parsing: " + ex);
                }
            },
            error => Debug.LogError("Error fetching streams: " + error)
        );
    }


    /// <summary>
    /// Trim initial points with little movement to avoid privacy-zone jitter / warmup drift.
    /// </summary>
    private void TrimLeadingStationary(List<Vector2> latlng, List<float> timeSec, float minMeters = 8f, float windowSec = 6f)
    {
        if (latlng == null || timeSec == null || latlng.Count < 3 || latlng.Count != timeSec.Count) return;

        int lookAhead = 0;
        for (int i = 1; i < timeSec.Count; i++)
            if (timeSec[i] - timeSec[0] >= windowSec) { lookAhead = i; break; }

        if (lookAhead == 0) return;

        float moved = Haversine(latlng[0], latlng[lookAhead]);
        if (moved >= minMeters) return;

        float cum = 0f;
        int cut = 0;
        for (int i = 1; i < latlng.Count; i++)
        {
            cum += Haversine(latlng[i - 1], latlng[i]);
            if (cum >= minMeters) { cut = i; break; }
        }
        if (cut > 0 && cut < latlng.Count - 1)
        {
            latlng.RemoveRange(0, cut);
            float t0 = timeSec[cut];
            timeSec.RemoveRange(0, cut);
            for (int i = 0; i < timeSec.Count; i++) timeSec[i] -= t0; // rebase to 0
            Debug.Log($"Trimmed {cut} leading points ({cum:F1} m).");
        }

        float Haversine(Vector2 a, Vector2 b)
        {
            const double R = 6371000.0;
            double lat1 = a.x * Mathf.Deg2Rad, lon1 = a.y * Mathf.Deg2Rad;
            double lat2 = b.x * Mathf.Deg2Rad, lon2 = b.y * Mathf.Deg2Rad;
            double dLat = lat2 - lat1, dLon = lon2 - lon1;
            double h = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1) * Math.Cos(lat2) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return (float)(2 * R * Math.Asin(Math.Min(1, Math.Sqrt(h))));
        }
    }


    /// <summary>
    /// Simple GPS → ghost feed using Unity LocationService.
    /// Replace with ARCore/ARKit geospatial feed if available (still call SetPlayerLatLon).
    /// </summary>
    private System.Collections.IEnumerator PipeGpsToGhost(GhostRunnerManager ghost)
    {
        if (ghost == null) yield break;

        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("Location service disabled by user.");
            yield break;
        }

        // desiredAccuracyInMeters, updateDistanceInMeters
        Input.location.Start(5f, 1f);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait-- > 0)
            yield return new WaitForSeconds(1);

        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.LogWarning("Location service failed to start.");
            yield break;
        }

        var wait = new WaitForSeconds(0.20f); // ~5 Hz
        while (ghost != null && ghost.IsInitialized())
        {
            var d = Input.location.lastData;
            // Unity doesn't expose accuracy here; pass 0 or wire your own if available.
            ghost.SetPlayerLatLon(d.latitude, d.longitude, 0f);

            // stop conditions: location stopped or app lost focus could be added here
            if (Input.location.status != LocationServiceStatus.Running) break;

            yield return wait;
        }

        Input.location.Stop();
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
