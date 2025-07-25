using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostRunnerManager : MonoBehaviour
{
    public GameObject runnerPrefab; // Your ghost runner prefab
    public Transform runnerInstance;
    public float speedMultiplier = 1.0f; // To control playback speed

    // Path data from Strava
    public List<Vector3> pathPositions = new List<Vector3>();
    public List<float> timeStamps = new List<float>(); // in seconds
    public List<float> elevations = new List<float>(); // optional for future

    private int currentIndex = 0;
    private float currentTime = 0f;

    public void Init(List<List<float>> latlng, List<float> timestamps, List<float> elevation = null)
    {
        pathPositions.Clear();
        timeStamps.Clear();

        for (int i = 0; i < latlng.Count; i++)
        {
            Vector3 pos = GeoToWorld(latlng[i][0], latlng[i][1]);
            pathPositions.Add(pos);
        }

        timeStamps = timestamps;
        if (elevation != null)
            elevations = elevation;

        if (runnerInstance == null)
            runnerInstance = Instantiate(runnerPrefab).transform;

        runnerInstance.position = pathPositions[0];
        currentIndex = 0;
        currentTime = 0f;

        StartCoroutine(AnimateRunner());
    }

    private IEnumerator AnimateRunner()
    {
        while (currentIndex < pathPositions.Count - 1)
        {
            float timeBetweenPoints = (timeStamps[currentIndex + 1] - timeStamps[currentIndex]) / speedMultiplier;
            Vector3 start = pathPositions[currentIndex];
            Vector3 end = pathPositions[currentIndex + 1];

            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / timeBetweenPoints;
                runnerInstance.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            currentIndex++;
        }
    }

    private Vector3 GeoToWorld(float lat, float lng)
    {
        // Simple flat map projection – convert lat/lng to Unity world coords
        float scale = 1000f; // adjust as needed
        return new Vector3(lng * scale, 0f, lat * scale);
    }
}
