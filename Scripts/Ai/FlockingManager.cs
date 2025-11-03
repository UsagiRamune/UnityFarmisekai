using System.Collections.Generic;
using UnityEngine;

namespace Ai
{
    public class FlockingManager : MonoBehaviour
    {
        public static FlockingManager Instance { get; private set; }

        public float neighborRadius = 5f;
        public float separationWeight = 1f;
        public float alignmentWeight = 1f;
        public float cohesionWeight = 1f;
        public float targetWeight = 1f;
        public float maxSpeed = 4f;

        
        /*public Dictionary<AiSpawnPoint, List<AiController>> allFlocks =
            new Dictionary<AiSpawnPoint, List<AiController>>();*/
        
        public Dictionary<DonutSpawner, List<AiController>> allFlocksDonut = new Dictionary<DonutSpawner, List<AiController>>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        public void AddAgentToFlock(DonutSpawner donutSp, AiController agent)
        {
            if (!allFlocksDonut.ContainsKey(donutSp))
            {
                allFlocksDonut[donutSp] = new List<AiController>();
            }
            allFlocksDonut[donutSp].Add(agent);
        }

        public void RemoveAgentFromFlock(DonutSpawner donutSp, AiController agent)
        {
            if (allFlocksDonut.ContainsKey(donutSp))
            {
                allFlocksDonut[donutSp].Remove(agent);
            }
        }

        // ฟังก์ชันที่สมาชิกแต่ละตัวเรียกใช้เพื่อคำนวณทิศทาง
        public Vector3 GetFlockDirection(DonutSpawner donutSp, AiController agent)
        {
            if (donutSp == null) return Vector3.zero;
            if (!allFlocksDonut.ContainsKey(donutSp)) return Vector3.zero;
            
            Vector3 separation = Vector3.zero;
            Vector3 alignment = Vector3.zero;
            Vector3 cohesion = Vector3.zero;

            int neighborCount = 0;
            List<AiController> flockMembers = allFlocksDonut[donutSp];
            foreach (var otherAgent in allFlocksDonut[donutSp])
            {
                if (otherAgent == agent) continue;
                if (otherAgent.currentMode != AiMode.Flocking) continue;                 // <- กรองโหมด
                if (EventManager.Instance.HasAgentReachedRally(otherAgent)) continue; 
                
                float distance = Vector3.Distance(agent.transform.position, otherAgent.transform.position);
                if (distance < neighborRadius)
                {
                    // Separation(การแยกตัว)
                    if (distance > 0)
                    {
                        float strength = 1.0f - (distance / neighborRadius);
                        Vector3 awayFromAgent = (agent.transform.position - otherAgent.transform.position);
                        separation += awayFromAgent * strength;
                    }

                    // Alignment(การจัดแนว) และ Cohesion(การรวมตัว)
                    alignment += otherAgent.GetComponent<UnityEngine.AI.NavMeshAgent>().velocity;
                    cohesion += otherAgent.transform.position;
                    neighborCount++;
                }
            }

            // คำนวณค่าเฉลี่ย
            if (neighborCount > 0)
            {
                alignment /= neighborCount;
                cohesion /= neighborCount;
                cohesion = (cohesion - agent.transform.position).normalized;
            }

            // เพิ่มแรงเข้าหาเป้าหมาย (rally point)
            Vector3 rallyDirection = (EventManager.Instance.GetRallyPoint().position - agent.transform.position).normalized;

            // รวมแรงทั้งหมด
            Vector3 finalDirection = (separation * separationWeight) +
                                     (alignment.normalized * alignmentWeight) +
                                     (cohesion * cohesionWeight) +
                                     (rallyDirection * targetWeight);
                                 
            return finalDirection;
        }
    }
}