using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class RouteManager : MonoBehaviour
{
    [Header("References")]
    public MapClickHandler mapClickHandler;
    public StaticMapLoader mapLoader;   // Your static map script
    public RectTransform mapRect;       // UI map area
    public LineRenderer lineRenderer;

    [Header("Mapbox")]
    public string mapboxToken = "YOUR_MAPBOX_ACCESS_TOKEN";
    private const string baseUrl = "https://api.mapbox.com/directions/v5/mapbox/driving/";


    private void Start()
    {
        if (lineRenderer == null)
        {
            lineRenderer.useWorldSpace = false; // For UI
        
        }
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

        // Find "coordinates":[[lon,lat],...]
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
                double.TryParse(parts[0], out double lon) &&
                double.TryParse(parts[1], out double lat))
            {
                coords.Add(new Vector2((float)lat, (float)lon)); // store as (lat,lon)
            }
        }

        return coords;
    }

 private void DrawRoute(List<Vector2> coords)
{
    Debug.Log($"🛣️ Drawing route with {coords.Count} points");
    lineRenderer.positionCount = coords.Count;

    for (int i = 0; i < coords.Count; i++)
    {
        // Show lat/lon in Console
        Debug.Log($"📍 Point {i}: Lat = {coords[i].x}, Lon = {coords[i].y}");

        Vector2 worldPos = LatLonToUI(coords[i].x, coords[i].y);
        lineRenderer.SetPosition(i, worldPos);
    }
}

    private Vector3 LatLonToUI(double lat, double lon)
    {
        // Approximate projection: map center = mapLoader.lat/lon
        float x = (float)((lon - mapLoader.lon) / 360.0 * mapRect.rect.width);
        float y = (float)((lat - mapLoader.lat) / 180.0 * mapRect.rect.height);

        // Offset so (0,0) is at map center
        return new Vector3(x, y, 0);
    }
}
