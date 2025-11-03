using System.Collections.Generic;
using UnityEngine;

namespace Ai
{
    public class AiSpawnPoint : MonoBehaviour
    {
        [Header("Spawning")] 
        public GameObject aiPrefab;
        public int minSpawnCount = 3;
        public int maxSpawnCount = 6;
        public float activationRadius = 80f;
        public float wanderRadius = 30f;
        
        [Header("Spawning Placement")]
        public float spawnPlacementRadius = 10f;

        [Header("Dependencies")] 
        public Transform playerTransform;

        private int _spawnCount;
        private List<AiController> _spawnedAgents = new List<AiController>();
        private bool _hasSpawned = false;

        void Awake()
        {
            _spawnCount = Random.Range(minSpawnCount, maxSpawnCount + 1);

            if (playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player) playerTransform = player.transform;
            }
        }
        
        void Update()
        {
            if (!_hasSpawned && playerTransform != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                if (distanceToPlayer <= activationRadius)
                {
                    SpawnAgents();
                }
            }
        }
        
        public IReadOnlyList<AiController> SpawnedAgents => _spawnedAgents;
        void SpawnAgents()
        {
            _hasSpawned = true;
            Debug.Log($"Spawning {_spawnCount} agents at {transform.position}");

            int spawnedCount = 0;
            int maxSpawnAttempts = _spawnCount * 2; // ป้องกัน infinity loop
            
            while (spawnedCount < _spawnCount && maxSpawnAttempts > 0)
            {
                Vector2 randomCircle = Random.insideUnitCircle * spawnPlacementRadius;
                Vector3 randomPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
                
                Vector3 finalSpawnPos = transform.position;
                if (UnityEngine.AI.NavMesh.SamplePosition(randomPos, out var hit, spawnPlacementRadius, UnityEngine.AI.NavMesh.AllAreas))
                {
                    finalSpawnPos = hit.position;
                }
                if (IsPositionAvailable(finalSpawnPos, 1.5f))
                {
                    GameObject agentObj = Instantiate(aiPrefab, finalSpawnPos, Quaternion.identity);
                    AiController controller = agentObj.GetComponent<AiController>();
                    if (controller != null)
                    {
                        controller.Initialize(this);
                        _spawnedAgents.Add(controller);
                    }
                    spawnedCount++;
                }
                maxSpawnAttempts--;
            }
        }
        
        private bool IsPositionAvailable(Vector3 position, float radius)
        {
            Collider[] hitColliders = Physics.OverlapSphere(position, radius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.GetComponent<AiController>() != null)
                {
                    return false;
                }
            }

            return true;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, activationRadius);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, wanderRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, spawnPlacementRadius);
        }
    }
}