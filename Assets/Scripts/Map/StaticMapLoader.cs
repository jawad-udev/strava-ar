using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public class StaticMapLoader : MonoBehaviour
{
    [Header("UI References")]
    public RawImage mapImage;
    public Slider zoomSlider;

    [Header("Mapbox Settings")]
    [Tooltip("Your Mapbox access token here")]
    public string mapboxToken = "YOUR_MAPBOX_ACCESS_TOKEN";
    public int width = 512, height = 512;

    [Header("Default Location")]
    public double lat = 37.7749;   // Example: San Francisco
    public double lon = -122.4194;

    private int zoom = 14;
    private bool isLoading = false;

    void Start()
    {
        if (zoomSlider != null)
        {
            zoomSlider.minValue = 1;
            zoomSlider.maxValue = 20;
            zoomSlider.wholeNumbers = true;
            zoomSlider.value = zoom;
            zoomSlider.onValueChanged.AddListener(OnZoomSliderChanged);
        }

        StartCoroutine(LoadMap());
    }

    private void OnZoomSliderChanged(float value)
    {
        int newZoom = Mathf.RoundToInt(value);
        if (newZoom != zoom && !isLoading)
        {
            zoom = newZoom;
            StartCoroutine(LoadMap());
        }
    }

    public IEnumerator LoadMap()
    {
        isLoading = true;

        string url = $"https://api.mapbox.com/styles/v1/mapbox/streets-v11/static/{lon},{lat},{zoom},0,0/{width}x{height}?access_token={mapboxToken}";
        Debug.Log("Loading map: " + url);

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
            mapImage.texture = tex;
        }
        else
        {
            Debug.LogError("Map load failed: " + www.error);
        }

        isLoading = false;
    }

    // ✅ Allow external scripts (like MapClickHandler) to refresh map
    public void UpdateMap(double newLat, double newLon)
    {
        lat = newLat;
        lon = newLon;
        StartCoroutine(LoadMap());
    }

    public int GetZoom() => zoom;
}
