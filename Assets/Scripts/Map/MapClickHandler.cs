using UnityEngine;
using UnityEngine.UI;
using Mapbox.Utils;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MapClickHandler : MonoBehaviour
{
    [Header("References")]
    public StaticMapLoader mapLoader;   // Your StaticMapLoader script
    public RectTransform mapRect;       // The RawImage RectTransform
    public Button routeButton;          // Button to request route

    private Vector2d startPos = Vector2d.zero;
    private Vector2d endPos = Vector2d.zero;
    private bool selectingStart = true;
    private bool pointsLocked = false;

    void Start()
    {
        if (routeButton != null)
            routeButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (pointsLocked) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (TryGetLatLonFromClick(Input.mousePosition, out Vector2d latLon))
            {
                if (selectingStart)
                {
                    startPos = latLon;
                    selectingStart = false;
                    Debug.Log($"✅ Start position set: {startPos}");
                }
                else
                {
                    endPos = latLon;
                    selectingStart = true;
                    Debug.Log($"✅ End position set: {endPos}");

                    if (routeButton != null)
                        routeButton.gameObject.SetActive(true);
                }
            }
        }
    }

    /// <summary>
    /// Converts a screen click into a lat/lon, only if the click is on the RawImage map.
    /// </summary>
    private bool TryGetLatLonFromClick(Vector2 screenPos, out Vector2d latLon)
    {
        latLon = Vector2d.zero;

        // Raycast check against UI
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = screenPos;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        bool clickedOnMap = false;
        foreach (var result in results)
        {
            // ✅ Ensure the clicked object is THIS map RawImage
            if (result.gameObject.GetComponent<RawImage>() != null &&
                result.gameObject.transform == mapRect.transform)
            {
                clickedOnMap = true;
                break;
            }
        }

        if (!clickedOnMap) return false; // ❌ Ignore clicks outside map

        // Convert to local point inside RawImage
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(mapRect, screenPos, null, out Vector2 localPoint))
        {
            float u = (localPoint.x + mapRect.rect.width * 0.5f) / mapRect.rect.width;
            float v = (localPoint.y + mapRect.rect.height * 0.5f) / mapRect.rect.height;

            latLon = PixelToLatLon(u, v, mapLoader.lat, mapLoader.lon, mapLoader.GetZoom(), mapLoader.width, mapLoader.height);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Pixel to Lat/Lon conversion helpers (Web Mercator projection).
    /// </summary>
    private Vector2d PixelToLatLon(float u, float v, double centerLat, double centerLon, int zoom, int width, int height)
    {
        Vector2d centerPixel = LatLonToPixel(centerLat, centerLon, zoom);

        double dx = (u - 0.5) * width;
        double dy = (v - 0.5) * height;

        double px = centerPixel.x + dx;
        double py = centerPixel.y + dy;

        return PixelToLatLon(px, py, zoom);
    }

    private Vector2d LatLonToPixel(double lat, double lon, int zoom)
    {
        double mapSize = 256 * System.Math.Pow(2, zoom);
        double x = (lon + 180.0) / 360.0 * mapSize;
        double sinLat = System.Math.Sin(lat * System.Math.PI / 180.0);
        double y = (0.5 - System.Math.Log((1 + sinLat) / (1 - sinLat)) / (4 * System.Math.PI)) * mapSize;
        return new Vector2d(x, y);
    }

    private Vector2d PixelToLatLon(double x, double y, int zoom)
    {
        double mapSize = 256 * System.Math.Pow(2, zoom);
        double lon = x / mapSize * 360.0 - 180.0;
        double n = System.Math.PI - 2.0 * System.Math.PI * y / mapSize;
        double lat = 180.0 / System.Math.PI * System.Math.Atan(0.5 * (System.Math.Exp(n) - System.Math.Exp(-n)));
        return new Vector2d(lat, lon);
    }

    // ✅ Helpers for RouteManager
    public bool HasBothPoints() => startPos != Vector2d.zero && endPos != Vector2d.zero;
    public Vector2d GetStart() => startPos;
    public Vector2d GetEnd() => endPos;

    public void LockPoints() => pointsLocked = true;
}
