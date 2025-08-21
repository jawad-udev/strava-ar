using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// GhostRunnerWithStrava
/// - Drives a ghost using device GPS in real time
/// - Records live GPS points (lat, lon, time)
/// - Exports GPX on EndRun and can upload to Strava Uploads API
/// </summary>
public class GhostRunnerWithStrava : MonoBehaviour
{
    [Header("Ghost Visuals")]
    public GameObject ghostPrefab;
    public Transform ghostInstance;
    public float leadMeters = 3f;                 // how far ahead of player to place the ghost
    public float positionLerp = 0.2f;             // ghost smoothing
    public float rotationLerp = 0.2f;             // ghost facing smoothing
    public string animatorSpeedParam = "Speed";   // optional Animator float param for speed
    public bool scaleAnimatorSpeed = true;
    public float animNormSpeedMS = 3.0f;          // velocity -> Animator.speed=1 at this m/s
    public Vector2 animSpeedClamp = new Vector2(0.6f, 1.6f);

    [Header("Grounding")]
    public LayerMask groundMask;                  // e.g. your ARPlane/terrain layers
    public float groundRayHeight = 10f;           // raycast from above player
    public float footOffset = 0.02f;              // lift off the ground to avoid z-fighting

    [Header("Recording & Export")]
    public string defaultActivityName = "Shadow Runner";
    public string defaultActivityType = "run";    // for Strava uploads: run, ride, walk, etc.
    public bool writeGpxWhenEndRun = true;

    [Header("Strava Upload (optional)")]
    [Tooltip("Bearer access token obtained via Strava OAuth; DO NOT hardcode client secret in app.")]
    public string stravaAccessToken = "";         // set at runtime from your auth flow
    public bool uploadToStravaOnEnd = false;      // auto-upload after EndRun
    [Tooltip("Optional: prepend this to the activity name when uploading.")]
    public string stravaNamePrefix = "Shadow Runner: ";

    // --- internals ---
    private bool isRunning = false;
    private Animator ghostAnimator;
    private Vector3 lastGhostPos;
    private float ghostVelEMA = 0f;
    private const float velTau = 0.25f; // smoothing time constant (s)

    // Recorded samples
    private List<Sample> samples = new List<Sample>();

    // Last heading (for placing ghost ahead)
    private Vector3 lastForward = Vector3.forward;

    // A single GPS sample
    [Serializable]
    private struct Sample
    {
        public double lat, lon;
        public double ele;                 // optional
        public DateTime utc;               // timestamp
    }

    void Awake()
    {
        Services.GameService.ghostRunnerWithStrava = this;
    }

    // ========================
    // Public control
    // ========================

    public void StartRun()
    {
        if (isRunning) return;

        StartCoroutine(StartGpsAndRun());
    }

    public void EndRun()
    {
        if (!isRunning) return;
        isRunning = false;

        // Stop GPS
        if (Input.location.status == LocationServiceStatus.Running)
            Input.location.Stop();

        // Export GPX
        string filePath = "";
        if (writeGpxWhenEndRun && samples.Count >= 2)
        {
            filePath = SaveGpxToDisk(
                samples,
                (string.IsNullOrWhiteSpace(defaultActivityName) ? "Shadow Runner" : defaultActivityName)
            );
            Debug.Log($"GPX written: {filePath}");
        }

        // Upload to Strava (optional)
        if (uploadToStravaOnEnd && !string.IsNullOrEmpty(stravaAccessToken) && !string.IsNullOrEmpty(filePath))
        {
            StartCoroutine(UploadGpxToStrava(filePath,
                $"{stravaNamePrefix}{defaultActivityName}".Trim(),
                defaultActivityType));
        }
    }

    // ========================
    // GPS loop
    // ========================

