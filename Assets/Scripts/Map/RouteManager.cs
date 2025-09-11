using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class RouteManager : MonoBehaviour
{
    [Header("References")]
    public MapClickHandler mapClickHandler;
    public StaticMapLoader mapLoader;   
    public RectTransform mapRect;       
    public LineRenderer lineRenderer;   
    public RectTransform uiPlayerIcon;  

    [Header("3D Player")]
    public GameObject playerPrefab;     
    private PlayerMover playerMover;    // Reference to PlayerMover

    [Header("Mapbox")]
    public string mapboxToken = "YOUR_MAPBOX_ACCESS_TOKEN";
    private const string baseUrl = "https://api.mapbox.com/directions/v5/mapbox/walking/";

    private List<Vector3> uiRoutePoints = new List<Vector3>();
    private List<Vector3> worldRoutePoints = new List<Vector3>();

    private void Start()
    {
        if (lineRenderer != null)
            lineRenderer.useWorldSpace = false; // route drawn on UI canvas
    }

    public void RequestRoute()
    {
        if (!mapClickHandler.HasBothPoints())
        {
            Debug.LogWarning("⚠️ Start and End points not set!");
            return;
        }
        StartCoroutine(GetRoute());
    }

    private IEnumerator GetRoute()
    {
        var start = mapClickHandler.GetStart(); // (lat, lon)
        var end = mapClickHandler.GetEnd();

        string url = $"{baseUrl}{start.y},{start.x};{end.y},{end.x}?geometries=geojson&access_token={mapboxToken}";
        Debug.Log("📡 Requesting route: " + url);

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string jsonText = www.downloadHandler.text;
            var coords = ExtractCoordinates(jsonText);

            if (coords.Count > 0)
            {
                DrawRoute(coords);
            }
            else
            {
                Debug.LogError("❌ Failed to parse coordinates from response");
            }
        }
        else
        {
            Debug.LogError("❌ Route request failed: " + www.error);
        }
    }

    private List<Vector2> ExtractCoordinates(string jsonText)
    {
        List<Vector2> coords = new List<Vector2>();

        int startIdx = jsonText.IndexOf("\"coordinates\":");
        if (startIdx == -1) return coords;

        startIdx = jsonText.IndexOf("[[", startIdx);
        int endIdx = jsonText.IndexOf("]]", startIdx);
        if (startIdx == -1 || endIdx == -1) return coords;

        string coordBlock = jsonText.Substring(startIdx + 2, endIdx - startIdx - 2);
        string[] pairs = coordBlock.Split(new string[] { "],[" }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string pair in pairs)
        {
            string[] parts = pair.Split(',');
            if (parts.Length == 2 &&
                float.TryParse(parts[0], out float lon) &&
                float.TryParse(parts[1], out float lat))
            {
                coords.Add(new Vector2(lat, lon));
            }
        }
        return coords;
    }

    private void DrawRoute(List<Vector2> coords)
    {
        Debug.Log($"🛣️ Drawing route with {coords.Count} points");

        uiRoutePoints.Clear();
        worldRoutePoints.Clear();

        lineRenderer.positionCount = coords.Count;

        for (int i = 0; i < coords.Count; i++)
        {
            // UI
            Vector3 uiPos = LatLonToUI(coords[i].x, coords[i].y);
            uiRoutePoints.Add(uiPos);
            lineRenderer.SetPosition(i, uiPos);

            // World
            Vector3 worldPos = LatLonToWorld(coords[i].x, coords[i].y);
            worldRoutePoints.Add(worldPos);
        }

        // Spawn player with PlayerMover
        if (playerPrefab != null && worldRoutePoints.Count > 1)
        {
            if (playerMover == null)
            {
                GameObject instance = Instantiate(playerPrefab, worldRoutePoints[0], Quaternion.identity);
                playerMover = instance.GetComponent<PlayerMover>();
            }
            playerMover.SetPath(worldRoutePoints);
        }
    }

private Vector3 LatLonToUI(float lat, float lon)
{
    float normalizedX = ((float)lon + 180f) / 360f;
    float normalizedY = ((float)lat + 90f) / 180f;

    float x = (normalizedX * mapRect.rect.width) - (mapRect.rect.width / 2f);
    float y = (normalizedY * mapRect.rect.height) - (mapRect.rect.height / 2f);

    return new Vector3(x, y, 0);
}


private Vector3 LatLonToWorld(float lat, float lon)
{
    float scale = 1000f; // adjust as needed
    float x = ((float)lon - (float)mapLoader.lon) * scale;
    float z = ((float)lat - (float)mapLoader.lat) * scale;
    return new Vector3(x, 0, z);
}

}
