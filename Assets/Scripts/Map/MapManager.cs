using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Newtonsoft.Json;
using System.Globalization;

public class MapManager : MonoBehaviour
{
    [Header("References")]
    public AbstractMap map;
    public Camera mainCamera;
    public GameObject startPinPrefab;
    public GameObject endPinPrefab;
    public LineRenderer routeLinePrefab; // set LineRenderer prefab in inspector
    public GameObject playerPrefab;

    [Header("Mapbox")]
    public string mapboxAccessToken; // set in inspector (or use Mapbox config)
    public string routingProfile = "walking"; // "walking" | "driving" | "cycling"

    [Header("Player")]
    public float playerSpawnHeight = 1.0f;

    private Vector2d? startLatLon;
    private Vector2d? endLatLon;
    private GameObject startMarker;
    private GameObject endMarker;
    private LineRenderer routeLine;
    private GameObject playerInstance;
    private List<Vector3> routeWorldPositions;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Raycast to map
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Convert world hit to lat/lon
                Vector2d clickedLatLon = map.WorldToGeoPosition(hit.point);

                if (!startLatLon.HasValue)
                {
                    startLatLon = clickedLatLon;
                    PlaceMarker(ref startMarker, startPinPrefab, clickedLatLon);
                    Debug.Log("Start set: " + clickedLatLon);
                }
                else if (!endLatLon.HasValue)
                {
                    endLatLon = clickedLatLon;
                    PlaceMarker(ref endMarker, endPinPrefab, clickedLatLon);
                    Debug.Log("End set: " + clickedLatLon);
                    // Request route now
                    StartCoroutine(RequestRouteAndDraw(startLatLon.Value, endLatLon.Value));
                }
                else
                {
                    // If both set, reset and set new start
                    ClearAll();
                    startLatLon = clickedLatLon;
                    PlaceMarker(ref startMarker, startPinPrefab, clickedLatLon);
                }
            }
        }
    }

    void PlaceMarker(ref GameObject marker, GameObject prefab, Vector2d latlon)
    {
        if (marker != null) Destroy(marker);
        Vector3 pos = map.GeoToWorldPosition(latlon, true);
        pos.y += 0.05f; // slight lift
        marker = Instantiate(prefab, pos, Quaternion.identity, map.transform);
    }

    IEnumerator RequestRouteAndDraw(Vector2d start, Vector2d end)
    {
        // Build Mapbox Directions URL:
        // Mapbox needs lon,lat pairs
        string startStr = $"{start.y.ToString(CultureInfo.InvariantCulture)},{start.x.ToString(CultureInfo.InvariantCulture)}";
        string endStr = $"{end.y.ToString(CultureInfo.InvariantCulture)},{end.x.ToString(CultureInfo.InvariantCulture)}";

        string url = $"https://api.mapbox.com/directions/v5/mapbox/{routingProfile}/{startStr};{endStr}" +
                     $"?geometries=geojson&overview=full&steps=false&access_token={mapboxAccessToken}";

        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Directions request failed: " + uwr.error + " | " + uwr.downloadHandler.text);
                yield break;
            }

            var json = uwr.downloadHandler.text;
            var resp = JsonConvert.DeserializeObject<MapboxDirectionsResponse>(json);
            if (resp == null || resp.routes == null || resp.routes.Count == 0)
            {
                Debug.LogError("No route returned from Mapbox.");
                yield break;
            }

            var coordinates = resp.routes[0].geometry.coordinates; // list of [lon,lat] pairs
            routeWorldPositions = new List<Vector3>(coordinates.Count);

            foreach (var lonlat in coordinates)
            {
                double lon = lonlat[0];
                double lat = lonlat[1];
                Vector2d latlon = new Vector2d(lat, lon); // Vector2d(lat, lon)
                Vector3 worldPos = map.GeoToWorldPosition(latlon, true);
                worldPos.y += playerSpawnHeight;
                routeWorldPositions.Add(worldPos);
            }

            DrawRoute(routeWorldPositions);
            SpawnPlayerAt(routeWorldPositions[0]);
            var mover = playerInstance.GetComponent<PlayerMover>();
            if (mover != null) mover.SetPath(routeWorldPositions);
        }
    }

    void DrawRoute(List<Vector3> pts)
    {
        if (routeLine == null)
        {
            routeLine = Instantiate(routeLinePrefab, map.transform);
            routeLine.positionCount = 0;
        }

        routeLine.positionCount = pts.Count;
        routeLine.SetPositions(pts.ToArray());
    }

    void SpawnPlayerAt(Vector3 pos)
    {
        if (playerInstance != null) Destroy(playerInstance);
        playerInstance = Instantiate(playerPrefab, pos, Quaternion.identity);
    }

    public void ClearAll()
    {
        if (startMarker != null) Destroy(startMarker);
        if (endMarker != null) Destroy(endMarker);
        if (routeLine != null) Destroy(routeLine.gameObject);
        if (playerInstance != null) Destroy(playerInstance);
        startLatLon = null;
        endLatLon = null;
        routeWorldPositions = null;
    }

    // Mapbox directions JSON classes (only the geometry we need)
    [System.Serializable]
    public class MapboxDirectionsResponse { public List<Route> routes; }
    [System.Serializable]
    public class Route { public Geometry geometry; }
    [System.Serializable]
    public class Geometry { public List<List<double>> coordinates; }
}
