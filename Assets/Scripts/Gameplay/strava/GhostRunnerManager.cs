using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GhostRunnerManager
/// - Builds a route spline from Strava lat/lon/time streams
/// - Advances a ghost runner in sync with the player's progress (map-matched to the route)
/// - Supports lead by meters or by recorded time
/// - Plays a running animation based on world velocity
/// - Can be anchored anywhere and draped to the ground (Physics raycasts)
/// </summary>
public class GhostRunnerManager : MonoBehaviour
{
    // -------------------------
    // Inspector-configurable
    // -------------------------
    [Header("Ghost")]
    [Tooltip("Prefab of the ghost runner character (should have Animator).")]
    public GameObject runnerPrefab;
    public Transform runnerInstance;

    [Tooltip("Keep the ghost this many meters ahead of the runner (set 0 to disable).")]
    public float leadMeters = 4f;

    [Tooltip("Or keep the ghost this many seconds ahead along the recorded Strava timing (set 0 to disable).")]
    public float leadSeconds = 0f;

    [Range(0.01f, 1f)] public float followLerp = 0.18f;
    [Range(0.01f, 1f)] public float rotLerp = 0.18f;

    [Header("Animation")]
    [Tooltip("Animator float parameter for speed (m/s).")]
    public string speedParam = "Speed";
    [Tooltip("If true, scales Animator.speed from ghost velocity.")]
    public bool scaleAnimatorSpeed = true;
    [Tooltip("Velocity (m/s) that maps to Animator.speed = 1.0")]
    public float animNormSpeedMS = 3.0f;
    [Tooltip("Clamp range for Animator.speed when scaling.")]
    public Vector2 animSpeedClamp = new Vector2(0.6f, 1.6f);

    [Header("Route Drawing")]
    [SerializeField] private LineRenderer routeLine;

    [Header("Grounding")]
    [Tooltip("Small lift to avoid z-fighting with the plane.")]
    [SerializeField] private float footOffset = 0.02f;

    [Header("Debug")]
    public bool showDebugLines = false;

    // -------------------------
    // Internal state
    // -------------------------
    // Raw streams (Strava)
    private List<Vector2> latLngRaw = new List<Vector2>(); // (lat, lon)
    private List<float> timeStamps = new List<float>();  // seconds from start

    // Route & pacing
    private Route route;            // spline nodes in world space (meters)
    private PaceProfile pace;       // S(t) and T(s)

    // Player tracking / map-matching
    private MapMatcher matcher;
    private float sYou = 0f;        // runner cumulative distance along route (meters)
    private float sYouEMA = 0f;     // smoothed along-track distance
    private float lateralError = 0f;

    // Coordinate origin
    private Vector2 originLatLon;   // activity start lat/lon
    private Vector3 worldOrigin;    // where (0,0) meters sits in world before anchoring

    // Anchor
    private Transform anchorParent; // where this manager is parented (tap-to-place)

    // Animation / velocity
    private Animator runnerAnimator;
    private Vector3 lastGhostPos;
    private float ghostVelMS;       // smoothed m/s
    private float velEMA_tau = 0.25f;

    // Flags
    private bool isInitialized = false;
    private bool _debugDrawn = false;

    void Awake()
    {
        Services.GameService.ghostRunner = this;
    }

    // =========================
    // Public API
    // =========================