    private IEnumerator StartGpsAndRun()
    {
        // Ensure ghost exists
        if (ghostInstance == null && ghostPrefab != null)
            ghostInstance = Instantiate(ghostPrefab).transform;

        if (ghostInstance == null)
        {
            Debug.LogError("GhostRunner: ghost prefab/instance missing.");
            yield break;
        }

        ghostAnimator = ghostInstance.GetComponent<Animator>();
        lastGhostPos = ghostInstance.position;

        // Start location service
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

        isRunning = true;
        samples.Clear();

        var wait = new WaitForSeconds(0.20f); // ~5 Hz polling

        // Initial sample (if available) to seed placement
        var d0 = Input.location.lastData;
        Vector3 world0 = WorldFromGPS(d0.latitude, d0.longitude);
        Vector3 grounded0 = GroundY(world0);
        ghostInstance.position = grounded0;
        lastGhostPos = grounded0;

        while (isRunning && Input.location.status == LocationServiceStatus.Running)
        {
            var d = Input.location.lastData;

            // Record a sample each loop (or add a distance/time filter if you prefer)
            samples.Add(new Sample
            {
                lat = d.latitude,
                lon = d.longitude,
                ele = 0, // Unity doesn't give altitude accuracy; 0 is fine for GPX
                utc = DateTime.UtcNow
            });

            // Move ghost ahead of the player's ground position along heading
            Vector3 playerPos = GroundY(WorldFromGPS(d.latitude, d.longitude));
            Vector3 heading = ComputeHeading(lastForward, lastGhostPos, playerPos);
            lastForward = heading;

            Vector3 targetPos = playerPos + heading * leadMeters;
            Vector3 newPos = Vector3.Lerp(ghostInstance.position, targetPos, positionLerp);
            ghostInstance.position = newPos;

            // Face the run direction
            if (heading.sqrMagnitude > 1e-4f)
            {
                Quaternion to = Quaternion.LookRotation(heading, Vector3.up);
                ghostInstance.rotation = Quaternion.Slerp(ghostInstance.rotation, to, rotationLerp);
            }

            // Drive animation by velocity
            float instVel = (Time.deltaTime > 1e-4f)
                ? Vector3.Distance(newPos, lastGhostPos) / Time.deltaTime
                : 0f;
            float alphaV = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(velTau, 1e-3f));
            ghostVelEMA = Mathf.Lerp(ghostVelEMA, instVel, alphaV);
            lastGhostPos = newPos;

            if (ghostAnimator != null)
            {
                ghostAnimator.SetFloat(animatorSpeedParam, ghostVelEMA);
                if (scaleAnimatorSpeed)
                {
                    float norm = (animNormSpeedMS <= 0.01f) ? 1f : ghostVelEMA / animNormSpeedMS;
                    ghostAnimator.speed = Mathf.Clamp(norm, animSpeedClamp.x, animSpeedClamp.y);
                }
            }

            yield return wait;
        }

