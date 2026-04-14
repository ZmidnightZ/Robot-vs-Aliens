using UnityEngine;
using System.Collections.Generic;

public class ChunkProps : MonoBehaviour
{
    [System.Serializable]
    public class Prop
    {
        public GameObject prefab;
        public Sprite[] sprites;                 // multiple variants (7 sprites)
        [Range(0f, 1f)] public float spawnChance;
    }

    [Header("Props")]
    public List<Prop> props;

    [Header("Spawn Settings")]
    public int attempts = 30;      // how many tries to spawn props
    public float chunkSize = 20f; // match your chunk size
    public float overlapCheckRadius = 0.3f;    // radius for overlap test (used for blocking props)
    public LayerMask blockingLayers = ~0;      // layers considered when checking overlaps

    public float minScale = 0.9f;
    public float maxScale = 1.1f;

    // runtime pools: one queue per prefab
    private readonly Dictionary<GameObject, Queue<GameObject>> _propPool = new Dictionary<GameObject, Queue<GameObject>>();
    private static Transform _poolRoot;
    private Transform _t;

    void Awake()
    {
        _t = transform;
        if (_poolRoot == null)
        {
            var go = new GameObject("[PropPoolRoot]");
            DontDestroyOnLoad(go);
            _poolRoot = go.transform;
        }
    }

    void OnEnable()
    {
        SpawnProps();
    }

    // Public so ChunkPool (or others) can explicitly trigger spawning
    public void SpawnProps()
    {
        GenerateProps();
    }

    private void GenerateProps()
    {
        if (props == null || props.Count == 0 || attempts <= 0 || chunkSize <= 0f)
            return;

        // Return existing children to pool (pool-friendly)
        CleanOldProps();

        float half = chunkSize * 0.5f;

        for (int i = 0; i < attempts; i++)
        {
            // random position inside chunk (centered)
            Vector2 local = new Vector2(
                Random.Range(-half, half),
                Random.Range(-half, half)
            );
            Vector3 spawnPos = _t.position + new Vector3(local.x, local.y, 0f);

            // choose a prop (original per-prop independent-chance semantics)
            Prop chosen = null;
            foreach (var p in props)
            {
                if (p == null || p.prefab == null)
                    continue;
                if (Random.value < p.spawnChance)
                {
                    chosen = p;
                    break;
                }
            }

            if (chosen == null)
                continue;

            // If the prefab has a Collider2D, treat it as blocking and check overlaps
            bool isBlocking = chosen.prefab.GetComponentInChildren<Collider2D>() != null;
            if (isBlocking)
            {
                // skip placement if overlap detected
                if (Physics2D.OverlapCircle(spawnPos, overlapCheckRadius, blockingLayers) != null)
                    continue;
            }

            // obtain instance (pooled or new)
            GameObject instance = GetFromPool(chosen.prefab);
            if (instance == null)
                continue;

            // position/parent/activate
            instance.transform.SetParent(_t, false);
            instance.transform.position = spawnPos;
            //instance.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            float s = Random.Range(minScale, maxScale);
            instance.transform.localScale = Vector3.one * s;

            // set sprite variant if provided (no sorting changes here)
            if (chosen.sprites != null && chosen.sprites.Length > 0)
            {
                var sr = instance.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = chosen.sprites[Random.Range(0, chosen.sprites.Length)];
                }
            }

            instance.SetActive(true);
        }
    }

    private void CleanOldProps()
    {
        for (int i = _t.childCount - 1; i >= 0; i--)
        {
            var child = _t.GetChild(i).gameObject;
            ReturnToPool(child);
        }
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        if (prefab == null)
            return null;

        if (!_propPool.TryGetValue(prefab, out var q))
        {
            q = new Queue<GameObject>();
            _propPool[prefab] = q;
        }

        while (q.Count > 0)
        {
            var obj = q.Dequeue();
            if (obj != null)
                return obj;
        }

        // instantiate new instance and attach a small marker to identify origin
        var inst = Object.Instantiate(prefab, _poolRoot);
        var marker = inst.GetComponent<PooledPropMarker>();
        if (marker == null)
            marker = inst.AddComponent<PooledPropMarker>();
        marker.OriginPrefab = prefab;
        inst.SetActive(false);
        return inst;
    }

    private void ReturnToPool(GameObject go)
    {
        if (go == null)
            return;

        // if this is a chunk child that was spawned by this system, it should have a marker
        var marker = go.GetComponent<PooledPropMarker>();
        if (marker != null && marker.OriginPrefab != null)
        {
            go.SetActive(false);
            go.transform.SetParent(_poolRoot, false);

            if (!_propPool.TryGetValue(marker.OriginPrefab, out var q))
            {
                q = new Queue<GameObject>();
                _propPool[marker.OriginPrefab] = q;
            }
            q.Enqueue(go);
        }
        else
        {
            // unknown object — just destroy to avoid orphaned objects
            Object.Destroy(go);
        }
    }

    // small helper to map instance -> prefab origin for pooling
    private class PooledPropMarker : MonoBehaviour
    {
        public GameObject OriginPrefab;
    }
}