    /// <summary>
    /// Initialize from Strava lat/lon/time streams.
    /// timestampsSeconds must match latLngPoints.Count.
    /// </summary>
    public void Init(List<Vector2> latLngPoints, List<float> timestampsSeconds)
    {
        if (latLngPoints == null || timestampsSeconds == null ||
            latLngPoints.Count < 2 || latLngPoints.Count != timestampsSeconds.Count)
        {
            Debug.LogError("GhostRunnerManager.Init: invalid streams.");
            return;
        }

        // Store raw streams
        latLngRaw.Clear(); timeStamps.Clear();
        latLngRaw.AddRange(latLngPoints);
        timeStamps.AddRange(timestampsSeconds);

        // Choose a provisional world origin (can be anything; often Camera pose at init)
        worldOrigin = GetProvisionalWorldOrigin();
        originLatLon = latLngRaw[0];

        // Build route nodes in world space + cumulative distance S and times T
        var nodes = new List<Vector3>(latLngRaw.Count);
        float[] S = new float[latLngRaw.Count];
        float[] T = new float[timeStamps.Count];

        Vector3 prev = LatLonToMeters(originLatLon, latLngRaw[0]) + worldOrigin;
        nodes.Add(prev);
        float cum = 0f;

        for (int i = 1; i < latLngRaw.Count; i++)
        {
            Vector3 p = LatLonToMeters(originLatLon, latLngRaw[i]) + worldOrigin;
            cum += Vector3.Distance(prev, p);
            nodes.Add(p);
            S[i] = cum;
            prev = p;
        }
        for (int i = 0; i < timeStamps.Count; i++) T[i] = timeStamps[i];

        route = new Route(nodes.ToArray());
        pace = new PaceProfile(T, S);
        matcher = new MapMatcher(route);

        // Spawn ghost if needed
        if (runnerInstance == null && runnerPrefab != null)
            runnerInstance = Instantiate(runnerPrefab).transform;
        if (runnerInstance == null)
        {
            Debug.LogError("GhostRunnerManager: runnerPrefab/instance missing.");
            return;
        }

        runnerInstance.position = route.Sample(0f);
        lastGhostPos = runnerInstance.position;

        // Animator
        runnerAnimator = runnerInstance.GetComponent<Animator>();
        if (runnerAnimator == null)
            Debug.LogWarning("GhostRunnerManager: Animator not found on runnerPrefab. Animation won't play.");

        DrawRoute();
        if (showDebugLines) DebugDrawPathOnce();

        // Reset trackers
        isInitialized = true;
        sYou = 0f; sYouEMA = 0f; ghostVelMS = 0f;
    }

    /// <summary>
    /// Attach the whole system under a given anchor (e.g., after tap-to-place).
    /// </summary>
    public void AttachToAnchor(Transform anchor)
    {
        if (anchor == null) return;
        anchorParent = anchor;
        // Parent this manager so content is locked under the anchor
        transform.SetParent(anchorParent, worldPositionStays: true);

        // Keep ghost at route start (we'll drape Y later)
        if (runnerInstance != null && route != null)
            runnerInstance.position = route.Sample(0f);
    }

    /// <summary>
    /// Drop the route nodes down onto colliders in groundMask so the ghost runs on the ground plane.
    /// Call this AFTER AttachToAnchor (tap placement).
    /// </summary>
    public void DrapeRouteToGround(LayerMask groundMask)
    {
        if (route == null || route.nodes == null || route.nodes.Length == 0) return;

        for (int i = 0; i < route.nodes.Length; i++)
        {
            Vector3 p = route.nodes[i];
            Vector3 origin = p + Vector3.up * 10f;
            if (Physics.Raycast(origin, Vector3.down, out var hit, 50f, groundMask))
            {
                route.nodes[i] = new Vector3(p.x, hit.point.y + footOffset, p.z);
            }
            else
            {
                // Fallback: use anchor Y if available
                float baseY = (anchorParent != null) ? anchorParent.position.y : p.y;
                route.nodes[i] = new Vector3(p.x, baseY + footOffset, p.z);
            }
        }

        // Refresh visuals
        DrawRoute();

        // Put ghost exactly on ground at start
        if (runnerInstance != null)
        {
            var start = route.Sample(0f);
            float y = route.nodes.Length > 0 ? route.nodes[0].y : start.y;
            runnerInstance.position = new Vector3(start.x, y, start.z);
        }
    }

    /// <summary>Feed latest GPS fix; called whenever you get a new location update.</summary>
    public void SetPlayerLatLon(double lat, double lon, float accuracyMeters = 0f)
    {
        if (!isInitialized || route == null || matcher == null) return;
        if (accuracyMeters > 0f && accuracyMeters > 30f) return; // ignore terrible fixes

        Vector3 pos = LatLonToMeters(originLatLon, new Vector2((float)lat, (float)lon)) +
                      (anchorParent != null ? anchorParent.position : worldOrigin);

        (float sProj, float latErr) = matcher.Project(pos);
        lateralError = latErr;

        // Prevent big backward snaps; allow minor retreats
        float sClamped = (sProj + 0.2f < sYou) ? sYou - 0.2f : sProj;
        sYou = Mathf.Clamp(sClamped, 0f, route.Length);

        // EMA smoothing of along-track distance
        float alphaS = 1f - Mathf.Exp(-4f * Time.deltaTime);
        sYouEMA = Mathf.Lerp(sYouEMA, sYou, alphaS);
    }

