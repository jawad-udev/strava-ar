using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostRunnerManager : MonoBehaviour
{
    [Header("Ghost Settings")]
    public GameObject runnerPrefab;
    public Transform runnerInstance;
    public float speedMultiplier = 1.0f;

    [Header("Route Drawing")]
    [SerializeField] private LineRenderer routeLine;

    [Header("Debug Info")]
    public bool showDebugLines = false;

    // Path data
    private List<Vector3> pathPositions = new List<Vector3>();
    private List<float> timeStamps = new List<float>(); // seconds
    private List<float> elevations = new List<float>();

    private int currentIndex = 0;
    private float currentTime = 0f;
    private bool isPlaying = false;
    private Vector2 baseLatLon;
    private Vector3 arOrigin;

    public GameObject startPointMarkerPrefab;

    // === Initialization ===
    public void Init(List<Vector2> latLngPoints, List<float> timestamps, List<float> elevation = null)
    {
        pathPositions.Clear();
        timeStamps.Clear();
        elevations.Clear();
        currentIndex = 0;
        currentTime = 0f;
        isPlaying = false;

        if (latLngPoints == null || latLngPoints.Count < 2 || timestamps == null || latLngPoints.Count != timestamps.Count)
        {
            Debug.LogError("Invalid path or timestamp data.");
            return;
        }
        
        baseLatLon = latLngPoints[0];
        arOrigin = Camera.main.transform.position;

        for (int i = 0; i < latLngPoints.Count; i++)
        {
            Vector3 pos = ToWorldPosition(latLngPoints[i]);
            pathPositions.Add(pos);
            timeStamps.Add(timestamps[i]);
        }

        if (elevation != null && elevation.Count == latLngPoints.Count)
            elevations = elevation;

        if (runnerInstance == null)
            runnerInstance = Instantiate(runnerPrefab).transform;

        runnerInstance.position = pathPositions[0];

        // Draw the ghost route line
        DrawRoute(latLngPoints);

        if (showDebugLines)
            DrawPathDebug();

        StartCoroutine(AnimateRunner());
    }

    // === Animate the Ghost Runner ===
    private IEnumerator AnimateRunner()
    {
        isPlaying = true;

        while (currentIndex < pathPositions.Count - 1)
        {
            float timeStart = timeStamps[currentIndex];
            float timeEnd = timeStamps[currentIndex + 1];

            float duration = Mathf.Max((timeEnd - timeStart) / speedMultiplier, 0.01f);
            Vector3 startPos = pathPositions[currentIndex];
            Vector3 endPos = pathPositions[currentIndex + 1];

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                runnerInstance.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            currentIndex++;
        }

        isPlaying = false;
        Debug.Log("Ghost playback completed.");
    }

    // === Convert LatLng to World Position ===
    public Vector3 ToWorldPosition(Vector2 latLon)
    {
        float scaleFactor = 10f;

        float deltaLat = latLon.x - baseLatLon.x;
        float deltaLon = latLon.y - baseLatLon.y;

        Vector3 offset = new Vector3(deltaLon * scaleFactor, 0, deltaLat * scaleFactor);
        return arOrigin + offset;
    }

    // === Debug Draw Path ===
    private void DrawPathDebug()
    {
        for (int i = 0; i < pathPositions.Count - 1; i++)
        {
            Debug.DrawLine(pathPositions[i], pathPositions[i + 1], Color.cyan, 10f);
        }
    }

    // === Draw Ghost Route Line ===
    public void DrawRoute(List<Vector2> latlngList)
    {
        if (routeLine == null)
        {
            Debug.LogWarning("Route LineRenderer is not assigned.");
            return;
        }

        if (latlngList == null || latlngList.Count == 0)
        {
            Debug.LogWarning("LatLng list is empty.");
            return;
        }

        routeLine.positionCount = latlngList.Count;

        for (int i = 0; i < latlngList.Count; i++)
        {
            Vector3 worldPos = ToWorldPosition(latlngList[i]);
            routeLine.SetPosition(i, worldPos);
        }

        Debug.Log($" Route drawn with {latlngList.Count} points.");
    }

    // === Public API ===
    public bool IsPlaying() => isPlaying;
}
