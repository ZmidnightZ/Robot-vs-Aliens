using UnityEngine;
using System.Collections.Generic;

public class ChunkPool : MonoBehaviour
{
    public GameObject chunkPrefab;
    public int poolSize = 10;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(chunkPrefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetChunk()
    {
        GameObject obj;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
            obj.SetActive(true);

            // Trigger prop spawning when a chunk is taken from the pool.
            var chunkProps = obj.GetComponent<ChunkProps>();
            if (chunkProps != null)
            {
                chunkProps.SpawnProps();
            }

            return obj;
        }
        else
        {
            obj = Instantiate(chunkPrefab); // fallback
            // Newly instantiated chunk may already run OnEnable, but call SpawnProps to be explicit.
            var chunkProps = obj.GetComponent<ChunkProps>();
            if (chunkProps != null)
            {
                chunkProps.SpawnProps();
            }
            return obj;
        }
    }

    public void ReturnChunk(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}