    /// <summary>Is the ghost manager initialized (streams loaded, route built)?</summary>
    public bool IsInitialized() => isInitialized;

    /// <summary>Gap between player and ghost in meters (positive => player ahead).</summary>
    public float GetGapMeters()
    {
        if (!isInitialized || route == null) return 0f;
        float sGhost = (leadSeconds > 0.01f)
            ? pace.SAtTime(pace.TAtDist(sYouEMA) + leadSeconds)
            : sYouEMA + Mathf.Max(leadMeters, 0f);
        sGhost = Mathf.Clamp(sGhost, 0f, route.Length);
        return sYou - sGhost;
    }

    /// <summary>Map-matching lateral error (meters).</summary>
    public float GetLateralErrorMeters() => lateralError;

    /// <summary>World-space start position of the route.</summary>
    public Vector3 GetStartWorldPos() => route != null ? route.nodes[0] : transform.position;

    /// <summary>World-space end position of the route.</summary>
    public Vector3 GetEndWorldPos() => route != null ? route.nodes[^1] : transform.position;

    // =========================
    // Unity loop
    // =========================
    private void Update()
    {
        if (!isInitialized || runnerInstance == null || route == null) return;

        // Decide where the ghost should be along the route
        float sGhost;
        if (leadSeconds > 0.01f)
        {
            float tGhost = pace.TAtDist(sYouEMA) + leadSeconds;
            sGhost = pace.SAtTime(tGhost);
        }
        else
        {
            sGhost = sYouEMA + Mathf.Max(leadMeters, 0f);
        }
        sGhost = Mathf.Clamp(sGhost, 0f, route.Length);

        // Move/rotate ghost
        Vector3 targetPos = route.Sample(sGhost);
        Vector3 newPos = Vector3.Lerp(runnerInstance.position, targetPos, followLerp);
        runnerInstance.position = newPos;

        Vector3 fwd = route.Tangent(sGhost);
        if (fwd.sqrMagnitude > 1e-6f)
        {
            Quaternion to = Quaternion.LookRotation(fwd, Vector3.up);
            runnerInstance.rotation = Quaternion.Slerp(runnerInstance.rotation, to, rotLerp);
        }

        // Animation: estimate velocity and drive Animator
        float instVel = (Time.deltaTime > 1e-4f)
            ? Vector3.Distance(newPos, lastGhostPos) / Time.deltaTime
            : 0f;

        float alphaV = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(velEMA_tau, 1e-3f));
        ghostVelMS = Mathf.Lerp(ghostVelMS, instVel, alphaV);
        lastGhostPos = newPos;

        if (runnerAnimator != null)
        {
            runnerAnimator.SetFloat(speedParam, ghostVelMS);
            if (scaleAnimatorSpeed)
            {
                float norm = (animNormSpeedMS <= 0.01f) ? 1f : ghostVelMS / animNormSpeedMS;
                runnerAnimator.speed = Mathf.Clamp(norm, animSpeedClamp.x, animSpeedClamp.y);
            }
        }

