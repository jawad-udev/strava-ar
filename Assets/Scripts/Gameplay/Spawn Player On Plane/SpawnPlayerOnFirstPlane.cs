using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlaneManager))]
public class SpawnPlayerOnPlane : MonoBehaviour
{
    public GameObject playerPrefab;
    public float heightOffset = 0.05f;

    private ARPlaneManager planeManager;
    private bool hasSpawned = false;

    void Awake()
    {
        planeManager = GetComponent<ARPlaneManager>();
    }

    void Update()
    {
        if (hasSpawned || playerPrefab == null)
            return;

        // If at least one plane exists
        foreach (var plane in planeManager.trackables)
        {
            if (plane.trackingState == TrackingState.Tracking)
            {
                SpawnOnPlane(plane);
                break; // spawn once, then stop
            }
        }
    }

    private void SpawnOnPlane(ARPlane plane)
    {
        // Plane center in world space
        Vector3 spawnPos = plane.transform.TransformPoint(plane.center);

        // Adjust based on prefab pivot:
        Renderer rend = playerPrefab.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            float prefabHeight = rend.bounds.size.y;
            // Move down by half its height if pivot is at center
            spawnPos.y += prefabHeight / 2f * -1f;
        }

        // Face toward the camera
        Vector3 lookDir = Camera.main.transform.position - spawnPos;
        lookDir.y = 0f;
        Quaternion spawnRot = Quaternion.LookRotation(lookDir);

        Instantiate(playerPrefab, spawnPos, spawnRot);
        hasSpawned = true;

        // Stop detection if desired
        planeManager.requestedDetectionMode = PlaneDetectionMode.None;
        foreach (var p in planeManager.trackables)
            p.gameObject.SetActive(false);
    }

}
