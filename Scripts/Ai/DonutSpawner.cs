using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; 

[System.Serializable]
public struct SpawnConfig
{
    public string type;
    public GameObject[] prefab; // reference to the spawned object
    public int limit;
    public float interval;
}
public struct SpawnedEntity
{
    public string type;             // same type as config
    public GameObject gameObject;   // actual spawned instance

    public SpawnedEntity(string t, GameObject go)
    {
        type = t;
        gameObject = go;
    }
}
public class DonutSpawner : MonoBehaviour
{
    public static DonutSpawner instance;
    public SpawnConfig[] spawnConfigs;

    [Header("Donut Area")]
    public Vector3 outerBounds = new Vector3(8, 1, 8);
    public Vector3 innerBounds = new Vector3(5, 1, 5);

    private List<SpawnedEntity> activeEntities = new List<SpawnedEntity>();
    private Dictionary<string, float> spawnTimers = new Dictionary<string, float>();
    public Transform player;
    public float playerSafeRadius = 3f;
    void Start()
    {
        instance = this;
        // Init spawn timers
        foreach (var config in spawnConfigs)
        {
            spawnTimers[config.type] = 0f;
        }
    }
    void Update()
    {
        // Clean up nulls
        activeEntities.RemoveAll(e => e.gameObject == null);

        foreach (var config in spawnConfigs)
        {
            // Count how many of this type are alive
            int currentCount = activeEntities.FindAll(e => e.type == config.type).Count;

            if (currentCount >= config.limit)
                continue; // Already at limit

            // Tick timer
            spawnTimers[config.type] -= Time.deltaTime;
            if (spawnTimers[config.type] <= 0f)
            {
                if (GetRandomSpawnPoint(out Vector3 spawnPoint))
                {
                    // pick random prefab from array
                    GameObject prefab = config.prefab[Random.Range(0, config.prefab.Length)];
                    GameObject go = Instantiate(prefab, spawnPoint, Quaternion.identity);
                    activeEntities.Add(new SpawnedEntity(config.type, go));

                    // If the thing has AI, hook it back
                    var ai = go.GetComponent<Ai.AiController>();
                    if (ai != null)
                    {
                        ai.donutArea = this;
                        if (Ai.FlockingManager.Instance != null)
                            Ai.FlockingManager.Instance.AddAgentToFlock(this, ai);
                    }
                }

                // Reset timer
                spawnTimers[config.type] = config.interval;
            }
        }
    }

    public void OnEntityDeath(GameObject obj)
    {
        activeEntities.RemoveAll(e => e.gameObject == obj);
    }

    public bool GetRandomSpawnPoint(out Vector3 result)
    {
        for (int i = 0; i < 10; i++)
        {
            float randomX = Random.Range(-outerBounds.x / 2, outerBounds.x / 2);
            float randomZ = Random.Range(-outerBounds.z / 2, outerBounds.z / 2);

            if (Mathf.Abs(randomX) > innerBounds.x * 0.5f || Mathf.Abs(randomZ) > innerBounds.z * 0.5f)
            {
                Vector3 randomPoint = transform.position + new Vector3(randomX, 0, randomZ);

                // Check if too close to player
                if (player != null && Vector3.Distance(randomPoint, player.position) < playerSafeRadius)
                    continue; // skip this point

                // Check if point is valid on NavMesh
                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, outerBounds.y, NavMesh.AllAreas))
                {
                    result = hit.position;
                    return true;
                }
            }
        }

        result = Vector3.zero;
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, outerBounds);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, innerBounds);
    }
    
}