using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public Transform player;
    public GameObject chunkPrefab;
    public int chunkSize = 20;
    public int viewDistance = 2;

    public ChunkPool pool;
    private Dictionary<Vector2, GameObject> activeChunks = new Dictionary<Vector2, GameObject>();

    void Update()
    {
        Vector2 playerChunk = new Vector2(
            Mathf.Floor(player.position.x / chunkSize),
            Mathf.Floor(player.position.y / chunkSize)
        );

        List<Vector2> neededChunks = new List<Vector2>();

        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int y = -viewDistance; y <= viewDistance; y++)
            {
                Vector2 coord = playerChunk + new Vector2(x, y);
                neededChunks.Add(coord);

                if (!activeChunks.ContainsKey(coord))
                {
                    // Obtain chunk from pool if available, otherwise instantiate
                    Vector3 position = new Vector3(coord.x * chunkSize, coord.y * chunkSize, 0);
                    GameObject chunk;
                    if (pool != null)
                    {
                        chunk = pool.GetChunk();
                        chunk.transform.position = position;
                    }
                    else
                    {
                        chunk = Instantiate(chunkPrefab, position, Quaternion.identity);
                    }

                    activeChunks.Add(coord, chunk);
                }
            }
        }

        // Remove far chunks
        List<Vector2> toRemove = new List<Vector2>();

        foreach (var chunk in activeChunks)
        {
            if (!neededChunks.Contains(chunk.Key))
            {
                if (pool != null)
                {
                    pool.ReturnChunk(chunk.Value);
                }
                else
                {
                    Destroy(chunk.Value);
                }

                toRemove.Add(chunk.Key);
            }
        }

        foreach (var coord in toRemove)
        {
            activeChunks.Remove(coord);
        }
    }

    void SpawnChunk(Vector2 coord)
    {
        Vector3 position = new Vector3(coord.x * chunkSize, coord.y * chunkSize, 0);
        GameObject chunk = Instantiate(chunkPrefab, position, Quaternion.identity);

        // Add random objects
        int random = Random.Range(0, 3);

        if (random == 0)
        {
            // spawn tree / rock prefab
        }

        // Keep the legacy dictionary in sync if SpawnChunk is used elsewhere
        if (!activeChunks.ContainsKey(coord))
        {
            activeChunks.Add(coord, chunk);
        }
    }
}