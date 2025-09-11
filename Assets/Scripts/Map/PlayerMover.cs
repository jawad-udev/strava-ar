using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 1.4f;     // Normal human walking speed (m/s)
    public float runSpeed = 3.0f;      // Running speed (m/s)
    public float rotationSpeed = 8f;   // Turning smoothness

    [Header("Modes")]
    public bool fastMode = false;      // Debug speed-up
    public bool useGPS = false;        // GPS mode vs simulated route

    private List<Vector3> path;        // Route waypoints
    private int index = 0;             
    private bool moving = false;

    private Animator animator;

    // GPS tracking
    private double lastLat, lastLon;
    private float gpsSpeed;            // Real-world speed (m/s)
    private float lastUpdateTime;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (useGPS)
            StartCoroutine(StartGPS());
    }

    // ----------------- GPS INIT -----------------
    IEnumerator StartGPS()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("❌ GPS not enabled!");
            yield break;
        }

        Input.location.Start(1f, 0.1f);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("❌ GPS failed.");
            yield break;
        }

        Debug.Log("✅ GPS started");
        lastLat = Input.location.lastData.latitude;
        lastLon = Input.location.lastData.longitude;
        lastUpdateTime = Time.time;
    }

    // ----------------- PATH SETUP -----------------
    public void SetPath(List<Vector3> worldPositions)
    {
        path = new List<Vector3>(worldPositions);
        index = 0;

        if (path.Count > 0 && !useGPS)
        {
            transform.position = path[0];
            moving = true;
            animator?.SetTrigger("Running");
        }
    }

    void Update()
    {
        if (useGPS)
            UpdateGPSMovement();
        else
            UpdateSimulatedMovement();
    }

    // ----------------- SIMULATED MOVEMENT -----------------
    void UpdateSimulatedMovement()
    {
        if (!moving || path == null || index >= path.Count - 1) return;

        float currentSpeed = fastMode ? runSpeed : walkSpeed;
        Vector3 target = path[index + 1];
        Vector3 dir = target - transform.position;
        float step = currentSpeed * Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, target, step);

        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * rotationSpeed);
        }

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            index++;
            if (index >= path.Count - 1)
            {
                moving = false;
                Debug.Log("✅ Destination reached (Simulated).");
                animator?.SetTrigger("Idle");
            }
        }
    }

    // ----------------- GPS MOVEMENT -----------------
    void UpdateGPSMovement()
    {
        if (Input.location.status != LocationServiceStatus.Running || path == null || path.Count == 0)
            return;

        double lat = Input.location.lastData.latitude;
        double lon = Input.location.lastData.longitude;

        // Calculate real speed (m/s) based on GPS distance and time
        float deltaTime = Time.time - lastUpdateTime;
        if (deltaTime > 0.5f) // update every half sec
        {
            float distance = HaversineDistance(lastLat, lastLon, lat, lon);
            gpsSpeed = distance / deltaTime;

            lastLat = lat;
            lastLon = lon;
            lastUpdateTime = Time.time;
        }

        // Convert to Unity position (relative to first path point lat/lon)
        Vector3 gpsPos = GPSLatLonToWorld(lat, lon, path[0].z, path[0].x);

        // Smooth move toward GPS pos
        transform.position = Vector3.Lerp(transform.position, gpsPos, Time.deltaTime * 5f);

        // Animation based on speed
        if (gpsSpeed < 0.3f)
        {
            animator?.SetTrigger("Idle");
        }
        else if (gpsSpeed < runSpeed)
        {
            animator?.SetTrigger("Running");
        }
        else
        {
            animator?.SetTrigger("Running");
        }
    }

    // ----------------- HELPERS -----------------
    // Convert GPS Lat/Lon to Unity X/Z (rough meters)
    Vector3 GPSLatLonToWorld(double lat, double lon, double refLat, double refLon)
    {
        float scale = 1000f; // rough scale
        float x = (float)((lon - refLon) * scale);
        float z = (float)((lat - refLat) * scale);
        return new Vector3(x, 0, z);
    }

    // Haversine formula (distance in meters between 2 GPS points)
    float HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // Earth radius in meters

        float dLat = Mathf.Deg2Rad * (float)(lat2 - lat1);
        float dLon = Mathf.Deg2Rad * (float)(lon2 - lon1);

        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                  Mathf.Cos(Mathf.Deg2Rad * (float)lat1) * Mathf.Cos(Mathf.Deg2Rad * (float)lat2) *
                  Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);

        float c = 2f * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1f - a));

        return (float)(R * c);
    }

    // Debug toggle
    public void ToggleFastMode()
    {
        fastMode = !fastMode;
        Debug.Log("⚡ Fast Mode: " + fastMode);
    }
}
