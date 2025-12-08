using NaughtyAttributes;
using System.Collections.Generic;
using UC;
using UnityEngine;

public class PresentSpawner : MonoBehaviour
{
    [SerializeField, MinMaxSlider(0.5f, 5.0f)]
    private Vector2 spawnInterval = new Vector2(2.0f, 3.0f);
    [SerializeField]
    private int     initialPresents = 20;
    [SerializeField]
    private int     maxPresents = 50;
    [SerializeField]
    private List<GameObject> presentPrefabs;

    BoxCollider         spawnArea;
    List<GameObject>    presents = new();
    float               timer;

    void Start()
    {
        Cursor.visible = false;

        spawnArea = GetComponent<BoxCollider>();
        for (int i = 0; i < initialPresents; i++)
        {
            SpawnPresent();
        }

        timer = spawnInterval.Random();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0.0f)
        {
            SpawnPresent();
            timer = spawnInterval.Random();
        }
    }

    void SpawnPresent()
    {
        if (presents.Count >= maxPresents)
        {
            // Remove one random present
            var idx = Random.Range(0, presents.Count);
            var p = presents[idx];
            presents.RemoveAt(idx);
            p.gameObject.Delete();
        }

        int tries = 0;
        while (tries < 50)
        {
            tries++;
            Vector3 position = spawnArea.bounds.Random();
            foreach (var p in presents)
            {
                if (Vector3.Distance(p.transform.position, position) > 50.0f) continue;
            }

            var dir = Random.onUnitSphere;
            var newPresent = Instantiate(presentPrefabs.Random(), position, Quaternion.LookRotation(dir, dir.Perpendicular()));
            presents.Add(newPresent);
            break;
        }
    }
}
