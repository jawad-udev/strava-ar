using UnityEngine;
using UnityEngine.UI;

public class MapClickHandler : MonoBehaviour
{
    // [Header("References")]
    // public StaticMapLoader mapLoader;
    // public RectTransform mapRect;

    // private bool startSelected = false;
    // private Vector2d startPos;
    // private Vector2d endPos;

    // void Update()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         Vector2 localPoint;
    //         if (RectTransformUtility.ScreenPointToLocalPointInRectangle(mapRect, Input.mousePosition, null, out localPoint))
    //         {
    //             // Normalize click to 0–1 coordinates
    //             Vector2 normalized = new Vector2(
    //                 (localPoint.x + mapRect.rect.width * 0.5f) / mapRect.rect.width,
    //                 (localPoint.y + mapRect.rect.height * 0.5f) / mapRect.rect.height
    //             );

    //             // Convert normalized point → lat/lon
    //             Vector2d latLon = ScreenToLatLon(normalized, mapLoader.lat, mapLoader.lon, mapLoader.GetZoom());

    //             if (!startSelected)
    //             {
    //                 startPos = latLon;
    //                 startSelected = true;
    //                 Debug.Log("Start position set: " + startPos);
    //             }
    //             else
    //             {
    //                 endPos = latLon;
    //                 startSelected = false;
    //                 Debug.Log("End position set: " + endPos);

    //                 // ✅ Example: Here you can request a route from Mapbox Directions API
    //             }
    //         }
    //     }
    // }

    // private Vector2d ScreenToLatLon(Vector2 normalized, double centerLat, double centerLon, int zoom)
    // {
    //     // Simple approximate conversion for demo purposes
    //     double worldSize = 256 * Mathf.Pow(2, zoom);

    //     double lon = (normalized.x - 0.5) * 360.0 + centerLon;
    //     double lat = (0.5 - normalized.y) * 180.0 + centerLat;

    //     return new Vector2d(lat, lon);
    // }
}

// Helper struct
// [System.Serializable]
// public struct Vector2d
// {
//     public double x;
//     public double y;
//     public Vector2d(double x, double y) { this.x = x; this.y = y; }
//     public override string ToString() => $"({x}, {y})";
// }
