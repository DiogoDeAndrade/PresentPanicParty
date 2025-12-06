using System.Collections.Generic;
using UC;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private List<Gift> gifts;
    [SerializeField] private int        minGifts;
    [SerializeField] private GameObject spawnEffectPrefab;
    [SerializeField] private LayerMask  avoidLayers;

    NavMeshSurface surface;

    List<Gift> activeGifts = new();

    void Start()
    {
        surface = GetComponent<NavMeshSurface>();
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
}
