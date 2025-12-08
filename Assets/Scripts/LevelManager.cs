using System.Collections.Generic;
using System.Linq;
using TMPro;
using UC;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private List<Gift>     gifts;
    [SerializeField] private int            minGifts;
    [SerializeField] private GameObject     spawnEffectPrefab;
    [SerializeField] private LayerMask      avoidLayers;
    [SerializeField] private Essence        essencePrefab;
    [SerializeField] private int            minEssences;
    [SerializeField] private float          levelDuration;
    [SerializeField] private CanvasGroup    levelEndPanel;
    [SerializeField] private SoundDef       levelCompleteSound;
    [SerializeField] private Camera         mainGameCamera;

    NavMeshSurface surface;

    List<Gift>      activeGifts = new();
    List<Essence>   activeEssences = new();
    float           levelTime;

    static LevelManager instance;

    private void Awake()
    {
        Cursor.visible = false;
    }

    void Start()
    {
        instance = this;
        surface = GetComponent<NavMeshSurface>();
        levelTime = levelDuration;
        levelEndPanel.alpha = 0.0f;        
    }

    // Update is called once per frame
    void Update()
    {
        activeGifts.RemoveAll((g) => g == null);
        if (activeGifts.Count < minGifts)
        {
            var position = GetRandomPositionOnNavMesh();

            if (position != Vector3.zero)
            {
                var gift = Instantiate(gifts.Random(), position, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                activeGifts.Add(gift);

                Instantiate(spawnEffectPrefab, position, Quaternion.identity);
            }
        }

        activeEssences.RemoveAll((g) => g == null);
        if (activeEssences.Count < minEssences)
        {
            var position = GetRandomPositionOnNavMesh();

            if (position != Vector3.zero)
            {
                var essence = Instantiate(essencePrefab, position + Vector3.up * 0.5f, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                activeEssences.Add(essence);
            }
        }

        if (levelTime > 0.0f)
        {
            levelTime -= Time.deltaTime;
            if (levelTime <= 0.0f)
            {
                levelTime = 0.0f;

                // Finish level
                LevelComplete();
            }
        }
    }

    void LevelComplete()
    {
        levelCompleteSound?.Play();

        var players = FindObjectsByType<Player>(FindObjectsSortMode.None).ToList();
        players.Sort((p1, p2) => p2.score.CompareTo(p1.score));
        var winner = players[0];
        winner.Celebrate();
        for (int i = 1; i < players.Count; i++)
        {
            players[i].Lose();
        }

        levelEndPanel.FadeIn(0.2f);
        var text = levelEndPanel.GetComponentInChildren<TextMeshProUGUI>();
        text.text = $"Player {players[0].playerId + 1} wins!";

        var orbitOnWin = mainGameCamera.GetComponent<OrbitOnWin>();
        orbitOnWin.SetTarget(players[0].transform);
    }

    Vector3 GetRandomPositionOnNavMesh()
    {
        // Use the baked NavMeshSurface bounds (in local space)
        var bounds = surface.navMeshData.sourceBounds;

        int     maxTries = 50;
        float   maxSampleDistance = 2.0f;

        for (int i = 0; i < maxTries; i++)
        {
            // Random point in the local AABB of the surface
            var localPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.center.y, // height roughly in the middle of the surface
                Random.Range(bounds.min.z, bounds.max.z)
            );

            // Convert to world space
            var worldPoint = surface.transform.TransformPoint(localPoint);

            // Project to the NavMesh
            if (NavMesh.SamplePosition(worldPoint, out var hit, maxSampleDistance, NavMesh.AllAreas))
            {
                // Check if there players and other gifts nearby
                if (!HasPlayersOrGiftsNear(hit.position, 4.0f))
                {
                    return hit.position;
                }
            }
        }

        // Fallback (if we somehow fail to find anything)
        return Vector3.zero;
    }

    private bool HasPlayersOrGiftsNear(Vector3 position, float radius)
    {
        return Physics.OverlapSphere(position, radius, avoidLayers).Length > 0;
    }

    public static Vector3 GetRandomPositionOnNavMesh(Vector3 position, float radius)
    {
        int maxTries = 50;
        float maxSampleDistance = 2.0f;

        for (int i = 0; i < maxTries; i++)
        {
            // Random point in the local AABB of the surface
            var localPoint = position + Random.insideUnitSphere.x0z() * radius;

            // Convert to world space
            var worldPoint = instance.surface.transform.TransformPoint(localPoint);

            // Project to the NavMesh
            if (NavMesh.SamplePosition(worldPoint, out var hit, maxSampleDistance, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        // Fallback (if we somehow fail to find anything)
        return Vector3.zero;
    }

    public static float timeRemaining => instance.levelTime;
    public static bool isDone => instance.levelTime <= 0.0f;
}
