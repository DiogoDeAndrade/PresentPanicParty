using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitOnWin : MonoBehaviour
{
    [SerializeField] private Transform targetObject;    // The winner

    [Header("Orbit")]
    [SerializeField] private float orbitSpeed = 30f;
    [SerializeField] private float heightOffset = 1.5f;

    [Header("Intro (winner focus)")]
    [SerializeField] private float centerMoveDuration = 1.5f;
    [SerializeField] private float zoomDuration = 1.5f;
    [SerializeField] private float finalOrthoSize = 3.5f;

    [Header("Cycling between other players")]
    [SerializeField] private float cycleDelay = 4f;           // Time after SetTarget before cycling starts
    [SerializeField] private float timePerPlayerShot = 2f;    // Time each player stays on screen
    [SerializeField] private float cyclingOrbitSpeed = 20f;

    private enum State { Idle, Intro, Cycling }
    private State state = State.Idle;

    private float pitch;
    private float yaw;

    private Vector3 startCenter;
    private Vector3 endCenter;
    private float startOrthoSize;
    private float orbitRadius;

    private float timer;       // Global since intro start
    private Camera cam;

    // Cycling data
    private List<Transform> shotTargets = new();
    private int currentShotIndex = -1;
    private float shotTimer;
    private Vector3 currentShotCenter;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    /// <summary>
    /// Call this when the match ends and you know the winner.
    /// </summary>
    public void SetTarget(Transform winner)
    {
        targetObject = winner;
        if (targetObject == null)
        {
            state = State.Idle;
            return;
        }

        SetupInitialState();
        state = State.Intro;
    }

    void SetupInitialState()
    {
        // Store orientation
        Vector3 euler = transform.rotation.eulerAngles;
        pitch = euler.x;
        yaw = euler.y;

        // Initial look-at point on ground plane
        startCenter = ComputeGroundLookPoint();

        // End center: winner + offset
        endCenter = targetObject.position + Vector3.up * heightOffset;

        // Cache orthographic size and orbit radius
        startOrthoSize = cam.orthographicSize;

        orbitRadius = (transform.position - startCenter).magnitude;
        if (orbitRadius < 0.01f)
            orbitRadius = 10f;

        timer = 0f;
        shotTimer = 0f;
    }

    Vector3 ComputeGroundLookPoint()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        float denom = ray.direction.y;

        if (Mathf.Abs(denom) < 0.0001f)
        {
            // Almost horizontal, fallback
            return transform.position + transform.forward * 10f;
        }

        float t = -ray.origin.y / denom;
        if (t < 0) t = 0;
        return ray.origin + ray.direction * t;
    }

    void Update()
    {
        if (state == State.Idle || targetObject == null)
            return;

        timer += Time.deltaTime;

        switch (state)
        {
            case State.Intro:
                UpdateIntro();
                if (timer >= cycleDelay)
                {
                    StartCycling();
                }
                break;

            case State.Cycling:
                UpdateCycling();
                break;
        }
    }

    void UpdateIntro()
    {
        float tMove = centerMoveDuration > 0f
            ? Mathf.Clamp01(timer / centerMoveDuration)
            : 1f;

        float tZoom = zoomDuration > 0f
            ? Mathf.Clamp01(timer / zoomDuration)
            : 1f;

        // Ease-out curves
        float easedMove = 1f - Mathf.Pow(1f - tMove, 3f);
        float easedZoom = 1f - Mathf.Pow(1f - tZoom, 3f);

        // Interpolate center from original look point to winner
        Vector3 center = Vector3.Lerp(startCenter, endCenter, easedMove);

        // Orbit
        yaw += orbitSpeed * Time.deltaTime;
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 offset = rot * Vector3.back * orbitRadius;
        transform.position = center + offset;
        transform.rotation = rot;

        // Zoom in
        cam.orthographicSize = Mathf.Lerp(startOrthoSize, finalOrthoSize, easedZoom);
    }

    void StartCycling()
    {
        // Build list of other players
        shotTargets.Clear();
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        foreach (var p in players)
        {
            if (p != null && p.transform != targetObject)
                shotTargets.Add(p.transform);
        }

        // Go back to winner at the end, and so forth
        shotTargets.Add(targetObject);

        currentShotIndex = -1;
        shotTimer = 0f;

        AdvanceShot();

        // Snap zoom fully to final size for cycling
        cam.orthographicSize = finalOrthoSize;

        state = State.Cycling;
    }

    void AdvanceShot()
    {
        if (shotTargets.Count == 0)
            return;

        currentShotIndex = (currentShotIndex + 1) % shotTargets.Count;

        // Clean up dead entries if any
        int safety = 0;
        while ((shotTargets[currentShotIndex] == null) && safety < shotTargets.Count)
        {
            shotTargets.RemoveAt(currentShotIndex);
            if (shotTargets.Count == 0)
                return;
            currentShotIndex %= shotTargets.Count;
            safety++;
        }

        if (shotTargets.Count == 0)
            return;

        Transform t = shotTargets[currentShotIndex];
        currentShotCenter = t.position + Vector3.up * heightOffset;

        // Hard cut of center (what you asked for)
        // Keep pitch, yaw, radius as they are for continuity of motion

        shotTimer = 0f;
    }

    void UpdateCycling()
    {
        if (shotTargets.Count == 0)
            return;

        shotTimer += Time.deltaTime;
        if (shotTimer >= timePerPlayerShot)
        {
            AdvanceShot();
        }

        // Orbit around currentShotCenter
        yaw += cyclingOrbitSpeed * Time.deltaTime;
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 offset = rot * Vector3.back * orbitRadius;
        transform.position = currentShotCenter + offset;
        transform.rotation = rot;

        // Keep zoom fixed during cycling
        cam.orthographicSize = finalOrthoSize;
    }
}