        if (showDebugLines) DebugDrawPathOnce();
    }

    // =========================
    // Helpers
    // =========================

    private Vector3 GetProvisionalWorldOrigin()
    {
        // Before anchoring, use camera as a reasonable local origin
        return Camera.main != null ? Camera.main.transform.position : Vector3.zero;
    }

    // Local meters using equirectangular projection around origin
    private static Vector3 LatLonToMeters(Vector2 originLatLon, Vector2 latLon)
    {
        const double R = 6378137.0; // WGS84
        double lat0 = originLatLon.x * Mathf.Deg2Rad;
        double lon0 = originLatLon.y * Mathf.Deg2Rad;
        double lat = latLon.x * Mathf.Deg2Rad;
        double lon = latLon.y * Mathf.Deg2Rad;

        double dLat = lat - lat0;
        double dLon = (lon - lon0) * Math.Cos(0.5 * (lat + lat0));
        float x = (float)(dLon * R); // east
        float z = (float)(dLat * R); // north
        return new Vector3(x, 0f, z);
    }

    private void DrawRoute()
    {
        if (routeLine == null || route == null) return;
        routeLine.positionCount = route.nodes.Length;
        for (int i = 0; i < route.nodes.Length; i++)
            routeLine.SetPosition(i, route.nodes[i]);
    }

    private void DebugDrawPathOnce()
    {
        if (_debugDrawn || route == null) return;
        for (int i = 0; i < route.nodes.Length - 1; i++)
            Debug.DrawLine(route.nodes[i], route.nodes[i + 1], Color.cyan, 10f);
        _debugDrawn = true;
    }

    // =========================
    // Internal classes
    // =========================

    private class Route
    {
        public readonly Vector3[] nodes;
        public readonly float[] s; // cumulative length (meters)
        public float Length => s[^1];

        public Route(Vector3[] nodes)
        {
            this.nodes = nodes;
            s = new float[nodes.Length];
            s[0] = 0f;
            for (int i = 1; i < nodes.Length; i++)
                s[i] = s[i - 1] + Vector3.Distance(nodes[i - 1], nodes[i]);
        }

        public Vector3 Sample(float dist)
        {
            dist = Mathf.Clamp(dist, 0f, Length);
            int i = Array.BinarySearch(s, dist);
            if (i >= 0) return nodes[i];
            i = ~i;
            int i0 = Mathf.Clamp(i - 1, 0, nodes.Length - 2);
            float span = s[i0 + 1] - s[i0];
            float u = span > 1e-4f ? (dist - s[i0]) / span : 0f;
            return Vector3.LerpUnclamped(nodes[i0], nodes[i0 + 1], u);
        }

        public Vector3 Tangent(float dist)
        {
            dist = Mathf.Clamp(dist, 0f, Length);
            int i = Array.BinarySearch(s, dist);
            if (i <= 0) i = ~i;
            int i0 = Mathf.Clamp(i - 1, 0, nodes.Length - 2);
            return (nodes[i0 + 1] - nodes[i0]).normalized;
        }
    }

    private class PaceProfile
    {
        private readonly float[] t; // seconds
        private readonly float[] S; // meters

        public float TotalTime => t[^1];
        public float TotalDist => S[^1];

        public PaceProfile(float[] timeSeconds, float[] cumMeters)
        {
            t = timeSeconds; S = cumMeters;
        }

        public float SAtTime(float timeSec)
        {
            timeSec = Mathf.Clamp(timeSec, 0f, TotalTime);
            int i = Array.BinarySearch(t, timeSec);
            if (i >= 0) return S[i];
            i = ~i;
            int i0 = Mathf.Clamp(i - 1, 0, t.Length - 2);
            float span = t[i0 + 1] - t[i0];
            float u = span > 1e-4f ? (timeSec - t[i0]) / span : 0f;
            return Mathf.Lerp(S[i0], S[i0 + 1], u);
        }

        public float TAtDist(float dist)
        {
            dist = Mathf.Clamp(dist, 0f, TotalDist);
            int i = Array.BinarySearch(S, dist);
            if (i >= 0) return t[i];
            i = ~i;
            int i0 = Mathf.Clamp(i - 1, 0, S.Length - 2);
            float span = S[i0 + 1] - S[i0];
            float u = span > 1e-4f ? (dist - S[i0]) / span : 0f;
            return Mathf.Lerp(t[i0], t[i0 + 1], u);
        }
    }

    private class MapMatcher
    {
        private readonly Route route;
        private int lastIdx = 0;

        public MapMatcher(Route route) { this.route = route; }

        /// <summary>
        /// Project world pos onto the route; returns (sAlong meters, lateral error meters).
        /// Uses a sliding window around last best segment for stability.
        /// </summary>
        public (float sAlong, float lateralError) Project(Vector3 pos, int window = 24)
        {
            int start = Mathf.Max(0, lastIdx - 2);
            int end = Mathf.Min(route.nodes.Length - 2, lastIdx + window);

            float bestS = 0f, bestLat = float.MaxValue;
            int bestI = lastIdx;

            for (int i = start; i <= end; i++)
            {
                Vector3 a = route.nodes[i];
                Vector3 b = route.nodes[i + 1];
                Vector3 ab = b - a;
                float ab2 = Vector3.Dot(ab, ab);
                float u = ab2 > 1e-6f ? Mathf.Clamp01(Vector3.Dot(pos - a, ab) / ab2) : 0f;
                Vector3 p = a + u * ab;
                float latErr = (pos - p).magnitude;
                if (latErr < bestLat)
                {
                    bestLat = latErr;
                    bestS = route.s[i] + u * (route.s[i + 1] - route.s[i]);
                    bestI = i;
                }
            }
            lastIdx = bestI;
            return (bestS, bestLat);
        }
    }
}