        Debug.Log("GPS loop ended.");
    }

    // ========================
    // World/Ground helpers
    // ========================

    // Super-lightweight local projection around first sample
    private bool seededOrigin = false;
    private double originLat = 0, originLon = 0;
    private Vector3 originWorld = Vector3.zero;

    private Vector3 WorldFromGPS(double lat, double lon)
    {
        const double R = 6378137.0;
        if (!seededOrigin)
        {
            seededOrigin = true;
            originLat = lat; originLon = lon;
            originWorld = (ghostInstance != null) ? ghostInstance.position : Vector3.zero;
        }

        double lat0 = originLat * Mathf.Deg2Rad;
        double lon0 = originLon * Mathf.Deg2Rad;
        double LAT = lat * Mathf.Deg2Rad;
        double LON = lon * Mathf.Deg2Rad;

        double dLat = LAT - lat0;
        double dLon = (LON - lon0) * Math.Cos(0.5 * (LAT + lat0));

        float x = (float)(dLon * R);
        float z = (float)(dLat * R);
        return originWorld + new Vector3(x, 0f, z);
    }

    private Vector3 GroundY(Vector3 approx)
    {
        Vector3 from = approx + Vector3.up * groundRayHeight;
        if (Physics.Raycast(from, Vector3.down, out var hit, groundRayHeight * 2f, groundMask))
        {
            return new Vector3(approx.x, hit.point.y + footOffset, approx.z);
        }
        // Fallback: keep current Y (or 0) if no collider
        return new Vector3(approx.x, approx.y + footOffset, approx.z);
    }

    private Vector3 ComputeHeading(Vector3 prevForward, Vector3 fromPos, Vector3 toPos)
    {
        Vector3 dir = (toPos - fromPos);
        dir.y = 0f;
        if (dir.sqrMagnitude < 1e-4f) return prevForward.sqrMagnitude > 0 ? prevForward : Vector3.forward;
        return dir.normalized;
    }

    // ========================
    // GPX export
    // ========================

    private string SaveGpxToDisk(List<Sample> track, string activityName)
    {
        string gpx = BuildGpx(track, activityName);
        string fileName = $"shadow_runner_{DateTime.UtcNow:yyyyMMdd_HHmmss}.gpx";
        string path = System.IO.Path.Combine(Application.persistentDataPath, fileName);
        System.IO.File.WriteAllText(path, gpx, Encoding.UTF8);
        return path;
    }

    private string BuildGpx(List<Sample> track, string activityName)
    {
        // Minimal GPX 1.1
        var sb = new StringBuilder();
        sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
        sb.AppendLine(@"<gpx version=""1.1"" creator=""ShadowRunner"" xmlns=""http://www.topografix.com/GPX/1/1"">");
        sb.AppendLine($"  <metadata><name>{EscapeXml(activityName)}</name><time>{Iso8601Utc(track[0].utc)}</time></metadata>");
        sb.AppendLine("  <trk>");
        sb.AppendLine($"    <name>{EscapeXml(activityName)}</name>");
        sb.AppendLine("    <trkseg>");
        for (int i = 0; i < track.Count; i++)
        {
            var s = track[i];
            sb.AppendLine($"      <trkpt lat=\"{s.lat.ToString(CultureInfo.InvariantCulture)}\" lon=\"{s.lon.ToString(CultureInfo.InvariantCulture)}\">");
            sb.AppendLine($"        <ele>{s.ele.ToString(CultureInfo.InvariantCulture)}</ele>");
            sb.AppendLine($"        <time>{Iso8601Utc(s.utc)}</time>");
            sb.AppendLine("      </trkpt>");
        }
        sb.AppendLine("    </trkseg>");
        sb.AppendLine("  </trk>");
        sb.AppendLine("</gpx>");
        return sb.ToString();
    }

    private string Iso8601Utc(DateTime dt) => dt.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
    private string EscapeXml(string s) => string.IsNullOrEmpty(s) ? "" : s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

    // ========================
    // Strava upload
    // ========================

    /// <summary>
    /// Upload a GPX to Strava Uploads API.
    /// Requires a valid OAuth access token. Do NOT put your client secret in the app.
    /// </summary>
    public IEnumerator UploadGpxToStrava(string filePath, string activityName, string activityType = "run")
    {
        if (string.IsNullOrEmpty(stravaAccessToken))
        {
            Debug.LogError("Strava access token is empty. Aborting upload.");
            yield break;
        }
        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogError("GPX file not found: " + filePath);
            yield break;
        }

        byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

        // Build multipart/form-data
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", fileBytes, System.IO.Path.GetFileName(filePath), "application/gpx+xml");
        form.AddField("data_type", "gpx");            // gpx|fit|tcx
        form.AddField("name", activityName);
        form.AddField("activity_type", activityType); // run/ride/walk/etc.
        // Optional flags:
        // form.AddField("trainer", "false");
        // form.AddField("commute", "false");
        // form.AddField("description", "Recorded via Shadow Runner");

        using (UnityWebRequest req = UnityWebRequest.Post("https://www.strava.com/api/v3/uploads", form))
        {
            req.SetRequestHeader("Authorization", "Bearer " + stravaAccessToken);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Strava upload failed: {req.responseCode} {req.error}\n{req.downloadHandler.text}");
                yield break;
            }

            // Strava processes uploads asynchronously. The response includes an upload ID you can poll
            // if you want to show status, otherwise it will appear in the athlete’s feed once processed.
            Debug.Log("Strava upload accepted: " + req.downloadHandler.text);
        }
    }
}
